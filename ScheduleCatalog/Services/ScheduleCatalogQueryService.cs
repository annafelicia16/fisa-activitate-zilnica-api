using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;
using FisaActivitateZilnicaApi.ScheduleCatalog.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ScheduleCatalog.Services.Interfaces;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Services;

public class ScheduleCatalogQueryService(IScheduleCatalogRepository repository)
    : IScheduleCatalogQueryService
{
    public Task<IReadOnlyList<CatalogItemResponse>> GetFacultiesAsync(
        int externalTeacherId,
        CancellationToken ct = default
    )
    {
        RequireTeacher(externalTeacherId);
        return repository.GetFacultiesAsync(externalTeacherId, ct);
    }

    public Task<IReadOnlyList<CatalogItemResponse>> GetSpecializationsAsync(
        int externalTeacherId,
        int facultyId,
        CancellationToken ct = default
    )
    {
        RequireTeacher(externalTeacherId);
        RequirePositive(facultyId, "facultyId");
        return repository.GetSpecializationsAsync(externalTeacherId, facultyId, ct);
    }

    public Task<IReadOnlyList<CatalogYearResponse>> GetYearsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        CancellationToken ct = default
    )
    {
        RequireTeacher(externalTeacherId);
        RequirePositive(facultyId, "facultyId");
        RequirePositive(specializationId, "specializationId");
        return repository.GetYearsAsync(externalTeacherId, facultyId, specializationId, ct);
    }

    public Task<IReadOnlyList<CatalogGroupResponse>> GetGroupsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        CancellationToken ct = default
    )
    {
        RequireTeacher(externalTeacherId);
        RequirePositive(facultyId, "facultyId");
        RequirePositive(specializationId, "specializationId");
        RequirePositive(year, "year");
        return repository.GetGroupsAsync(externalTeacherId, facultyId, specializationId, year, ct);
    }

    public Task<IReadOnlyList<CatalogItemResponse>> GetSubjectsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        string groupKey,
        CancellationToken ct = default
    )
    {
        RequireTeacher(externalTeacherId);
        RequirePositive(facultyId, "facultyId");
        RequirePositive(specializationId, "specializationId");
        RequirePositive(year, "year");
        RequireGroupKey(groupKey);
        return repository.GetSubjectsAsync(
            externalTeacherId,
            facultyId,
            specializationId,
            year,
            groupKey,
            ct
        );
    }

    public Task<IReadOnlyList<CatalogRoomResponse>> GetRoomsAsync(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        string groupKey,
        int subjectId,
        CancellationToken ct = default
    )
    {
        RequireTeacher(externalTeacherId);
        RequirePositive(facultyId, "facultyId");
        RequirePositive(specializationId, "specializationId");
        RequirePositive(year, "year");
        RequireGroupKey(groupKey);
        RequirePositive(subjectId, "subjectId");
        return repository.GetRoomsAsync(
            externalTeacherId,
            facultyId,
            specializationId,
            year,
            groupKey,
            subjectId,
            ct
        );
    }

    private static void RequireTeacher(int externalTeacherId)
    {
        if (externalTeacherId <= 0)
            throw new ArgumentException(
                "Query parameter 'externalTeacherId' must be greater than 0."
            );
    }

    private static void RequirePositive(int value, string name)
    {
        if (value <= 0)
            throw new ArgumentException($"Query parameter '{name}' must be greater than 0.");
    }

    private static void RequireGroupKey(string groupKey)
    {
        if (string.IsNullOrWhiteSpace(groupKey))
            throw new ArgumentException("Query parameter 'groupKey' is required.");
    }
}
