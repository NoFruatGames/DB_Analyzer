using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using MySql.Data.MySqlClient;
using DB_Analyzer_dll.DBQueries;
namespace DB_Analyzer_dll
{
    public class DBAnalyzer
    {
        private DBToolsProxy inputDBTool;
        private DBToolsProxy outputDBTool;
        //public DbProviderFactory InputProviderFactory { get { return inputDBTool.Factory; } }
        //public DbProviderFactory OutputProviderFactory { get { return outputDBTool.Factory; } }
        public OutputType OType { get; private set; }
        public InputType IType { get; private set; }
        public void SetInputServer(InputType type, string connectionString)
        {
            if(inputDBTool != null)
                inputDBTool.CloseConnectionAsync();
            if(type == InputType.sql_server)
            {
                IType = InputType.sql_server;
                if (inputDBTool == null || inputDBTool.Type != InputType.sql_server)
                    inputDBTool = new DBToolsProxy(DbProviderFactories.GetFactory(Providers.SqlServerName), new SqlQueries(), connectionString, InputType.sql_server);
                else
                    inputDBTool.ChangeConnectionString(connectionString);
            }
                
            else if(type == InputType.mysql)
            {
                IType = InputType.mysql;
                if (inputDBTool == null || inputDBTool.Type != InputType.mysql)
                    inputDBTool = new DBToolsProxy(DbProviderFactories.GetFactory(Providers.MySqlName), new MySqlQueries(), connectionString, InputType.mysql);
                else
                    inputDBTool.ChangeConnectionString(connectionString);
            }
                
        }
        public void SetOutputType(OutputType type, string outputString)
        {
            if(outputDBTool != null)
                outputDBTool.CloseConnectionAsync();
            if (type == OutputType.sql_server)
            {
                OType = OutputType.sql_server;
                if (outputDBTool == null || outputDBTool.Type != InputType.sql_server)
                    outputDBTool = new DBToolsProxy(DbProviderFactories.GetFactory(Providers.SqlServerName), new SqlQueries(), outputString, InputType.sql_server);
                else
                    outputDBTool.ChangeConnectionString(outputString);
            }
            else if (type == OutputType.mysql)
            {
                OType = OutputType.mysql;
                if (outputDBTool == null || outputDBTool.Type != InputType.mysql)
                    outputDBTool = new DBToolsProxy(DbProviderFactories.GetFactory(Providers.MySqlName), new MySqlQueries(), outputString, InputType.mysql);
                else
                    outputDBTool.ChangeConnectionString(outputString);
            }
        }
        public DBAnalyzer()
        {
            Providers.RegisterProviders();
        }
        public async Task Analyze(string inputDatabaseName, string outputDatabaseName, bool createDatabase=false)
        {
            if (string.IsNullOrEmpty(inputDatabaseName) || string.IsNullOrEmpty(outputDatabaseName)) throw new Exception("database name cannot be empty");
            try
            {
                if(OType != OutputType.text_file)
                {
                    if (!createDatabase)
                    {
                        if (await outputDBTool.OpenConnection())
                        {
                            await outputDBTool.ChangeDatabaseAsync(outputDatabaseName);
                            if (!await outputDBTool.CheckTableExistAsync(TableType.dbs, false))
                                await outputDBTool.CreateTableAsync(TableType.dbs, false);
                            if (!await outputDBTool.CheckTableExistAsync(TableType.common_info, false))
                                await outputDBTool.CreateTableAsync(TableType.common_info, false);
                            if (!await outputDBTool.CheckTableExistAsync(TableType.tables_info, false))
                                await outputDBTool.CreateTableAsync(TableType.tables_info, true);
                        }
                        await outputDBTool.CloseConnectionAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static class Providers
        {
            public static readonly string SqlServerName = "sql server";
            public static readonly string MySqlName = "mysql";
            internal static void RegisterProviders()
            {
                DbProviderFactories.RegisterFactory(SqlServerName, SqlClientFactory.Instance);
                DbProviderFactories.RegisterFactory(MySqlName, MySqlClientFactory.Instance);
            }
            public static List<string> GetProvidersList()
            {
                return DbProviderFactories.GetProviderInvariantNames().ToList();
            }
        }

        public async Task<List<string>> GetInputDatabasesAsync()
        {
            return await inputDBTool.GetDatabasesAsync();
        }
        public async Task<List<string>> GetOutputDatabasesAsync()
        {
            var dbsWithoutCheck = await outputDBTool.GetDatabasesAsync(false);
            var dbsWithCheck = new List<string>();
            foreach (var db in dbsWithoutCheck)
            {
                var tables = await outputDBTool.GetTablesFromDatabaseAsync(db, false);
                if(tables.Count == 0 || (tables.Count == 3 && tables.All(table => new[] { "dbs", "common_info", "tables_info" }.Contains(table))))
                {
                    dbsWithCheck.Add(db);
                }
            }
            await outputDBTool.CloseConnectionAsync();
            return dbsWithCheck;
        }

        public async Task ChangeInputDatabase(string database)
        {
            await inputDBTool.ChangeDatabaseAsync(database);
        }
        public async Task ChangeOutputDatabase(string database)
        {
            await outputDBTool.ChangeDatabaseAsync(database);
        }
        ~DBAnalyzer()
        {
            inputDBTool.CloseConnectionAsync();
            outputDBTool.CloseConnectionAsync();
        }

    }
    public enum InputType
    {
        sql_server, mysql
    };
    public enum OutputType
    {
        sql_server, mysql, text_file
    };
}
