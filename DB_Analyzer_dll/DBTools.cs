using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB_Analyzer_dll.DBQueries;
using System.Data.Common;
namespace DB_Analyzer_dll
{
    internal class DBToolsProxy
    {
        private DBTools original;
        private int getDatabasesCount;
        private List<string> databases;
        public readonly InputType providerType;
        public DBToolsProxy(DbProviderFactory factory, IDBQueries queries, string connectionString, InputType type)
        {
            original = new DBTools(factory, queries, connectionString);
            getDatabasesCount = 5;
            databases = new List<string>();
            providerType = type;
        }
        public async Task ChangeDatabaseAsync(string databaseName)
        {
            await original.ChangeDatabaseAsync(databaseName);
        }
        public void ChangeConnectionString(string connectionString)
        {
            original.ChangeConnectionString(connectionString);
        }

        public async Task<List<string>> GetDatabasesAsync()
        {
            getDatabasesCount += 1;
            if(getDatabasesCount >= 5)
            {
                getDatabasesCount = 0;
                databases = await original.GetDatabasesAsync();
            }
            return databases;
        }

        private class DBTools
        {
            private IDBQueries queries;
            private DbProviderFactory factory;
            private DbConnection connection;
            public DBTools(DbProviderFactory factory, IDBQueries queries, string connectionString)
            {
                this.factory = factory;
                this.queries = queries;
                connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;
            }
            public async Task ChangeDatabaseAsync(string databaseName)
            {
                await connection.ChangeDatabaseAsync(databaseName);
            }
            public void ChangeConnectionString(string connectionString)
            {
                connection.ConnectionString = connectionString;
            }

            public async Task<List<string>> GetDatabasesAsync()
            {
                List<string> databases = new List<string>();
                try
                {
                    await connection.OpenAsync();
                    using var command = factory.CreateCommand();
                    command.CommandText = queries.GetDatabasesQuery();
                    command.Connection = connection;
                    using var reader = await command.ExecuteReaderAsync();
                    while(await reader.ReadAsync())
                    {
                        databases.Add(reader.GetString(0));
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
                return databases;
            }
        }
    }
    
}
