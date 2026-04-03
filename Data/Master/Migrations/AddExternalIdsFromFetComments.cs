using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309018)]
public class AddExternalIdsFromFetComments : Migration
{
    public override void Up()
    {
        Alter.Table("Teachers").AddColumn("ExternalTeacherId").AsInt32().Nullable();
        Alter.Table("Subjects").AddColumn("ExternalSubjectId").AsInt32().Nullable();

        Alter.Table("ActivityStudents").AddColumn("PlanMatterProviderExternalId").AsInt32().Nullable();
        Alter.Table("ActivityStudents").AddColumn("FacultyExternalId").AsInt32().Nullable();
        Alter.Table("ActivityStudents").AddColumn("MetaSpecializationExternalId").AsInt32().Nullable();
        Alter.Table("ActivityStudents").AddColumn("StudyYearNumber").AsInt32().Nullable();
        Alter.Table("ActivityStudents").AddColumn("GroupExternalId").AsString(255).Nullable();
        Alter.Table("ActivityStudents").AddColumn("SpecializationExternalId").AsInt32().Nullable();
        Alter.Table("ActivityStudents").AddColumn("SubjectExternalId").AsInt32().Nullable();
    }

    public override void Down()
    {
        Delete.Column("ExternalTeacherId").FromTable("Teachers");
        Delete.Column("ExternalSubjectId").FromTable("Subjects");

        Delete.Column("PlanMatterProviderExternalId").FromTable("ActivityStudents");
        Delete.Column("FacultyExternalId").FromTable("ActivityStudents");
        Delete.Column("MetaSpecializationExternalId").FromTable("ActivityStudents");
        Delete.Column("StudyYearNumber").FromTable("ActivityStudents");
        Delete.Column("GroupExternalId").FromTable("ActivityStudents");
        Delete.Column("SpecializationExternalId").FromTable("ActivityStudents");
        Delete.Column("SubjectExternalId").FromTable("ActivityStudents");
    }
}
