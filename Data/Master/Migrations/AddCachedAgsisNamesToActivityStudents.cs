using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

// Caches the friendly AGSIS names (resolved at FET import / backfill) onto each
// ActivityStudents row so the schedule-catalog cascade can read them 100% from
// the local DB without touching AGSIS. EffectiveSpecializationExternalId is the
// resolved specialization id (own > group's > shortname hint) — the stable key
// the cascade filters on, since SpecializationExternalId is null for most rows.
[Migration(20260530001)]
public class AddCachedAgsisNamesToActivityStudents : Migration
{
    public override void Up()
    {
        Alter
            .Table("ActivityStudents")
            .AddColumn("FacultyName")
            .AsString(255)
            .Nullable()
            .AddColumn("SpecializationName")
            .AsString(255)
            .Nullable()
            .AddColumn("SubjectName")
            .AsString(255)
            .Nullable()
            .AddColumn("ResolvedGroupName")
            .AsString(255)
            .Nullable()
            .AddColumn("EffectiveSpecializationExternalId")
            .AsInt32()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("FacultyName").FromTable("ActivityStudents");
        Delete.Column("SpecializationName").FromTable("ActivityStudents");
        Delete.Column("SubjectName").FromTable("ActivityStudents");
        Delete.Column("ResolvedGroupName").FromTable("ActivityStudents");
        Delete.Column("EffectiveSpecializationExternalId").FromTable("ActivityStudents");
    }
}
