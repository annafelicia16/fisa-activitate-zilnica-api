using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Data.Master;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.DailyActivities.Repositories;

public class DailyActivityRecordAttachmentsRepository(MasterDbContext masterDbContext)
    : IDailyActivityRecordAttachmentsRepository
{
    private readonly MasterDbContext _masterDbContext = masterDbContext;

    public async Task<List<DailyActivityRecordAttachment>> GetByRecordId(string recordId)
    {
        return await _masterDbContext
            .DailyActivityRecordAttachments.AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(a => a.DailyActivityRecordId == recordId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountByRecordId(string recordId)
    {
        return await _masterDbContext
            .DailyActivityRecordAttachments.AsNoTracking()
            .CountAsync(a => a.DailyActivityRecordId == recordId);
    }

    public async Task<DailyActivityRecordAttachment?> GetById(string id)
    {
        return await _masterDbContext
            .DailyActivityRecordAttachments.AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddRange(IEnumerable<DailyActivityRecordAttachment> attachments)
    {
        _masterDbContext.DailyActivityRecordAttachments.AddRange(attachments);
        await _masterDbContext.SaveChangesAsync();
    }

    public async Task Delete(DailyActivityRecordAttachment attachment)
    {
        _masterDbContext.DailyActivityRecordAttachments.Remove(attachment);
        await _masterDbContext.SaveChangesAsync();
    }
}
