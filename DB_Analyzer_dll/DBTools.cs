using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB_Analyzer_dll.DBQueries;
using System.Data.Common;
namespace DB_Analyzer_dll
{
    internal class DBTools
    {
        private IDBQueries queries;
        private DbProviderFactory factory;
        private DbConnection connection;
        internal DBTools(DbProviderFactory factory, IDBQueries queries, string connectionString)
        {
            this.factory = factory;
            this.queries = queries;
            connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
        }
    }
}
