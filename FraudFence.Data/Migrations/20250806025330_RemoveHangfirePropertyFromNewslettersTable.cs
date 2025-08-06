using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudFence.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHangfirePropertyFromNewslettersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "Newsletters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "Newsletters",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
