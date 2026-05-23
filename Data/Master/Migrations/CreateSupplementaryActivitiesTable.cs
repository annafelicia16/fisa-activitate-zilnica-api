using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

// 2026.05.09 - 002
[Migration(20260509002)]
public class CreateSupplementaryActivitiesTable : Migration
{
    public override void Up()
    {
        Create
            .Table("SupplementaryActivities")
            .WithColumn("Id")
            .AsString(36)
            .PrimaryKey()
            .NotNullable()
            .WithColumn("ExternalTeacherId")
            .AsInt32()
            .NotNullable()
            .WithColumn("Date")
            .AsDateTime()
            .NotNullable()
            .WithColumn("ActivityType")
            .AsString(255)
            .NotNullable()
            .WithColumn("Observations")
            .AsString(4000)
            .Nullable()
            .WithColumn("TotalHours")
            .AsDouble()
            .NotNullable();
    }

    public override void Down() => Delete.Table("SupplementaryActivities");
}
