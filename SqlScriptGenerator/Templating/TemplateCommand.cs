using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{
    abstract class TemplateCommand
    {
        public TemplateState State { get; set; }

        public int EndBlockLineIndex { get; set; } = -1;

        public abstract void Process(string args);
    }
}
