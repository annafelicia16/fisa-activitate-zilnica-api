using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309014)]
public class CreateActivityStudentsTable : Migration
{
    public override void Up()
    {
        Create.Table("ActivityStudents")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("ActivityId").AsInt32().NotNullable().ForeignKey("FK_ActivityStudents_Activities", "Activities", "Id")
            .WithColumn("StudentsName").AsString(255).NotNullable();
    }

    public override void Down() => Delete.Table("ActivityStudents");
}
