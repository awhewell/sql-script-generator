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

        public static void LoadOptionDefaultsFromProject(string projectFileName, Options options)
        {
            if(!String.IsNullOrEmpty(projectFileName)) {
                var model = Load(projectFileName);

                if(String.IsNullOrEmpty(options.ConnectionString)) {
                    options.ConnectionString = model.ConnectionString;
                }
                if(options.DatabaseEngine == DatabaseEngine.None) {
                    options.DatabaseEngine = model.DatabaseEngine;
                }
            }
        }

        public static void Save(ProjectModel projectModel)
        {
            var folder = Path.GetDirectoryName(projectModel.ProjectFileName);
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            var jsonText = JsonConvert.SerializeObject(projectModel);

            var formattedClone = JsonConvert.DeserializeObject<ProjectModel>(jsonText);
            formattedClone.EntityTemplates.Sort((lhs, rhs) => String.Compare(lhs.Entity, rhs.Entity, ignoreCase: true));
            jsonText = JsonConvert.SerializeObject(formattedClone, Formatting.Indented);

            File.WriteAllText(projectModel.ProjectFileName, jsonText);
        }
    }
}
