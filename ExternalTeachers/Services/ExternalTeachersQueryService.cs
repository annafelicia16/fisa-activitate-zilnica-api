using FisaActivitateZilnicaApi.ExternalTeachers.Models;
using FisaActivitateZilnicaApi.ExternalTeachers.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalTeachers.Services.Interfaces;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Services;

public class ExternalTeachersQueryService(IExternalTeachersRepository externalTeachersRepository)
    : IExternalTeachersQueryService
{
    private readonly IExternalTeachersRepository _externalTeachersRepository =
        externalTeachersRepository;

    public async Task<ExternalTeacher> GetExternalTeacherByEmailAsync(
        string email,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Query parameter 'email' is required.");

        string normalizedEmail = email.Trim();

        ExternalTeacher externalTeacher =
            await _externalTeachersRepository.GetExternalTeacherByEmailAsync(normalizedEmail, ct)
            ?? throw new KeyNotFoundException(
                $"External teacher with email '{normalizedEmail}' was not found."
            );

        return externalTeacher;
    }
}
