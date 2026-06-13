using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Repositories.Interfaces;

public interface IScheduleCatalogRepository
{
    Task<IReadOnlyList<CatalogItemResponse>> GetFacultiesAsync(
        int externalTeacherId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<CatalogItemResponse>> GetSpecializationsAsync(
        int externalTeacherId,
        int facultyId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<CatalogYearResponse>> GetYearsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<CatalogGroupResponse>> GetGroupsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<CatalogItemResponse>> GetSubjectsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        string groupKey,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<CatalogRoomResponse>> GetRoomsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        string groupKey,
        int subjectId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<string>> SearchSubjectNamesAsync(
        string facultyName,
        string specializationName,
        int year,
        string groupName,
        string? search,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<string>> SearchRoomNamesAsync(
        string facultyName,
        string specializationName,
        int year,
        string groupName,
        string subjectName,
        string? search,
        CancellationToken ct = default
    );
}
