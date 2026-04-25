using FisaActivitateZilnicaApi.ExternalTeachers.Models;
using FisaActivitateZilnicaApi.Schedules.Models;
using Microsoft.EntityFrameworkCore;
using SubjectModel = FisaActivitateZilnicaApi.Schedules.Models.Subject;

namespace FisaActivitateZilnicaApi.Data.University;

public class UniversityDbContext(DbContextOptions<UniversityDbContext> options) : DbContext(options)
{
    public DbSet<ExternalTeacher> ExternalTeachers => Set<ExternalTeacher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExternalTeacher>().ToTable("Profesor", "dbo");
    }
}
