using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.Models;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;
using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;
using FisaActivitateZilnicaApi.Schedules.Models;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

namespace FisaActivitateZilnicaApi.Schedules.Services;

public class SchedulesQueryService(
    ISchedulesRepository schedulesRepository,
    IExternalReferencesRepository externalReferencesRepository,
    IDailyActivityRecordsRepository dailyActivityRecordsRepository
) : ISchedulesQueryService
{
    private static readonly IReadOnlyDictionary<DayOfWeek, string[]> DayNameAliases =
        new Dictionary<DayOfWeek, string[]>
        {
            [DayOfWeek.Monday] = ["Luni", "Monday"],
            [DayOfWeek.Tuesday] = ["Marți", "Marti", "Tuesday"],
            [DayOfWeek.Wednesday] = ["Miercuri", "Wednesday"],
            [DayOfWeek.Thursday] = ["Joi", "Thursday"],
            [DayOfWeek.Friday] = ["Vineri", "Friday"],
            [DayOfWeek.Saturday] = ["Sâmbătă", "Sambata", "Saturday"],
            [DayOfWeek.Sunday] = ["Duminică", "Duminica", "Sunday"],
        };

    public async Task<IReadOnlyList<TeacherScheduleSlotResponse>> GetTeacherDaySlotsAsync(
        GetTeacherScheduleSlotsRequest request,
        CancellationToken ct = default
    )
    {
        ValidateRequest(request);

        var results = await schedulesRepository.GetTeacherDaySlotsByExternalIdsAsync(
            request.ScheduleId,
            request.DayName.Trim(),
            request.ExternalTeacherId,
            request.ExternalSubjectId,
            ct
        );

        return results.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<TeacherScheduleSlotResponse>> GetTeacherDaySlotsByDateAsync(
        GetTeacherDaySlotsByDateRequest request,
        CancellationToken ct = default
    )
    {
        if (request.ExternalTeacherId <= 0)
            throw new ArgumentException(
                "Query parameter 'externalTeacherId' must be greater than 0."
            );

        if (request.Date == default)
            throw new ArgumentException("Query parameter 'date' is required.");

        DateTime date = request.Date.Date;

        IReadOnlyList<Schedule> schedules =
            await schedulesRepository.GetSchedulesByExternalTeacherIdAsync(
                request.ExternalTeacherId,
                ct
            );

        Schedule? matching = ResolveScheduleForDate(schedules, date);
        if (matching == null)
            return [];

        string[] dayNames = DayNameAliases.TryGetValue(date.DayOfWeek, out var aliases)
            ? aliases
            : [];

        if (dayNames.Length == 0)
            return [];

        int? internalTeacherId = await schedulesRepository.FindInternalTeacherIdAsync(
            matching.Id,
            request.ExternalTeacherId,
            ct
        );
        if (internalTeacherId == null)
            return [];

        IReadOnlyList<TeacherScheduleSlotResult> rows =
            await schedulesRepository.GetTeacherDaySlotsByInternalIdAsync(
                matching.Id,
                dayNames,
                internalTeacherId.Value,
                request.ExternalTeacherId,
                ct
            );

        // Resolve university-DB friendly names (subject / faculty / specialization / group)
        // in batched roundtrips, then overlay them onto each slot.
        var subjectIds = rows.Where(r => r.SubjectExternalId is > 0)
            .Select(r => r.SubjectExternalId!.Value)
            .Distinct()
            .ToArray();
        var facultyIds = rows.Where(r => r.FacultyExternalId is > 0)
            .Select(r => r.FacultyExternalId!.Value)
            .Distinct()
            .ToArray();
        var groupIds = rows
            .Select(r => TryParseFirstInt(r.GroupExternalId))
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        // Await sequentially — DbContext is not safe for concurrent use within the same
        // scope, so Task.WhenAll on multiple queries against UniversityDbContext throws
        // InvalidOperationException ("A second operation was started on this context...").
        Dictionary<int, ExternalSubject> subjects = (
            await externalReferencesRepository.GetSubjectsByIdsAsync(subjectIds, ct)
        ).ToDictionary(s => (int)s.IdMaterie);
        Dictionary<int, ExternalFaculty> faculties = (
            await externalReferencesRepository.GetFacultiesByIdsAsync(facultyIds, ct)
        ).ToDictionary(f => (int)f.IdFacultate);
        Dictionary<int, ExternalGroup> groups = (
            await externalReferencesRepository.GetGroupsByIdsAsync(groupIds, ct)
        ).ToDictionary(g => (int)g.IdGrupe);

        // Third fallback: COURSES often have no id_grupe and a friendly grupa name like
        // "IE3" meaning "all year-3 IE groups". The Specializare can be resolved by
        // matching its DenumireScurtaSpecializare ("IE") and scoping to the slot's
        // faculty via Grupe.
        var shortNameFacultyPairs = rows
            .Where(r =>
                !(r.SpecializationExternalId > 0)
                && TryParseFirstInt(r.GroupExternalId) <= 0
                && r.FacultyExternalId is > 0
            )
            .Select(r => (Prefix: ExtractAlphaPrefix(r.GroupNames), FacultyId: r.FacultyExternalId!.Value))
            .Where(p => !string.IsNullOrEmpty(p.Prefix))
            .Distinct()
            .ToArray();
        var shortNames = shortNameFacultyPairs
            .Select(p => p.Prefix!)
            .Distinct()
            .ToArray();
        var fallbackFacultyIds = shortNameFacultyPairs
            .Select(p => p.FacultyId)
            .Distinct()
            .ToArray();

        var shortNameLookups =
            await externalReferencesRepository.GetSpecializationsByShortNameAndFacultiesAsync(
                shortNames,
                fallbackFacultyIds,
                ct
            );
        Dictionary<(string ShortName, int FacultyId), int> shortNameToSpecId =
            shortNameLookups
                .GroupBy(l => (
                    ShortName: l.DenumireScurta!.ToUpperInvariant(),
                    FacultyId: (int)l.FacultyId
                ))
                .ToDictionary(g => g.Key, g => (int)g.First().IdSpecializare);

        // Effective specialization id per row: own id_specializare > group's id_specializare
        // > short-name+faculty hint.
        var specializationIds = new HashSet<int>();
        foreach (var row in rows)
        {
            int effectiveSpec = ResolveEffectiveSpecializationId(
                row,
                groups,
                shortNameToSpecId
            );
            if (effectiveSpec > 0)
                specializationIds.Add(effectiveSpec);
        }

        Dictionary<int, ExternalSpecialization> specializations = (
            await externalReferencesRepository.GetSpecializationsByIdsAsync(
                specializationIds.ToArray(),
                ct
            )
        ).ToDictionary(s => (int)s.IdSpecializare);

        var responses = rows
            .Select(r =>
                MapToResponse(r, subjects, faculties, specializations, groups, shortNameToSpecId)
            )
            .ToList();

        // Drop slots that already have a daily-activity-record pointing at them.
        // Match is by slot identity (ActivitySlotId FK) — set by the client when
        // the user fills the form from a scheduled slot. Records without a slot
        // FK (legacy / ad-hoc entries) are ignored here and the slot stays.
        IEnumerable<GetDailyActivityRecordResponse> dayRecords =
            await dailyActivityRecordsRepository.QueryDailyActivityRecords(
                request.ExternalTeacherId,
                departmentName: null,
                year: null,
                groupName: null,
                subjectName: null,
                roomName: null,
                revenueType: null,
                activityType: null,
                startDate: date,
                endDate: date.AddDays(1)
            );

        HashSet<int> recordedSlotIds = dayRecords
            .Where(r => r.ActivitySlotId.HasValue)
            .Select(r => r.ActivitySlotId!.Value)
            .ToHashSet();

        return responses.Where(slot => !recordedSlotIds.Contains(slot.SlotId)).ToList();
    }

    // Resolution lives in the shared SpecializationResolver so the read path and the
    // import-time caching agree. These remain thin wrappers to keep the existing call
    // sites (which pass a TeacherScheduleSlotResult) unchanged.
    private static int ResolveEffectiveSpecializationId(
        TeacherScheduleSlotResult row,
        IReadOnlyDictionary<int, ExternalGroup> groups,
        IReadOnlyDictionary<(string ShortName, int FacultyId), int>? shortNameToSpecId = null
    ) =>
        SpecializationResolver.ResolveEffectiveSpecializationId(
            row.SpecializationExternalId,
            row.GroupExternalId,
            row.FacultyExternalId,
            row.GroupNames,
            groups,
            shortNameToSpecId
        );

    private static int TryParseFirstInt(string? value) =>
        SpecializationResolver.TryParseFirstInt(value);

    private static string? ExtractAlphaPrefix(string? grupa) =>
        SpecializationResolver.ExtractAlphaPrefix(grupa);

    // Picks the schedule whose semester contains the given date and whose OddWeek
    // flag matches the parity of that date's week relative to the semester start.
    // Convention: week 1 of the semester (weeks-from-start == 0) is the odd week.
    private static Schedule? ResolveScheduleForDate(
        IReadOnlyList<Schedule> schedules,
        DateTime date
    )
    {
        Schedule? candidate = schedules.FirstOrDefault(s =>
            s.ScheduleSemester != null
            && s.ScheduleSemester.StartDate.Date <= date
            && date <= s.ScheduleSemester.EndDate.Date
        );

        if (candidate == null)
            return null;

        ScheduleSemester semester = candidate.ScheduleSemester;
        int daysFromStart = (date - semester.StartDate.Date).Days;
        int weeksFromStart = daysFromStart / 7;
        bool isOddWeek = weeksFromStart % 2 == 0;

        return schedules.FirstOrDefault(s =>
            s.ScheduleSemesterId == semester.Id && s.OddWeek == isOddWeek
        ) ?? candidate;
    }

    public Task<int> BackfillActivityStudentsCommentRefsAsync(CancellationToken ct = default) =>
        schedulesRepository.BackfillActivityStudentsCommentRefsAsync(ct);

    // Re-resolves and caches the AGSIS friendly names (faculty/specialization/subject/
    // group) + the effective specialization id onto existing ActivityStudents rows,
    // for schedules imported before name-caching existed. Same batched resolution as
    // FetImportService; idempotent (re-running recomputes the same values).
    public async Task<int> BackfillActivityStudentsAgsisNamesAsync(CancellationToken ct = default)
    {
        List<ActivityStudents> rows =
            await schedulesRepository.GetActivityStudentsForBackfillAsync(ct);
        if (rows.Count == 0)
            return 0;

        int[] subjectIds = rows.Where(r => r.SubjectExternalId is > 0)
            .Select(r => r.SubjectExternalId!.Value)
            .Distinct()
            .ToArray();
        int[] facultyIds = rows.Where(r => r.FacultyExternalId is > 0)
            .Select(r => r.FacultyExternalId!.Value)
            .Distinct()
            .ToArray();
        int[] groupIds = rows.Select(r => SpecializationResolver.TryParseFirstInt(r.GroupExternalId))
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        Dictionary<int, ExternalSubject> subjectsById = (
            await externalReferencesRepository.GetSubjectsByIdsAsync(subjectIds, ct)
        ).ToDictionary(s => (int)s.IdMaterie);
        Dictionary<int, ExternalFaculty> facultiesById = (
            await externalReferencesRepository.GetFacultiesByIdsAsync(facultyIds, ct)
        ).ToDictionary(f => (int)f.IdFacultate);
        Dictionary<int, ExternalGroup> groupsById = (
            await externalReferencesRepository.GetGroupsByIdsAsync(groupIds, ct)
        ).ToDictionary(g => (int)g.IdGrupe);

        var shortNameFacultyPairs = rows
            .Where(r =>
                !(r.SpecializationExternalId is > 0)
                && SpecializationResolver.TryParseFirstInt(r.GroupExternalId) <= 0
                && r.FacultyExternalId is > 0
            )
            .Select(r =>
                (
                    Prefix: SpecializationResolver.ExtractAlphaPrefix(r.GroupNames),
                    FacultyId: r.FacultyExternalId!.Value
                )
            )
            .Where(p => !string.IsNullOrEmpty(p.Prefix))
            .Distinct()
            .ToArray();
        string[] shortNames = shortNameFacultyPairs.Select(p => p.Prefix!).Distinct().ToArray();
        int[] shortNameFacultyIds = shortNameFacultyPairs
            .Select(p => p.FacultyId)
            .Distinct()
            .ToArray();

        Dictionary<(string ShortName, int FacultyId), int> shortNameToSpecId = (
            await externalReferencesRepository.GetSpecializationsByShortNameAndFacultiesAsync(
                shortNames,
                shortNameFacultyIds,
                ct
            )
        )
            .GroupBy(l => (
                ShortName: l.DenumireScurta!.ToUpperInvariant(),
                FacultyId: (int)l.FacultyId
            ))
            .ToDictionary(g => g.Key, g => (int)g.First().IdSpecializare);

        int[] effectiveSpecIds = rows
            .Select(r =>
                SpecializationResolver.ResolveEffectiveSpecializationId(
                    r.SpecializationExternalId,
                    r.GroupExternalId,
                    r.FacultyExternalId,
                    r.GroupNames,
                    groupsById,
                    shortNameToSpecId
                )
            )
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
        Dictionary<int, ExternalSpecialization> specializationsById = (
            await externalReferencesRepository.GetSpecializationsByIdsAsync(effectiveSpecIds, ct)
        ).ToDictionary(s => (int)s.IdSpecializare);
        IReadOnlyDictionary<int, StudyCycle> cyclesBySpecId =
            await externalReferencesRepository.GetSpecializationCyclesByIdsAsync(effectiveSpecIds, ct);

        foreach (ActivityStudents row in rows)
        {
            int effectiveSpecId = SpecializationResolver.ResolveEffectiveSpecializationId(
                row.SpecializationExternalId,
                row.GroupExternalId,
                row.FacultyExternalId,
                row.GroupNames,
                groupsById,
                shortNameToSpecId
            );
            int firstGroupId = SpecializationResolver.TryParseFirstInt(row.GroupExternalId);

            row.FacultyName =
                row.FacultyExternalId is int facId
                && facultiesById.TryGetValue(facId, out ExternalFaculty? faculty)
                    ? faculty.Denumire
                    : null;
            row.SpecializationName =
                effectiveSpecId > 0
                && specializationsById.TryGetValue(
                    effectiveSpecId,
                    out ExternalSpecialization? specialization
                )
                    ? specialization.Denumire
                    : null;
            row.SubjectName =
                row.SubjectExternalId is int subjId
                && subjectsById.TryGetValue(subjId, out ExternalSubject? subject)
                    ? subject.Denumire
                    : null;
            row.ResolvedGroupName =
                firstGroupId > 0 && groupsById.TryGetValue(firstGroupId, out ExternalGroup? group)
                    ? group.Nume
                    : row.GroupNames;
            row.EffectiveSpecializationExternalId = effectiveSpecId > 0 ? effectiveSpecId : null;
            row.SpecializationCycle =
                effectiveSpecId > 0
                && cyclesBySpecId.TryGetValue(effectiveSpecId, out StudyCycle cycle)
                && cycle != StudyCycle.Unknown
                    ? cycle
                    : null;
        }

        await schedulesRepository.SaveChangesAsync(ct);
        return rows.Count;
    }

    private static TeacherScheduleSlotResponse MapToResponse(TeacherScheduleSlotResult x) =>
        MapToResponse(x, null, null, null, null, null);

    private static TeacherScheduleSlotResponse MapToResponse(
        TeacherScheduleSlotResult x,
        IReadOnlyDictionary<int, ExternalSubject>? subjects,
        IReadOnlyDictionary<int, ExternalFaculty>? faculties,
        IReadOnlyDictionary<int, ExternalSpecialization>? specializations,
        IReadOnlyDictionary<int, ExternalGroup>? groups,
        IReadOnlyDictionary<(string ShortName, int FacultyId), int>? shortNameToSpecId
    )
    {
        string? materieName = null;
        string? materieShortName = null;
        if (
            subjects is not null
            && x.SubjectExternalId is int subjectId
            && subjects.TryGetValue(subjectId, out var subject)
        )
        {
            materieName = subject.Denumire;
            materieShortName = subject.DenumireScurta;
        }

        string? facultateName = null;
        if (
            faculties is not null
            && x.FacultyExternalId is int facultyId
            && faculties.TryGetValue(facultyId, out var faculty)
        )
        {
            facultateName = faculty.Denumire;
        }

        string? specializareName = null;
        int effectiveSpecId = groups is null
            ? (x.SpecializationExternalId > 0 ? x.SpecializationExternalId : 0)
            : ResolveEffectiveSpecializationId(x, groups, shortNameToSpecId);
        if (
            specializations is not null
            && effectiveSpecId > 0
            && specializations.TryGetValue(effectiveSpecId, out var specialization)
        )
        {
            specializareName = specialization.Denumire;
        }

        // Friendly AGSIS group name (dbo.Grupe.Nume, e.g. "8MF141") resolved
        // from the first parseable id in GroupExternalId. Skips whole-class
        // sentinels and unparseable values.
        string? grupaName = null;
        if (groups is not null)
        {
            int groupId = TryParseFirstInt(x.GroupExternalId);
            if (groupId > 0 && groups.TryGetValue(groupId, out var group))
            {
                grupaName = group.Nume;
            }
        }

        return new TeacherScheduleSlotResponse(
            x.SlotId,
            x.ScheduleId,
            x.ActivityId,
            x.HourName,
            x.DayName,
            x.SubjectName,
            x.CourseTypeTag,
            x.RoomName,
            x.Duration,
            x.ExternalTeacherId,
            x.PlanMatterProviderExternalId,
            x.FacultyExternalId,
            x.MetaSpecializationExternalId,
            x.StudyYearNumber,
            x.Semester,
            x.PlataNB,
            x.OldActivityTag,
            x.GroupExternalId,
            x.GroupNames,
            x.SpecializationExternalId,
            x.SubjectExternalId,
            materieName,
            materieShortName,
            facultateName,
            specializareName,
            grupaName
        );
    }

    public async Task<IReadOnlyList<ScheduleResponse>> GetSchedulesByExternalTeacherIdAsync(
        int externalTeacherId,
        CancellationToken ct = default
    )
    {
        if (externalTeacherId <= 0)
            throw new ArgumentException(
                "Query parameter 'externalTeacherId' must be greater than 0."
            );

        var schedules = await schedulesRepository.GetSchedulesByExternalTeacherIdAsync(
            externalTeacherId,
            ct
        );

        return schedules
            .Select(schedule => new ScheduleResponse(
                schedule.Id,
                schedule.Name,
                schedule.ScheduleSemester.ScheduleYear.Value,
                schedule.ScheduleSemester.Number,
                schedule.ScheduleSemester.StartDate,
                schedule.ScheduleSemester.EndDate,
                schedule.OddWeek
            ))
            .ToList();
    }

    public async Task<IReadOnlyList<ScheduleResponse>> GetAllSchedulesAsync(
        CancellationToken ct = default
    )
    {
        var schedules = await schedulesRepository.GetAllSchedulesAsync(ct);
        return schedules
            .Select(schedule => new ScheduleResponse(
                schedule.Id,
                schedule.Name,
                schedule.ScheduleSemester.ScheduleYear.Value,
                schedule.ScheduleSemester.Number,
                schedule.ScheduleSemester.StartDate,
                schedule.ScheduleSemester.EndDate,
                schedule.OddWeek
            ))
            .ToList();
    }

    private static void ValidateRequest(GetTeacherScheduleSlotsRequest request)
    {
        if (request.ScheduleId <= 0)
            throw new ArgumentException("Query parameter 'scheduleId' must be greater than 0.");

        if (string.IsNullOrWhiteSpace(request.DayName))
            throw new ArgumentException("Query parameter 'dayName' is required.");

        if (request.ExternalTeacherId <= 0)
            throw new ArgumentException("Query parameter 'externalTeacherId' must be greater than 0.");

        if (request.ExternalSubjectId <= 0)
            throw new ArgumentException("Query parameter 'externalSubjectId' must be greater than 0.");
    }
}
