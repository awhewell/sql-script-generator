using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Models
{
    public class ColumnModel
    {
        public string Name { get; private set; }

        public ColumnCollection Parent { get; private set; }

        public int Ordinal { get; private set; }

        public ColumnModel(ColumnCollection parent, string name, int ordinal)
        {
            Parent =    parent;
            Name =      name;
            Ordinal =   ordinal;
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
