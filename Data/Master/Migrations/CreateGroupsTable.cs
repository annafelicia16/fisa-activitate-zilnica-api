using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309010)]
public class CreateGroupsTable : Migration
{
    public override void Up()
    {
        Create.Table("Groups")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("YearId").AsInt32().NotNullable().ForeignKey("FK_Groups_Years", "Years", "Id")
            .WithColumn("NumberOfStudents").AsInt32().NotNullable()
            .WithColumn("Comments").AsString(2000).Nullable();
    }

    public override void Down() => Delete.Table("Groups");
}
