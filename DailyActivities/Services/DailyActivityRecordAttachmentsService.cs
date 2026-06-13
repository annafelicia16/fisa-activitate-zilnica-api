using AutoMapper;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;

namespace FisaActivitateZilnicaApi.DailyActivities.Services;

// Stores attachment files on local disk under
// {FILE_STORAGE_PATH|./FileStorage}/daily-activity-records/{recordId}/ with
// server-generated names ("{attachmentId}{ext}") so nothing user-controlled
// ever becomes part of a path. Original names live only in the DB.
public class DailyActivityRecordAttachmentsService(
    IDailyActivityRecordAttachmentsRepository attachmentsRepository,
    IDailyActivityRecordsRepository dailyActivityRecordsRepository,
    IMapper mapper,
    ILogger<DailyActivityRecordAttachmentsService> logger
) : IDailyActivityRecordAttachmentsService
{
    private const int MaxFilesPerRecord = 20;
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

    private readonly IDailyActivityRecordAttachmentsRepository _attachmentsRepository =
        attachmentsRepository;
    private readonly IDailyActivityRecordsRepository _dailyActivityRecordsRepository =
        dailyActivityRecordsRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<DailyActivityRecordAttachmentsService> _logger = logger;

    private readonly string _storageRoot = Path.Combine(
        Environment.GetEnvironmentVariable("FILE_STORAGE_PATH")
            ?? Path.Combine(Directory.GetCurrentDirectory(), "FileStorage"),
        "daily-activity-records"
    );

    public async Task<IReadOnlyList<GetDailyActivityRecordAttachmentResponse>> UploadAttachmentsAsync(
        string recordId,
        IReadOnlyList<IFormFile> files,
        CancellationToken ct = default
    )
    {
        await EnsureRecordExistsAsync(recordId);

        if (files.Count == 0)
            throw new ArgumentException("At least one file is required.");

        int existingCount = await _attachmentsRepository.CountByRecordId(recordId);
        if (existingCount + files.Count > MaxFilesPerRecord)
            throw new ArgumentException(
                $"A record can have at most {MaxFilesPerRecord} attachments "
                    + $"({existingCount} already uploaded)."
            );

        var attachments = new List<DailyActivityRecordAttachment>(files.Count);
        foreach (IFormFile file in files)
        {
            string fileName = SanitizeFileName(file.FileName);
            ValidateFile(file, fileName);

            var attachment = new DailyActivityRecordAttachment
            {
                DailyActivityRecordId = recordId,
                FileName = fileName,
                StoredFileName = string.Empty,
                ContentType = NormalizeContentType(file.ContentType),
                SizeBytes = file.Length,
            };
            attachment.StoredFileName =
                attachment.Id + Path.GetExtension(fileName).ToLowerInvariant();
            attachments.Add(attachment);
        }

        string recordDirectory = RecordDirectory(recordId);
        Directory.CreateDirectory(recordDirectory);

        var writtenPaths = new List<string>(files.Count);
        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                string path = Path.Combine(recordDirectory, attachments[i].StoredFileName);
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
            .Select(_mapper.Map<GetDailyActivityRecordAttachmentResponse>)
            .ToList();
    }

    public async Task<
        IReadOnlyList<GetDailyActivityRecordAttachmentResponse>
    > GetAttachmentsForRecordAsync(string recordId)
    {
        await EnsureRecordExistsAsync(recordId);

        var attachments = await _attachmentsRepository.GetByRecordId(recordId);
        return attachments.Select(_mapper.Map<GetDailyActivityRecordAttachmentResponse>).ToList();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)>
        OpenAttachmentForDownloadAsync(string attachmentId)
    {
        DailyActivityRecordAttachment attachment = await GetAttachmentOrThrowAsync(attachmentId);

        string path = Path.Combine(
            RecordDirectory(attachment.DailyActivityRecordId),
            attachment.StoredFileName
        );
        if (!File.Exists(path))
            throw new KeyNotFoundException("Attachment file is missing from storage.");

        Stream stream = File.OpenRead(path);
        return (stream, attachment.ContentType, attachment.FileName);
    }

    public async Task DeleteAttachmentAsync(string attachmentId)
    {
        DailyActivityRecordAttachment attachment = await GetAttachmentOrThrowAsync(attachmentId);

        TryDeleteFile(
            Path.Combine(RecordDirectory(attachment.DailyActivityRecordId), attachment.StoredFileName)
        );
        await _attachmentsRepository.Delete(attachment);
    }

    public Task DeleteAllAttachmentFilesForRecordAsync(string recordId)
    {
        string recordDirectory = RecordDirectory(recordId);
        try
        {
            if (Directory.Exists(recordDirectory))
                Directory.Delete(recordDirectory, recursive: true);
        }
        catch (Exception ex)
        {
            // Orphaned files are harmless; the record and rows are already gone.
            _logger.LogWarning(
                ex,
                "Could not delete attachment folder for record {RecordId}.",
                recordId
            );
        }
        return Task.CompletedTask;
    }

    private string RecordDirectory(string recordId)
    {
        // Route ids flow into the path — only GUID-shaped values are accepted.
        if (!Guid.TryParse(recordId, out Guid parsed))
            throw new ArgumentException("Route parameter 'id' must be a valid GUID.");
        return Path.Combine(_storageRoot, parsed.ToString());
    }

    private async Task EnsureRecordExistsAsync(string recordId)
    {
        if (!Guid.TryParse(recordId, out _))
            throw new ArgumentException("Route parameter 'id' must be a valid GUID.");

        GetDailyActivityRecordResponse? record =
            await _dailyActivityRecordsRepository.GetDailyActivityRecordById(recordId);
        if (record == null)
            throw new KeyNotFoundException("Daily activity record does not exist.");
    }

    private async Task<DailyActivityRecordAttachment> GetAttachmentOrThrowAsync(
        string attachmentId
    )
    {
        if (!Guid.TryParse(attachmentId, out _))
            throw new ArgumentException("Route parameter 'attachmentId' must be a valid GUID.");

        DailyActivityRecordAttachment? attachment = await _attachmentsRepository.GetById(
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
