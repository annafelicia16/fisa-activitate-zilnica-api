using AutoMapper;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.SupplementaryActivities.Models;

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
        CreateMap<DailyActivityRecordAttachment, GetDailyActivityRecordAttachmentResponse>();

        CreateMap<CreateSupplementaryActivityRequest, SupplementaryActivity>();
        CreateMap<UpdateSupplementaryActivityRequest, SupplementaryActivity>()
            .ForAllMembers(options =>
                options.Condition((src, dest, srcMember) => srcMember != null)
            );
        CreateMap<SupplementaryActivity, GetSupplementaryActivityResponse>();
        CreateMap<SupplementaryActivityAttachment, GetSupplementaryActivityAttachmentResponse>();
    }
}
