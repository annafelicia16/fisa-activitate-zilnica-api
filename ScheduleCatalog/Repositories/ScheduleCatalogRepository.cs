using FisaActivitateZilnicaApi.Data.Master;
using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;
using FisaActivitateZilnicaApi.ScheduleCatalog.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Models;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Repositories;

// All queries are teacher-scoped over the local MasterDbContext (names cached at
// import). The base join — Teachers(ExternalTeacherId) → ActivityTeachers →
// ActivityStudents — mirrors SchedulesRepository.GetTeacherFacultyExternalIdsAsync.
//
// Note: each query does its Distinct()/OrderBy() over an anonymous (or scalar)
// projection — which SQL Server can translate — and only maps to the response
// record AFTER materialization. EF Core cannot translate Distinct() applied to a
// projection built with a record constructor.
public class ScheduleCatalogRepository(MasterDbContext db) : IScheduleCatalogRepository
{
    private const string WholeClass = "-";

    // Cached AGSIS names arrive in mixed casing and cedilla/comma-below
    // diacritics depending on the import source — match them
    // case/accent-insensitively.
    private const string NameCollation = "Latin1_General_100_CI_AI";

    public async Task<IReadOnlyList<CatalogItemResponse>> GetFacultiesAsync(
        int externalTeacherId,
        CancellationToken ct = default
    )
    {
        var rows = await (
            from teacher in db.Teachers.AsNoTracking()
            join at in db.ActivityTeachers.AsNoTracking() on teacher.Id equals at.TeacherId
            join s in db.ActivityStudents.AsNoTracking() on at.ActivityId equals s.ActivityId
            where
                teacher.ExternalTeacherId == externalTeacherId
                && s.FacultyExternalId != null
                && s.FacultyExternalId > 0
                && s.FacultyName != null
            select new { Id = s.FacultyExternalId!.Value, Name = s.FacultyName! }
        )
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return rows.Select(x => new CatalogItemResponse(x.Id, x.Name)).ToList();
    }

    public async Task<IReadOnlyList<CatalogItemResponse>> GetSpecializationsAsync(
        int externalTeacherId,
        int facultyId,
        CancellationToken ct = default
    )
    {
        var rows = await (
            from teacher in db.Teachers.AsNoTracking()
            join at in db.ActivityTeachers.AsNoTracking() on teacher.Id equals at.TeacherId
            join s in db.ActivityStudents.AsNoTracking() on at.ActivityId equals s.ActivityId
            where
                teacher.ExternalTeacherId == externalTeacherId
                && s.FacultyExternalId == facultyId
                && s.EffectiveSpecializationExternalId != null
                && s.EffectiveSpecializationExternalId > 0
                && s.SpecializationName != null
            select new { Id = s.EffectiveSpecializationExternalId!.Value, Name = s.SpecializationName! }
        )
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return rows.Select(x => new CatalogItemResponse(x.Id, x.Name)).ToList();
    }

    public async Task<IReadOnlyList<CatalogYearResponse>> GetYearsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        CancellationToken ct = default
    )
    {
        List<int> years = await (
            from teacher in db.Teachers.AsNoTracking()
            join at in db.ActivityTeachers.AsNoTracking() on teacher.Id equals at.TeacherId
            join s in db.ActivityStudents.AsNoTracking() on at.ActivityId equals s.ActivityId
            where
                teacher.ExternalTeacherId == externalTeacherId
                && s.FacultyExternalId == facultyId
                && s.EffectiveSpecializationExternalId == specializationId
                && s.StudyYearNumber != null
            select s.StudyYearNumber!.Value
        )
            .Distinct()
            .OrderBy(year => year)
            .ToListAsync(ct);

        return years.Select(year => new CatalogYearResponse(year)).ToList();
    }

    public async Task<IReadOnlyList<CatalogGroupResponse>> GetGroupsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        CancellationToken ct = default
    )
    {
        List<string?> raw = await (
            from teacher in db.Teachers.AsNoTracking()
            join at in db.ActivityTeachers.AsNoTracking() on teacher.Id equals at.TeacherId
            join s in db.ActivityStudents.AsNoTracking() on at.ActivityId equals s.ActivityId
            where
                teacher.ExternalTeacherId == externalTeacherId
                && s.FacultyExternalId == facultyId
                && s.EffectiveSpecializationExternalId == specializationId
                && s.StudyYearNumber == year
            select s.ResolvedGroupName
        )
            .Distinct()
            .ToListAsync(ct);

        // Whole-class slots (null / "-1" / empty) collapse to a single "-" option.
        return raw.Select(NormalizeGroup)
            .Distinct()
            .OrderBy(name => name == WholeClass ? 1 : 0)
            .ThenBy(name => name)
            .Select(name => new CatalogGroupResponse(name, name))
            .ToList();
    }

    public async Task<IReadOnlyList<CatalogItemResponse>> GetSubjectsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        string groupKey,
        CancellationToken ct = default
    )
    {
        IQueryable<ActivityStudents> query =
            from teacher in db.Teachers.AsNoTracking()
            join at in db.ActivityTeachers.AsNoTracking() on teacher.Id equals at.TeacherId
            join s in db.ActivityStudents.AsNoTracking() on at.ActivityId equals s.ActivityId
            where
                teacher.ExternalTeacherId == externalTeacherId
                && s.FacultyExternalId == facultyId
                && s.EffectiveSpecializationExternalId == specializationId
                && s.StudyYearNumber == year
                && s.SubjectExternalId != null
                && s.SubjectExternalId > 0
                && s.SubjectName != null
            select s;

        query = ApplyGroupFilter(query, groupKey);

        var rows = await query
            .Select(s => new { Id = s.SubjectExternalId!.Value, Name = s.SubjectName! })
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return rows.Select(x => new CatalogItemResponse(x.Id, x.Name)).ToList();
    }

    public async Task<IReadOnlyList<CatalogRoomResponse>> GetRoomsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        string groupKey,
        int subjectId,
        CancellationToken ct = default
    )
    {
        // Rooms live on ActivitySlot (local, per-schedule), not ActivityStudents —
        // inner-join Rooms so only slots with a bound room surface.
        var baseQuery =
            from slot in db.ActivitySlots.AsNoTracking()
            join activity in db.Activities.AsNoTracking() on slot.ActivityId equals activity.Id
            join at in db.ActivityTeachers.AsNoTracking() on activity.Id equals at.ActivityId
            join teacher in db.Teachers.AsNoTracking() on at.TeacherId equals teacher.Id
            join s in db.ActivityStudents.AsNoTracking() on activity.Id equals s.ActivityId
            join room in db.Rooms.AsNoTracking() on slot.RoomId equals room.Id
            where
                teacher.ExternalTeacherId == externalTeacherId
                && s.FacultyExternalId == facultyId
                && s.EffectiveSpecializationExternalId == specializationId
                && s.StudyYearNumber == year
                && s.SubjectExternalId == subjectId
            select new { RoomName = room.Name, GroupName = s.ResolvedGroupName };

        baseQuery =
            groupKey == WholeClass
                ? baseQuery.Where(x =>
                    x.GroupName == null
                    || x.GroupName == ""
                    || x.GroupName == "-1"
                    || x.GroupName == "-"
                )
                : baseQuery.Where(x => x.GroupName == groupKey);

        List<string> rooms = await baseQuery
            .Select(x => x.RoomName)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(ct);

        return rooms.Select(name => new CatalogRoomResponse(name)).ToList();
    }

    // Name-keyed variants for the activity-record form cascade: NOT teacher
    // scoped (the disciplines a group has are schedule-wide facts), matched on
    // the AGSIS names cached at import. A concrete group also includes
    // whole-class rows — lectures belong to every group of that year — while
    // "-" means "all groups" and applies no group constraint at all.
    public async Task<IReadOnlyList<string>> SearchSubjectNamesAsync(
        string facultyName,
        string specializationName,
        int year,
        string groupName,
        string? search,
        CancellationToken ct = default
    )
    {
        IQueryable<ActivityStudents> query = db.ActivityStudents.AsNoTracking()
            .Where(s =>
                EF.Functions.Collate(s.FacultyName, NameCollation) == facultyName
                && EF.Functions.Collate(s.SpecializationName, NameCollation)
                    == specializationName
                && s.StudyYearNumber == year
                && s.SubjectName != null
            );

        query = ApplyGroupFilterIncludingWholeClass(query, groupName);

        string term = (search ?? string.Empty).Trim();
        if (term.Length > 0)
            query = query.Where(s => s.SubjectName!.Contains(term));

        return await query
            .Select(s => s.SubjectName!)
            .Distinct()
            .OrderBy(name => name)
            .Take(100)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> SearchRoomNamesAsync(
        string facultyName,
        string specializationName,
        int year,
        string groupName,
        string subjectName,
        string? search,
        CancellationToken ct = default
    )
    {
        var baseQuery =
            from slot in db.ActivitySlots.AsNoTracking()
            join activity in db.Activities.AsNoTracking() on slot.ActivityId equals activity.Id
            join s in db.ActivityStudents.AsNoTracking() on activity.Id equals s.ActivityId
            join room in db.Rooms.AsNoTracking() on slot.RoomId equals room.Id
            where
                EF.Functions.Collate(s.FacultyName, NameCollation) == facultyName
                && EF.Functions.Collate(s.SpecializationName, NameCollation)
                    == specializationName
                && s.StudyYearNumber == year
                && EF.Functions.Collate(s.SubjectName, NameCollation) == subjectName
            select new { RoomName = room.Name, GroupName = s.ResolvedGroupName };

        baseQuery =
            groupName == WholeClass
                ? baseQuery
                : baseQuery.Where(x =>
                    EF.Functions.Collate(x.GroupName, NameCollation) == groupName
                    || x.GroupName == null
                    || x.GroupName == ""
                    || x.GroupName == "-1"
                    || x.GroupName == "-"
                );

        IQueryable<string> names = baseQuery.Select(x => x.RoomName);

        string term = (search ?? string.Empty).Trim();
        if (term.Length > 0)
            names = names.Where(name => name.Contains(term));

        return await names.Distinct().OrderBy(name => name).Take(100).ToListAsync(ct);
    }

    private static IQueryable<ActivityStudents> ApplyGroupFilterIncludingWholeClass(
        IQueryable<ActivityStudents> query,
        string groupName
    ) =>
        groupName == WholeClass
            ? query
            : query.Where(s =>
                EF.Functions.Collate(s.ResolvedGroupName, NameCollation) == groupName
                || s.ResolvedGroupName == null
                || s.ResolvedGroupName == ""
                || s.ResolvedGroupName == "-1"
                || s.ResolvedGroupName == "-"
            );

    private static IQueryable<ActivityStudents> ApplyGroupFilter(
        IQueryable<ActivityStudents> query,
        string groupKey
    ) =>
        groupKey == WholeClass
            ? query.Where(s =>
                s.ResolvedGroupName == null
                || s.ResolvedGroupName == ""
                || s.ResolvedGroupName == "-1"
                || s.ResolvedGroupName == "-"
            )
            : query.Where(s => s.ResolvedGroupName == groupKey);

    private static string NormalizeGroup(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return WholeClass;
        string trimmed = value.Trim();
        return trimmed is "-1" or "-" ? WholeClass : trimmed;
    }
}
