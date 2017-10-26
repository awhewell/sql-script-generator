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
using Dapper;
using System.Data.SqlClient;
using System.Data;

namespace SqlScriptGenerator.SqlServer
{
    class SqlServerEngine : IEngine
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

        private int GetObjectId(SqlConnection connection, params string[] nameParts)
        {
            return connection.QuerySingle<int>("SELECT OBJECT_ID(@nameParts)", new {
                @nameParts = String.Join(".", nameParts.Select(r => $"[{r}]")),
            });
        }

        public string SanitiseConnectionString(string connectionString)
        {
            var builder = CreateConnectionStringBuilder(new Options() { ConnectionString = connectionString, AskForPassword = false });
            if(!String.IsNullOrEmpty(builder.Password)) {
                builder.Password = "******";
            }

            return builder.ConnectionString;
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
                ReadViewMetadata(connection, schema);
                ReadUserDefinedTableTypeMetadata(connection, schema);
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
                ReadSysColumnMetadata(connection, schema, table, GetObjectId(connection, schema.Name, table.Name));
            }
        }

        private void ReadViewMetadata(SqlConnection connection, SchemaModel schema)
        {
            schema.Views.Clear();

            foreach(var viewMeta in connection.Query<Models.IS_Table>($@"
                SELECT *
                FROM   INFORMATION_SCHEMA.TABLES
                WHERE  [TABLE_CATALOG] = @databaseName
                AND    [TABLE_SCHEMA] = @schemaName
                AND    [TABLE_TYPE] = 'VIEW'
            ", new {
                @databaseName = schema.Database.Name,
                @schemaName = schema.Name,
            }).ToArray()) {
                var view = new ViewModel(schema, viewMeta.TABLE_NAME, schema.IsCaseSensitive);
                schema.Views.Add(view.Name, view);
                ReadSysColumnMetadata(connection, schema, view, GetObjectId(connection, schema.Name, view.Name));
            }
        }

        private void ReadUserDefinedTableTypeMetadata(SqlConnection connection, SchemaModel schema)
        {
            schema.UserDefinedTableTypes.Clear();

            foreach(var udttMeta in connection.Query<Models.SysTableTypes>($@"
                SELECT [udtt].*
                      ,[schema].[name] AS [schema_name]
                FROM   [sys].[table_types] AS [udtt]
                JOIN   [sys].[schemas]     AS [schema] ON [udtt].[schema_id] = [schema].[schema_id]
                WHERE  [schema].[name] = @schemaName
            ", new {
                @schemaName = schema.Name,
            }).ToArray()) {
                var udtt = new UserDefinedTableTypeModel(schema, udttMeta.name, schema.IsCaseSensitive);
                schema.UserDefinedTableTypes.Add(udtt.Name, udtt);
                ReadSysColumnMetadata(connection, schema, udtt, udttMeta.type_table_object_id);
            }
        }

        private void ReadSysColumnMetadata(SqlConnection connection, SchemaModel schema, ColumnCollection columnCollection, int objectId)
        {
            columnCollection.Columns.Clear();

            foreach(var columnMeta in connection.Query<Models.SysColumns>($@"
                SELECT    [syscol].*
                         ,[type].[name] AS [type_name]
                         ,[type].[precision] AS [type_precision]
                         ,[type].[scale] AS [type_scale]
                         ,[type].[is_nullable] AS [type_is_nullable]
                         ,[default].[definition] AS [default_definition]
                FROM      [sys].[columns]             AS [syscol]
                JOIN      [sys].[types]               AS [type]    ON  [syscol].[system_type_id] = [type].[system_type_id]
                                                                   AND [syscol].[user_type_id] = [type].[user_type_id]
                LEFT JOIN [sys].[default_constraints] AS [default] ON  [syscol].[default_object_id] = [default].[object_id]
                WHERE  [object_id] = @objectId
            ", new {
                @objectId = objectId
            }).ToArray()) {
                columnCollection.Columns.Add(columnMeta.name, new ColumnModel(
                    columnCollection,
                    columnMeta.name,
                    columnMeta.column_id,
                    FormatSqlType(columnMeta),
                    hasDefaultValue:    !String.IsNullOrEmpty(columnMeta.default_definition),
                    isIdentity:         columnMeta.is_identity,
                    isNullable:         columnMeta.is_nullable ?? columnMeta.type_is_nullable,
                    isComputed:         columnMeta.is_computed,
                    isCaseSensitive:    string.IsNullOrEmpty(columnMeta.collation_name) ? columnCollection.IsCaseSensitive : !columnMeta.collation_name.Contains("_CI_")
                ));
            }
        }

        private string FormatSqlType(Models.SysColumns columnMeta)
        {
            var result = new StringBuilder(columnMeta.type_name);

            switch(result.ToString().ToLower()) {
                case "binary":
                case "char":
                case "nchar":
                    result.Append($"({columnMeta.max_length})");
                    break;
                case "nvarchar":
                case "varbinary":
                case "varchar":
                    result.AppendFormat("({0})", columnMeta.max_length == -1 ? "max" : columnMeta.max_length.ToString());
                    break;
                case "decimal":
                case "numeric":
                    if(columnMeta.precision != 18) {
                        result.Append($"({columnMeta.precision}, {columnMeta.scale})");
                    }
                    break;
                case "float":
                case "real":
                    if(columnMeta.precision != columnMeta.type_precision) {
                        result.Append($"({columnMeta.precision})");
                    }
                    break;
                case "datetime2":
                case "datetimeoffset":
                case "time":
                    if(columnMeta.scale != columnMeta.type_scale) {
                        result.Append($"({columnMeta.scale})");
                    }
                    break;
            }

            return result.ToString();
        }
    }
}
