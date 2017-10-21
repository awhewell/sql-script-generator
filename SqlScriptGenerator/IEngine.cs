using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    interface IEngine
    {
        DatabaseModel ReadDatabaseMetadata(Options options);
    }
}
