// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    class CommandRunner_DumpMetadata : CommandRunner
    {
        public override bool Run()
        {
            ProjectStorage.LoadOptionDefaultsFromProject(Options.ProjectFileName, Options);

            if(Options.DatabaseEngine == DatabaseEngine.None)   OptionsParser.Usage("Database engine must be specified");
            if(String.IsNullOrEmpty(Options.ConnectionString))  OptionsParser.Usage("Connection string must be supplied");

            var engine = DatabaseEngineFactory.CreateEngine(Options.DatabaseEngine);
            StdOut.WriteLine($"Metadata dump");
            StdOut.WriteLine($"Connection string: {engine.SanitiseConnectionString(Options.ConnectionString)}");

            var database = engine.ReadDatabaseMetadata(Options);
            DumpMetadata(database);

            return true;
        }

        public static void DumpMetadata(DatabaseModel database)
        {
            StdOut.WriteLine($"[{database.Name}]");
            foreach(var schema in database.Schemas.Values.OrderBy(r => r.Name)) {
                DumpMetadata(schema);
            }
        }

        private static void DumpMetadata(SchemaModel schema)
        {
            StdOut.WriteLine($"    [{schema.Name}]");
            foreach(var table in schema.Tables.Values.OrderBy(r => r.Name)) {
                DumpMetadata(table);
            }
            foreach(var view in schema.Views.Values.OrderBy(r => r.Name)) {
                DumpMetadata(view);
            }
            foreach(var udtt in schema.UserDefinedTableTypes.Values.OrderBy(r => r.Name)) {
                DumpMetadata(udtt);
            }
        }

        private static void DumpMetadata(ColumnCollection columnCollection)
        {
            StdOut.WriteLine($"        [{columnCollection.Name}]");
            foreach(var column in columnCollection.Columns.Values.OrderBy(r => r.Ordinal)) {
                DumpMetadata(column);
            }
        }

        private static void DumpMetadata(ColumnModel column)
        {
            StdOut.WriteLine($"            [{column.Name}]");
        }
    }
}
