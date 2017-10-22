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
            if(Options.DatabaseEngine == DatabaseEngine.None)   OptionsParser.Usage("Database engine must be specified");
            if(String.IsNullOrEmpty(Options.ConnectionString))  OptionsParser.Usage("Connection string must be supplied");

            StdOut.WriteLine($"Metadata dump");
            StdOut.WriteLine($"Connection string: {Options.ConnectionString}");
            StdOut.WriteLine($"Database:          {Options.DatabaseName}");

            var engine = DatabaseEngineFactory.CreateEngine(Options.DatabaseEngine);
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
