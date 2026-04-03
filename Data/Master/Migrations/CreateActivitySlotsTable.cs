using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309017)]
public class CreateActivitySlotsTable : Migration
{
    public override void Up()
    {
        Create
            .Table("ActivitySlots")
            .WithColumn("Id")
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn("ScheduleId")
            .AsInt32()
            .NotNullable()
            .ForeignKey("FK_ActivitySlots_Schedules", "Schedules", "Id")
            .WithColumn("ActivityId")
            .AsInt32()
            .NotNullable()
            .ForeignKey("FK_ActivitySlots_Activities", "Activities", "Id")
            .WithColumn("DayId")
            .AsInt32()
            .NotNullable()
            .ForeignKey("FK_ActivitySlots_Days", "Days", "Id")
            .WithColumn("HourId")
            .AsInt32()
            .NotNullable()
            .ForeignKey("FK_ActivitySlots_Hours", "Hours", "Id")
            .WithColumn("RoomId")
            .AsInt32()
            .Nullable()
            .ForeignKey("FK_ActivitySlots_Rooms", "Rooms", "Id");

        Create
            .UniqueConstraint("UQ_ActivitySlots_Activity_Day_Hour")
            .OnTable("ActivitySlots")
            .Columns("ActivityId", "DayId", "HourId");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_ActivitySlots_Activity_Day_Hour").FromTable("ActivitySlots");
        Delete.Table("ActivitySlots");
    }
}
