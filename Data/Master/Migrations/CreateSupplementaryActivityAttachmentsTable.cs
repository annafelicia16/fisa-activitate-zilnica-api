using System.Data;
using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

// Files attached to supplementary activities, stored on local disk; this table
// holds the metadata. Rows cascade-delete with their activity (the disk files
// are cleaned up by SupplementaryActivitiesCommandService on delete).
[Migration(20260611002)]
public class CreateSupplementaryActivityAttachmentsTable : Migration
{
    public override void Up()
    {
        Create
            .Table("SupplementaryActivityAttachments")
            .WithColumn("Id")
            .AsString(36)
            .PrimaryKey()
            .NotNullable()
            .WithColumn("SupplementaryActivityId")
            .AsString(36)
            .NotNullable()
            .ForeignKey(
                "FK_SupplementaryActivityAttachments_SupplementaryActivities",
                "SupplementaryActivities",
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
            .Index("IX_SupplementaryActivityAttachments_SupplementaryActivityId")
            .OnTable("SupplementaryActivityAttachments")
            .OnColumn("SupplementaryActivityId");
    }

    public override void Down() => Delete.Table("SupplementaryActivityAttachments");
}
