using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data.Common;
namespace DB_Analyzer.DB_Tools
{
    class SQLServerTool : DBTool
    {
        public SQLServerTool(string? connectionString) : base(connectionString)
        {
        }
        public override string SelectedDatabase
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString);
                return builder.InitialCatalog;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var builder = new SqlConnectionStringBuilder(ConnectionString);
                    builder.InitialCatalog = value;
                    ConnectionString = builder.ConnectionString;
                }
                else
                {
                    throw new ArgumentException("Database name cannot be empty or null.");
                }
            }
        }
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
}
