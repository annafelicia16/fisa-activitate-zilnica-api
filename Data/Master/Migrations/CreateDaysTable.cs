using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309003)]
public class CreateDaysTable : Migration
{
    public override void Up()
    {
        Create.Table("Days")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable();
    }

    public override void Down() => Delete.Table("Days");
}
