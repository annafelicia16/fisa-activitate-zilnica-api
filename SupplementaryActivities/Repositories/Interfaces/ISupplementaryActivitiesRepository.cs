using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Repositories.Interfaces;

public interface ISupplementaryActivitiesRepository
{
    Task<GetSupplementaryActivityResponse?> GetSupplementaryActivityById(string id);
    Task<IEnumerable<GetSupplementaryActivityResponse>> QuerySupplementaryActivities(
        int externalTeacherId,
        DateTime? startDate,
        DateTime? endDate
    );
    Task<GetSupplementaryActivityResponse> CreateSupplementaryActivity(
        CreateSupplementaryActivityRequest request
    );
    Task<GetSupplementaryActivityResponse> UpdateSupplementaryActivity(
        UpdateSupplementaryActivityRequest request
    );
    Task<GetSupplementaryActivityResponse> DeleteSupplementaryActivity(string id);
}
