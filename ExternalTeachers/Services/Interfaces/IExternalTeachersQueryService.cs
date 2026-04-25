using FisaActivitateZilnicaApi.ExternalTeachers.Models;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Services.Interfaces;

public interface IExternalTeachersQueryService
{
    Task<ExternalTeacher> GetExternalTeacherByEmailAsync(
        string email,
        CancellationToken ct = default
    );
}
