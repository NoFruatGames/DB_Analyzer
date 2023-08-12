using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Analyzer_dll.DBQueries
{
    internal class SqlQueries :IDBQueries
    {
        public SqlQueries() { }
        public string GetDatabasesQuery()
        {
            return @$"SELECT name " +
                   @$"FROM sys.databases " +
                   @$"WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') " +
                   @$"AND name NOT LIKE 'resource%';";
        }
        public string GetTablesQuery()
        {
            return "SELECT TABLE_NAME " +
                   "FROM INFORMATION_SCHEMA.TABLES " +
                   "WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE 'sysdiagrams';";
        }
        public string CheckTableExistQuery()
        {
            return "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName;";
        }
        public string CreateDBSTableQuery()
        {
            return "CREATE TABLE dbs( " +
                   "	id int NOT NULL PRIMARY KEY IDENTITY(1,1)," +
                   "	server varchar(10) NOT NULL, " +
                   "	db_name varchar(50) NOT NULL," +
                   "	check_date datetime NOT NULL " +
                   ");";
        }
        public string CreateCommonInfoTableQuery()
        {
            return "CREATE TABLE common_info( " +
                   "	id int NOT NULL PRIMARY KEY IDENTITY(1,1), " +
                   "	tables_count int NULL, " +
                   "	procedures_count int NULL, " +
                   "	db_id int NOT NULL, " +
                   "	CONSTRAINT FK_common_db FOREIGN KEY(db_id) REFERENCES dbs(id) " +
                   ");";
        }
        public string CreateTablesInfoTableQuery()
        {
            return "CREATE TABLE tables_info( " +
                   "	id int NOT NULL PRIMARY KEY IDENTITY(1,1), " +
                   "	table_name varchar(50) NULL, " +
                   "	row_count int NULL, " +
                   "	db_id int NOT NULL," +
                   "	CONSTRAINT FK_tables_db FOREIGN KEY(db_id) REFERENCES dbs(id)" +
                   ");";
        }
        public string CreateDatabaseQuery(string databaseName)
        {
            return $"CREATE DATABASE {databaseName};";
        }
        public string InsertIntoDbsTableQuery()
        {
            return "INSERT INTO dbs(server, db_name, check_date)" +
                " VALUES(@ServerName, @DatabaseName, @CheckDate);" +
                " SELECT SCOPE_IDENTITY();";
        }
        public string InsertIntoCommonInfoTableQuery()
        {
            return "INSERT INTO common_info(tables_count, procedures_count, db_id)" +
                " VALUES(@TablesCount, @ProceduresCount, @DbId);" +
                " SELECT SCOPE_IDENTITY();";
        }
        public string InsertIntoTablesInfoTableQuery()
        {
            return "INSERT INTO tables_info(table_name, row_count, db_id)" +
                " VALUES(@TableName, @RowCount, @DbId);" +
                " SELECT SCOPE_IDENTITY();";
        }
        public string GetTablesRowsQuery()
        {
            return "DECLARE @TableName NVARCHAR(128) " +
                    "DECLARE @Query NVARCHAR(MAX) " +
                    "CREATE TABLE #TableRecordCounts ( " +
                    "    TableName NVARCHAR(128), " +
                    "    RecordCount INT, " +
                    ") " +
                    "DECLARE TableCursor CURSOR FOR " +
                    "SELECT name " +
                    "FROM sys.tables " +
                    "WHERE name <> 'sysdiagrams' " +
                    "OPEN TableCursor " +
                    "FETCH NEXT FROM TableCursor INTO @TableName " +
                    "WHILE @@FETCH_STATUS = 0 " +
                    "BEGIN " +
                    "    SET @Query = N'SELECT ''' + @TableName + ''' AS TableName, COUNT(*) AS RecordCount FROM ' + @TableName " +
                    "    INSERT INTO #TableRecordCounts (TableName, RecordCount) " +
                    "    EXEC sp_executesql @Query " +
                    "    FETCH NEXT FROM TableCursor INTO @TableName " +
                    "END " +
                    "CLOSE TableCursor " +
                    "DEALLOCATE TableCursor " +
                    "SELECT * FROM #TableRecordCounts " +
                    "DROP TABLE #TableRecordCounts;";
        }
        public string GetProceduresCountQuery()
        {
            return "SELECT COUNT(name) " +
                   "FROM sys.procedures";
        }
    }
}
