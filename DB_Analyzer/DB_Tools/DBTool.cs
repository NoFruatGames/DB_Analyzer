using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Analyzer.DB_Tools
{
    abstract class DBTool
    {
        public string ConnectionString { get; protected set; }
        public abstract string SelectedDatabase { get; set; }
        public DBTool(string? connectionString)
        {
            if (connectionString == null) throw new Exception("connection string is null");
            ConnectionString = connectionString;
        }
        public abstract List<string> GetDatabases();
        public abstract Task<List<string>> GetDatabasesAsync();
        public abstract List<string> GetTables();
        public abstract Task<List<string>> GetTablesAsync();

    }
}
