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
        public override bool Run()
        {
            ProjectStorage.LoadOptionDefaultsFromProject(Options.ProjectFileName, Options);

            if(String.IsNullOrEmpty(Options.TemplateFileName))  OptionsParser.Usage("Missing template filename");
            if(String.IsNullOrEmpty(Options.EntityName))        OptionsParser.Usage("Missing entity name");
            if(Options.DatabaseEngine == DatabaseEngine.None)   OptionsParser.Usage("Database engine must be specified");
            if(String.IsNullOrEmpty(Options.ConnectionString))  OptionsParser.Usage("Connection string must be supplied");

            var project = ProjectStorage.Load(Options.ProjectFileName);

            var engine = DatabaseEngineFactory.CreateEngine(Options.DatabaseEngine);
            var database = engine.ReadDatabaseMetadata(Options);
            var entityPathParts = EntityResolver.SplitEntityPathParts(Options.EntityName);
            var entity = EntityResolver.Resolve(database, entityPathParts);
            if(entity == null) {
                OptionsParser.Usage($"Could not resolve entity path {Options.EntityName}");
            }

            var model = new TemplateModel() {
                Database = database,
                Schema = EntityResolver.FindSchema(entity),
                Entity = entity,
                Table = entity as TableModel,
                View = entity as ViewModel,
                UDTT = entity as UserDefinedTableTypeModel,
            };
            if(entity is ColumnCollection columnCollection) {
                model.Columns.AddRange(columnCollection?.Columns.Values.OrderBy(r => r.Ordinal));
            }

            var templateFileName = ProjectModel.ApplyPath(project?.TemplateFolderFullPath, Options.TemplateFileName);
            var templateSwitches = TemplateSwitchesStorage.LoadFromTemplate(templateFileName);
            if(templateSwitches.ParseErrors.Count > 0) {
                StdOut.WriteLine($"Template switch errors in {templateFileName}");
                foreach(var parseError in templateSwitches.ParseErrors) {
                    StdOut.WriteLine(parseError);
                }
                Environment.Exit(1);
            }

            if(String.IsNullOrEmpty(Options.ScriptFileName)) {
                Options.ScriptFileName = model.ApplyModelToFileSpec(templateSwitches.FileSpec);
            }
            if(String.IsNullOrEmpty(Options.ScriptFileName)) {
                OptionsParser.Usage("Script filename must either be supplied or specified as a FILESPEC switch in the template");
            }
            var scriptFileName = ProjectModel.ApplyPath(project?.ScriptFolderFullPath, Options.ScriptFileName);

            StdOut.WriteLine($"Project:    {Options.ProjectFileName}");
            StdOut.WriteLine($"Connection: {engine.SanitiseConnectionString(Options.ConnectionString)}");
            StdOut.WriteLine($"Entity:     {Options.EntityName}");
            StdOut.WriteLine($"Template:   {templateFileName}");
            StdOut.WriteLine($"Script:     {scriptFileName}");

            var generator = new Generator();
            var content = generator.ApplyModelToTemplate(templateFileName, model);

            var folder = Path.GetDirectoryName(Path.GetFullPath(scriptFileName));
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(scriptFileName, content);

            return true;
        }
    }
}
