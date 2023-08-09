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
        private List<DBInfo> dBInfos;
        public readonly InputType providerType;
        public DbProviderFactory Factory { get { return original.Factory; } }
        public IDBQueries Queries { get { return original.Queries; } }
        public DbConnection Connection { get { return original.Connection; } }
        public DBToolsProxy(DbProviderFactory factory, IDBQueries queries, string connectionString, InputType type)
        {
            original = new DBTools(factory, queries, connectionString);
            getDatabasesCount = 5;
            databases = new List<string>();
            providerType = type;
            dBInfos = new List<DBInfo>();
        }
        public async Task ChangeDatabaseAsync(string databaseName)
        {
            await original.ChangeDatabaseAsync(databaseName);
        }
        public void ChangeConnectionString(string connectionString)
        {
            original.ChangeConnectionString(connectionString);
        }
        public string GetCurnetDatabase()
        {
            return original.GetCurnetDatabase();
        }
        public async Task<List<string>> GetDatabasesAsync()
        {
            getDatabasesCount += 1;
            if(getDatabasesCount >= 5)
            {
                getDatabasesCount = 0;
                databases = await original.GetDatabasesAsync();
                bool flag = true;
                foreach(var db in databases)
                {
                    for(int i = 0; i < dBInfos.Count; ++i)
                    {
                        if (db == dBInfos[i].database)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        DBInfo bInfo = new DBInfo() { database = db, updateCount = 0 };
                        bInfo.Tables = await original.GetTablesFromDatabaseAsync(db);
                        dBInfos.Add(bInfo);
                    }
                }
            }
            return databases;
        }

        public async Task<List<string>> GetTablesFromDatabaseAsync(string dbName)
        {
            try
            {
                if (string.IsNullOrEmpty(dbName)) throw new Exception("Database not selected");
                for (int i = 0; i < dBInfos.Count; ++i)
                {
                    if (dBInfos[i].database == dbName)
                    {
                        DBInfo dB = dBInfos[i];
                        dB.updateCount += 1;
                        if (dB.updateCount >= 5)
                        {
                            dB.Tables = await original.GetTablesFromDatabaseAsync(dbName);
                        }
                        dBInfos[i] = dB;
                        return dB.Tables;
                    }
                }
            }catch(Exception ex)
            {
                throw;
            }
            return new List<string>();
        }
        
        private class DBTools
        {
            public IDBQueries Queries { get; private set; }
            public DbProviderFactory Factory { get; private set; }
            public DbConnection Connection { get; private set; }
            public DBTools(DbProviderFactory factory, IDBQueries queries, string connectionString)
            {
                Factory = factory;
                Queries = queries;
                Connection = factory.CreateConnection();
                Connection.ConnectionString = connectionString;
            }
            public async Task ChangeDatabaseAsync(string databaseName)
            {
                await Connection.ChangeDatabaseAsync(databaseName);
            }
            public void ChangeConnectionString(string connectionString)
            {
                Connection.ConnectionString = connectionString;
            }

            public async Task<List<string>> GetDatabasesAsync()
            {
                List<string> databases = new List<string>();
                try
                {
                    await Connection.OpenAsync();
                    using var command = Factory.CreateCommand();
                    command.CommandText = Queries.GetDatabasesQuery();
                    command.Connection = Connection;
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
                    await Connection.CloseAsync();
                }
                return databases;
            }
            public string GetCurnetDatabase()
            {
                return Connection.Database;
            }
            public async Task<List<string>> GetTablesFromDatabaseAsync(string dbName)
            {
                List<string> tables = new List<string>();
                try
                {
                    await Connection.OpenAsync();
                    await Connection.ChangeDatabaseAsync(dbName);
                    using var command = Factory.CreateCommand();
                    command.CommandText = Queries.GetTablesQuery();
                    command.Connection = Connection;
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    await Connection.CloseAsync();
                }
                return tables;
            }
        }
        private struct DBInfo
        {
            public string database;
            public List<string> Tables;
            public int updateCount;
        }
    }
    
}
