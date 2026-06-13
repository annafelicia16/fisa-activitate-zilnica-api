using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Services.Interfaces;

public interface IScheduleCatalogQueryService
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
    Task<IReadOnlyList<CatalogSubjectResponse>> SearchSubjectsAsync(
        string? faculty,
        string? specialization,
        int? year,
        string? group,
        string? search,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<CatalogRoomResponse>> SearchRoomsAsync(
        string? faculty,
        string? specialization,
        int? year,
        string? group,
        string? subject,
        string? search,
        CancellationToken ct = default
    );
}
