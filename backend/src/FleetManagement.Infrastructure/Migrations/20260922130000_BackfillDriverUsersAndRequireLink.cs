using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagement.Infrastructure.Migrations;

public partial class BackfillDriverUsersAndRequireLink : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO "Users" ("Id", "FirstName", "LastName", "Email", "PasswordHash", "IsActive", "CreatedAt", "IsDeleted")
            SELECT d."UserId", d."FirstName", d."LastName",
                   COALESCE(NULLIF(d."Email", ''), 'legacy-driver-' || d."Id" || '@fleetos.local'),
                   'PBKDF2-SHA256$120000$AQIDBAUGBwgJCgsMDQ4PEA==$A0NeQ0/vLS0lWIsdgU8DW2UO253y3Z6j+bRX2yjRvxY=',
                   TRUE, NOW(), FALSE
            FROM "Drivers" d
            WHERE d."UserId" IS NULL;

            INSERT INTO "UserRoles" ("UserId", "RoleId")
            SELECT d."UserId", r."Id"
            FROM "Drivers" d CROSS JOIN "Roles" r
            WHERE r."Name" = 'Driver' AND d."UserId" IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM "UserRoles" ur WHERE ur."UserId" = d."UserId" AND ur."RoleId" = r."Id");

            ALTER TABLE "Drivers" ALTER COLUMN "UserId" SET NOT NULL;
            ALTER TABLE "Drivers" DROP CONSTRAINT IF EXISTS "FK_Drivers_Users_UserId";
            ALTER TABLE "Drivers" ADD CONSTRAINT "FK_Drivers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Drivers_UserId" ON "Drivers" ("UserId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("ALTER TABLE \"Drivers\" ALTER COLUMN \"UserId\" DROP NOT NULL;");
}