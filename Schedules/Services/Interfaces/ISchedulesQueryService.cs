using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;

namespace FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

public interface ISchedulesQueryService
{
    Task<IReadOnlyList<TeacherScheduleSlotResponse>> GetTeacherDaySlotsAsync(
        GetTeacherScheduleSlotsRequest request,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ScheduleResponse>> GetSchedulesByExternalTeacherIdAsync(
        int externalTeacherId,
        CancellationToken ct = default
    );
}
