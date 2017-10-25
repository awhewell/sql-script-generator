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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    class CommandRunner_GenerateScript : CommandRunner
    {
        private int _Iteration;

        public override bool Run()
        {
            ProjectStorage.LoadOptionDefaultsFromProject(Options.ProjectFileName, Options);

            if(Options.DatabaseEngine == DatabaseEngine.None)   OptionsParser.Usage("Database engine must be specified");
            if(String.IsNullOrEmpty(Options.ConnectionString))  OptionsParser.Usage("Connection string must be supplied");

            var project = ProjectStorage.Load(Options.ProjectFileName);
            var engine = DatabaseEngineFactory.CreateEngine(Options.DatabaseEngine);
            var database = engine.ReadDatabaseMetadata(Options);

            if(!String.IsNullOrEmpty(Options.TemplateFileName) && !String.IsNullOrEmpty(Options.EntityName)) {
                GenerateScript(Options.TemplateFileName, Options.EntityName, project, engine, database, isMultiScriptCreate: false);
            } else if(!String.IsNullOrEmpty(Options.TemplateFileName)) {
                if(project == null) {
                    OptionsParser.Usage("Missing project");
                }
                foreach(var entityTemplate in project.EntityTemplates.Where(r => r.Templates.Contains(Options.TemplateFileName, StringComparer.OrdinalIgnoreCase))) {
                    GenerateScript(Options.TemplateFileName, entityTemplate.Entity, project, engine, database, isMultiScriptCreate: true);
                }
            } else if(!String.IsNullOrEmpty(Options.EntityName)) {
                if(project == null) {
                    OptionsParser.Usage("Missing project");
                }
                var entityTemplate = project.EntityTemplates.FirstOrDefault(r => String.Equals(r.Entity, Options.EntityName, StringComparison.OrdinalIgnoreCase));
                if(entityTemplate == null) {
                    OptionsParser.Usage($"Cannot find the entity {Options.EntityName} in the project entity templates - did you specify the full name?");
                }
                foreach(var templateFileName in entityTemplate.Templates) {
                    GenerateScript(templateFileName, entityTemplate.Entity, project, engine, database, isMultiScriptCreate: true);
                }
            } else if(project == null) {
                OptionsParser.Usage("You must specify the project if you want to regenerate all templates for all entities");
            } else {
                foreach(var entityTemplate in project.EntityTemplates) {
                    foreach(var templateFileName in entityTemplate.Templates) {
                        GenerateScript(templateFileName, entityTemplate.Entity, project, engine, database, isMultiScriptCreate: true);
                    }
                }
            }

            return true;
        }

        private void GenerateScript(string templateFileName, string entityName, ProjectModel project, IEngine engine, DatabaseModel database, bool isMultiScriptCreate)
        {
            ++_Iteration;

            var entityPathParts = EntityResolver.SplitEntityPathParts(entityName);
            var entity = EntityResolver.Resolve(database, entityPathParts);
            if(entity == null) {
                OptionsParser.Usage($"Could not resolve entity path {entityName}");
            }

            var model = new TemplateModel(entity?.IsCaseSensitive ?? database.IsCaseSensitive) {
                Database = database,
                Schema = EntityResolver.FindSchema(entity),
                Entity = entity,
                Table = entity as TableModel,
                View = entity as ViewModel,
                UDTT = entity as UserDefinedTableTypeModel,
            };
            if(entity is ColumnCollection columnCollection) {
                var ordinal = 0;
                foreach(var col in columnCollection.Columns.Values.OrderBy(r => r.Ordinal)) {
                    model.Columns.Add(col);
                    model.ColumnsByName.Add(col.Name, col);
                    model.ColumnsByOrdinal.Add(ordinal++, col);
                }
            }

            templateFileName = ProjectModel.ApplyPath(project?.TemplateFolderFullPath, templateFileName);
            var templateSwitches = TemplateSwitchesStorage.LoadFromTemplate(templateFileName);
            if(templateSwitches.ParseErrors.Count > 0) {
                StdOut.WriteLine($"Template switch errors in {templateFileName}");
                foreach(var parseError in templateSwitches.ParseErrors) {
                    StdOut.WriteLine(parseError);
                }
                Environment.Exit(1);
            }

            var scriptFileName = Options.ScriptFileName;
            if(isMultiScriptCreate && !String.IsNullOrEmpty(scriptFileName)) {
                OptionsParser.Usage("You can only specify a script filename when generating a single script");
            }
            if(String.IsNullOrEmpty(scriptFileName)) {
                scriptFileName = model.ApplyModelToFileSpec(templateSwitches.FileSpec);
            }
            if(String.IsNullOrEmpty(scriptFileName)) {
                OptionsParser.Usage("Script filename must either be supplied or specified as a FILESPEC switch in the template");
            }
            scriptFileName = ProjectModel.ApplyPath(project?.ScriptFolderFullPath, scriptFileName);

            if(_Iteration == 1) {
                StdOut.WriteLine($"Project:    {Options.ProjectFileName}");
                StdOut.WriteLine($"Connection: {engine.SanitiseConnectionString(Options.ConnectionString)}");
            }
            StdOut.WriteLine("-------------------------------------------------------------------------------");
            StdOut.WriteLine($"Entity:     {entityName}");
            StdOut.WriteLine($"Template:   {templateFileName}");
            StdOut.WriteLine($"Script:     {scriptFileName}");

            /*
            var templateEngine = new Templating.TemplateEngine() {
                Model = model,
                Options = Options,
                Switches = templateSwitches,
            };
            */
            var templateEngine = new TemplateEngineV2() {
                Model = model,
                Options = Options,
                Switches = templateSwitches,
            };
            var content = templateEngine.ApplyTemplate(File.ReadAllText(templateFileName));

            var folder = Path.GetDirectoryName(Path.GetFullPath(scriptFileName));
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(scriptFileName, content);
        }
    }
}
