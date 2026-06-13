using AutoMapper;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Data.Master;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.DailyActivities.Repositories;

public class DailyActivityRecordsRepository(MasterDbContext masterDbContext, IMapper mapper)
    : IDailyActivityRecordsRepository
{
    private readonly MasterDbContext _masterDbContext = masterDbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<GetDailyActivityRecordResponse?> GetDailyActivityRecordById(string id)
    {
        DailyActivityRecord? dailyActivityRecord = await _masterDbContext
            .DailyActivityRecords.AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(dar => dar.Id == id);

        if (dailyActivityRecord == null)
            return null;

        GetDailyActivityRecordResponse response = _mapper.Map<GetDailyActivityRecordResponse>(
            dailyActivityRecord
        );
        response.AttachmentCount = await _masterDbContext
            .DailyActivityRecordAttachments.AsNoTracking()
            .CountAsync(a => a.DailyActivityRecordId == id);
        return response;
    }

    public async Task<IEnumerable<GetDailyActivityRecordResponse>> QueryDailyActivityRecords(
        int externalTeacherId,
        string? departmentName,
        int? year,
        string? groupName,
        string? subjectName,
        string? roomName,
        RevenueType? revenueType,
        ActivityType? activityType,
        DateTime? startDate,
        DateTime? endDate
    )
    {
        IQueryable<DailyActivityRecord> query = _masterDbContext
            .DailyActivityRecords.AsNoTracking()
            .IgnoreAutoIncludes()
            .AsQueryable();

        query = query.Where(dar => dar.ExternalTeacherId == externalTeacherId);

        if (departmentName != null && !string.IsNullOrWhiteSpace(departmentName))
            query = query.Where(dar => dar.DepartmentName.Contains(departmentName));

        if (year != null)
            query = query.Where(dar => dar.Year == year);

        if (groupName != null && !string.IsNullOrWhiteSpace(groupName))
            query = query.Where(dar => dar.GroupName.Contains(groupName));

        if (subjectName != null && !string.IsNullOrWhiteSpace(subjectName))
            query = query.Where(dar => dar.SubjectName.Contains(subjectName));

        if (roomName != null && !string.IsNullOrWhiteSpace(roomName))
            query = query.Where(dar => dar.RoomName.Contains(roomName));

        if (revenueType != null)
            query = query.Where(dar => dar.RevenueType == revenueType);

        if (activityType != null)
            query = query.Where(dar => dar.ActivityType == activityType);

        if (startDate != null)
            query = query.Where(dar => dar.StartDate >= startDate);

        if (endDate != null)
            query = query.Where(dar => dar.EndDate <= endDate);

        List<GetDailyActivityRecordResponse> responses = await query
            .Select(dar => _mapper.Map<GetDailyActivityRecordResponse>(dar))
            .ToListAsync();

        // Attachment counts stitched in with one grouped query (the count lives
        // on the response, not the model) — drives the list paperclip indicator.
        if (responses.Count > 0)
        {
            string[] ids = responses.Select(r => r.Id).ToArray();
            Dictionary<string, int> counts = await _masterDbContext
                .DailyActivityRecordAttachments.AsNoTracking()
                .Where(a => ids.Contains(a.DailyActivityRecordId))
                .GroupBy(a => a.DailyActivityRecordId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            foreach (GetDailyActivityRecordResponse response in responses)
                response.AttachmentCount = counts.GetValueOrDefault(response.Id, 0);
        }

        return responses;
    }

    public async Task<IEnumerable<MonthlyActivitySummaryResponse>> GetMonthlySummaries(
        int externalTeacherId
    )
    {
        var rows = await _masterDbContext
            .DailyActivityRecords.AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(dar => dar.ExternalTeacherId == externalTeacherId)
            .GroupBy(dar => new { dar.StartDate.Year, dar.StartDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                RecordCount = g.Count(),
                TotalConventionalHours = g.Sum(x => x.ConventionalHours),
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();

        return rows.Select(r => new MonthlyActivitySummaryResponse(
            r.Year,
            r.Month,
            r.RecordCount,
            r.TotalConventionalHours,
            MonthlySheetStatus.Draft
        ));
    }

    public async Task<GetDailyActivityRecordResponse> CreateDailyActivityRecord(
        CreateDailyActivityRecordRequest request
    )
    {
        DailyActivityRecord dailyActivityRecord = _mapper.Map<DailyActivityRecord>(request);
        _masterDbContext.DailyActivityRecords.Add(dailyActivityRecord);
        await _masterDbContext.SaveChangesAsync();
        return _mapper.Map<GetDailyActivityRecordResponse>(dailyActivityRecord);
    }

    public async Task<GetDailyActivityRecordResponse> UpdateDailyActivityRecord(
        UpdateDailyActivityRecordRequest request
    )
    {
        DailyActivityRecord dailyActivityRecord = (
            await _masterDbContext
                .DailyActivityRecords.AsNoTracking()
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync(dar => dar.Id == request.Id)
        )!;

        _mapper.Map(request, dailyActivityRecord);

        _masterDbContext.DailyActivityRecords.Update(dailyActivityRecord);
        await _masterDbContext.SaveChangesAsync();

        return _mapper.Map<GetDailyActivityRecordResponse>(dailyActivityRecord);
    }

    public async Task<GetDailyActivityRecordResponse> DeleteDailyActivityRecord(string id)
    {
        DailyActivityRecord dailyActivityRecord = (
            await _masterDbContext
                .DailyActivityRecords.AsNoTracking()
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync(dar => dar.Id == id)
        )!;

        _masterDbContext.DailyActivityRecords.Remove(dailyActivityRecord);
        await _masterDbContext.SaveChangesAsync();

        return _mapper.Map<GetDailyActivityRecordResponse>(dailyActivityRecord);
    }
}
