using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
namespace DB_Analyzer
{
    abstract class DBTool
    {
        public string? ConnectionString { get; set; }
        public DBTool(string? connectionString)
        {
            
            ConnectionString = connectionString;
        }
        public abstract List<string> GetDatabases();
        public abstract Task<List<string>> GetDatabasesAsync();
    }

    class SQLServerTool :DBTool
    {
        public SQLServerTool(string? connectionString) :base(connectionString)
        {}
        public override List<string> GetDatabases()
        {
            if (ConnectionString == null)
                throw new Exception("connection string is null");
            List<string> databases = new List<string>();    
            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    databases = new List<string>();
                    string query = @$"SELECT name " +
                                    @$"FROM sys.databases " +
                                    @$"WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') " +
                                    @$"AND name NOT LIKE 'resource%';";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader =  cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            databases.Add(reader.GetFieldValue<string>(0));
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw;
                }
            }
            return databases;
        }
        public override async Task<List<string>> GetDatabasesAsync()
        {
            if (ConnectionString == null)
                throw new Exception("Connection string is null");

            List<string> databases = new List<string>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    string query = @"SELECT name " +
                                   @"FROM sys.databases " +
                                   @"WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') " +
                                   @"AND name NOT LIKE 'resource%';";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            databases.Add(reader.GetFieldValue<string>(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return databases;
        }
    }
    class MySQLTool : DBTool
    {
        public MySQLTool(string? connectionString) : base(connectionString)
        {}
        public override List<string> GetDatabases()
        {
            if (ConnectionString == null) throw new Exception("connection string is null");
            List<string> databases = new List<string>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    databases = new List<string>();
                    string query = @$"SHOW DATABASES " +
                                    @$"WHERE `Database` NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys');";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            databases.Add(reader.GetFieldValue<string>(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return databases;
        }
        public override async Task<List<string>> GetDatabasesAsync()
        {
            if (ConnectionString == null)
                throw new Exception("Connection string is null");

            List<string> databases = new List<string>();

            using (var conn = new MySqlConnection (ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    string querry = @$"SHOW DATABASES " +
                                    @$"WHERE `Database` NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys');";

                    using (var cmd = new MySqlCommand(querry, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            databases.Add(reader.GetFieldValue<string>(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return databases;
        }
    }
}
