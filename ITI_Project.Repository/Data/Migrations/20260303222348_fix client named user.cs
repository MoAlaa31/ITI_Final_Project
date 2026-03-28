using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class fixclientnameduser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Clients_UserId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhoneNumbers_Clients_UserId",
                table: "UserPhoneNumbers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RequestOffers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserPhoneNumbers",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhoneNumbers_UserId",
                table: "UserPhoneNumbers",
                newName: "IX_UserPhoneNumbers_ClientId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Posts",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                newName: "IX_Posts_ClientId");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "RequestOffers",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Clients_ClientId",
                table: "Posts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhoneNumbers_Clients_ClientId",
                table: "UserPhoneNumbers",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Clients_ClientId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhoneNumbers_Clients_ClientId",
                table: "UserPhoneNumbers");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "RequestOffers");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "UserPhoneNumbers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhoneNumbers_ClientId",
                table: "UserPhoneNumbers",
                newName: "IX_UserPhoneNumbers_UserId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Posts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_ClientId",
                table: "Posts",
                newName: "IX_Posts_UserId");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RequestOffers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Clients_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhoneNumbers_Clients_UserId",
                table: "UserPhoneNumbers",
                column: "UserId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
