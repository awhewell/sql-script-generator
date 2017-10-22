﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    class Options
    {
        public Command Command { get; set; }

        public DatabaseEngine DatabaseEngine { get; set; } = DatabaseEngine.SqlServer;

        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public bool AskForPassword { get; set; }
    }
}