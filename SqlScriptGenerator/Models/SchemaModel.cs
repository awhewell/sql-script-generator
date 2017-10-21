using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Models
{
    class SchemaModel
    {
        public string Name { get; private set; }

        public DatabaseModel Database { get; private set; }

        public bool IsCaseSensitive { get; private set; }

        public Dictionary<string, TableModel> Tables { get; private set; } = new Dictionary<string, TableModel>();

        public SchemaModel(DatabaseModel parent, string name, bool isCaseSensitive)
        {
            Database = parent;
            Name = name;
            IsCaseSensitive = isCaseSensitive;

            if(!IsCaseSensitive) {
                Tables = new Dictionary<string, TableModel>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
