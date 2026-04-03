using FluentMigrator;

namespace FisaActivitateZilnicaApi.Data.Master.Migrations;

[Migration(20260309007)]
public class CreateRoomsTable : Migration
{
    public override void Up()
    {
        Create.Table("Rooms")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("Capacity").AsInt32().Nullable()
            .WithColumn("Virtual").AsBoolean().NotNullable()
            .WithColumn("BuildingId").AsInt32().Nullable().ForeignKey("FK_Rooms_Buildings", "Buildings", "Id")
            .WithColumn("Comments").AsString(2000).Nullable();
    }

    public override void Down() => Delete.Table("Rooms");
}
