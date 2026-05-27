using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampaignApp.Migrations
{
    /// <inheritdoc />
    public partial class TotalDatabaseFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Factions_FactionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "MapRegions");

            migrationBuilder.AlterColumn<int>(
                name: "FactionId",
                table: "Users",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ControllingFactionId = table.Column<int>(type: "INTEGER", nullable: true),
                    ResourceType = table.Column<string>(type: "TEXT", nullable: false),
                    Coordinates = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sectors_Factions_ControllingFactionId",
                        column: x => x.ControllingFactionId,
                        principalTable: "Factions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sectors_ControllingFactionId",
                table: "Sectors",
                column: "ControllingFactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Factions_FactionId",
                table: "Users",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Factions_FactionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Sectors");

            migrationBuilder.AlterColumn<int>(
                name: "FactionId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "MapRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ControllingFactionId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SvgCoordinatesJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapRegions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapRegions_Factions_ControllingFactionId",
                        column: x => x.ControllingFactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapRegions_ControllingFactionId",
                table: "MapRegions",
                column: "ControllingFactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Factions_FactionId",
                table: "Users",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
