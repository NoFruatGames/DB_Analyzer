using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB_Analyzer_dll.DBQueries;
using System.Data.Common;
namespace DB_Analyzer_dll
{
    internal class DBToolsFacade
    {
        private DBTools original;
        private List<string> databases;
        private List<DBInfo> dBInfos;
        public InputType Type { get; private set; }
        public DBToolsFacade(DbProviderFactory factory, IDBQueries queries, string connectionString, InputType type)
        {
            original = new DBTools(factory, queries, connectionString);
            databases = new List<string>();
            dBInfos = new List<DBInfo>();
            Type = type;
        }
        public async Task ChangeDatabaseAsync(string databaseName)
        {
            try
            {
                await original.ChangeDatabaseAsync(databaseName);
            }catch(Exception ex)
            {
                throw;
            }
           
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
            try
            {
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
            catch (Exception ex)
            {
                throw;
            }

            return databases;
        }
        public async Task CloseConnectionAsync()
        {
            try
            {
                if (original.Connection.State == System.Data.ConnectionState.Open)
                    await original.Connection.CloseAsync();
            }catch(Exception ex)
            {
                throw;
            }

        }
        public async Task<bool> OpenConnection()
        {
            try
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
            }catch(Exception ex)
            {
                throw;
            }
            
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
            try
            {
                await original.CreateTableAsync(table, closeConnection);
            }catch(Exception ex)
            {
                throw;
            }
            
        }
        public async Task<bool> CheckTableExistAsync(TableType table, bool closeConnection = true)
        {
            try
            {
                return await original.CheckTableExistAsync(table, closeConnection);
            }catch(Exception ex)
            {
                throw;
            }
            
        }
        public async Task CreateDatabaseAsync(string databaseName, bool closeConnection = true)
        {
            try
            {
                await original.CreateDatabaseAsync(databaseName, closeConnection);
                databases.Add(databaseName);
                dBInfos.Add(new DBInfo() { database = databaseName, updateCount = 5 });
            }catch(Exception ex)
            {
                throw;
            }
            

        }
        public async Task<int> InsertToCommonInfoTableAsync(int? tablesCount, int? proceduresCount, int dbId, bool closeConnection = true)
        {
            try
            {
                return await original.InsertToCommonInfoTableAsync(tablesCount, proceduresCount, dbId, closeConnection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> InsertToTablesInfoTableAsync(string? tableName, int? rowsCount, int dbId, bool closeConnection = true)
        {
            try
            {
                return await original.InsertToTablesInfoTableAsync(tableName, rowsCount, dbId, closeConnection);
            }catch(Exception ex)
            {
                throw;
            }
        }
        public async Task<int> InsertToDBSTableAsync(string serverName, string databaseName, DateTime checkDate, bool closeConnection = true)
        {
            try
            {
                return await original.InsertToDBSTableAsync(serverName, databaseName, checkDate, closeConnection);
            }catch(Exception ex)
            {
                throw;
            }
        }
        public async Task<int> GetProceduresCountAsync(bool closeConnection = true)
        {
            try
            {
                return await original.GetProceduresCountAsync(closeConnection);
            }catch(Exception ex)
            {
                throw;
            }
        }
        public async Task<List<TableRowsInfo>> GetTablesRowsAsync(bool closeConnection = true)
        {
            try
            {
                return await original.GetTablesRowsAsync(closeConnection);
            }catch(Exception ex)
            {
                throw;
            }
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
                try
                {
                    await Connection.ChangeDatabaseAsync(databaseName);
                }catch(Exception ex)
                {
                    throw;
                }
                
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
                    using (var command = Factory.CreateCommand())
                    {
                        command.CommandText = Queries.GetDatabasesQuery();
                        command.Connection = Connection;
                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            databases.Add(reader.GetString(0));
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
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
                    using (var command = Factory.CreateCommand())
                    {
                        command.CommandText = Queries.GetTablesQuery();
                        command.Connection = Connection;
                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
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
                    using (var command = Factory.CreateCommand())
                    {
                        if (table == TableType.dbs)
                            command.CommandText = Queries.CreateDBSTableQuery();
                        else if (table == TableType.common_info)
                            command.CommandText = Queries.CreateCommonInfoTableQuery();
                        else if (table == TableType.tables_info)
                            command.CommandText = Queries.CreateTablesInfoTableQuery();
                        command.Connection = Connection;
                        await command.ExecuteNonQueryAsync();
                    }
                    
                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
                        await Connection.CloseAsync();
                }
            }
            public async Task<bool> CheckTableExistAsync(TableType table, bool closeConnection=true)
            {
                object res = 0;
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();          
                    using (var command = Factory.CreateCommand())
                    {
                        var param = Factory.CreateParameter();
                        var paramDbName = Factory.CreateParameter();
                        paramDbName.ParameterName = "DatabaseName";
                        paramDbName.Value = Connection.Database;
                        paramDbName.Direction = System.Data.ParameterDirection.Input;
                        paramDbName.DbType = System.Data.DbType.String;
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
                        command.Parameters.Add(paramDbName);
                        res = await command.ExecuteScalarAsync();
                    }
                    
                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
                        await Connection.CloseAsync();
                }
                return Convert.ToInt32(res) == 0 ? false : true;
            }
            public async Task CreateDatabaseAsync(string databaseName, bool closeConnection = true)
            {
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using (var command = Factory.CreateCommand())
                    {
                        command.Connection = Connection;
                        command.CommandText = Queries.CreateDatabaseQuery(databaseName);
                        await command.ExecuteNonQueryAsync();
                    }


                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
                        await Connection.CloseAsync();
                }
            }
            public async Task<int> InsertToDBSTableAsync(string serverName, string databaseName, DateTime checkDate, bool closeConnection=true)
            {
                object res = 0;
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using (var command = Factory.CreateCommand())
                    {
                        command.CommandText = Queries.InsertIntoDbsTableQuery();
                        command.Connection = Connection;
                        var paramServerName = Factory.CreateParameter();
                        paramServerName.ParameterName = "ServerName";
                        paramServerName.Value = serverName;
                        paramServerName.DbType = System.Data.DbType.String;
                        paramServerName.Direction = System.Data.ParameterDirection.Input;
                        var paramDatabaseName = Factory.CreateParameter();
                        paramDatabaseName.ParameterName = "DatabaseName";
                        paramDatabaseName.Value = databaseName;
                        paramDatabaseName.DbType = System.Data.DbType.String;
                        paramDatabaseName.Direction = System.Data.ParameterDirection.Input;
                        var paramCheckDate = Factory.CreateParameter();
                        paramCheckDate.ParameterName = "CheckDate";
                        paramCheckDate.Value = checkDate;
                        paramCheckDate.DbType = System.Data.DbType.DateTime;
                        paramCheckDate.Direction = System.Data.ParameterDirection.Input;
                        command.Parameters.Add(paramServerName);
                        command.Parameters.Add(paramDatabaseName);
                        command.Parameters.Add(paramCheckDate);
                        res = await command.ExecuteScalarAsync();
                    }
                    
                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
                        await Connection.CloseAsync();
                }
                return Convert.ToInt32(res);
            }
            public async Task<int> InsertToTablesInfoTableAsync(string? tableName, int? rowsCount, int dbId, bool closeConnection = true)
            {
                object res = 0;
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using (var command = Factory.CreateCommand())
                    {
                        command.CommandText = Queries.InsertIntoTablesInfoTableQuery();
                        command.Connection = Connection;
                        var paramTableName = Factory.CreateParameter();
                        paramTableName.ParameterName = "TableName";
                        if (tableName == null)
                            paramTableName.Value = DBNull.Value;
                        else
                            paramTableName.Value = tableName;
                        paramTableName.DbType = System.Data.DbType.String;
                        paramTableName.Direction = System.Data.ParameterDirection.Input;
                        var paramRowsCount = Factory.CreateParameter();
                        paramRowsCount.ParameterName = "RowCount";
                        if (rowsCount == null)
                            paramRowsCount.Value = DBNull.Value;
                        else
                            paramRowsCount.Value = rowsCount;
                        paramRowsCount.DbType = System.Data.DbType.Int32;
                        paramRowsCount.Direction = System.Data.ParameterDirection.Input;
                        var paramDbId = Factory.CreateParameter();
                        paramDbId.ParameterName = "DbId";
                        paramDbId.Value = dbId;
                        paramDbId.DbType = System.Data.DbType.Int32;
                        paramDbId.Direction = System.Data.ParameterDirection.Input;
                        command.Parameters.Add(paramTableName);
                        command.Parameters.Add(paramRowsCount);
                        command.Parameters.Add(paramDbId);
                        res = await command.ExecuteScalarAsync();
                    }
                    
                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
                        await Connection.CloseAsync();
                }
                return Convert.ToInt32(res);
            }
            public async Task<int> InsertToCommonInfoTableAsync(int? tablesCount, int? proceduresCount, int dbId, bool closeConnection = true)
            {
                object res = 0;
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using (var command = Factory.CreateCommand())
                    {
                        command.CommandText = Queries.InsertIntoCommonInfoTableQuery();
                        command.Connection = Connection;
                        var paramTablesCount = Factory.CreateParameter();
                        paramTablesCount.ParameterName = "TablesCount";
                        if (tablesCount == null)
                            paramTablesCount.Value = DBNull.Value;
                        else
                            paramTablesCount.Value = tablesCount;
                        paramTablesCount.DbType = System.Data.DbType.Int32;
                        paramTablesCount.Direction = System.Data.ParameterDirection.Input;
                        var paramProceduresCount = Factory.CreateParameter();
                        paramProceduresCount.ParameterName = "ProceduresCount";
                        if (proceduresCount == null)
                            paramProceduresCount.Value = DBNull.Value;
                        else
                            paramProceduresCount.Value = proceduresCount;
                        paramProceduresCount.DbType = System.Data.DbType.Int32;
                        paramProceduresCount.Direction = System.Data.ParameterDirection.Input;
                        var paramDbId = Factory.CreateParameter();
                        paramDbId.ParameterName = "DbId";
                        paramDbId.Value = dbId;
                        paramDbId.DbType = System.Data.DbType.Int32;
                        paramDbId.Direction = System.Data.ParameterDirection.Input;
                        command.Parameters.Add(paramTablesCount);
                        command.Parameters.Add(paramProceduresCount);
                        command.Parameters.Add(paramDbId);
                        res = await command.ExecuteScalarAsync();
                    }

                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
                        await Connection.CloseAsync();
                }
                return Convert.ToInt32(res);
            }
            public async Task<int> GetProceduresCountAsync(bool closeConnection=true)
            {
                object res = 0;
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using (var command = Factory.CreateCommand())
                    {
                        command.CommandText = Queries.GetProceduresCountQuery();
                        command.Connection = Connection;
                        var paramDbName = Factory.CreateParameter();
                        paramDbName.ParameterName = "DatabaseName";
                        paramDbName.Value = Connection.Database;
                        paramDbName.DbType = System.Data.DbType.String;
                        paramDbName.Direction = System.Data.ParameterDirection.Input;
                        command.Parameters.Add(paramDbName);

                        res = await command.ExecuteScalarAsync();
                    }

                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
                        await Connection.CloseAsync();
                }
                return Convert.ToInt32(res);
            }
            public async Task<List<TableRowsInfo>> GetTablesRowsAsync(bool closeConnection = true)
            {
                var res = new List<TableRowsInfo>();
                try
                {
                    if (Connection.State == System.Data.ConnectionState.Closed)
                        await Connection.OpenAsync();
                    using (var command = Factory.CreateCommand())
                    {
                        command.CommandText = Queries.GetTablesRowsQuery();
                        command.Connection = Connection;
                        var paramDbName = Factory.CreateParameter();
                        paramDbName.ParameterName = "DatabaseName";
                        paramDbName.Value = Connection.Database;
                        paramDbName.DbType = System.Data.DbType.String;
                        paramDbName.Direction = System.Data.ParameterDirection.Input;
                        command.Parameters.Add(paramDbName);
                        var reader = await command.ExecuteReaderAsync();
                        while(await reader.ReadAsync())
                        {
                            res.Add(new TableRowsInfo() { tableName = reader.GetString(0), rowsCount = Convert.ToInt32(reader.GetValue(1)) });
                        }
                    }

                }
                catch (Exception ex)
                {
                    await Connection.CloseAsync();
                    throw;
                }
                finally
                {
                    if (Connection.State == System.Data.ConnectionState.Open && closeConnection)
                        await Connection.CloseAsync();
                }
                return res;
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
    internal struct TableRowsInfo
    {
        public string tableName;
        public int rowsCount;
    }
}
