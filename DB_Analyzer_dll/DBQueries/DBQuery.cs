using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Analyzer_dll.DBQueries
{
    internal interface IDBQueries
    {
        string GetDatabasesQuery();
        string GetTablesQuery();
        string CheckTableExistQuery();
        string CreateDBSTableQuery();
        string CreateCommonInfoTableQuery();
        string CreateTablesInfoTableQuery();
        string CreateDatabaseQuery(string databaseName);
        string InsertIntoDbsTableQuery();
        string InsertIntoCommonInfoTableQuery();
        string InsertIntoTablesInfoTableQuery();
        string GetTablesRowsQuery();
        string GetProceduresCountQuery();
    }
}
