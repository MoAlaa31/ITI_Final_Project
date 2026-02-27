using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class changethelastusertoclient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Clients_UserId",
                table: "ServiceRequests");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ServiceRequests",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceRequests_UserId",
                table: "ServiceRequests",
                newName: "IX_ServiceRequests_ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Clients_ClientId",
                table: "ServiceRequests",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Clients_ClientId",
                table: "ServiceRequests");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "ServiceRequests",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceRequests_ClientId",
                table: "ServiceRequests",
                newName: "IX_ServiceRequests_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Clients_UserId",
                table: "ServiceRequests",
                column: "UserId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
