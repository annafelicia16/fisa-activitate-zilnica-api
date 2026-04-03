using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309008)]
public class CreateTeachersTable : Migration
{
    public override void Up()
    {
        Create.Table("Teachers")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("TargetNumberOfHours").AsInt32().NotNullable()
            .WithColumn("Comments").AsString(2000).Nullable();
    }

    public override void Down() => Delete.Table("Teachers");
}
