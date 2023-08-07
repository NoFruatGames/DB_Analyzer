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
        public void SetInputDatabase(InputType type, string connectionString)
        {
            if(type == InputType.sql_server)
            {
                if (inputDBTool == null || inputDBTool.providerType != InputType.sql_server)
                    inputDBTool = new DBToolsProxy(DbProviderFactories.GetFactory(Providers.SqlServerName), new SqlQueries(), connectionString, InputType.sql_server);
                else
                    inputDBTool.ChangeConnectionString(connectionString);
            }
                
            else if(type == InputType.mysql)
            {
                if (inputDBTool == null || inputDBTool.providerType != InputType.mysql)
                    inputDBTool = new DBToolsProxy(DbProviderFactories.GetFactory(Providers.MySqlName), new MySqlQueries(), connectionString, InputType.mysql);
                else
                    inputDBTool.ChangeConnectionString(connectionString);
            }
                
        }
        public void SetOutputType(OutputType type, string outputString)
        {
            if (type == OutputType.sql_server)
            {
                if (outputDBTool == null || outputDBTool.providerType != InputType.sql_server)
                    outputDBTool = new DBToolsProxy(DbProviderFactories.GetFactory(Providers.SqlServerName), new SqlQueries(), outputString, InputType.sql_server);
                else
                    outputDBTool.ChangeConnectionString(outputString);
            }
            else if (type == OutputType.mysql)
            {
                if (outputDBTool == null || outputDBTool.providerType != InputType.mysql)
                    outputDBTool = new DBToolsProxy(DbProviderFactories.GetFactory(Providers.MySqlName), new MySqlQueries(), outputString, InputType.mysql);
                else
                    outputDBTool.ChangeConnectionString(outputString);
            }
        }
        public DBAnalyzer()
        {
            Providers.RegisterProviders();
        }
        public async void Analyze()
        {

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
            return await outputDBTool.GetDatabasesAsync();
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
