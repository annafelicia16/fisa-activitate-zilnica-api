using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309011)]
public class CreateSubgroupsTable : Migration
{
    public override void Up()
    {
        Create.Table("Subgroups")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("GroupId").AsInt32().NotNullable().ForeignKey("FK_Subgroups_Groups", "Groups", "Id")
            .WithColumn("NumberOfStudents").AsInt32().NotNullable()
            .WithColumn("Comments").AsString(2000).Nullable();
    }

    public override void Down() => Delete.Table("Subgroups");
}
