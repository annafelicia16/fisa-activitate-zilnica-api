using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;

public interface ISupplementaryActivitiesQueryService
{
    Task<GetSupplementaryActivityResponse> GetSupplementaryActivityByIdAsync(string id);
    Task<IEnumerable<GetSupplementaryActivityResponse>> QuerySupplementaryActivitiesAsync(
        int teacherId,
        DateTime? startDate,
        DateTime? endDate
    );
}
