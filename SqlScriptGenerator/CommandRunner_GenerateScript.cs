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
            if(String.IsNullOrEmpty(Options.TemplateFileName))  OptionsParser.Usage("Missing template filename");
            if(String.IsNullOrEmpty(Options.ScriptFileName))    OptionsParser.Usage("Missing script filename");
            if(String.IsNullOrEmpty(Options.EntityName))        OptionsParser.Usage("Missing entity name");
            if(Options.DatabaseEngine == DatabaseEngine.None)   OptionsParser.Usage("Database engine must be specified");
            if(String.IsNullOrEmpty(Options.ConnectionString))  OptionsParser.Usage("Connection string must be supplied");

            var project = ProjectStorage.Load(Options.ProjectFileName);
            var templateFileName = ProjectModel.ApplyPath(project?.TemplateFolderFullPath, Options.TemplateFileName);
            var scriptFileName = ProjectModel.ApplyPath(project?.ScriptFolderFullPath, Options.ScriptFileName);

            StdOut.WriteLine($"Project:    {Options.ProjectFileName}");
            StdOut.WriteLine($"Template:   {templateFileName}");
            StdOut.WriteLine($"Script:     {scriptFileName}");
            StdOut.WriteLine($"Connection: {Options.ConnectionString}");
            StdOut.WriteLine($"Entity:     {Options.EntityName}");

            var engine = DatabaseEngineFactory.CreateEngine(Options.DatabaseEngine);
            var database = engine.ReadDatabaseMetadata(Options);

            var model = new TemplateModel() {
                Database = database.Name,
            };

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
