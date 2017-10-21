using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.SqlServer.Models
{
    class IS_Column
    {
        public string   TABLE_CATALOG               { get; set; }
        public string   TABLE_SCHEMA                { get; set; }
        public string   TABLE_NAME                  { get; set; }
        public string   COLUMN_NAME                 { get; set; }
        public int      ORDINAL_POSITION            { get; set; }
        public string   COLUMN_DEFAULT              { get; set; }
        public string   IS_NULLABLE                 { get; set; }
        public string   DATA_TYPE                   { get; set; }
        public long?    CHARACTER_MAXIMUM_LENGTH    { get; set; }
        public long?    CHARACTER_OCTET_LENGTH      { get; set; }
        public int?     NUMERIC_PRECISION           { get; set; }
        public int?     NUMERIC_PRECISION_RADIX     { get; set; }
        public int?     NUMERIC_SCALE               { get; set; }
        public int?     DATETIME_PRECISION          { get; set; }
        public string   CHARACTER_SET_CATALOG       { get; set; }
        public string   CHARACTER_SET_SCHEMA        { get; set; }
        public string   COLLATION_CATALOG           { get; set; }
        public string   COLLATION_SCHEMA            { get; set; }
        public string   COLLATION_NAME              { get; set; }
        public string   DOMAIN_CATALOG              { get; set; }
        public string   DOMAIN_SCHEMA               { get; set; }
        public string   DOMAIN_NAME                 { get; set; }

        public bool     IsNullable      { get { return IS_NULLABLE != "NO"; } }
        public bool?    IsCaseSensitive { get { return COLLATION_NAME?.Contains("_CI_"); } }
    }
}
