using FisaActivitateZilnicaApi.Data.University;
using FisaActivitateZilnicaApi.ExternalReferences.Models;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.ExternalReferences.Repositories;

public class ExternalReferencesRepository(UniversityDbContext db) : IExternalReferencesRepository
{
    // AGSIS names are referenced by value from several sources with mixed
    // casing and cedilla/comma-below diacritics — compare them
    // case/accent-insensitively.
    private const string NameCollation = "Latin1_General_100_CI_AI";

    public async Task<IReadOnlyList<ExternalSubject>> GetSubjectsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    )
    {
        if (ids.Count == 0)
            return [];

        var longIds = ids.Select(i => (long)i).ToArray();
        return await db.ExternalSubjects.AsNoTracking()
            .Where(s => longIds.Contains(s.IdMaterie))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ExternalFaculty>> GetFacultiesByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    )
    {
        if (ids.Count == 0)
            return [];

        var longIds = ids.Select(i => (long)i).ToArray();
        return await db.ExternalFaculties.AsNoTracking()
            .Where(f => longIds.Contains(f.IdFacultate))
            .ToListAsync(ct);
    }

    // Only faculties that run study programs in the given academic year — a
    // faculty qualifies when at least one dbo.Grupe row ties it to a
    // specialization in that year. This drops AGSIS organizational units
    // (library, dorms, defunct colleges) that live in dbo.Facultate.
    public async Task<IReadOnlyList<ExternalFaculty>> SearchFacultiesAsync(
        string? search,
        string academicYearLabel,
        CancellationToken ct = default
    )
    {
        IQueryable<ExternalFaculty> query = db.ExternalFaculties.AsNoTracking()
            .Where(f =>
                db.ExternalGroups.Any(g =>
                    g.IdFacultate == f.IdFacultate
                    && g.IdSpecializare != null
                    && db.ExternalAcademicYears.Any(a =>
                        a.IdAnUniv == g.IdAnUniv && a.Denumire.Contains(academicYearLabel)
                    )
                )
            );

        string term = (search ?? string.Empty).Trim();
        if (term.Length > 0)
        {
            query = query.Where(f =>
                f.Denumire.Contains(term)
                || (f.DenumireScurta != null && f.DenumireScurta.Contains(term))
            );
        }

        return await query.OrderBy(f => f.Denumire).Take(50).ToListAsync(ct);
    }

    // Specializations have no direct faculty column in AGSIS; the link goes
    // through dbo.Grupe. Restricting groups to the current academic year keeps
    // the list to programs actually running now (FIESC: 17 vs 319 all-time).
    public async Task<IReadOnlyList<ExternalSpecialization>> SearchSpecializationsByFacultyAsync(
        string facultyName,
        string academicYearLabel,
        string? search,
        CancellationToken ct = default
    )
    {
        string faculty = facultyName.Trim();
        if (faculty.Length == 0)
            return [];

        IQueryable<ExternalSpecialization> query =
            from s in db.ExternalSpecializations.AsNoTracking()
            where
                db.ExternalGroups.Any(g =>
                    g.IdSpecializare == s.IdSpecializare
                    && db.ExternalFaculties.Any(f =>
                        f.IdFacultate == g.IdFacultate
                        && EF.Functions.Collate(f.Denumire, NameCollation) == faculty
                    )
                    && db.ExternalAcademicYears.Any(a =>
                        a.IdAnUniv == g.IdAnUniv && a.Denumire.Contains(academicYearLabel)
                    )
                )
            select s;

        string term = (search ?? string.Empty).Trim();
        if (term.Length > 0)
        {
            query = query.Where(s =>
                s.Denumire.Contains(term)
                || (s.DenumireScurta != null && s.DenumireScurta.Contains(term))
            );
        }

        return await query.OrderBy(s => s.Denumire).Take(50).ToListAsync(ct);
    }

    // Study years offered for a faculty + specialization (matched by name, as
    // the client form stores names) in the given academic year, again resolved
    // through dbo.Grupe.
    public async Task<IReadOnlyList<ExternalStudyYear>> GetStudyYearsByFacultyAndSpecializationAsync(
        string facultyName,
        string specializationName,
        string academicYearLabel,
        CancellationToken ct = default
    )
    {
        string faculty = facultyName.Trim();
        string specialization = specializationName.Trim();
        if (faculty.Length == 0 || specialization.Length == 0)
            return [];

        return await (
            from y in db.ExternalStudyYears.AsNoTracking()
            where
                db.ExternalGroups.Any(g =>
                    g.IdAnStudiu == y.IdAnStudiu
                    && db.ExternalFaculties.Any(f =>
                        f.IdFacultate == g.IdFacultate
                        && EF.Functions.Collate(f.Denumire, NameCollation) == faculty
                    )
                    && db.ExternalSpecializations.Any(s =>
                        s.IdSpecializare == g.IdSpecializare
                        && EF.Functions.Collate(s.Denumire, NameCollation) == specialization
                    )
                    && db.ExternalAcademicYears.Any(a =>
                        a.IdAnUniv == g.IdAnUniv && a.Denumire.Contains(academicYearLabel)
                    )
                )
            select y
        )
            .OrderBy(y => y.NrAnStudiu)
            .ToListAsync(ct);
    }

    // Group names for a faculty + specialization + study year in the given
    // academic year. Values are dbo.Grupe.Nume — the same strings the imported
    // schedule caches as ResolvedGroupName, so form values line up everywhere.
    public async Task<IReadOnlyList<string>> SearchGroupNamesAsync(
        string facultyName,
        string specializationName,
        int year,
        string academicYearLabel,
        string? search,
        CancellationToken ct = default
    )
    {
        string faculty = facultyName.Trim();
        string specialization = specializationName.Trim();
        if (faculty.Length == 0 || specialization.Length == 0 || year <= 0)
            return [];

        IQueryable<string> query =
            from g in db.ExternalGroups.AsNoTracking()
            where
                db.ExternalFaculties.Any(f =>
                    f.IdFacultate == g.IdFacultate
                    && EF.Functions.Collate(f.Denumire, NameCollation) == faculty
                )
                && db.ExternalSpecializations.Any(s =>
                    s.IdSpecializare == g.IdSpecializare
                    && EF.Functions.Collate(s.Denumire, NameCollation) == specialization
                )
                && db.ExternalStudyYears.Any(y =>
                    y.IdAnStudiu == g.IdAnStudiu && y.NrAnStudiu == year
                )
                && db.ExternalAcademicYears.Any(a =>
                    a.IdAnUniv == g.IdAnUniv && a.Denumire.Contains(academicYearLabel)
                )
            select g.Nume;

        string term = (search ?? string.Empty).Trim();
        if (term.Length > 0)
            query = query.Where(name => name.Contains(term));

        return await query.Distinct().OrderBy(name => name).Take(100).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ExternalSpecialization>> GetSpecializationsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    )
    {
        if (ids.Count == 0)
            return [];

        var longIds = ids.Select(i => (long)i).ToArray();
        return await db.ExternalSpecializations.AsNoTracking()
            .Where(s => longIds.Contains(s.IdSpecializare))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ExternalGroup>> GetGroupsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    )
    {
        if (ids.Count == 0)
            return [];

        var longIds = ids.Select(i => (long)i).ToArray();
        return await db.ExternalGroups.AsNoTracking()
            .Where(g => longIds.Contains(g.IdGrupe))
            .ToListAsync(ct);
    }

    public async Task<ExternalDepartment?> GetActiveDepartmentForTeacherAsync(
        long externalTeacherId,
        DateTime today,
        CancellationToken ct = default
    )
    {
        DateTime day = today.Date;
        return await (
            from dp in db.ExternalDepartmentTeachers.AsNoTracking()
            join d in db.ExternalDepartments.AsNoTracking()
                on dp.IdDepartament equals d.IdDepartament
            where
                dp.IdProfesor == externalTeacherId
                && dp.Activ == true
                && (dp.DataPanaCand == null || dp.DataPanaCand >= day)
            orderby dp.DataDeCand descending
            select d
        ).FirstOrDefaultAsync(ct);
    }

    public async Task<
        IReadOnlyList<SpecializationByFacultyLookup>
    > GetSpecializationsByShortNameAndFacultiesAsync(
        IReadOnlyCollection<string> shortNames,
        IReadOnlyCollection<int> facultyIds,
        CancellationToken ct = default
    )
    {
        if (shortNames.Count == 0 || facultyIds.Count == 0)
            return [];

        var longFacultyIds = facultyIds.Select(i => (long)i).ToArray();
        var names = shortNames.ToArray();

        return await (
            from spec in db.ExternalSpecializations.AsNoTracking()
            join grp in db.ExternalGroups.AsNoTracking()
                on spec.IdSpecializare equals grp.IdSpecializare
            where
                spec.DenumireScurta != null
                && names.Contains(spec.DenumireScurta)
                && grp.IdFacultate != null
                && longFacultyIds.Contains(grp.IdFacultate.Value)
            select new SpecializationByFacultyLookup(
                spec.IdSpecializare,
                spec.Denumire,
                spec.DenumireScurta,
                grp.IdFacultate!.Value
            )
        )
            .Distinct()
            .ToListAsync(ct);
    }
}
