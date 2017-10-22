using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.SqlServer.Models
{
    class SysTableTypes
    {
        public string name { get; set; }
        public int system_type_id { get; set; }
        public int user_type_id { get; set; }
        public string schema_name { get; set; }
        public int principal_id { get; set; }
        public int type_table_object_id { get; set; }
    }
}
