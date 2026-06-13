using FisaActivitateZilnicaApi.SupplementaryActivities.Models;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Repositories.Interfaces;

// Returns models (not responses) — the attachments service needs StoredFileName
// to resolve on-disk paths, and the response DTO deliberately omits it.
public interface ISupplementaryActivityAttachmentsRepository
{
    Task<List<SupplementaryActivityAttachment>> GetByActivityId(string activityId);
    Task<int> CountByActivityId(string activityId);
    Task<SupplementaryActivityAttachment?> GetById(string id);
    Task AddRange(IEnumerable<SupplementaryActivityAttachment> attachments);
    Task Delete(SupplementaryActivityAttachment attachment);
}
