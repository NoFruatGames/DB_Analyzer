using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Analyzer.DBQueries
{
    class SqlQueries :IDBQueries
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
                   "	table_name varchar(50) NOT NULL, " +
                   "	row_count int NULL, " +
                   "	db_id int NOT NULL," +
                   "	CONSTRAINT FK_tables_db FOREIGN KEY(db_id) REFERENCES dbs(id)" +
                   ");";
        }
    }
}
