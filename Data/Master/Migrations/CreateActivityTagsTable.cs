using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309005)]
public class CreateActivityTagsTable : Migration
{
    public override void Up()
    {
        Create.Table("ActivityTags")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("Printable").AsBoolean().NotNullable()
            .WithColumn("Comments").AsString(2000).Nullable();
    }

    public override void Down() => Delete.Table("ActivityTags");
}
