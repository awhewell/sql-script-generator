using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    abstract class CommandRunner
    {
        public Options Options { get; set; }

        public abstract bool Run();
    }
}
