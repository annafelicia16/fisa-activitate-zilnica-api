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
}
