using FisaActivitateZilnicaApi.Data.University;
using FisaActivitateZilnicaApi.ExternalTeachers.Models;
using FisaActivitateZilnicaApi.ExternalTeachers.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Repositories;

public class ExternalTeachersRepository(UniversityDbContext universityDbContext)
    : IExternalTeachersRepository
{
    private readonly UniversityDbContext _universityDbContext = universityDbContext;

    public async Task<ExternalTeacher?> GetExternalTeacherByEmailAsync(
        string email,
        CancellationToken ct = default
    )
    {
        return await _universityDbContext
            .ExternalTeachers.AsNoTracking()
            .FirstOrDefaultAsync(teacher => teacher.Email == email, ct);
    }

    public async Task<ExternalTeacher?> GetExternalTeacherByIdAsync(
        long idProfesor,
        CancellationToken ct = default
    )
    {
        return await _universityDbContext
            .ExternalTeachers.AsNoTracking()
            .FirstOrDefaultAsync(teacher => teacher.IdProfesor == idProfesor, ct);
    }
}
