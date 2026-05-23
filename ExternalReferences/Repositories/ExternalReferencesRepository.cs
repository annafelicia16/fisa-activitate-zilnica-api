using FisaActivitateZilnicaApi.Data.University;
using FisaActivitateZilnicaApi.ExternalReferences.Models;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.ExternalReferences.Repositories;

public class ExternalReferencesRepository(UniversityDbContext db) : IExternalReferencesRepository
{
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
