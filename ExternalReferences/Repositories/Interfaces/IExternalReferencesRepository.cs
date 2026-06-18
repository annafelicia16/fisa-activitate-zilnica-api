using FisaActivitateZilnicaApi.ExternalReferences.Models;

namespace FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;

public interface IExternalReferencesRepository
{
    Task<IReadOnlyList<ExternalSubject>> GetSubjectsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ExternalFaculty>> GetFacultiesByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ExternalFaculty>> SearchFacultiesAsync(
        string? search,
        string academicYearLabel,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ExternalSpecialization>> SearchSpecializationsByFacultyAsync(
        string facultyName,
        string academicYearLabel,
        string? search,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ExternalStudyYear>> GetStudyYearsByFacultyAndSpecializationAsync(
        string facultyName,
        string specializationName,
        string academicYearLabel,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<string>> SearchGroupNamesAsync(
        string facultyName,
        string specializationName,
        int year,
        string academicYearLabel,
        string? search,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ExternalSpecialization>> GetSpecializationsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ExternalGroup>> GetGroupsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<SpecializationByFacultyLookup>> GetSpecializationsByShortNameAndFacultiesAsync(
        IReadOnlyCollection<string> shortNames,
        IReadOnlyCollection<int> facultyIds,
        CancellationToken ct = default
    );

    Task<ExternalDepartment?> GetActiveDepartmentForTeacherAsync(
        long externalTeacherId,
        DateTime today,
        CancellationToken ct = default
    );

    // Resolves the study cycle (bachelor/master/doctorate) for each specialization id.
    // Ids that cannot be classified map to StudyCycle.Unknown. See StudyCycleResolver.
    Task<IReadOnlyDictionary<int, StudyCycle>> GetSpecializationCyclesByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default
    );
}
