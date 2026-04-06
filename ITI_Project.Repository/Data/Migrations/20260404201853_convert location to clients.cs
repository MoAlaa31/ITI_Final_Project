using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class convertlocationtoclients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_governorates_GovernorateId",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_regions_RegionId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_GovernorateId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_RegionId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "AddressText",
                table: "ServiceRequestLocations");

            migrationBuilder.DropColumn(
                name: "GovernorateId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Providers");

            migrationBuilder.AddColumn<int>(
                name: "GovernorateId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_GovernorateId",
                table: "Clients",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_RegionId",
                table: "Clients",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_governorates_GovernorateId",
                table: "Clients",
                column: "GovernorateId",
                principalTable: "governorates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_regions_RegionId",
                table: "Clients",
                column: "RegionId",
                principalTable: "regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_governorates_GovernorateId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_regions_RegionId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_GovernorateId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_RegionId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "GovernorateId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "AddressText",
                table: "ServiceRequestLocations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GovernorateId",
                table: "Providers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                table: "Providers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_GovernorateId",
                table: "Providers",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_RegionId",
                table: "Providers",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_governorates_GovernorateId",
                table: "Providers",
                column: "GovernorateId",
                principalTable: "governorates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_regions_RegionId",
                table: "Providers",
                column: "RegionId",
                principalTable: "regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
