using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB_Analyzer.DB_Tools;
namespace DB_Analyzer
{
    class Analyzer
    {
        public DBTool InputTool { get; set; }
        public DBTool OutputTool { get; set; }
        public Analyzer() { }
        public Analyzer(DBTool inputTool, DBTool outputTool)
        {
            InputTool = inputTool;
            OutputTool = outputTool;
        }
        public async void Analyze()
        {
            try
            {
                if(!await OutputTool.CheckTableExistAsync("dbs"))
                    OutputTool.CreateDBSTableAsync();
                if (!await OutputTool.CheckTableExistAsync("common_info"))
                    OutputTool.CreateCommonInfoTableAsync();
                if (!await OutputTool.CheckTableExistAsync("tables_info"))
                    OutputTool.CreateTablesInfoTableAsync();

            }catch(Exception ex)
            {
                throw;
            }
        }
    }
}
