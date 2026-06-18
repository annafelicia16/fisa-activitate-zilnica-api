using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

// Caches the study cycle (1 = bachelor, 2 = master, 3 = doctorate) of each
// ActivityStudents row's effective specialization, resolved from AGSIS at FET
// import / backfill alongside the other cached names. Null when undetermined.
[Migration(20260618001)]
public class AddSpecializationCycleToActivityStudents : Migration
{
    public override void Up()
    {
        Alter
            .Table("ActivityStudents")
            .AddColumn("SpecializationCycle")
            .AsInt32()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("SpecializationCycle").FromTable("ActivityStudents");
    }
}
