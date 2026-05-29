using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineMatch.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteIsActiveColumnInSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
