using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309012)]
public class CreateActivitiesTable : Migration
{
    public override void Up()
    {
        Create.Table("Activities")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SubjectId").AsInt32().NotNullable().ForeignKey("FK_Activities_Subjects", "Subjects", "Id")
            .WithColumn("ActivityTagId").AsInt32().Nullable().ForeignKey("FK_Activities_ActivityTags", "ActivityTags", "Id")
            .WithColumn("Duration").AsInt32().NotNullable()
            .WithColumn("TotalDuration").AsInt32().NotNullable()
            .WithColumn("ActivityGroupId").AsInt32().NotNullable()
            .WithColumn("NumberOfStudents").AsInt32().Nullable()
            .WithColumn("Active").AsBoolean().NotNullable()
            .WithColumn("Comments").AsString(4000).Nullable();
    }

    public override void Down() => Delete.Table("Activities");
}
