using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using MySql.Data.MySqlClient;
using DB_Analyzer_dll.DBQueries;
using System.IO;
namespace DB_Analyzer_dll
{
    public class DBAnalyzer
    {
        private DBToolsFacade inputDBTool;
        private DBToolsFacade outputDBTool;
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
                    inputDBTool = new DBToolsFacade(DbProviderFactories.GetFactory(Providers.SqlServerName), new SqlQueries(), connectionString, InputType.sql_server);
                else
                    inputDBTool.ChangeConnectionString(connectionString);
            }
                
            else if(type == InputType.mysql)
            {
                IType = InputType.mysql;
                if (inputDBTool == null || inputDBTool.Type != InputType.mysql)
                    inputDBTool = new DBToolsFacade(DbProviderFactories.GetFactory(Providers.MySqlName), new MySqlQueries(), connectionString, InputType.mysql);
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
                    outputDBTool = new DBToolsFacade(DbProviderFactories.GetFactory(Providers.SqlServerName), new SqlQueries(), outputString, InputType.sql_server);
                else
                    outputDBTool.ChangeConnectionString(outputString);
            }
            else if (type == OutputType.mysql)
            {
                OType = OutputType.mysql;
                if (outputDBTool == null || outputDBTool.Type != InputType.mysql)
                    outputDBTool = new DBToolsFacade(DbProviderFactories.GetFactory(Providers.MySqlName), new MySqlQueries(), outputString, InputType.mysql);
                else
                    outputDBTool.ChangeConnectionString(outputString);
            }
            else
            {
                OType = OutputType.text_file;
            }
        }
        public bool TablesCountCheck { get; set; } = false;
        public bool ProceduresCountCheck { get; set; } = false;
        public bool TablesRowsCheck { get; set; } = false;
        public DBAnalyzer()
        {
            Providers.RegisterProviders();
        }
        public async Task Analyze(string inputDatabaseName, string outputDatabaseName, bool createDatabase=false)
        {
            if (string.IsNullOrEmpty(inputDatabaseName) || string.IsNullOrEmpty(outputDatabaseName)) throw new Exception("database name cannot be empty");
            try
            {
                int? tablesCount = null;
                int? proceduresCount = null;
                List<TableRowsInfo>? rowsInfos=null;
                DateTime curnetDateTime = DateTime.Now;
                await inputDBTool.OpenConnection();
                await inputDBTool.ChangeDatabaseAsync(inputDatabaseName);
                if (TablesCountCheck)
                    tablesCount = (await inputDBTool.GetTablesFromDatabaseAsync(inputDatabaseName, false)).Count;
                if (ProceduresCountCheck)
                    proceduresCount = await inputDBTool.GetProceduresCountAsync(false);
                if (TablesRowsCheck)
                    rowsInfos = await inputDBTool.GetTablesRowsAsync(true);
                await inputDBTool.CloseConnectionAsync();
                if (OType != OutputType.text_file)
                {

                    if (await outputDBTool.OpenConnection())
                    {
                        if (createDatabase)
                        {
                            if (!(await outputDBTool.GetDatabasesAsync(false)).Contains(outputDatabaseName))
                            {
                                await outputDBTool.CreateDatabaseAsync(outputDatabaseName, false);
                            }
                            else
                            {
                                throw new Exception("Invalid database name: database exist");
                            }
                        }
                        await outputDBTool.ChangeDatabaseAsync(outputDatabaseName);
                        if (!await outputDBTool.CheckTableExistAsync(TableType.dbs, false))
                            await outputDBTool.CreateTableAsync(TableType.dbs, false);
                        if (!await outputDBTool.CheckTableExistAsync(TableType.common_info, false))
                            await outputDBTool.CreateTableAsync(TableType.common_info, false);
                        if (!await outputDBTool.CheckTableExistAsync(TableType.tables_info, false))
                            await outputDBTool.CreateTableAsync(TableType.tables_info, true);

                        int curnetDBCheck = await outputDBTool.InsertToDBSTableAsync(IType.ToString(), inputDatabaseName, curnetDateTime, false);
                        if(rowsInfos != null)
                        {
                            foreach(var row in rowsInfos)
                            {
                                await outputDBTool.InsertToTablesInfoTableAsync(row.tableName, row.rowsCount, curnetDBCheck, false);
                            }
                        }
                        await outputDBTool.InsertToCommonInfoTableAsync(tablesCount, proceduresCount, curnetDBCheck, true);
                    }
                    
                }
                else
                {
                    var writer = new StreamWriter(outputDatabaseName, false);
                    writer.WriteLine("Server: " + IType.ToString());
                    writer.WriteLine("Database name: " + inputDatabaseName);
                    writer.WriteLine("Check date: " + curnetDateTime.ToString());
                    if (tablesCount != null)
                        writer.WriteLine("Tables count: " + tablesCount.ToString());
                    if (proceduresCount != null)
                        writer.WriteLine("Procedures count: " + proceduresCount.ToString());
                    if(rowsInfos != null)
                    {
                        writer.WriteLine("Table name: rows count");
                        foreach (var row in rowsInfos)
                        {
                            writer.WriteLine(row.tableName + ": " + row.rowsCount.ToString());
                        }
                    }
                    await writer.FlushAsync();
                    writer.Close();
                }

            }
            catch (Exception ex)
            {
                if(inputDBTool != null)
                    await inputDBTool.CloseConnectionAsync();
                if(outputDBTool != null)
                    await outputDBTool.CloseConnectionAsync();
                throw;
            }
            finally
            {
                if (inputDBTool != null)
                    await inputDBTool.CloseConnectionAsync();
                if (outputDBTool != null)
                    await outputDBTool.CloseConnectionAsync();
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
