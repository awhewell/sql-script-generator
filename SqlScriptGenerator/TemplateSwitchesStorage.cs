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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    static class TemplateSwitchesStorage
    {
        public const string Prefix = "--#";
        private static readonly Regex SwitchLineKeyValueRegex = new Regex(Prefix + @"\s*(?<key>\S+)(\s*(?<value>.*))?");
        private static readonly Regex SwitchLinesRegex =        new Regex(@"(?<switchLine>^\s*" + Prefix + ".*\r?\n)", RegexOptions.Multiline);

        public static TemplateSwitchesModel LoadFromTemplate(string templateFileName)
        {
            var result = new TemplateSwitchesModel();

            if(!String.IsNullOrEmpty(templateFileName)) {
                foreach(var line in LoadTemplateSwitchLines(templateFileName).Select(r => r.Trim())) {
                    var match = SwitchLineKeyValueRegex.Match(line);
                    var key = match.Groups["key"].Value;
                    var value = (match.Groups["value"].Value ?? "").Trim();
                    if(!match.Success || String.IsNullOrEmpty(key)) {
                        result.ParseErrors.Add($"Invalid template switch line: \"{line}\"");
                        continue;
                    }

                    var needsValue = false;
                    switch(key.ToLower()) {
                        case "filespec":    result.FileSpec = value; needsValue = true; break;
                        default:
                            result.ParseErrors.Add($"Unknown template switch \"{key}\"");
                            break;
                    }

                    if(needsValue && value == "") {
                        result.ParseErrors.Add($"Missing template switch value in \"{line}\"");
                    }
                }
            }

            return result;
        }

        private static string[] LoadTemplateSwitchLines(string templateFileName)
        {
            if(!File.Exists(templateFileName)) {
                throw new FileNotFoundException($"Cannot load template {templateFileName}, it does not exist");
            }

            return File.ReadAllLines(templateFileName).Where(r => r.Trim().StartsWith(Prefix)).ToArray();
        }

        public static string RemoveSwitchesFromSource(string templateSource)
        {
            return SwitchLinesRegex.Replace(templateSource, "");
        }
    }
}
