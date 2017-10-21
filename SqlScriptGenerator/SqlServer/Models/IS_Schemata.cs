using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.SqlServer.Models
{
    class IS_Schemata
    {
        public string CATALOG_NAME { get; set; }
        public string SCHEMA_NAME { get; set; }
        public string SCHEMA_OWNER { get; set; }
        public string DEFAULT_CHARACTER_SET_CATALOG { get; set; }
        public string DEFAULT_CHARACTER_SET_SCHEMA { get; set; }
        public string DEFAULT_CHARACTER_SET_NAME { get; set; }
    }
}
