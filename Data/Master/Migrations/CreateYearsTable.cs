using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309009)]
public class CreateYearsTable : Migration
{
    public override void Up()
    {
        Create.Table("Years")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("NumberOfStudents").AsInt32().NotNullable()
            .WithColumn("Comments").AsString(2000).Nullable();
    }

    public override void Down() => Delete.Table("Years");
}
