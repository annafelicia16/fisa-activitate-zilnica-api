using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

// Adds a nullable foreign-key to ActivitySlots so a daily activity record can be
// traced back to the scheduled slot it satisfies. Used by the schedule-slot
// listing to drop slots that have already been recorded, replacing the brittle
// (subject, hour) heuristic. Nullable because users can still log ad-hoc
// activity that doesn't correspond to any scheduled slot.
[Migration(20260524001)]
public class AddActivitySlotIdToDailyActivityRecords : Migration
{
    public override void Up()
    {
        Alter
            .Table("DailyActivityRecords")
            .AddColumn("ActivitySlotId")
            .AsInt32()
            .Nullable()
            .ForeignKey(
                "FK_DailyActivityRecords_ActivitySlots",
                "ActivitySlots",
                "Id"
            );

        Create
            .Index("IX_DailyActivityRecords_ActivitySlotId")
            .OnTable("DailyActivityRecords")
            .OnColumn("ActivitySlotId");
    }

    public override void Down()
    {
        Delete
            .Index("IX_DailyActivityRecords_ActivitySlotId")
            .OnTable("DailyActivityRecords");
        Delete
            .ForeignKey("FK_DailyActivityRecords_ActivitySlots")
            .OnTable("DailyActivityRecords");
        Delete.Column("ActivitySlotId").FromTable("DailyActivityRecords");
    }
}
