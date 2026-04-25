using AutoMapper;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;

namespace FisaActivitateZilnicaApi.System;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateDailyActivityRecordRequest, DailyActivityRecord>();
        CreateMap<UpdateDailyActivityRecordRequest, DailyActivityRecord>()
            .ForAllMembers(options =>
                options.Condition((src, dest, srcMember) => srcMember != null)
            );
        CreateMap<DailyActivityRecord, GetDailyActivityRecordResponse>();
    }
}
