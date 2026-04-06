using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class addservicetoserviceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ServiceId",
                table: "ServiceRequests",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Services_ServiceId",
                table: "ServiceRequests",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Services_ServiceId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ServiceId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "ServiceRequests");
        }
    }
}
