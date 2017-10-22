using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Models
{
    public class TemplateModel
    {
        public string Database { get; set; }

        public string Schema { get; set; }

        public string Entity { get; set; }

        public List<ColumnModel> Columns { get; private set; } = new List<ColumnModel>();
    }
}
