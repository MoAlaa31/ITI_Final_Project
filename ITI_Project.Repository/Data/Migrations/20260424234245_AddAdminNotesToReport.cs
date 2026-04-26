using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminNotesToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "Reports");
        }
    }
}
