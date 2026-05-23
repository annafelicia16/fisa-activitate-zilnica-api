using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260406001)]
public class CreateDailyActivityRecordsTable : Migration
{
    public override void Up()
    {
        Create
            .Table("DailyActivityRecords")
            .WithColumn("Id")
            .AsString(36)
            .PrimaryKey()
            .NotNullable()
            .WithColumn("ExternalTeacherId")
            .AsInt32()
            .NotNullable()
            .WithColumn("DepartmentName")
            .AsString(255)
            .NotNullable()
            .WithColumn("Year")
            .AsInt32()
            .NotNullable()
            .WithColumn("GroupName")
            .AsString(255)
            .NotNullable()
            .WithColumn("SubgroupName")
            .AsString(255)
            .Nullable()
            .WithColumn("SubjectName")
            .AsString(255)
            .NotNullable()
            .WithColumn("RoomName")
            .AsString(255)
            .NotNullable()
            .WithColumn("RevenueType")
            .AsInt32()
            .NotNullable()
            .WithColumn("ActivityType")
            .AsInt32()
            .NotNullable()
            .WithColumn("Observations")
            .AsString(4000)
            .Nullable()
            .WithColumn("StartDate")
            .AsDateTime()
            .NotNullable()
            .WithColumn("EndDate")
            .AsDateTime()
            .NotNullable();
    }

    public override void Down() => Delete.Table("DailyActivityRecords");
}
