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
        public InputType Type { get; private set; }
        //public DbProviderFactory Factory { get { return original.Factory; } }
        //public IDBQueries Queries { get { return original.Queries; } }
        //public DbConnection Connection { get { return original.Connection; } }
        public DBToolsProxy(DbProviderFactory factory, IDBQueries queries, string connectionString, InputType type)
        {
            original = new DBTools(factory, queries, connectionString);
            getDatabasesCount = 5;
            databases = new List<string>();
            dBInfos = new List<DBInfo>();
            Type = type;
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
        public async Task<List<string>> GetDatabasesAsync(bool closeConnection = true)
        {
            getDatabasesCount += 1;
            if (getDatabasesCount >= 5)
            {
                getDatabasesCount = 0;
                databases = await original.GetDatabasesAsync(closeConnection);
                bool flag = true;
                foreach (var db in databases)
                {
                    for (int i = 0; i < dBInfos.Count; ++i)
                    {
                        if (db == dBInfos[i].database)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        DBInfo bInfo = new DBInfo() { database = db, updateCount = 5 };
                        dBInfos.Add(bInfo);
                    }
                }
            }
            return databases;
        }
        public async Task CloseConnectionAsync()
        {
            if (original.Connection.State == System.Data.ConnectionState.Open)
                await original.Connection.CloseAsync();
        }
        public async Task<bool> OpenConnection()
        {
            if (original.Connection.State == System.Data.ConnectionState.Closed)
            {
                await original.Connection.OpenAsync();
                if (original.Connection.State == System.Data.ConnectionState.Open)
                    return true;
                else
                    return false;
            }
            else if (original.Connection.State == System.Data.ConnectionState.Open)
                return true;
            else
                return false;
        }
        public async Task<List<string>> GetTablesFromDatabaseAsync(string dbName, bool closeConnection = true)
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
                            dB.Tables = await original.GetTablesFromDatabaseAsync(dbName, closeConnection);
                        }
                        dBInfos[i] = dB;
                        return dB.Tables;
                    }
                }
            } catch (Exception ex)
            {
                throw;
            }
            return new List<string>();
        }
        public async Task CreateTableAsync(TableType table, bool closeConnection = true)
        {
            await original.CreateTableAsync(table, closeConnection);
        }
        public async Task<bool> CheckTableExistAsync(TableType table, bool closeConnection = true)
        {
            return await original.CheckTableExistAsync(table, closeConnection);
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

            public async Task<List<string>> GetDatabasesAsync(bool closeConnection = true)
            {
                List<string> databases = new List<string>();
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using var command = Factory.CreateCommand();
                    command.CommandText = Queries.GetDatabasesQuery();
                    command.Connection = Connection;
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
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
                    if (Connection.State == System.Data.ConnectionState.Broken || (Connection.State == System.Data.ConnectionState.Open && closeConnection))
                        await Connection.CloseAsync();
                }
                return databases;
            }
            public string GetCurnetDatabase()
            {
                return Connection.Database;
            }
            public async Task<List<string>> GetTablesFromDatabaseAsync(string dbName, bool closeConnection = true)
            {
                List<string> tables = new List<string>();
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
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
                    if (Connection.State == System.Data.ConnectionState.Broken || (Connection.State == System.Data.ConnectionState.Open && closeConnection))
                        await Connection.CloseAsync();
                }
                return tables;
            }
            public async Task CreateTableAsync(TableType table, bool closeConnection = true)
            {
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using var command = Factory.CreateCommand();
                    if (table == TableType.dbs)
                        command.CommandText = Queries.CreateDBSTableQuery();
                    else if (table == TableType.common_info)
                        command.CommandText = Queries.CreateCommonInfoTableQuery();
                    else if (table == TableType.tables_info)
                        command.CommandText = Queries.CreateTablesInfoTableQuery();
                    command.Connection = Connection;
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Broken || (Connection.State == System.Data.ConnectionState.Open && closeConnection))
                        await Connection.CloseAsync();
                }
            }
            public async Task<bool> CheckTableExistAsync(TableType table, bool closeConnection=true)
            {
                int res = 0;
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using var command = Factory.CreateCommand();
                    var param = Factory.CreateParameter();
                    command.CommandText = Queries.CheckTableExistQuery();
                    command.Connection = Connection;
                    param.ParameterName = "TableName";
                    if (table == TableType.common_info)
                        param.Value = "common_info";
                    else if (table == TableType.dbs)
                        param.Value = "dbs";
                    else if (table == TableType.tables_info)
                        param.Value = "tables_info";
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.DbType = System.Data.DbType.String;
                    command.Parameters.Add(param);
                    res = (int)await command.ExecuteScalarAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Broken || (Connection.State == System.Data.ConnectionState.Open && closeConnection))
                        await Connection.CloseAsync();
                }
                return res == 0 ? false : true;
            }
        }
        private struct DBInfo
        {
            public string database;
            public List<string> Tables;
            public int updateCount;
        }
    }
    internal enum TableType
    {
        dbs, common_info, tables_info
    };
}
