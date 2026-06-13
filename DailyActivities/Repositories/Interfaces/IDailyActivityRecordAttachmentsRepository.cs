using FisaActivitateZilnicaApi.DailyActivities.Models;

namespace FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;

// Returns models (not responses) — the attachments service needs StoredFileName
// to resolve on-disk paths, and the response DTO deliberately omits it.
public interface IDailyActivityRecordAttachmentsRepository
{
    Task<List<DailyActivityRecordAttachment>> GetByRecordId(string recordId);
    Task<int> CountByRecordId(string recordId);
    Task<DailyActivityRecordAttachment?> GetById(string id);
    Task AddRange(IEnumerable<DailyActivityRecordAttachment> attachments);
    Task Delete(DailyActivityRecordAttachment attachment);
}
