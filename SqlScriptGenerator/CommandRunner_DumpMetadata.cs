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

            Stdout.WriteLine($"Metadata dump");
            Stdout.WriteLine($"Connection string: {Options.ConnectionString}");
            Stdout.WriteLine($"Database:          {Options.DatabaseName}");

            var engine = DatabaseEngineFactory.CreateEngine(Options.DatabaseEngine);
            var database = engine.ReadDatabaseMetadata(Options);
            DumpMetadata(database);

            return true;
        }

        public static void DumpMetadata(DatabaseModel database)
        {
            Stdout.WriteLine($"[{database.Name}]");
            foreach(var schema in database.Schemas.Values.OrderBy(r => r.Name)) {
                DumpMetadata(schema);
            }
        }

        private static void DumpMetadata(SchemaModel schema)
        {
            Stdout.WriteLine($"    [{schema.Name}]");
            foreach(var table in schema.Tables.Values.OrderBy(r => r.Name)) {
                DumpMetadata(table);
            }
        }

        private static void DumpMetadata(ColumnCollection columnCollection)
        {
            Stdout.WriteLine($"        [{columnCollection.Name}]");
            foreach(var column in columnCollection.Columns.Values.OrderBy(r => r.Ordinal)) {
                DumpMetadata(column);
            }
        }

        private static void DumpMetadata(ColumnModel column)
        {
            Stdout.WriteLine($"            [{column.Name}]");
        }
    }
}
