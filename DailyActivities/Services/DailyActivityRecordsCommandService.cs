using System.Text.RegularExpressions;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;
using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

namespace FisaActivitateZilnicaApi.DailyActivities.Services;

public class DailyActivityRecordsCommandService(
    IDailyActivityRecordsRepository dailyActivityRecordsRepository,
    ISchedulesQueryService schedulesQueryService,
    IDailyActivityRecordAttachmentsService dailyActivityRecordAttachmentsService
) : IDailyActivityRecordsCommandService
{
    private const int MaxAutoFillRangeDays = 366;

    private static readonly Regex SlotStartTimeRegex = new(
        @"^(\d{1,2})(?::(\d{2}))?",
        RegexOptions.Compiled
    );

    private readonly IDailyActivityRecordsRepository _dailyActivityRecordsRepository =
        dailyActivityRecordsRepository;
    private readonly ISchedulesQueryService _schedulesQueryService = schedulesQueryService;
    private readonly IDailyActivityRecordAttachmentsService _dailyActivityRecordAttachmentsService =
        dailyActivityRecordAttachmentsService;

    public async Task<GetDailyActivityRecordResponse> CreateDailyActivityRecordAsync(
        CreateDailyActivityRecordRequest request
    )
    {
        ValidateCreateRequest(request);
        return await _dailyActivityRecordsRepository.CreateDailyActivityRecord(request);
    }

    // Walks every day in [StartDate, EndDate] (capped at today so the future is
    // never pre-filled) and creates a record for each scheduled slot that doesn't
    // already have one. The schedule query already drops slots that have a record
    // pointing at them, so re-running is idempotent. Slots missing a required
    // field (unresolved study program / group / room / year) are skipped and
    // counted rather than persisted as invalid rows.
    public async Task<AutoFillDailyActivityRecordsResponse> AutoFillDailyActivityRecordsAsync(
        AutoFillDailyActivityRecordsRequest request,
        CancellationToken ct = default
    )
    {
        ValidateAutoFillRequest(request);

        DateTime today = DateTime.UtcNow.Date;
        DateTime rangeStart = request.StartDate.Date;
        DateTime rangeEnd = request.EndDate.Date < today ? request.EndDate.Date : today;

        string department = request.DepartmentName?.Trim() ?? string.Empty;

        var created = new List<GetDailyActivityRecordResponse>();
        int skipped = 0;

        for (DateTime date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            ct.ThrowIfCancellationRequested();

            IReadOnlyList<TeacherScheduleSlotResponse> slots =
                await _schedulesQueryService.GetTeacherDaySlotsByDateAsync(
                    new GetTeacherDaySlotsByDateRequest
                    {
                        ExternalTeacherId = request.ExternalTeacherId,
                        Date = date,
                    },
                    ct
                );

            foreach (TeacherScheduleSlotResponse slot in slots)
            {
                CreateDailyActivityRecordRequest? createRequest = BuildCreateRequestFromSlot(
                    slot,
                    date,
                    request.ExternalTeacherId,
                    department
                );

                if (createRequest is null)
                {
                    skipped++;
                    continue;
                }

                created.Add(
                    await _dailyActivityRecordsRepository.CreateDailyActivityRecord(createRequest)
                );
            }
        }

        return new AutoFillDailyActivityRecordsResponse(created.Count, skipped, created);
    }

    public async Task<GetDailyActivityRecordResponse> UpdateDailyActivityRecordAsync(
        UpdateDailyActivityRecordRequest request
    )
    {
        ValidateUpdateRequest(request);
        await EnsureRecordExistsAsync(request.Id);

        return await _dailyActivityRecordsRepository.UpdateDailyActivityRecord(request);
    }

    public async Task<GetDailyActivityRecordResponse> DeleteDailyActivityRecordAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Route parameter 'id' is required.");

        await EnsureRecordExistsAsync(id);

        // Attachment rows die with the record via the FK cascade; the disk
        // files need explicit cleanup, after the DB delete succeeds.
        GetDailyActivityRecordResponse deleted =
            await _dailyActivityRecordsRepository.DeleteDailyActivityRecord(id);
        await _dailyActivityRecordAttachmentsService.DeleteAllAttachmentFilesForRecordAsync(id);
        return deleted;
    }

    private async Task EnsureRecordExistsAsync(string id)
    {
        GetDailyActivityRecordResponse? existingRecord =
            await _dailyActivityRecordsRepository.GetDailyActivityRecordById(id);

        if (existingRecord == null)
            throw new KeyNotFoundException("Daily activity record does not exist.");
    }

    // Maps one scheduled slot onto a create request, replicating the client's
    // slotToForm + formToPayload mapping (conventional-hours multiplier, start/end
    // time, group/faculty fallbacks). Returns null when a [Required] field can't be
    // resolved, signalling the caller to skip the slot.
    private static CreateDailyActivityRecordRequest? BuildCreateRequestFromSlot(
        TeacherScheduleSlotResponse slot,
        DateTime date,
        int externalTeacherId,
        string department
    )
    {
        (string kind, double multiplier) = ResolveActivityKind(
            !string.IsNullOrEmpty(slot.OldActivityTag) ? slot.OldActivityTag : slot.CourseTypeTag
        );

        double duration = slot.Duration > 0 ? slot.Duration : 1;
        // Mirrors the client's conventionalHours(): Math.round(actual * mult * 10) / 10.
        // Math.Floor(x + 0.5) matches JS Math.round's half-up behaviour for positives.
        double conventionalHours = Math.Floor(duration * multiplier * 10 + 0.5) / 10;

        (int hour, int minute) = ParseSlotStartTime(slot.HourName);
        var start = new DateTime(
            date.Year,
            date.Month,
            date.Day,
            hour,
            minute,
            0,
            DateTimeKind.Unspecified
        );
        DateTime end = start.AddHours(duration);

        string faculty = FirstNonEmpty(slot.FacultateName, department);
        string departmentName = !string.IsNullOrWhiteSpace(department) ? department : faculty;
        string studyProgram = slot.SpecializareName?.Trim() ?? string.Empty;
        string group = DisplayGroup(
            FirstNonEmpty(slot.GrupaName, FirstCsvToken(slot.Grupa), slot.IdGrupa)
        );
        string subject = FirstNonEmpty(slot.MaterieName, slot.SubjectName);
        string room = slot.RoomName?.Trim() ?? string.Empty;
        int year = slot.NrAnStudii ?? 0;
        RevenueType revenue =
            slot.PlataNB == 1 ? RevenueType.BaseSalary
            : slot.PlataNB == 0 ? RevenueType.HourlyPay
            : RevenueType.BaseSalary;

        // Same fields the manual create path enforces — skip rather than persist a
        // record the validator would reject.
        if (
            string.IsNullOrWhiteSpace(departmentName)
            || string.IsNullOrWhiteSpace(faculty)
            || string.IsNullOrWhiteSpace(studyProgram)
            || string.IsNullOrWhiteSpace(group)
            || string.IsNullOrWhiteSpace(subject)
            || string.IsNullOrWhiteSpace(room)
            || year <= 0
        )
        {
            return null;
        }

        return new CreateDailyActivityRecordRequest
        {
            ExternalTeacherId = externalTeacherId,
            DepartmentName = departmentName,
            FacultyName = faculty,
            StudyProgram = studyProgram,
            CourseType = kind,
            Year = year,
            GroupName = group,
            SubjectName = subject,
            RoomName = room,
            RevenueType = revenue,
            ActivityType = ActivityType.RegularHours,
            ConventionalHours = conventionalHours,
            Observations = null,
            StartDate = start,
            EndDate = end,
            ActivitySlotId = slot.SlotId,
        };
    }

    // Normalizes a FET/legacy activity tag into the canonical kind + its
    // conventional-hours multiplier. Unknown tags fall back to "Curs", matching the
    // client's canonicalKind().
    private static (string Kind, double Multiplier) ResolveActivityKind(string? raw)
    {
        string value = (raw ?? string.Empty).Trim().ToLowerInvariant();
        if (value.StartsWith("curs") || value.StartsWith("course") || value.StartsWith("lect"))
            return ("Curs", 2.0);
        if (value.StartsWith("seminar"))
            return ("Seminar", 1.0);
        if (value.StartsWith("lab") || value.StartsWith("lucrare"))
            return ("Laborator", 1.0);
        if (value.StartsWith("proiect") || value.StartsWith("project"))
            return ("Proiect", 1.0);
        return ("Curs", 2.0);
    }

    // Parses the slot's start time from its many shapes ("08:00-09:50", "8-10",
    // "12", "12:00"), returning (hour, minute) or (0, 0) when unparseable.
    private static (int Hour, int Minute) ParseSlotStartTime(string? hourName)
    {
        if (string.IsNullOrWhiteSpace(hourName))
            return (0, 0);

        Match match = SlotStartTimeRegex.Match(hourName);
        if (!match.Success)
            return (0, 0);

        int hour = int.TryParse(match.Groups[1].Value, out int h) ? h : 0;
        int minute =
            match.Groups[2].Success && int.TryParse(match.Groups[2].Value, out int m) ? m : 0;

        if (hour is < 0 or > 23)
            hour = 0;
        if (minute is < 0 or > 59)
            minute = 0;

        return (hour, minute);
    }

    private static string FirstCsvToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        return value.Split(',')[0].Trim();
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return string.Empty;
    }

    // AGSIS "-1" / "-" are whole-class sentinels → render as "-"; anything else is
    // returned trimmed. Mirrors the client's displayGroup().
    private static string DisplayGroup(string? value)
    {
        if (value is null)
            return string.Empty;
        string trimmed = value.Trim();
        if (trimmed is "-1" or "-")
            return "-";
        return trimmed;
    }

    private static void ValidateAutoFillRequest(AutoFillDailyActivityRecordsRequest request)
    {
        if (request.ExternalTeacherId <= 0)
            throw new ArgumentException("Field 'externalTeacherId' must be greater than 0.");

        if (request.StartDate == default)
            throw new ArgumentException("Field 'startDate' is required.");

        if (request.EndDate == default)
            throw new ArgumentException("Field 'endDate' is required.");

        if (request.StartDate.Date > request.EndDate.Date)
            throw new ArgumentException("'startDate' cannot be greater than 'endDate'.");

        if ((request.EndDate.Date - request.StartDate.Date).TotalDays > MaxAutoFillRangeDays)
            throw new ArgumentException($"Date range cannot exceed {MaxAutoFillRangeDays} days.");
    }

    private static void ValidateCreateRequest(CreateDailyActivityRecordRequest request)
    {
        if (request.ExternalTeacherId <= 0)
            throw new ArgumentException("Field 'externalTeacherId' must be greater than 0.");

        if (string.IsNullOrWhiteSpace(request.DepartmentName))
            throw new ArgumentException("Field 'departmentName' is required.");

        if (string.IsNullOrWhiteSpace(request.FacultyName))
            throw new ArgumentException("Field 'facultyName' is required.");

        if (string.IsNullOrWhiteSpace(request.StudyProgram))
            throw new ArgumentException("Field 'studyProgram' is required.");

        if (string.IsNullOrWhiteSpace(request.CourseType))
            throw new ArgumentException("Field 'courseType' is required.");

        if (request.Year <= 0)
            throw new ArgumentException("Field 'year' must be greater than 0.");

        if (string.IsNullOrWhiteSpace(request.GroupName))
            throw new ArgumentException("Field 'groupName' is required.");

        if (string.IsNullOrWhiteSpace(request.SubjectName))
            throw new ArgumentException("Field 'subjectName' is required.");

        if (string.IsNullOrWhiteSpace(request.RoomName))
            throw new ArgumentException("Field 'roomName' is required.");

        if (request.ConventionalHours < 0)
            throw new ArgumentException("Field 'conventionalHours' must be 0 or greater.");

        if (request.StartDate > request.EndDate)
            throw new ArgumentException("'startDate' cannot be greater than 'endDate'.");
    }

    private static void ValidateUpdateRequest(UpdateDailyActivityRecordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            throw new ArgumentException("Field 'id' is required.");

        if (request.Year.HasValue && request.Year.Value <= 0)
            throw new ArgumentException("Field 'year' must be greater than 0.");

        if (request.ConventionalHours.HasValue && request.ConventionalHours.Value < 0)
            throw new ArgumentException("Field 'conventionalHours' must be 0 or greater.");

        if (
            request.StartDate.HasValue
            && request.EndDate.HasValue
            && request.StartDate > request.EndDate
        )
            throw new ArgumentException("'startDate' cannot be greater than 'endDate'.");
    }
}
