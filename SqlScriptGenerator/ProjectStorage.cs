using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    static class ProjectStorage
    {
        public static ProjectModel Load(string fileName)
        {
            ProjectModel result = null;

            if(!String.IsNullOrEmpty(fileName)) {
                if(!File.Exists(fileName)) {
                    throw new FileNotFoundException($"Could not load project file {fileName}, it does not exist");
                }

                var jsonText = File.ReadAllText(fileName);
                result = JsonConvert.DeserializeObject<ProjectModel>(jsonText);
                result.ProjectFileName = fileName;
            }

            return result;
        }

        public static void Save(ProjectModel projectModel)
        {
            var folder = Path.GetDirectoryName(projectModel.ProjectFileName);
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            var jsonText = JsonConvert.SerializeObject(projectModel, Formatting.Indented);
            File.WriteAllText(projectModel.ProjectFileName, jsonText);
        }
    }
}
