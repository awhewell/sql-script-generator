using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SqlScriptGenerator.Models
{
    class ProjectModel
    {
        [JsonIgnore]
        public string ProjectFileName { get; set; }

        [JsonIgnore]
        public string ProjectFolder { get { return String.IsNullOrEmpty(ProjectFileName) ? Environment.CurrentDirectory : Path.GetDirectoryName(Path.GetFullPath(ProjectFileName)); } }

        public string TemplateFolder { get; set; }

        [JsonIgnore]
        public string TemplateFolderFullPath { get { return GetFullPath(TemplateFolder); } }

        public string ScriptFolder { get; set; }

        [JsonIgnore]
        public string ScriptFolderFullPath { get { return GetFullPath(ScriptFolder); } }

        private string GetFullPath(string path)
        {
            path = (path ?? "").Trim();
            return Path.IsPathRooted(path) ? path : Path.Combine(ProjectFolder, path);
        }

        public static string ApplyPath(string fullPath, string fileName)
        {
            return String.IsNullOrEmpty(fullPath) || Path.IsPathRooted(fileName) ? fileName : Path.Combine(fullPath, fileName);
        }
    }
}
