using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260419001)]
public class RefactorSchedulesToYearsAndSemesters : Migration
{
    public override void Up()
    {
        Create
            .Table("ScheduleYears")
            .WithColumn("Id")
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn("Value")
            .AsInt32()
            .NotNullable();

        Create.UniqueConstraint("UQ_ScheduleYears_Value").OnTable("ScheduleYears").Column("Value");

        Create
            .Table("ScheduleSemesters")
            .WithColumn("Id")
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn("ScheduleYearId")
            .AsInt32()
            .NotNullable()
            .ForeignKey("FK_ScheduleSemesters_ScheduleYears", "ScheduleYears", "Id")
            .WithColumn("Number")
            .AsInt32()
            .NotNullable()
            .WithColumn("StartDate")
            .AsDate()
            .NotNullable()
            .WithColumn("EndDate")
            .AsDate()
            .NotNullable();

        Create
            .UniqueConstraint("UQ_ScheduleSemesters_ScheduleYearId_Number")
            .OnTable("ScheduleSemesters")
            .Columns("ScheduleYearId", "Number");

        Alter.Table("Schedules").AddColumn("ScheduleSemesterId").AsInt32().Nullable();

        Execute.Sql(
            """
            INSERT INTO ScheduleYears ([Value])
            SELECT DISTINCT s.[Year]
            FROM Schedules s
            LEFT JOIN ScheduleYears sy ON sy.[Value] = s.[Year]
            WHERE sy.Id IS NULL;
            """
        );

        Execute.Sql(
            """
            INSERT INTO ScheduleSemesters (ScheduleYearId, [Number], StartDate, EndDate)
            SELECT DISTINCT sy.Id, s.Semester, '2000-01-01', '2000-01-01'
            FROM Schedules s
            INNER JOIN ScheduleYears sy ON sy.[Value] = s.[Year]
            LEFT JOIN ScheduleSemesters ss
                ON ss.ScheduleYearId = sy.Id
                AND ss.[Number] = s.Semester
            WHERE ss.Id IS NULL;
            """
        );

        Execute.Sql(
            """
            UPDATE s
            SET s.ScheduleSemesterId = ss.Id
            FROM Schedules s
            INNER JOIN ScheduleYears sy ON sy.[Value] = s.[Year]
            INNER JOIN ScheduleSemesters ss
                ON ss.ScheduleYearId = sy.Id
                AND ss.[Number] = s.Semester;
            """
        );

        Alter.Table("Schedules")
            .AlterColumn("ScheduleSemesterId")
            .AsInt32()
            .NotNullable()
            .ForeignKey("FK_Schedules_ScheduleSemesters", "ScheduleSemesters", "Id");

        Delete.UniqueConstraint("UQ_Schedules_Year_Semester_OddWeek").FromTable("Schedules");

        Delete.Column("Year").FromTable("Schedules");
        Delete.Column("Semester").FromTable("Schedules");

        Create
            .UniqueConstraint("UQ_Schedules_ScheduleSemesterId_OddWeek")
            .OnTable("Schedules")
            .Columns("ScheduleSemesterId", "OddWeek");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_Schedules_ScheduleSemesterId_OddWeek").FromTable("Schedules");

        Alter.Table("Schedules").AddColumn("Year").AsInt32().Nullable();
        Alter.Table("Schedules").AddColumn("Semester").AsInt32().Nullable();

        Execute.Sql(
            """
            UPDATE s
            SET
                s.[Year] = sy.[Value],
                s.Semester = ss.[Number]
            FROM Schedules s
            INNER JOIN ScheduleSemesters ss ON ss.Id = s.ScheduleSemesterId
            INNER JOIN ScheduleYears sy ON sy.Id = ss.ScheduleYearId;
            """
        );

        Alter.Table("Schedules").AlterColumn("Year").AsInt32().NotNullable();
        Alter.Table("Schedules").AlterColumn("Semester").AsInt32().NotNullable();

        Delete.ForeignKey("FK_Schedules_ScheduleSemesters").OnTable("Schedules");
        Delete.Column("ScheduleSemesterId").FromTable("Schedules");

        Create
            .UniqueConstraint("UQ_Schedules_Year_Semester_OddWeek")
            .OnTable("Schedules")
            .Columns("Year", "Semester", "OddWeek");

        Delete.UniqueConstraint("UQ_ScheduleSemesters_ScheduleYearId_Number")
            .FromTable("ScheduleSemesters");
        Delete.ForeignKey("FK_ScheduleSemesters_ScheduleYears").OnTable("ScheduleSemesters");
        Delete.Table("ScheduleSemesters");

        Delete.UniqueConstraint("UQ_ScheduleYears_Value").FromTable("ScheduleYears");
        Delete.Table("ScheduleYears");
    }
}
