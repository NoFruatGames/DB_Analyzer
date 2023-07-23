using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using MySql.Data.MySqlClient;
namespace DB_Analyzer.DB_Tools
{
    class MySQLTool : DBTool
    {
        public MySQLTool(string? connectionString) : base(connectionString)
        { }
        public override string SelectedDatabase
        {
            get
            {
                var builder = new MySqlConnectionStringBuilder(ConnectionString);
                return builder.Database;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var builder = new MySqlConnectionStringBuilder(ConnectionString);
                    builder.Database = value;
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

            using (var conn = new MySqlConnection(ConnectionString))
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
