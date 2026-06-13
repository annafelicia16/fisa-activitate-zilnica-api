using AutoMapper;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.SupplementaryActivities.Models;
using FisaActivitateZilnicaApi.SupplementaryActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Services;

// Mirrors DailyActivityRecordAttachmentsService for supplementary activities:
// files live under {FILE_STORAGE_PATH|./FileStorage}/supplementary-activities/
// {activityId}/ with server-generated names ("{attachmentId}{ext}") so nothing
// user-controlled ever becomes part of a path.
public class SupplementaryActivityAttachmentsService(
    ISupplementaryActivityAttachmentsRepository attachmentsRepository,
    ISupplementaryActivitiesRepository supplementaryActivitiesRepository,
    IMapper mapper,
    ILogger<SupplementaryActivityAttachmentsService> logger
) : ISupplementaryActivityAttachmentsService
{
    private const int MaxFilesPerActivity = 20;
    private const long MaxFileSizeBytes = 100L * 1024 * 1024;

    private static readonly HashSet<string> BlockedExtensions = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ".exe",
        ".dll",
        ".bat",
        ".cmd",
        ".com",
        ".msi",
        ".scr",
        ".ps1",
        ".sh",
        ".vbs",
        ".js",
    };

    private readonly ISupplementaryActivityAttachmentsRepository _attachmentsRepository =
        attachmentsRepository;
    private readonly ISupplementaryActivitiesRepository _supplementaryActivitiesRepository =
        supplementaryActivitiesRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<SupplementaryActivityAttachmentsService> _logger = logger;

    private readonly string _storageRoot = Path.Combine(
        Environment.GetEnvironmentVariable("FILE_STORAGE_PATH")
            ?? Path.Combine(Directory.GetCurrentDirectory(), "FileStorage"),
        "supplementary-activities"
    );

    public async Task<
        IReadOnlyList<GetSupplementaryActivityAttachmentResponse>
    > UploadAttachmentsAsync(
        string activityId,
        IReadOnlyList<IFormFile> files,
        CancellationToken ct = default
    )
    {
        await EnsureActivityExistsAsync(activityId);

        if (files.Count == 0)
            throw new ArgumentException("At least one file is required.");

        int existingCount = await _attachmentsRepository.CountByActivityId(activityId);
        if (existingCount + files.Count > MaxFilesPerActivity)
            throw new ArgumentException(
                $"An activity can have at most {MaxFilesPerActivity} attachments "
                    + $"({existingCount} already uploaded)."
            );

        var attachments = new List<SupplementaryActivityAttachment>(files.Count);
        foreach (IFormFile file in files)
        {
            string fileName = SanitizeFileName(file.FileName);
            ValidateFile(file, fileName);

            var attachment = new SupplementaryActivityAttachment
            {
                SupplementaryActivityId = activityId,
                FileName = fileName,
                StoredFileName = string.Empty,
                ContentType = NormalizeContentType(file.ContentType),
                SizeBytes = file.Length,
            };
            attachment.StoredFileName =
                attachment.Id + Path.GetExtension(fileName).ToLowerInvariant();
            attachments.Add(attachment);
        }

        string activityDirectory = ActivityDirectory(activityId);
        Directory.CreateDirectory(activityDirectory);

        var writtenPaths = new List<string>(files.Count);
        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                string path = Path.Combine(activityDirectory, attachments[i].StoredFileName);
                await using FileStream destination = File.Create(path);
                writtenPaths.Add(path);
                await files[i].CopyToAsync(destination, ct);
            }

            await _attachmentsRepository.AddRange(attachments);
        }
        catch
        {
            // Without DB rows the written files would be unreachable orphans —
            // best-effort removal before surfacing the original failure.
            foreach (string path in writtenPaths)
                TryDeleteFile(path);
            throw;
        }

        return attachments
            .Select(_mapper.Map<GetSupplementaryActivityAttachmentResponse>)
            .ToList();
    }

    public async Task<
        IReadOnlyList<GetSupplementaryActivityAttachmentResponse>
    > GetAttachmentsForActivityAsync(string activityId)
    {
        await EnsureActivityExistsAsync(activityId);

        var attachments = await _attachmentsRepository.GetByActivityId(activityId);
        return attachments
            .Select(_mapper.Map<GetSupplementaryActivityAttachmentResponse>)
            .ToList();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)>
        OpenAttachmentForDownloadAsync(string attachmentId)
    {
        SupplementaryActivityAttachment attachment = await GetAttachmentOrThrowAsync(
            attachmentId
        );

        string path = Path.Combine(
            ActivityDirectory(attachment.SupplementaryActivityId),
            attachment.StoredFileName
        );
        if (!File.Exists(path))
            throw new KeyNotFoundException("Attachment file is missing from storage.");

        Stream stream = File.OpenRead(path);
        return (stream, attachment.ContentType, attachment.FileName);
    }

    public async Task DeleteAttachmentAsync(string attachmentId)
    {
        SupplementaryActivityAttachment attachment = await GetAttachmentOrThrowAsync(
            attachmentId
        );

        TryDeleteFile(
            Path.Combine(
                ActivityDirectory(attachment.SupplementaryActivityId),
                attachment.StoredFileName
            )
        );
        await _attachmentsRepository.Delete(attachment);
    }

    public Task DeleteAllAttachmentFilesForActivityAsync(string activityId)
    {
        string activityDirectory = ActivityDirectory(activityId);
        try
        {
            if (Directory.Exists(activityDirectory))
                Directory.Delete(activityDirectory, recursive: true);
        }
        catch (Exception ex)
        {
            // Orphaned files are harmless; the activity and rows are already gone.
            _logger.LogWarning(
                ex,
                "Could not delete attachment folder for supplementary activity {ActivityId}.",
                activityId
            );
        }
        return Task.CompletedTask;
    }

    private string ActivityDirectory(string activityId)
    {
        // Route ids flow into the path — only GUID-shaped values are accepted.
        if (!Guid.TryParse(activityId, out Guid parsed))
            throw new ArgumentException("Route parameter 'id' must be a valid GUID.");
        return Path.Combine(_storageRoot, parsed.ToString());
    }

    private async Task EnsureActivityExistsAsync(string activityId)
    {
        if (!Guid.TryParse(activityId, out _))
            throw new ArgumentException("Route parameter 'id' must be a valid GUID.");

        GetSupplementaryActivityResponse? activity =
            await _supplementaryActivitiesRepository.GetSupplementaryActivityById(activityId);
        if (activity == null)
            throw new KeyNotFoundException("Supplementary activity does not exist.");
    }

    private async Task<SupplementaryActivityAttachment> GetAttachmentOrThrowAsync(
        string attachmentId
    )
    {
        if (!Guid.TryParse(attachmentId, out _))
            throw new ArgumentException("Route parameter 'attachmentId' must be a valid GUID.");

        SupplementaryActivityAttachment? attachment = await _attachmentsRepository.GetById(
            attachmentId
        );
        if (attachment == null)
            throw new KeyNotFoundException("Attachment does not exist.");
        return attachment;
    }

    private static void ValidateFile(IFormFile file, string sanitizedName)
    {
        if (file.Length == 0)
            throw new ArgumentException($"File '{sanitizedName}' is empty.");
        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException(
                $"File '{sanitizedName}' exceeds the {MaxFileSizeBytes / (1024 * 1024)} MB limit."
            );

        string extension = Path.GetExtension(sanitizedName);
        if (BlockedExtensions.Contains(extension))
            throw new ArgumentException(
                $"Files with the '{extension}' extension are not allowed."
            );
    }

    private static string SanitizeFileName(string? rawName)
    {
        string name = Path.GetFileName(rawName ?? string.Empty).Trim();
        if (name.Length == 0)
            name = "fisier";
        if (name.Length > 255)
            name = name[^255..];
        return name;
    }

    private static string NormalizeContentType(string? contentType)
    {
        string value = (contentType ?? string.Empty).Trim();
        if (value.Length == 0 || value.Length > 255)
            return "application/octet-stream";
        return value;
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete attachment file {Path}.", path);
        }
    }
}
