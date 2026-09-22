using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RequireDriverUserLink : Migration
    {
        // Legacy rows receive a unique legacy-driver email and the temporary password FleetTemp123!.
        // Administrators should rotate these credentials after the migration completes.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TEMP TABLE legacy_driver_users AS
                SELECT "Id" AS driver_id, gen_random_uuid() AS user_id
                FROM "Drivers"
                WHERE "UserId" IS NULL;

                INSERT INTO "Users" ("Id", "FirstName", "LastName", "Email", "PasswordHash", "IsActive", "CreatedAt", "IsDeleted")
                SELECT l.user_id, d."FirstName", d."LastName",
                       'legacy-driver-' || d."Id" || '@fleetos.local',
                       'PBKDF2-SHA256$120000$AQIDBAUGBwgJCgsMDQ4PEA==$A0NeQ0/vLS0lWIsdgU8DW2UO253y3Z6j+bRX2yjRvxY=',
                       TRUE, NOW(), FALSE
                FROM legacy_driver_users l JOIN "Drivers" d ON d."Id" = l.driver_id;

                UPDATE "Drivers" d SET "UserId" = l.user_id
                FROM legacy_driver_users l WHERE d."Id" = l.driver_id;

                INSERT INTO "UserRoles" ("UserId", "RoleId")
                SELECT l.user_id, r."Id"
                FROM legacy_driver_users l CROSS JOIN "Roles" r
                WHERE r."Name" = 'Driver';
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Users_UserId",
                table: "Drivers");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Drivers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Users_UserId",
                table: "Drivers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Users_UserId",
                table: "Drivers");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Drivers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "VehicleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UnassignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleAssignments_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleAssignments_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignments_AssignedAt",
                table: "VehicleAssignments",
                column: "AssignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignments_DriverId",
                table: "VehicleAssignments",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignments_Status",
                table: "VehicleAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignments_VehicleId",
                table: "VehicleAssignments",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Users_UserId",
                table: "Drivers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
