using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hangman.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    Username = table.Column<string>(maxLength: 100, nullable: false),
                    Password = table.Column<string>(maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(maxLength: 100, nullable: false),
                    LastName = table.Column<string>(maxLength: 100, nullable: false),
                    Role = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuessWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    Value = table.Column<string>(maxLength: 100, nullable: false),
                    GameRoomId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuessWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuessWords_GameRooms_GameRoomId",
                        column: x => x.GameRoomId,
                        principalTable: "GameRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameRoomUsers",
                columns: table => new
                {
                    GameRoomId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    IsHost = table.Column<bool>(nullable: false),
                    IsInRoom = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRoomUsers", x => new { x.GameRoomId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GameRoomUsers_GameRooms_GameRoomId",
                        column: x => x.GameRoomId,
                        principalTable: "GameRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameRoomUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameRound",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    Health = table.Column<int>(nullable: false),
                    IsOver = table.Column<bool>(nullable: false),
                    GuessWordId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRound", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameRound_GuessWords_GuessWordId",
                        column: x => x.GuessWordId,
                        principalTable: "GuessWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuessLetters",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    Value = table.Column<string>(maxLength: 1, nullable: false),
                    GuessWordId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuessLetters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuessLetters_GuessWords_GuessWordId",
                        column: x => x.GuessWordId,
                        principalTable: "GuessWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameRooms_Name",
                table: "GameRooms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameRoomUsers_UserId",
                table: "GameRoomUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRound_GuessWordId",
                table: "GameRound",
                column: "GuessWordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuessLetters_GuessWordId",
                table: "GuessLetters",
                column: "GuessWordId");

            migrationBuilder.CreateIndex(
                name: "IX_GuessLetters_Value_GuessWordId",
                table: "GuessLetters",
                columns: new[] { "Value", "GuessWordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuessWords_GameRoomId",
                table: "GuessWords",
                column: "GameRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameRoomUsers");

            migrationBuilder.DropTable(
                name: "GameRound");

            migrationBuilder.DropTable(
                name: "GuessLetters");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "GuessWords");

            migrationBuilder.DropTable(
                name: "GameRooms");
        }
    }
}
