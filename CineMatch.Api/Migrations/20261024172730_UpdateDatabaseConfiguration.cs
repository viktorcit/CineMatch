using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineMatch.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionMovies_Movies_MovieId1",
                table: "SessionMovies");

            migrationBuilder.DropIndex(
                name: "IX_SessionMovies_MovieId1",
                table: "SessionMovies");

            migrationBuilder.DropColumn(
                name: "MovieId1",
                table: "SessionMovies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MovieId1",
                table: "SessionMovies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SessionMovies_MovieId1",
                table: "SessionMovies",
                column: "MovieId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionMovies_Movies_MovieId1",
                table: "SessionMovies",
                column: "MovieId1",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
