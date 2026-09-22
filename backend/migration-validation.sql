CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "FirstName" character varying(50) NOT NULL,
        "LastName" character varying(50) NOT NULL,
        "Email" character varying(100) NOT NULL,
        "PasswordHash" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "Vehicles" (
        "Id" uuid NOT NULL,
        "RegistrationNumber" character varying(50) NOT NULL,
        "VIN" character varying(100),
        "EngineNumber" character varying(100) NOT NULL,
        "Make" character varying(100) NOT NULL,
        "Model" character varying(100) NOT NULL,
        "Year" integer NOT NULL,
        "VehicleType" integer NOT NULL,
        "FuelType" integer NOT NULL,
        "Transmission" integer NOT NULL,
        "Color" character varying(50) NOT NULL,
        "Mileage" numeric(18,2) NOT NULL,
        "Status" integer NOT NULL,
        "PurchaseDate" timestamp with time zone,
        "PurchasePrice" numeric(18,2),
        "RegistrationExpiry" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Vehicles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "Action" integer NOT NULL,
        "EntityType" character varying(100) NOT NULL,
        "EntityId" character varying(100) NOT NULL,
        "Changes" text,
        "IpAddress" character varying(50),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuditLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "Drivers" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "EmployeeNumber" character varying(50) NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "Phone" character varying(30) NOT NULL,
        "Email" character varying(150),
        "Address" character varying(250) NOT NULL,
        "LicenseNumber" character varying(50) NOT NULL,
        "LicenseClass" character varying(50) NOT NULL,
        "LicenseIssueDate" timestamp with time zone NOT NULL,
        "LicenseExpiry" timestamp with time zone NOT NULL,
        "Status" integer NOT NULL,
        "JoinDate" timestamp with time zone NOT NULL,
        "EmergencyContact" character varying(100) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Drivers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Drivers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "Notifications" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Type" integer NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Message" character varying(1000) NOT NULL,
        "ReferenceId" character varying(100),
        "ReferenceType" character varying(100),
        "IsRead" boolean NOT NULL,
        "ReadAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" text NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "UserRoles" (
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        CONSTRAINT "PK_UserRoles" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "MaintenanceRecords" (
        "Id" uuid NOT NULL,
        "VehicleId" uuid NOT NULL,
        "Type" integer NOT NULL,
        "Status" integer NOT NULL,
        "Description" character varying(500) NOT NULL,
        "ServiceProvider" character varying(200),
        "ScheduledDate" timestamp with time zone NOT NULL,
        "CompletedDate" timestamp with time zone,
        "OdometerReading" numeric(12,2),
        "Cost" numeric(12,2),
        "NextServiceOdometer" numeric(12,2),
        "NextServiceDate" timestamp with time zone,
        "Notes" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_MaintenanceRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MaintenanceRecords_Vehicles_VehicleId" FOREIGN KEY ("VehicleId") REFERENCES "Vehicles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "FuelRecords" (
        "Id" uuid NOT NULL,
        "VehicleId" uuid NOT NULL,
        "DriverId" uuid,
        "FuelDate" timestamp with time zone NOT NULL,
        "Litres" numeric(10,2) NOT NULL,
        "CostPerLitre" numeric(10,4) NOT NULL,
        "OdometerReading" numeric(12,2) NOT NULL,
        "FuelType" integer NOT NULL,
        "Station" text,
        "Notes" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_FuelRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FuelRecords_Drivers_DriverId" FOREIGN KEY ("DriverId") REFERENCES "Drivers" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_FuelRecords_Vehicles_VehicleId" FOREIGN KEY ("VehicleId") REFERENCES "Vehicles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "Trips" (
        "Id" uuid NOT NULL,
        "TripNumber" character varying(50) NOT NULL,
        "VehicleId" uuid NOT NULL,
        "DriverId" uuid NOT NULL,
        "StartLocation" character varying(200) NOT NULL,
        "Destination" character varying(200) NOT NULL,
        "StartTime" timestamp with time zone,
        "EndTime" timestamp with time zone,
        "StartingMileage" numeric(18,2),
        "EndingMileage" numeric(18,2),
        "Distance" numeric(18,2),
        "Purpose" character varying(200) NOT NULL,
        "Status" integer NOT NULL,
        "Notes" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Trips" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Trips_Drivers_DriverId" FOREIGN KEY ("DriverId") REFERENCES "Drivers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Trips_Vehicles_VehicleId" FOREIGN KEY ("VehicleId") REFERENCES "Vehicles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "VehicleAssignments" (
        "Id" uuid NOT NULL,
        "VehicleId" uuid NOT NULL,
        "DriverId" uuid NOT NULL,
        "AssignedAt" timestamp with time zone NOT NULL,
        "UnassignedAt" timestamp with time zone,
        "Status" integer NOT NULL,
        "AssignedByUserId" uuid,
        "Notes" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_VehicleAssignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_VehicleAssignments_Drivers_DriverId" FOREIGN KEY ("DriverId") REFERENCES "Drivers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_VehicleAssignments_Vehicles_VehicleId" FOREIGN KEY ("VehicleId") REFERENCES "Vehicles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "Incidents" (
        "Id" uuid NOT NULL,
        "VehicleId" uuid NOT NULL,
        "DriverId" uuid,
        "TripId" uuid,
        "Date" timestamp with time zone NOT NULL,
        "Location" character varying(200) NOT NULL,
        "Type" integer NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "Severity" integer NOT NULL,
        "PoliceReportNumber" character varying(100),
        "InsuranceClaimNumber" character varying(100),
        "EstimatedDamage" numeric(18,2),
        "ActualRepairCost" numeric(18,2),
        "Status" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Incidents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Incidents_Drivers_DriverId" FOREIGN KEY ("DriverId") REFERENCES "Drivers" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Incidents_Trips_TripId" FOREIGN KEY ("TripId") REFERENCES "Trips" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Incidents_Vehicles_VehicleId" FOREIGN KEY ("VehicleId") REFERENCES "Vehicles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "Inspections" (
        "Id" uuid NOT NULL,
        "VehicleId" uuid NOT NULL,
        "DriverId" uuid,
        "TripId" uuid,
        "Type" integer NOT NULL,
        "InspectionDate" timestamp with time zone NOT NULL,
        "Result" integer NOT NULL,
        "Notes" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Inspections" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Inspections_Drivers_DriverId" FOREIGN KEY ("DriverId") REFERENCES "Drivers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Inspections_Trips_TripId" FOREIGN KEY ("TripId") REFERENCES "Trips" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Inspections_Vehicles_VehicleId" FOREIGN KEY ("VehicleId") REFERENCES "Vehicles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE TABLE "InspectionItems" (
        "Id" uuid NOT NULL,
        "InspectionId" uuid NOT NULL,
        "ItemName" character varying(100) NOT NULL,
        "Status" integer NOT NULL,
        "Notes" character varying(250),
        CONSTRAINT "PK_InspectionItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_InspectionItems_Inspections_InspectionId" FOREIGN KEY ("InspectionId") REFERENCES "Inspections" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Drivers_EmployeeNumber" ON "Drivers" ("EmployeeNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Drivers_LicenseExpiry" ON "Drivers" ("LicenseExpiry");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Drivers_LicenseNumber" ON "Drivers" ("LicenseNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Drivers_Status" ON "Drivers" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Drivers_UserId" ON "Drivers" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_FuelRecords_DriverId" ON "FuelRecords" ("DriverId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_FuelRecords_VehicleId_FuelDate" ON "FuelRecords" ("VehicleId", "FuelDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Incidents_Date" ON "Incidents" ("Date");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Incidents_DriverId" ON "Incidents" ("DriverId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Incidents_Severity" ON "Incidents" ("Severity");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Incidents_Status" ON "Incidents" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Incidents_TripId" ON "Incidents" ("TripId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Incidents_VehicleId" ON "Incidents" ("VehicleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_InspectionItems_InspectionId" ON "InspectionItems" ("InspectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_InspectionItems_Status" ON "InspectionItems" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Inspections_DriverId" ON "Inspections" ("DriverId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Inspections_InspectionDate" ON "Inspections" ("InspectionDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Inspections_Result" ON "Inspections" ("Result");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Inspections_TripId" ON "Inspections" ("TripId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Inspections_VehicleId" ON "Inspections" ("VehicleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_MaintenanceRecords_Status_ScheduledDate" ON "MaintenanceRecords" ("Status", "ScheduledDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_MaintenanceRecords_VehicleId_ScheduledDate" ON "MaintenanceRecords" ("VehicleId", "ScheduledDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Trips_DriverId_StartTime" ON "Trips" ("DriverId", "StartTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Trips_StartTime" ON "Trips" ("StartTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Trips_Status" ON "Trips" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Trips_TripNumber" ON "Trips" ("TripNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Trips_VehicleId_StartTime" ON "Trips" ("VehicleId", "StartTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_VehicleAssignments_AssignedAt" ON "VehicleAssignments" ("AssignedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_VehicleAssignments_DriverId" ON "VehicleAssignments" ("DriverId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_VehicleAssignments_Status" ON "VehicleAssignments" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_VehicleAssignments_VehicleId" ON "VehicleAssignments" ("VehicleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Vehicles_RegistrationExpiry" ON "Vehicles" ("RegistrationExpiry");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Vehicles_RegistrationNumber" ON "Vehicles" ("RegistrationNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE INDEX "IX_Vehicles_Status" ON "Vehicles" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Vehicles_VIN" ON "Vehicles" ("VIN") WHERE "VIN" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922111107_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260922111107_InitialCreate', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922130053_RequireDriverUserLink') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922130053_RequireDriverUserLink') THEN
    ALTER TABLE "Drivers" DROP CONSTRAINT "FK_Drivers_Users_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922130053_RequireDriverUserLink') THEN
    ALTER TABLE "Drivers" ALTER COLUMN "UserId" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922130053_RequireDriverUserLink') THEN
    ALTER TABLE "Drivers" ADD CONSTRAINT "FK_Drivers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922130053_RequireDriverUserLink') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260922130053_RequireDriverUserLink', '8.0.4');
    END IF;
END $EF$;
COMMIT;

