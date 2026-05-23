using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260509001)]
public class AddFacultyAndCourseFieldsToDailyActivityRecords : Migration
{
    public override void Up()
    {
        Alter
            .Table("DailyActivityRecords")
            .AddColumn("FacultyName")
            .AsString(255)
            .NotNullable()
            .WithDefaultValue(string.Empty)
            .AddColumn("StudyProgram")
            .AsString(255)
            .NotNullable()
            .WithDefaultValue(string.Empty)
            .AddColumn("CourseType")
            .AsString(64)
            .NotNullable()
            .WithDefaultValue(string.Empty)
            .AddColumn("ConventionalHours")
            .AsDouble()
            .NotNullable()
            .WithDefaultValue(0);
    }

    public override void Down()
    {
        Delete.Column("FacultyName").FromTable("DailyActivityRecords");
        Delete.Column("StudyProgram").FromTable("DailyActivityRecords");
        Delete.Column("CourseType").FromTable("DailyActivityRecords");
        Delete.Column("ConventionalHours").FromTable("DailyActivityRecords");
    }
}
