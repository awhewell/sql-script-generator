using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Models
{
    public class ColumnCollection
    {
        public string Name { get; private set; }

        public bool IsCaseSensitive { get; private set; }

        public Dictionary<string, ColumnModel> Columns { get; private set; } = new Dictionary<string, ColumnModel>();

        public ColumnCollection(string name, bool isCaseSensitive)
        {
            Name = name;
            IsCaseSensitive = isCaseSensitive;

            if(!IsCaseSensitive) {
                Columns = new Dictionary<string, ColumnModel>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
