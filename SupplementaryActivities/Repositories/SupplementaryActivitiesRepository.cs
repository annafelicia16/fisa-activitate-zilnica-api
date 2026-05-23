using AutoMapper;
using FisaActivitateZilnicaApi.Data.Master;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.SupplementaryActivities.Models;
using FisaActivitateZilnicaApi.SupplementaryActivities.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Repositories;

public class SupplementaryActivitiesRepository(MasterDbContext masterDbContext, IMapper mapper)
    : ISupplementaryActivitiesRepository
{
    private readonly MasterDbContext _masterDbContext = masterDbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<GetSupplementaryActivityResponse?> GetSupplementaryActivityById(string id)
    {
        SupplementaryActivity? activity = await _masterDbContext
            .SupplementaryActivities.AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(sa => sa.Id == id);

        if (activity == null)
            return null;

        return _mapper.Map<GetSupplementaryActivityResponse>(activity);
    }

    public async Task<IEnumerable<GetSupplementaryActivityResponse>> QuerySupplementaryActivities(
        int externalTeacherId,
        DateTime? startDate,
        DateTime? endDate
    )
    {
        IQueryable<SupplementaryActivity> query = _masterDbContext
            .SupplementaryActivities.AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(sa => sa.ExternalTeacherId == externalTeacherId);

        if (startDate != null)
            query = query.Where(sa => sa.Date >= startDate);

        if (endDate != null)
            query = query.Where(sa => sa.Date <= endDate);

        return await query
            .OrderBy(sa => sa.Date)
            .Select(sa => _mapper.Map<GetSupplementaryActivityResponse>(sa))
            .ToListAsync();
    }

    public async Task<GetSupplementaryActivityResponse> CreateSupplementaryActivity(
        CreateSupplementaryActivityRequest request
    )
    {
        SupplementaryActivity activity = _mapper.Map<SupplementaryActivity>(request);
        _masterDbContext.SupplementaryActivities.Add(activity);
        await _masterDbContext.SaveChangesAsync();
        return _mapper.Map<GetSupplementaryActivityResponse>(activity);
    }

    public async Task<GetSupplementaryActivityResponse> UpdateSupplementaryActivity(
        UpdateSupplementaryActivityRequest request
    )
    {
        SupplementaryActivity activity = (
            await _masterDbContext
                .SupplementaryActivities.AsNoTracking()
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync(sa => sa.Id == request.Id)
        )!;

        _mapper.Map(request, activity);

        _masterDbContext.SupplementaryActivities.Update(activity);
        await _masterDbContext.SaveChangesAsync();

        return _mapper.Map<GetSupplementaryActivityResponse>(activity);
    }

    public async Task<GetSupplementaryActivityResponse> DeleteSupplementaryActivity(string id)
    {
        SupplementaryActivity activity = (
            await _masterDbContext
                .SupplementaryActivities.AsNoTracking()
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync(sa => sa.Id == id)
        )!;

        _masterDbContext.SupplementaryActivities.Remove(activity);
        await _masterDbContext.SaveChangesAsync();

        return _mapper.Map<GetSupplementaryActivityResponse>(activity);
    }
}
