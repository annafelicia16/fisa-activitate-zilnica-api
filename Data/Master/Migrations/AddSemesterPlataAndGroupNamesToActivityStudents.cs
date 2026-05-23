using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

// 2026.05.11 - 001
[Migration(20260511001)]
public class AddSemesterPlataAndGroupNamesToActivityStudents : Migration
{
    public override void Up()
    {
        Alter
            .Table("ActivityStudents")
            .AddColumn("Semester")
            .AsInt32()
            .Nullable()
            .AddColumn("PlataNB")
            .AsInt32()
            .Nullable()
            .AddColumn("OldActivityTag")
            .AsString(64)
            .Nullable()
            .AddColumn("GroupNames")
            .AsString(255)
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("Semester").FromTable("ActivityStudents");
        Delete.Column("PlataNB").FromTable("ActivityStudents");
        Delete.Column("OldActivityTag").FromTable("ActivityStudents");
        Delete.Column("GroupNames").FromTable("ActivityStudents");
    }
}
