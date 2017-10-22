using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Models
{
    class ViewModel : ColumnCollection
    {
        public SchemaModel Schema { get; private set; }

        public ViewModel(SchemaModel parent, string name, bool isCaseSensitive) : base(name, isCaseSensitive)
        {
            Schema = parent;
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
