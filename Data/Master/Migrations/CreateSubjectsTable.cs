using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309002)]
public class CreateSubjectsTable : Migration
{
    public override void Up()
    {
        Create.Table("Subjects")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("Comments").AsString(2000).Nullable();
    }

    public override void Down() => Delete.Table("Subjects");
}
