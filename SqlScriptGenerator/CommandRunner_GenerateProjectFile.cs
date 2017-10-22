using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    class CommandRunner_GenerateProjectFile : CommandRunner
    {
        public override bool Run()
        {
            if(String.IsNullOrEmpty(Options.ProjectFileName)) OptionsParser.Usage("Project filename must be specified");

            if(File.Exists(Options.ProjectFileName)) {
                StdOut.WriteLine($"Updating {Options.ProjectFileName}");
                var existing = ProjectStorage.Load(Options.ProjectFileName);
                ProjectStorage.Save(existing);
            } else {
                StdOut.WriteLine($"Creating {Options.ProjectFileName}");
                var newFile = new ProjectModel() {
                    ProjectFileName = Options.ProjectFileName,
                };
                ProjectStorage.Save(newFile);
            }

            return true;
        }
    }
}
