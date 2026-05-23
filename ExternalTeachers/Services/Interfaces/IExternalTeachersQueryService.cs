using FisaActivitateZilnicaApi.ExternalTeachers.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalTeachers.Models;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Services.Interfaces;

public interface IExternalTeachersQueryService
{
    Task<ExternalTeacher> GetExternalTeacherByEmailAsync(
        string email,
        CancellationToken ct = default
    );

    Task<TeacherProfileResponse> GetTeacherProfileAsync(
        int externalTeacherId,
        CancellationToken ct = default
    );
}
