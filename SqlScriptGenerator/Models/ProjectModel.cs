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
        public string ProjectFolder { get { return Path.GetDirectoryName(Path.GetFullPath(ProjectFileName)); } }

        public string TemplateRootFolder { get; set; }

        [JsonIgnore]
        public string TemplateFolderFullPath { get { return GetFullPath(TemplateRootFolder); } }

        public string OutputRootFolder { get; set; }

        [JsonIgnore]
        public string OutputRootFullPath { get { return GetFullPath(OutputRootFolder); } }

        private string GetFullPath(string path)
        {
            path = (path ?? "").Trim();
            return Path.IsPathRooted(path) ? path : Path.Combine(ProjectFolder, path);
        }
    }
}
