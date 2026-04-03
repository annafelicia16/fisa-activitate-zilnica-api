using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309004)]
public class CreateHoursTable : Migration
{
    public override void Up()
    {
        Create.Table("Hours")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable();
    }

    public override void Down() => Delete.Table("Hours");
}
