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
        private readonly string getDatabasesQuery = @$"SELECT name " +
                                                    @$"FROM sys.databases " +
                                                    @$"WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') " +
                                                    @$"AND name NOT LIKE 'resource%';";
        private readonly string getTablesQuery = "SELECT TABLE_NAME " +
                                                 "FROM INFORMATION_SCHEMA.TABLES " +
                                                 "WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE 'sysdiagrams';";
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
            try
            {
                return GetFromServer(getDatabasesQuery);
            }catch(Exception ex)
            {
                throw;
            }
            
        }
        public override async Task<List<string>> GetDatabasesAsync()
        {
            try
            {
                return await GetFromServerAsync(getDatabasesQuery);
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        private List<string> GetFromServer(string query)
        {
            List<string> databases = new List<string>();
            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    databases = new List<string>();
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
        private async Task<List<string>> GetFromServerAsync(string querry)
        {
            List<string> databases = new List<string>();
            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(querry, conn))
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

        public override List<string> GetTables()
        {
            try
            {
                return GetFromServer(getTablesQuery);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override Task<List<string>> GetTablesAsync()
        {
            try
            {
                return GetFromServerAsync(getTablesQuery);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
