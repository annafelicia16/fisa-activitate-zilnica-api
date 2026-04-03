using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

namespace FisaActivitateZilnicaApi.Schedules.Services;

public class SchedulesQueryService(ISchedulesRepository schedulesRepository) : ISchedulesQueryService
{
    public async Task<IReadOnlyList<TeacherScheduleSlotResponse>> GetTeacherDaySlotsAsync(
        GetTeacherScheduleSlotsRequest request,
        CancellationToken ct = default
    )
    {
        ValidateRequest(request);

        var results = await schedulesRepository.GetTeacherDaySlotsByExternalIdsAsync(
            request.ScheduleId,
            request.DayName.Trim(),
            request.ExternalTeacherId,
            request.ExternalSubjectId,
            ct
        );

        return results
            .Select(x => new TeacherScheduleSlotResponse(
                x.SlotId,
                x.ScheduleId,
                x.ActivityId,
                x.HourName,
                x.DayName,
                x.SubjectName,
                x.ExternalTeacherId,
                x.PlanMatterProviderExternalId,
                x.FacultyExternalId,
                x.MetaSpecializationExternalId,
                x.StudyYearNumber,
                x.GroupExternalId,
                x.SpecializationExternalId,
                x.SubjectExternalId
            ))
            .ToList();
    }

    private static void ValidateRequest(GetTeacherScheduleSlotsRequest request)
    {
        if (request.ScheduleId <= 0)
            throw new ArgumentException("Query parameter 'scheduleId' must be greater than 0.");

        if (string.IsNullOrWhiteSpace(request.DayName))
            throw new ArgumentException("Query parameter 'dayName' is required.");

        if (request.ExternalTeacherId <= 0)
            throw new ArgumentException("Query parameter 'externalTeacherId' must be greater than 0.");

        if (request.ExternalSubjectId <= 0)
            throw new ArgumentException("Query parameter 'externalSubjectId' must be greater than 0.");
    }
}
