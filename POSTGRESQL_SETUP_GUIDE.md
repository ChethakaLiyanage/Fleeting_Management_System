# PostgreSQL Setup Guide for Fleet Management System

## Prerequisites
To run this application with real PostgreSQL data, you need:

1. **PostgreSQL Server** installed and running
2. **Database** named `fleet_management` created
3. **User** `postgres` with password `Chethaka123#` (or update credentials in appsettings.json)

## Quick Start with Docker (Recommended)
If you don't have PostgreSQL installed, use Docker:

```bash
# Pull and run PostgreSQL
docker run --name fleet-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=Chethaka123# \
  -e POSTGRES_DB=fleet_management \
  -p 5432:5432 \
  -d postgres:15

# Verify it's running
docker ps | grep fleet-postgres
```

## Manual PostgreSQL Installation

### Windows
1. Download PostgreSQL from https://www.postgresql.org/download/windows/
2. Install with default options
3. During installation, set the password for `postgres` user to `Chethaka123#`
4. Use pgAdmin or command line to create database:
   ```sql
   CREATE DATABASE fleet_management;
   ```

### Linux (Ubuntu/Debian)
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Set password for postgres user
sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'Chethaka123#';"

# Create database
sudo -u postgres createdb fleet_management
```

### macOS (using Homebrew)
```bash
brew install postgresql
brew services start postgresql

# Set password for postgres user
psql -U postgres -c "ALTER USER postgres WITH PASSWORD 'Chethaka123#';"

# Create database
createdb fleet_management
```

## Connection String Configuration
The application expects this connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=fleet_management;Username=postgres;Password=Chethaka123#"
  }
}
```

## Troubleshooting

### Common Issues and Solutions:

#### 1. "Connection refused" or "Could not connect to server"
- **Cause**: PostgreSQL service not running or not listening on expected port
- **Solution**: 
  - Check if PostgreSQL service is running
  - Verify it's listening on port 5432: `netstat -tulpn | grep 5432`
  - Start service: `sudo systemctl start postgresql` (Linux) or via Services app (Windows)

#### 2. "Database does not exist"
- **Cause**: `fleet_management` database not created
- **Solution**: 
  - Connect to PostgreSQL: `psql -U postgres`
  - Create database: `CREATE DATABASE fleet_management;`
  - Or use pgAdmin to create the database

#### 3. "Password authentication failed"
- **Cause**: Incorrect username/password
- **Solution**:
  - Verify credentials in `appsettings.json` match PostgreSQL user
  - Reset password if needed: `ALTER USER postgres WITH PASSWORD 'Chethaka123#';`

#### 4. "Permission denied" or "role does not exist"
- **Cause**: User doesn't have sufficient privileges
- **Solution**:
  - Ensure `postgres` user exists and has CREATEDB privilege
  - Or create a specific user: 
    ```sql
    CREATE USER fleetuser WITH PASSWORD 'fleetpass';
    CREATE DATABASE fleet_management OWNER fleetuser;
    GRANT ALL PRIVILEGES ON DATABASE fleet_management TO fleetuser;
    ```
  - Update connection string accordingly

## Verifying Setup
Once PostgreSQL is running and configured:

1. **Test connection**: 
   ```bash
   psql -U postgres -h localhost -d fleet_management -c "SELECT version();"
   ```

2. **Run the application**: 
   ```bash
   cd backend/src/FleetManagement.API
   dotnet run
   ```

3. **Check logs** for successful database connection messages:
   ```
   info: Microsoft.EntityFrameworkCore.Database.Connection[20001]
         Connection opened to Database...
   ```

4. **Verify data seeding**:
   - Check that roles and sample data are inserted
   - API endpoints should return real data instead of empty results

## Application Architecture Notes

### Backend (C# .NET 8)
- Uses Entity Framework Core with Npgsql provider for PostgreSQL
- Automatic database migration/seeding on startup
- Repository pattern with service layer
- JWT authentication for security

### Frontend (TypeScript/React)
- Connects to backend API at `http://localhost:5000` (default)
- Expects authenticated endpoints to work with JWT tokens

### External Services (FastAPI)
- Configured to connect to `http://localhost:8000/health` for health checks
- Used for ML/AI predictions (separate service)

## Development Workflow
1. Ensure PostgreSQL is running
2. Update `appsettings.json` if using different credentials
3. Run `dotnet run` in the API project
4. The application will automatically:
   - Connect to PostgreSQL
   - Apply any pending migrations
   - Seed roles and sample data
   - Start serving API requests

## Production Considerations
For production deployment:
1. Use proper secrets management (Azure Key Vault, AWS Secrets Manager, etc.)
2. Configure connection pooling appropriately
3. Set up monitoring and health checks
4. Consider using managed PostgreSQL services (AWS RDS, Google Cloud SQL, Azure Database for PostgreSQL)
5. Set up backup and disaster recovery procedures