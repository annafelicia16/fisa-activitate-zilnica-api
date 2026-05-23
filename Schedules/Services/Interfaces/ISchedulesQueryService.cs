using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;

namespace FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

public interface ISchedulesQueryService
{
    Task<IReadOnlyList<TeacherScheduleSlotResponse>> GetTeacherDaySlotsAsync(
        GetTeacherScheduleSlotsRequest request,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<TeacherScheduleSlotResponse>> GetTeacherDaySlotsByDateAsync(
        GetTeacherDaySlotsByDateRequest request,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ScheduleResponse>> GetSchedulesByExternalTeacherIdAsync(
        int externalTeacherId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<ScheduleResponse>> GetAllSchedulesAsync(CancellationToken ct = default);
    Task<int> BackfillActivityStudentsCommentRefsAsync(CancellationToken ct = default);
}
