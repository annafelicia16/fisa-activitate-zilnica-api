using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309016)]
public class AddScheduleIdToAllTables : Migration
{
    public override void Up()
    {
        Alter
            .Table("Teachers")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_Teachers_Schedules", "Schedules", "Id");

        Alter
            .Table("Subjects")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_Subjects_Schedules", "Schedules", "Id");

        Alter
            .Table("ActivityTags")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_ActivityTags_Schedules", "Schedules", "Id");

        Alter
            .Table("Days")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_Days_Schedules", "Schedules", "Id");

        Alter
            .Table("Hours")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_Hours_Schedules", "Schedules", "Id");

        Alter
            .Table("Buildings")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_Buildings_Schedules", "Schedules", "Id");

        Alter
            .Table("Rooms")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_Rooms_Schedules", "Schedules", "Id");

        Alter
            .Table("Years")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_Years_Schedules", "Schedules", "Id");

        Alter
            .Table("Activities")
            .AddColumn("ScheduleId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_Activities_Schedules", "Schedules", "Id");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_Teachers_Schedules").OnTable("Teachers");
        Delete.Column("ScheduleId").FromTable("Teachers");

        Delete.ForeignKey("FK_Subjects_Schedules").OnTable("Subjects");
        Delete.Column("ScheduleId").FromTable("Subjects");

        Delete.ForeignKey("FK_ActivityTags_Schedules").OnTable("ActivityTags");
        Delete.Column("ScheduleId").FromTable("ActivityTags");

        Delete.ForeignKey("FK_Days_Schedules").OnTable("Days");
        Delete.Column("ScheduleId").FromTable("Days");

        Delete.ForeignKey("FK_Hours_Schedules").OnTable("Hours");
        Delete.Column("ScheduleId").FromTable("Hours");

        Delete.ForeignKey("FK_Buildings_Schedules").OnTable("Buildings");
        Delete.Column("ScheduleId").FromTable("Buildings");

        Delete.ForeignKey("FK_Rooms_Schedules").OnTable("Rooms");
        Delete.Column("ScheduleId").FromTable("Rooms");

        Delete.ForeignKey("FK_Years_Schedules").OnTable("Years");
        Delete.Column("ScheduleId").FromTable("Years");

        Delete.ForeignKey("FK_Activities_Schedules").OnTable("Activities");
        Delete.Column("ScheduleId").FromTable("Activities");
    }
}
