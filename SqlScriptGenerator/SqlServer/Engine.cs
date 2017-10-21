using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlScriptGenerator.Models;
using Dapper;
using System.Data.SqlClient;
using System.Data;

namespace SqlScriptGenerator.SqlServer
{
    class Engine : IEngine
    {
        private SqlConnection CreateOpenConnection(Options options)
        {
            var builder = CreateConnectionStringBuilder(options);
            var result = new SqlConnection(builder.ConnectionString);
            result.Open();

            return result;
        }

        private SqlConnectionStringBuilder CreateConnectionStringBuilder(Options options, string databaseName = null)
        {
            if(String.IsNullOrEmpty(options.ConnectionString)) {
                OptionsParser.Usage("Missing connection string");
            }

            var result = new SqlConnectionStringBuilder(options.ConnectionString);

            databaseName = databaseName ?? options.DatabaseName;
            if(!String.IsNullOrEmpty(databaseName)) {
                result.InitialCatalog = databaseName;
            }

            if(options.AskForPassword) {
                result.Password = OptionsParser.AskForPassword();
            }

            return result;
        }

        private string GetDatabaseName(SqlConnection connection)
        {
            return connection.QuerySingle<string>("SELECT DB_NAME()");
        }

        public DatabaseModel ReadDatabaseMetadata(Options options)
        {
            DatabaseModel result = null;

            using(var connection = CreateOpenConnection(options)) {
                var databaseMeta = connection.QuerySingleOrDefault<Models.SysDatabases>(@"
                    SELECT [name]
                          ,ISNULL([collation_name], (SELECT [collation_name] FROM sys.databases WHERE [name] = 'model')) as [collation_name]
                    FROM   [sys].[databases]
                    WHERE  [name] = @databaseName
                ", new {
                    @databaseName = GetDatabaseName(connection),
                });

                if(databaseMeta != null) {
                    result = new DatabaseModel(databaseMeta.name, databaseMeta.collation_name?.Contains("_CI_") ?? true);
                    ReadSchemaMetadata(connection, result);
                }
            }

            return result;
        }

        private void ReadSchemaMetadata(SqlConnection connection, DatabaseModel database)
        {
            database.Schemas.Clear();

            foreach(var schemaMeta in connection.Query<Models.IS_Schemata>($@"
                SELECT *
                FROM   INFORMATION_SCHEMA.SCHEMATA
                WHERE  [CATALOG_NAME] = @databaseName
                AND    [SCHEMA_NAME] NOT IN ('guest', 'INFORMATION_SCHEMA', 'sys')
                AND    [SCHEMA_NAME] NOT LIKE 'db!_%' ESCAPE '!'
            ", new {
                @databaseName = database.Name
            }).ToArray()) {
                var schema = new SchemaModel(database, schemaMeta.SCHEMA_NAME, database.IsCaseSensitive);
                database.Schemas.Add(schema.Name, schema);
                ReadTableMetadata(connection, schema);
            }
        }

        private void ReadTableMetadata(SqlConnection connection, SchemaModel schema)
        {
            schema.Tables.Clear();

            foreach(var tableMeta in connection.Query<Models.IS_Table>($@"
                SELECT *
                FROM   INFORMATION_SCHEMA.TABLES
                WHERE  [TABLE_CATALOG] = @databaseName
                AND    [TABLE_SCHEMA] = @schemaName
                AND    [TABLE_TYPE] = 'BASE TABLE'
            ", new {
                @databaseName = schema.Database.Name,
                @schemaName = schema.Name,
            }).ToArray()) {
                var table = new TableModel(schema, tableMeta.TABLE_NAME, schema.IsCaseSensitive);
                schema.Tables.Add(table.Name, table);
                ReadInformationSchemaColumnMetadata(connection, schema, table);
            }
        }

        private void ReadInformationSchemaColumnMetadata(SqlConnection connection, SchemaModel schema, ColumnCollection columnCollection)
        {
            columnCollection.Columns.Clear();

            foreach(var columnMeta in connection.Query<Models.IS_Column>($@"
                SELECT *
                FROM   INFORMATION_SCHEMA.COLUMNS
                WHERE  [TABLE_CATALOG] = @databaseName
                AND    [TABLE_SCHEMA] = @schemaName
                AND    [TABLE_NAME] = @tableName
            ", new {
                @databaseName = schema.Database.Name,
                @schemaName = schema.Name,
                @tableName = columnCollection.Name,
            }).ToArray()) {
                var column = new ColumnModel(columnCollection, columnMeta.COLUMN_NAME, columnMeta.ORDINAL_POSITION);
                columnCollection.Columns.Add(column.Name, column);
            }
        }
    }
}
