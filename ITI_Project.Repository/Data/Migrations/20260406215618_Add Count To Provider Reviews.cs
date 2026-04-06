using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCountToProviderReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RatingSum",
                table: "Providers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ReviewsCount",
                table: "Providers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingSum",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ReviewsCount",
                table: "Providers");
        }
    }
}
