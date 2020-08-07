using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERIK.Bot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordUsers",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    LfgPrepublishChannelId = table.Column<decimal>(nullable: false),
                    LfgPublishChannelId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedMessages",
                columns: table => new
                {
                    MessageId = table.Column<decimal>(nullable: false),
                    IsFinished = table.Column<bool>(nullable: false),
                    Published = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedMessages", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "MessageReaction",
                columns: table => new
                {
                    Guid = table.Column<Guid>(nullable: false),
                    UserId = table.Column<decimal>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    SavedMessageMessageId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReaction", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_MessageReaction_SavedMessages_SavedMessageMessageId",
                        column: x => x.SavedMessageMessageId,
                        principalTable: "SavedMessages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessageReaction_DiscordUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageReaction_SavedMessageMessageId",
                table: "MessageReaction",
                column: "SavedMessageMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReaction_UserId",
                table: "MessageReaction",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "MessageReaction");

            migrationBuilder.DropTable(
                name: "SavedMessages");

            migrationBuilder.DropTable(
                name: "DiscordUsers");
        }
    }
}
