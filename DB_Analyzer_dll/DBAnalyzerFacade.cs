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
        private DBTools inputDBTool;
        private DBTools outputDBTool;
        public void SetInputDatabase(InputType type, string connectionString)
        {
            if(type == InputType.sql_server)
                inputDBTool = new DBTools(DbProviderFactories.GetFactory(Providers.SqlServerName), new SqlQueries(), connectionString);
            else if(type == InputType.mysql)
                inputDBTool = new DBTools(DbProviderFactories.GetFactory(Providers.MySqlName), new MySqlQueries(), connectionString);
        }
        public void SetOutputType(OutputType type, string outputString)
        {
            if (type == OutputType.sql_server)
                inputDBTool = new DBTools(DbProviderFactories.GetFactory(Providers.SqlServerName), new SqlQueries(), outputString);
            else if (type == OutputType.mysql)
                inputDBTool = new DBTools(DbProviderFactories.GetFactory(Providers.MySqlName), new MySqlQueries(), outputString);
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
