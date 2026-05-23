using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.SupplementaryActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Services;

public class SupplementaryActivitiesQueryService(
    ISupplementaryActivitiesRepository supplementaryActivitiesRepository
) : ISupplementaryActivitiesQueryService
{
    private readonly ISupplementaryActivitiesRepository _supplementaryActivitiesRepository =
        supplementaryActivitiesRepository;

    public async Task<GetSupplementaryActivityResponse> GetSupplementaryActivityByIdAsync(
        string id
    )
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Query parameter 'id' is required.");

        GetSupplementaryActivityResponse? activity =
            await _supplementaryActivitiesRepository.GetSupplementaryActivityById(id);

        if (activity == null)
            throw new KeyNotFoundException("Supplementary activity does not exist.");

        return activity;
    }

    public async Task<
        IEnumerable<GetSupplementaryActivityResponse>
    > QuerySupplementaryActivitiesAsync(int teacherId, DateTime? startDate, DateTime? endDate)
    {
        if (teacherId <= 0)
            throw new ArgumentException("Query parameter 'teacherId' must be greater than 0.");

        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            throw new ArgumentException("'startDate' cannot be greater than 'endDate'.");

        return await _supplementaryActivitiesRepository.QuerySupplementaryActivities(
            teacherId,
            startDate,
            endDate
        );
    }
}
