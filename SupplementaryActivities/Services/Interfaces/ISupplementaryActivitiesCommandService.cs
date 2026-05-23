using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;

public interface ISupplementaryActivitiesCommandService
{
    Task<GetSupplementaryActivityResponse> CreateSupplementaryActivityAsync(
        CreateSupplementaryActivityRequest request
    );
    Task<GetSupplementaryActivityResponse> UpdateSupplementaryActivityAsync(
        UpdateSupplementaryActivityRequest request
    );
    Task<GetSupplementaryActivityResponse> DeleteSupplementaryActivityAsync(string id);
}
