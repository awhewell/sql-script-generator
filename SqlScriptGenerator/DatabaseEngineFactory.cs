using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    static class DatabaseEngineFactory
    {
        public static IEngine CreateEngine(DatabaseEngine engine)
        {
            switch(engine) {
                case DatabaseEngine.None:       return null;
                case DatabaseEngine.SqlServer:  return new SqlServer.SqlServerEngine();
                default:                        throw new NotImplementedException();
            }
        }
    }
}
