using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Analyzer.DBQueries
{
    interface IDBQueries
    {
        string GetDatabasesQuery();
        string GetTablesQuery();
        string CheckTableExistQuery();
        string CreateDBSTableQuery();
        string CreateCommonInfoTableQuery();
        string CreateTablesInfoTableQuery();
    }
}
