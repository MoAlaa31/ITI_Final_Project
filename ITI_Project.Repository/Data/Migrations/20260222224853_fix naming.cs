using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI_Project.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class fixnaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminActionLogs_Users_AdminId",
                table: "AdminActionLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReactions_Users_UserId",
                table: "CommentReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_Posts_PostId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_Users_UserId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Users_UserId",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_ReporterId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_ResolverId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_TargetUserId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Users_UserId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhoneNumbers_Users_UserId",
                table: "UserPhoneNumbers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostReactions",
                table: "PostReactions");

            migrationBuilder.DropIndex(
                name: "IX_PostReactions_UserId",
                table: "PostReactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PostReactions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Providers",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Providers_UserId",
                table: "Providers",
                newName: "IX_Providers_ClientId");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "PostReactions",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReactions_PostId",
                table: "PostReactions",
                newName: "IX_PostReactions_ClientId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Comments",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                newName: "IX_Comments_ClientId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CommentReactions",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentReactions_UserId",
                table: "CommentReactions",
                newName: "IX_CommentReactions_ClientId");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Providers",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostReactions",
                table: "PostReactions",
                columns: new[] { "ServicePostId", "ClientId" });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    PictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AdminActionLogs_Clients_AdminId",
                table: "AdminActionLogs",
                column: "AdminId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReactions_Clients_ClientId",
                table: "CommentReactions",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Clients_ClientId",
                table: "Comments",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_Clients_ClientId",
                table: "PostReactions",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_Posts_ServicePostId",
                table: "PostReactions",
                column: "ServicePostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Clients_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Clients_ClientId",
                table: "Providers",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Clients_ReporterId",
                table: "Reports",
                column: "ReporterId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Clients_ResolverId",
                table: "Reports",
                column: "ResolverId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Clients_TargetUserId",
                table: "Reports",
                column: "TargetUserId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Clients_UserId",
                table: "ServiceRequests",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminActionLogs_Clients_AdminId",
                table: "AdminActionLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReactions_Clients_ClientId",
                table: "CommentReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Clients_ClientId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_Clients_ClientId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_Posts_ServicePostId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Clients_UserId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Clients_ClientId",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Clients_ReporterId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Clients_ResolverId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Clients_TargetUserId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Clients_UserId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhoneNumbers_Clients_UserId",
                table: "UserPhoneNumbers");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostReactions",
                table: "PostReactions");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Providers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Providers_ClientId",
                table: "Providers",
                newName: "IX_Providers_UserId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "PostReactions",
                newName: "PostId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReactions_ClientId",
                table: "PostReactions",
                newName: "IX_PostReactions_PostId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Comments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ClientId",
                table: "Comments",
                newName: "IX_Comments_UserId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "CommentReactions",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentReactions_ClientId",
                table: "CommentReactions",
                newName: "IX_CommentReactions_UserId");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Providers",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PostReactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostReactions",
                table: "PostReactions",
                columns: new[] { "ServicePostId", "UserId" });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    PictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostReactions_UserId",
                table: "PostReactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminActionLogs_Users_AdminId",
                table: "AdminActionLogs",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReactions_Users_UserId",
                table: "CommentReactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_Posts_PostId",
                table: "PostReactions",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_Users_UserId",
                table: "PostReactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Users_UserId",
                table: "Providers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_ReporterId",
                table: "Reports",
                column: "ReporterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_ResolverId",
                table: "Reports",
                column: "ResolverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_TargetUserId",
                table: "Reports",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Users_UserId",
                table: "ServiceRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhoneNumbers_Users_UserId",
                table: "UserPhoneNumbers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
