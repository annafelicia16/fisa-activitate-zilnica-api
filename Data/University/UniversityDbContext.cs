using FisaActivitateZilnicaApi.ExternalReferences.Models;
using FisaActivitateZilnicaApi.ExternalTeachers.Models;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.Data.University;

public class UniversityDbContext(DbContextOptions<UniversityDbContext> options) : DbContext(options)
{
    public DbSet<ExternalTeacher> ExternalTeachers => Set<ExternalTeacher>();
    public DbSet<ExternalSubject> ExternalSubjects => Set<ExternalSubject>();
    public DbSet<ExternalFaculty> ExternalFaculties => Set<ExternalFaculty>();
    public DbSet<ExternalSpecialization> ExternalSpecializations =>
        Set<ExternalSpecialization>();
    public DbSet<ExternalGroup> ExternalGroups => Set<ExternalGroup>();
    public DbSet<ExternalAcademicYear> ExternalAcademicYears => Set<ExternalAcademicYear>();
    public DbSet<ExternalStudyYear> ExternalStudyYears => Set<ExternalStudyYear>();
    public DbSet<ExternalDepartment> ExternalDepartments => Set<ExternalDepartment>();
    public DbSet<ExternalDepartmentTeacher> ExternalDepartmentTeachers =>
        Set<ExternalDepartmentTeacher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExternalTeacher>().ToTable("Profesor", "dbo");
        modelBuilder.Entity<ExternalSubject>().ToTable("Materie", "dbo");
        modelBuilder.Entity<ExternalFaculty>().ToTable("Facultate", "dbo");
        modelBuilder.Entity<ExternalSpecialization>().ToTable("Specializare", "dbo");
        modelBuilder.Entity<ExternalGroup>().ToTable("Grupe", "dbo");
        modelBuilder.Entity<ExternalAcademicYear>().ToTable("AnUniversitar", "dbo");
        modelBuilder.Entity<ExternalStudyYear>().ToTable("AnStudiu", "dbo");
        modelBuilder.Entity<ExternalDepartment>().ToTable("Departament", "dbo");
        modelBuilder
            .Entity<ExternalDepartmentTeacher>()
            .ToTable("DepartamentProfesor", "dbo");
    }
}
