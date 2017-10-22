using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Models
{
    public class DatabaseModel
    {
        public string Name { get; private set; }

        public bool IsCaseSensitive { get; private set; }

        public Dictionary<string, SchemaModel> Schemas { get; private set; } = new Dictionary<string, SchemaModel>();

        public DatabaseModel(string name, bool isCaseSensitive)
        {
            Name = name;
            IsCaseSensitive = isCaseSensitive;

            if(!IsCaseSensitive) {
                Schemas = new Dictionary<string, SchemaModel>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
