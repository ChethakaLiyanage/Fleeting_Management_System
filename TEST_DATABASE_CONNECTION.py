#!/usr/bin/env python3
"""
Test script to verify PostgreSQL connection for Fleet Management System
Run this to check if your PostgreSQL is properly configured before starting the .NET application
"""

import psycopg2
import sys
from psycopg2.extras import RealDictCursor

def test_postgresql_connection():
    """Test connection to PostgreSQL database"""

    # Connection parameters matching appsettings.json
    conn_params = {
        'host': 'localhost',
        'port': 5432,
        'database': 'fleet_management',
        'user': 'postgres',
        'password': 'Chethaka123#'
    }

    print("Testing PostgreSQL connection...")
    print(f"Host: {conn_params['host']}")
    print(f"Port: {conn_params['port']}")
    print(f"Database: {conn_params['database']}")
    print(f"User: {conn_params['user']}")
    print()

    try:
        # Attempt to connect
        conn = psycopg2.connect(**conn_params)
        print("✅ Successfully connected to PostgreSQL!")

        # Test basic query
        with conn.cursor(cursor_factory=RealDictCursor) as cursor:
            cursor.execute("SELECT version();")
            version = cursor.fetchone()
            print(f"✅ PostgreSQL version: {version['version']}")

            # Check if our database exists
            cursor.execute("SELECT datname FROM pg_database WHERE datname = %s;",
                         (conn_params['database'],))
            db_exists = cursor.fetchone()
            if db_exists:
                print(f"✅ Database '{conn_params['database']}' exists")
            else:
                print(f"❌ Database '{conn_params['database']}' does not exist")
                print("   Run: CREATE DATABASE fleet_management;")

        conn.close()
        return True

    except psycopg2.OperationalError as e:
        print(f"❌ Failed to connect to PostgreSQL: {e}")
        print("\n🔧 Troubleshooting tips:")
        print("   1. Ensure PostgreSQL service is running")
        print("   2. Check if PostgreSQL is listening on localhost:5432")
        print("   3. Verify username/password are correct")
        print("   4. Check firewall settings")
        return False

    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        return False

def test_database_tables():
    """Check if required tables exist"""
    conn_params = {
        'host': 'localhost',
        'port': 5432,
        'database': 'fleet_management',
        'user': 'postgres',
        'password': 'Chethaka123#'
    }

    try:
        conn = psycopg2.connect(**conn_params)
        with conn.cursor(cursor_factory=RealDictCursor) as cursor:
            # Check for key tables from our application
            tables_to_check = [
                'users', 'roles', 'userroles',
                'vehicles', 'drivers', 'trips',
                'maintenance_records', 'fuel_records',
                'incidents', 'inspections'
            ]

            print("\nChecking for required tables:")
            for table in tables_to_check:
                cursor.execute("""
                    SELECT EXISTS (
                        SELECT FROM information_schema.tables
                        WHERE table_schema = 'public'
                        AND table_name = %s
                    );
                """, (table,))
                exists = cursor.fetchone()['exists']
                status = "✅" if exists else "❌"
                print(f"   {status} Table '{table}': {'exists' if exists else 'missing'}")

        conn.close()
        return True

    except Exception as e:
        print(f"❌ Error checking tables: {e}")
        return False

if __name__ == "__main__":
    print("=" * 60)
    print("Fleet Management System - PostgreSQL Connection Test")
    print("=" * 60)

    success = test_postgresql_connection()

    if success:
        test_database_tables()
        print("\n" + "=" * 60)
        print("✅ All tests passed! Your PostgreSQL is ready for the Fleet Management System.")
        print("🚀 You can now run: dotnet run (in backend/src/FleetManagement.API)")
        print("=" * 60)
        sys.exit(0)
    else:
        print("\n" + "=" * 60)
        print("❌ Connection test failed. Please fix PostgreSQL setup before running the application.")
        print("📖 See POSTGRESQL_SETUP_GUIDE.md for detailed instructions.")
        print("=" * 60)
        sys.exit(1)