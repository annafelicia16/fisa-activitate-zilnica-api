using FisaActivitateZilnicaApi.Schedules.Models;
using Microsoft.EntityFrameworkCore;
using SubjectModel = FisaActivitateZilnicaApi.Schedules.Models.Subject;

namespace FisaActivitateZilnicaApi.Data.University;

public class UniversityDbContext(DbContextOptions<UniversityDbContext> options) : DbContext(options)
{
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<SubjectModel> Subjects => Set<SubjectModel>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityTag> ActivityTags => Set<ActivityTag>();
    public DbSet<ActivityTeacher> ActivityTeachers => Set<ActivityTeacher>();
    public DbSet<ActivityStudents> ActivityStudents => Set<ActivityStudents>();
    public DbSet<Day> Days => Set<Day>();
    public DbSet<Hour> Hours => Set<Hour>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Year> Years => Set<Year>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Subgroup> Subgroups => Set<Subgroup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityTeacher>().HasKey(at => new { at.ActivityId, at.TeacherId });

        modelBuilder
            .Entity<ActivityTeacher>()
            .HasOne(at => at.Activity)
            .WithMany(a => a.Teachers)
            .HasForeignKey(at => at.ActivityId);

        modelBuilder
            .Entity<ActivityTeacher>()
            .HasOne(at => at.Teacher)
            .WithMany(t => t.ActivityTeachers)
            .HasForeignKey(at => at.TeacherId);

        modelBuilder
            .Entity<Activity>()
            .HasOne(a => a.Subject)
            .WithMany(s => s.Activities)
            .HasForeignKey(a => a.SubjectId);

        modelBuilder
            .Entity<Activity>()
            .HasOne(a => a.ActivityTag)
            .WithMany(t => t.Activities)
            .HasForeignKey(a => a.ActivityTagId)
            .IsRequired(false);

        modelBuilder
            .Entity<ActivityStudents>()
            .HasOne(a => a.Activity)
            .WithMany(a => a.StudentsSets)
            .HasForeignKey(a => a.ActivityId);

        modelBuilder
            .Entity<Group>()
            .HasOne(g => g.Year)
            .WithMany(y => y.Groups)
            .HasForeignKey(g => g.YearId);

        modelBuilder
            .Entity<Subgroup>()
            .HasOne(s => s.Group)
            .WithMany(g => g.Subgroups)
            .HasForeignKey(s => s.GroupId);

        modelBuilder
            .Entity<Room>()
            .HasOne(r => r.Building)
            .WithMany(b => b.Rooms)
            .HasForeignKey(r => r.BuildingId)
            .IsRequired(false);
    }
}
