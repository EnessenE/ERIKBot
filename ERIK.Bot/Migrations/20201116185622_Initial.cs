using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERIK.Bot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Guilds",
                table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    IconSupport = table.Column<bool>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Guilds", x => x.Id); });

            migrationBuilder.CreateTable(
                "Icon",
                table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Image = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Default = table.Column<bool>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Recurring = table.Column<bool>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Icon", x => x.Id);
                    table.ForeignKey(
                        "FK_Icon_Guilds_GuildId",
                        x => x.GuildId,
                        "Guilds",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_Icon_GuildId",
                "Icon",
                "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Icon");

            migrationBuilder.DropTable(
                "Guilds");
        }
    }
}