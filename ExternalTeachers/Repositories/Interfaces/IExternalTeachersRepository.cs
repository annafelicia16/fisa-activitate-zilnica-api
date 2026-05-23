using FisaActivitateZilnicaApi.ExternalTeachers.Models;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Repositories.Interfaces;

public interface IExternalTeachersRepository
{
    Task<ExternalTeacher?> GetExternalTeacherByEmailAsync(
        string email,
        CancellationToken ct = default
    );

    Task<ExternalTeacher?> GetExternalTeacherByIdAsync(
        long idProfesor,
        CancellationToken ct = default
    );
}
