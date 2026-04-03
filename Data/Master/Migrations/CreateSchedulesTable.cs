using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309015)]
public class CreateSchedulesTable : Migration
{
    public override void Up()
    {
        Create
            .Table("Schedules")
            .WithColumn("Id")
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn("Name")
            .AsString(255)
            .NotNullable()
            .WithColumn("Year")
            .AsInt32()
            .NotNullable()
            .WithColumn("Semester")
            .AsInt32()
            .NotNullable()
            .WithColumn("OddWeek")
            .AsBoolean()
            .NotNullable();

        Create
            .UniqueConstraint("UQ_Schedules_Year_Semester_OddWeek")
            .OnTable("Schedules")
            .Columns("Year", "Semester", "OddWeek");
    }

    public override void Down() => Delete.Table("Schedules");
}
