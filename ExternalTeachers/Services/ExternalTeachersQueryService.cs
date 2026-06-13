using FisaActivitateZilnicaApi.ExternalReferences.Models;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalTeachers.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalTeachers.Models;
using FisaActivitateZilnicaApi.ExternalTeachers.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalTeachers.Services.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Models;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
using FisaActivitateZilnicaApi.System;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Services;

public class ExternalTeachersQueryService(
    IExternalTeachersRepository externalTeachersRepository,
    ISchedulesRepository schedulesRepository,
    IExternalReferencesRepository externalReferencesRepository
) : IExternalTeachersQueryService
{
    public async Task<ExternalTeacher> GetExternalTeacherByEmailAsync(
        string email,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Query parameter 'email' is required.");

        string normalizedEmail = email.Trim();

        ExternalTeacher externalTeacher =
            await externalTeachersRepository.GetExternalTeacherByEmailAsync(normalizedEmail, ct)
            ?? throw new KeyNotFoundException(
                $"External teacher with email '{normalizedEmail}' was not found."
            );

        return externalTeacher;
    }

    public async Task<TeacherProfileResponse> GetTeacherProfileAsync(
        int externalTeacherId,
        CancellationToken ct = default
    )
    {
        if (externalTeacherId <= 0)
            throw new ArgumentException(
                "Route parameter 'externalTeacherId' must be greater than 0."
            );

        ExternalTeacher teacher =
            await externalTeachersRepository.GetExternalTeacherByIdAsync(externalTeacherId, ct)
            ?? throw new KeyNotFoundException(
                $"External teacher with id '{externalTeacherId}' was not found."
            );

        DateTime now = DateTime.UtcNow;

        // Sequentially awaited: faculties + department live on UniversityDbContext,
        // schedule data on MasterDbContext. EF Core forbids concurrent ops on the same
        // context and the call cost is dominated by the WAN hop, not the queries.
        IReadOnlyList<int> facultyIds =
            await schedulesRepository.GetTeacherFacultyExternalIdsAsync(externalTeacherId, ct);

        IReadOnlyList<TeacherFacultyResponse> faculties = [];
        if (facultyIds.Count > 0)
        {
            var rows = await externalReferencesRepository.GetFacultiesByIdsAsync(facultyIds, ct);
            faculties = rows
                .Select(f => new TeacherFacultyResponse(
                    (int)f.IdFacultate,
                    f.DenumireScurta ?? f.Denumire,
                    f.DenumireScurta
                ))
                .OrderBy(f => f.Name, StringComparer.Ordinal)
                .ToList();
        }

        ExternalDepartment? department =
            await externalReferencesRepository.GetActiveDepartmentForTeacherAsync(
                externalTeacherId,
                now,
                ct
            );
        TeacherDepartmentResponse? departmentResponse = department is null
            ? null
            : new TeacherDepartmentResponse(
                department.IdDepartament,
                department.Denumire,
                department.DenumireScurta
            );

        ScheduleSemester? activeSemester =
            await schedulesRepository.GetActiveScheduleSemesterAsync(now, ct);

        return new TeacherProfileResponse(
            teacher.IdProfesor,
            teacher.Nume,
            teacher.Prenume,
            teacher.Email,
            teacher.Cnp,
            departmentResponse,
            faculties,
            faculties.FirstOrDefault(),
            AcademicCalendar.CurrentYearLabel(now),
            activeSemester?.Number,
            activeSemester?.StartDate,
            activeSemester?.EndDate
        );
    }
}
