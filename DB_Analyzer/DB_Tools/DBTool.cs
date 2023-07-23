using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Analyzer.DB_Tools
{
    abstract class DBTool
    {
        public string? ConnectionString { get; set; }
        public abstract string SelectedDatabase { get; set; }
        public DBTool(string? connectionString)
        {
            ConnectionString = connectionString;
        }
        public abstract List<string> GetDatabases();
        public abstract Task<List<string>> GetDatabasesAsync();

    }
}
