using FisaActivitateZilnicaApi.Data.Master;
using FisaActivitateZilnicaApi.SupplementaryActivities.Models;
using FisaActivitateZilnicaApi.SupplementaryActivities.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Repositories;

public class SupplementaryActivityAttachmentsRepository(MasterDbContext masterDbContext)
    : ISupplementaryActivityAttachmentsRepository
{
    private readonly MasterDbContext _masterDbContext = masterDbContext;

    public async Task<List<SupplementaryActivityAttachment>> GetByActivityId(string activityId)
    {
        return await _masterDbContext
            .SupplementaryActivityAttachments.AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(a => a.SupplementaryActivityId == activityId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountByActivityId(string activityId)
    {
        return await _masterDbContext
            .SupplementaryActivityAttachments.AsNoTracking()
            .CountAsync(a => a.SupplementaryActivityId == activityId);
    }

    public async Task<SupplementaryActivityAttachment?> GetById(string id)
    {
        return await _masterDbContext
            .SupplementaryActivityAttachments.AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddRange(IEnumerable<SupplementaryActivityAttachment> attachments)
    {
        _masterDbContext.SupplementaryActivityAttachments.AddRange(attachments);
        await _masterDbContext.SaveChangesAsync();
    }

    public async Task Delete(SupplementaryActivityAttachment attachment)
    {
        _masterDbContext.SupplementaryActivityAttachments.Remove(attachment);
        await _masterDbContext.SaveChangesAsync();
    }
}
