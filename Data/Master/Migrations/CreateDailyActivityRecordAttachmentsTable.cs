using System.Data;
using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

// Files attached to daily activity records, stored on local disk; this table
// holds the metadata. Rows cascade-delete with their record (the disk files are
// cleaned up by DailyActivityRecordsCommandService on record delete).
[Migration(20260611001)]
public class CreateDailyActivityRecordAttachmentsTable : Migration
{
    public override void Up()
    {
        Create
            .Table("DailyActivityRecordAttachments")
            .WithColumn("Id")
            .AsString(36)
            .PrimaryKey()
            .NotNullable()
            .WithColumn("DailyActivityRecordId")
            .AsString(36)
            .NotNullable()
            .ForeignKey(
                "FK_DailyActivityRecordAttachments_DailyActivityRecords",
                "DailyActivityRecords",
                "Id"
            )
            .OnDelete(Rule.Cascade)
            .WithColumn("FileName")
            .AsString(255)
            .NotNullable()
            .WithColumn("StoredFileName")
            .AsString(255)
            .NotNullable()
            .WithColumn("ContentType")
            .AsString(255)
            .NotNullable()
            .WithColumn("SizeBytes")
            .AsInt64()
            .NotNullable()
            .WithColumn("CreatedAt")
            .AsDateTime()
            .NotNullable();

        Create
            .Index("IX_DailyActivityRecordAttachments_DailyActivityRecordId")
            .OnTable("DailyActivityRecordAttachments")
            .OnColumn("DailyActivityRecordId");
    }

    public override void Down() => Delete.Table("DailyActivityRecordAttachments");
}
