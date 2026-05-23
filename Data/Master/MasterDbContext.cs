using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.Schedules.Models;
using FisaActivitateZilnicaApi.SupplementaryActivities.Models;
using Microsoft.EntityFrameworkCore;
using SubjectModel = FisaActivitateZilnicaApi.Schedules.Models.Subject;

namespace FisaActivitateZilnicaApi.Data.Master;

public class MasterDbContext(DbContextOptions<MasterDbContext> options) : DbContext(options)
{
    public DbSet<ScheduleYear> ScheduleYears => Set<ScheduleYear>();
    public DbSet<ScheduleSemester> ScheduleSemesters => Set<ScheduleSemester>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
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
    public DbSet<ActivitySlot> ActivitySlots => Set<ActivitySlot>();
    public DbSet<DailyActivityRecord> DailyActivityRecords => Set<DailyActivityRecord>();
    public DbSet<SupplementaryActivity> SupplementaryActivities => Set<SupplementaryActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ScheduleSemester>()
            .HasOne(scheduleSemester => scheduleSemester.ScheduleYear)
            .WithMany(scheduleYear => scheduleYear.ScheduleSemesters)
            .HasForeignKey(scheduleSemester => scheduleSemester.ScheduleYearId);

        modelBuilder
            .Entity<Schedule>()
            .HasOne(schedule => schedule.ScheduleSemester)
            .WithMany(scheduleSemester => scheduleSemester.Schedules)
            .HasForeignKey(schedule => schedule.ScheduleSemesterId);

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
            .HasOne(activityStudents => activityStudents.Activity)
            .WithMany(activity => activity.StudentsSets)
            .HasForeignKey(activityStudents => activityStudents.ActivityId);

        modelBuilder
            .Entity<Group>()
            .HasOne(group => group.Year)
            .WithMany(year => year.Groups)
            .HasForeignKey(group => group.YearId);

        modelBuilder
            .Entity<Subgroup>()
            .HasOne(subgroup => subgroup.Group)
            .WithMany(group => group.Subgroups)
            .HasForeignKey(subgroup => subgroup.GroupId);

        modelBuilder
            .Entity<Room>()
            .HasOne(room => room.Building)
            .WithMany(building => building.Rooms)
            .HasForeignKey(room => room.BuildingId)
            .IsRequired(false);

        modelBuilder
            .Entity<Teacher>()
            .HasOne(t => t.Schedule)
            .WithMany(s => s.Teachers)
            .HasForeignKey(t => t.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<SubjectModel>()
            .HasOne(s => s.Schedule)
            .WithMany(sched => sched.Subjects)
            .HasForeignKey(s => s.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<ActivityTag>()
            .HasOne(t => t.Schedule)
            .WithMany(s => s.ActivityTags)
            .HasForeignKey(t => t.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<Activity>()
            .HasOne(a => a.Schedule)
            .WithMany(s => s.Activities)
            .HasForeignKey(a => a.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<Day>()
            .HasOne(d => d.Schedule)
            .WithMany(s => s.Days)
            .HasForeignKey(d => d.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<Hour>()
            .HasOne(h => h.Schedule)
            .WithMany(s => s.Hours)
            .HasForeignKey(h => h.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<Building>()
            .HasOne(b => b.Schedule)
            .WithMany(s => s.Buildings)
            .HasForeignKey(b => b.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<Room>()
            .HasOne(r => r.Schedule)
            .WithMany(s => s.Rooms)
            .HasForeignKey(r => r.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<Year>()
            .HasOne(y => y.Schedule)
            .WithMany(s => s.Years)
            .HasForeignKey(y => y.ScheduleId)
            .IsRequired(false);

        modelBuilder
            .Entity<ActivitySlot>()
            .HasOne(s => s.Schedule)
            .WithMany()
            .HasForeignKey(s => s.ScheduleId);

        modelBuilder
            .Entity<ActivitySlot>()
            .HasOne(s => s.Activity)
            .WithMany()
            .HasForeignKey(s => s.ActivityId);

        modelBuilder
            .Entity<ActivitySlot>()
            .HasOne(s => s.Day)
            .WithMany()
            .HasForeignKey(s => s.DayId);

        modelBuilder
            .Entity<ActivitySlot>()
            .HasOne(s => s.Hour)
            .WithMany()
            .HasForeignKey(s => s.HourId);

        modelBuilder
            .Entity<ActivitySlot>()
            .HasOne(s => s.Room)
            .WithMany()
            .HasForeignKey(s => s.RoomId)
            .IsRequired(false);
    }
}
