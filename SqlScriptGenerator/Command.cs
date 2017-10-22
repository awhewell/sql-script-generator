using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    public enum Command
    {
        None,

        DumpMetadata,

        GenerateProjectFile,

        GenerateScript,
    }
}
