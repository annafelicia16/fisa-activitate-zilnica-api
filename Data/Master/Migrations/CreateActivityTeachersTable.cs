using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309013)]
public class CreateActivityTeachersTable : Migration
{
    public override void Up()
    {
        Create.Table("ActivityTeachers")
            .WithColumn("ActivityId").AsInt32().NotNullable().ForeignKey("FK_ActivityTeachers_Activities", "Activities", "Id")
            .WithColumn("TeacherId").AsInt32().NotNullable().ForeignKey("FK_ActivityTeachers_Teachers", "Teachers", "Id");

        Create.PrimaryKey("PK_ActivityTeachers").OnTable("ActivityTeachers").Columns("ActivityId", "TeacherId");
    }

    public override void Down() => Delete.Table("ActivityTeachers");
}
