using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Analyzer_dll.DBQueries
{
    internal class MySqlQueries : IDBQueries
    {
        public MySqlQueries() { }
        public string GetDatabasesQuery()
        {
            return @$"SHOW DATABASES " +
                   @$"WHERE `Database` NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys');";
        }
        public string GetTablesQuery()
        {
            return "SHOW TABLES;";
        }
        public string CheckTableExistQuery()
        {
            return "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @DatabaseName;";
        }
        public string CreateDBSTableQuery()
        {
            return "CREATE TABLE dbs ( " +
                   "	  id INT NOT NULL AUTO_INCREMENT PRIMARY KEY," +
                   "   server VARCHAR(10) NOT NULL, " +
                   "	  db_name varchar(50) NOT NULL," +
                   "   check_date datetime NOT NULL " +
                   ");";
        }
        public string CreateCommonInfoTableQuery()
        {
            return "CREATE TABLE common_info( " +
                   "	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                   "	tables_count INT, " +
                   "	procedures_count INT, " +
                   "	db_id INT NOT NULL, " +
                   "	CONSTRAINT FK_common_db FOREIGN KEY (db_id) REFERENCES dbs(id) " +
                   ");";
        }
        public string CreateTablesInfoTableQuery()
        {
            return "CREATE TABLE tables_info( " +
                   "	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                   "	table_name VARCHAR(50), " +
                   "	row_count INT, " +
                   "	db_id int NOT NULL," +
                   "	CONSTRAINT FK_tables_db FOREIGN KEY (db_id) REFERENCES dbs(id)" +
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
                " SELECT LAST_INSERT_ID();";
        }
        public string InsertIntoCommonInfoTableQuery()
        {
            return "INSERT INTO common_info(tables_count, procedures_count, db_id)" +
                " VALUES(@TablesCount, @ProceduresCount, @DbId);" +
                " SELECT LAST_INSERT_ID();";
        }
        public string InsertIntoTablesInfoTableQuery()
        {
            return "INSERT INTO tables_info(table_name, row_count, db_id)" +
                " VALUES(@TableName, @RowCount, @DbId);" +
                " SELECT LAST_INSERT_ID();";
        }

        public string GetTablesRowsQuery()
        {
            return "SELECT table_name, table_rows " +
                   "FROM information_schema.tables " +
                   "WHERE table_schema = @DatabaseName;";

        }
        public string GetProceduresCountQuery()
        {
            return "SELECT COUNT(*) " +
                   "FROM information_schema.ROUTINES " +
                   "WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_SCHEMA = @DatabaseName;";
        }
    }
}
