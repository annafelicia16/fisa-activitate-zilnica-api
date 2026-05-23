using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

// Subgroup is no longer captured on daily activity records — records track the
// group only. Drops the now-unused column.
[Migration(20260524002)]
public class DropSubgroupNameFromDailyActivityRecords : Migration
{
    public override void Up()
    {
        Delete.Column("SubgroupName").FromTable("DailyActivityRecords");
    }

    public override void Down()
    {
        Alter
            .Table("DailyActivityRecords")
            .AddColumn("SubgroupName")
            .AsString(255)
            .Nullable();
    }
}
