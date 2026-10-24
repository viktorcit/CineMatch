using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineMatch.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Votes_SessionId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_SessionParticipants_SessionId",
                table: "SessionParticipants");

            migrationBuilder.DropIndex(
                name: "IX_SessionMovies_SessionId",
                table: "SessionMovies");

            migrationBuilder.DropColumn(
                name: "ParticipantNumber",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "CreatorClientId",
                table: "Sessions",
                newName: "CreatorUserId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "SessionParticipants",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "ParticipantId",
                table: "Votes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MovieId1",
                table: "SessionMovies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Overview",
                table: "Movies",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ParticipantId",
                table: "Votes",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_SessionId_ParticipantId_MovieId",
                table: "Votes",
                columns: new[] { "SessionId", "ParticipantId", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Code",
                table: "Sessions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CreatorUserId",
                table: "Sessions",
                column: "CreatorUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_SessionId_UserId",
                table: "SessionParticipants",
                columns: new[] { "SessionId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_UserId",
                table: "SessionParticipants",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionMovies_MovieId1",
                table: "SessionMovies",
                column: "MovieId1");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMovies_SessionId_MovieId",
                table: "SessionMovies",
                columns: new[] { "SessionId", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_TMdbId",
                table: "Movies",
                column: "TMdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserName",
                table: "AspNetUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionMovies_Movies_MovieId1",
                table: "SessionMovies",
                column: "MovieId1",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionParticipants_AspNetUsers_UserId",
                table: "SessionParticipants",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_AspNetUsers_CreatorUserId",
                table: "Sessions",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_AspNetUsers_ParticipantId",
                table: "Votes",
                column: "ParticipantId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionMovies_Movies_MovieId1",
                table: "SessionMovies");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionParticipants_AspNetUsers_UserId",
                table: "SessionParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_AspNetUsers_CreatorUserId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_AspNetUsers_ParticipantId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_ParticipantId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_SessionId_ParticipantId_MovieId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_Code",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_CreatorUserId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionParticipants_SessionId_UserId",
                table: "SessionParticipants");

            migrationBuilder.DropIndex(
                name: "IX_SessionParticipants_UserId",
                table: "SessionParticipants");

            migrationBuilder.DropIndex(
                name: "IX_SessionMovies_MovieId1",
                table: "SessionMovies");

            migrationBuilder.DropIndex(
                name: "IX_SessionMovies_SessionId_MovieId",
                table: "SessionMovies");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Movies_TMdbId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ParticipantId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "MovieId1",
                table: "SessionMovies");

            migrationBuilder.RenameColumn(
                name: "CreatorUserId",
                table: "Sessions",
                newName: "CreatorClientId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SessionParticipants",
                newName: "ClientId");

            migrationBuilder.AddColumn<int>(
                name: "ParticipantNumber",
                table: "Votes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "RefreshTokens",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Overview",
                table: "Movies",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_SessionId",
                table: "Votes",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_SessionId",
                table: "SessionParticipants",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMovies_SessionId",
                table: "SessionMovies",
                column: "SessionId");
        }
    }
}
