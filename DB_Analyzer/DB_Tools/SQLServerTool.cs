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
        private readonly string createDBSTableQuery = "CREATE TABLE dbs( " +
                                                      "	id int NOT NULL PRIMARY KEY IDENTITY(1,1)," +
                                                      "	server varchar(10) NOT NULL, " +
                                                      "	db_name varchar(50) NOT NULL," +
                                                      "	check_date datetime NOT NULL " +
                                                      ");";
        private readonly string createCommonInfoTableQuery = "CREATE TABLE common_info( " +
                                                             "	id int NOT NULL PRIMARY KEY IDENTITY(1,1), " +
                                                             "	tables_count int NULL, " +
                                                             "	procedures_count int NULL, " +
                                                             "	db_id int NOT NULL, " +
                                                             "	CONSTRAINT FK_common_db FOREIGN KEY(db_id) REFERENCES dbs(id) " +
                                                             ");";
        private readonly string createTablesInfoTableQuery = "CREATE TABLE tables_info( " +
                                                             "	id int NOT NULL PRIMARY KEY IDENTITY(1,1), " +
                                                             "	table_name varchar(50) NOT NULL, " +
                                                             "	row_count int NULL, " +
                                                             "	db_id int NOT NULL," +
                                                             "	CONSTRAINT FK_tables_db FOREIGN KEY(db_id) REFERENCES dbs(id)" +
                                                             ");";
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

        public override async Task<List<string>> GetTablesAsync()
        {
            try
            {
                return await GetFromServerAsync(getTablesQuery);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private void NoReturnQuery(string query)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                        cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        private async void NoReturnQueryAsync(string query)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(query, conn))
                        await cmd.ExecuteNonQueryAsync();

                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public override async void CreateDBSTableAsync()
        {
            try
            {
                NoReturnQueryAsync(createDBSTableQuery);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async void CreateCommonInfoTableAsync()
        {
            try
            {
                NoReturnQueryAsync(createCommonInfoTableQuery);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async void CreateTablesInfoTableAsync()
        {
            try
            {
                NoReturnQueryAsync(createTablesInfoTableQuery);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async Task<bool> CheckTableExistAsync(string tableName)
        {
            if (string.IsNullOrEmpty(SelectedDatabase))
                throw new Exception("databese not selected");
            Int32 res = 0;
            using (var conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    var par = new SqlParameter()
                    {
                        ParameterName = "@TableName",
                        Direction = System.Data.ParameterDirection.Input,
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Size = 50,
                        Value = tableName
                    };
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName;", conn))
                    {
                       cmd.Parameters.Add(par);
                       res = (Int32)await cmd.ExecuteScalarAsync();
                    }

                }
                catch (Exception ex)
                {
                    throw;
                }

            }
            return res != 0;
        }
    }
}
