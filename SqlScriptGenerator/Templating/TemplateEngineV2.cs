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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator
{
    /// <summary>
    /// The first attempt at a template engine paid lip service to using C#. This version is a bit
    /// more along the lines of Razor, it does the whole thing as a generated C# class.
    /// </summary>
    class TemplateEngineV2
    {
        private static readonly Regex SubstituteValueRegex = new Regex(@"(\{(?<value>.*?)\})");

        public TemplateSwitchesModel Switches { get; set; }

        public Options Options { get; set; }

        public TemplateModel Model { get; set; }

        public string ApplyTemplate(string templateSource)
        {
            if(Model == null)    throw new InvalidOperationException("Model not set");
            if(Switches == null) throw new InvalidOperationException("Switches not set");
            if(Options == null)  throw new InvalidOperationException("Options not set");

            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var @namespace = "GeneratorNamespace";
            var className = $"Generator{guid}";
            var source = GenerateSource(templateSource, @namespace, className);

            var compilerParameters = new CompilerParameters() {
                GenerateExecutable = false,
                GenerateInMemory = true,
            };
            compilerParameters.CompilerOptions = "/t:library /langversion:latest";
            compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");

            var codeProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
            var compileResults = codeProvider.CompileAssemblyFromSource(compilerParameters, source);

            if(compileResults.Errors.HasErrors) {
                var errors = String.Join(Environment.NewLine, compileResults.Errors.OfType<CompilerError>().Select(r => $"Line {r.Line}: {r.ErrorText}"));
                throw new InvalidOperationException($"Could not generate script:{Environment.NewLine}{errors}");
            }

            var assembly = compileResults.CompiledAssembly;
            var instance = assembly.CreateInstance($"{@namespace}.{className}");

            foreach(var modelProperty in Model.GetType().GetProperties()) {
                var modelValue = modelProperty.GetValue(Model, null);

                var instanceProperty = instance.GetType().GetProperty(modelProperty.Name);
                instanceProperty.SetValue(instance, modelValue);
            }

            var result = (string)instance.GetType().InvokeMember("GenerateScript", BindingFlags.InvokeMethod, null, instance, new object[]{});

            return result;
        }

        private string GenerateSource(string templateSource, string @namespace, string className)
        {
            var result = new StringBuilder();

            result.Append(@"
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using SqlScriptGenerator;

                namespace " + @namespace + @"
                {
                    public class " + className + @"
                    {
            ");

            foreach(var property in Model.GetType().GetProperties()) {
                result.AppendLine($"            public {TypeFormatter.FormatType(property.PropertyType)} {property.Name} {{ get; set; }}");
            }

            AddHelperMethods(result);

            result.Append(@"
                public string GenerateScript()
                {
                    var __output = new StringBuilder();
                    var __currentLine = new StringBuilder();

            ");

            var inCSharpBlock = false;
            var templateLines = templateSource.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            for(var lineIndex = 0;lineIndex < templateLines.Length;++lineIndex) {
                var originalLine = templateLines[lineIndex];
                var line = new StringBuilder(templateLines[lineIndex]);
                var trimmedLine = (line.ToString() ?? "").Trim();

                if(trimmedLine.StartsWith(TemplateSwitchesStorage.Prefix)) {
                    continue;
                } else if(trimmedLine == "/*;") {
                    if(inCSharpBlock) {
                        throw new InvalidOperationException($"Nested C# block at line {lineIndex + 1}");
                    }
                    inCSharpBlock = true;
                } else if(trimmedLine == ";*/") {
                    if(!inCSharpBlock) {
                        throw new InvalidOperationException($"Missing open C# block for close at line {lineIndex + 1}");
                    }
                    inCSharpBlock = false;
                } else if(trimmedLine.StartsWith("--;")) {
                    var cSharp = trimmedLine.Substring(3);
                    result.AppendLine($"#line {lineIndex + 1} // {originalLine}");
                    result.AppendLine(cSharp);
                } else if(inCSharpBlock) {
                    result.AppendLine($"#line {lineIndex + 1} // {originalLine}");
                    result.AppendLine(line.ToString());
                } else {
                    foreach(var match in SubstituteValueRegex.Matches(line.ToString()).OfType<Match>().OrderByDescending(r => r.Index)) {
                        var substituteValue = match.Groups["value"].Value ?? "";
                        if(substituteValue != "") {
                            var substituteWith = $"\" + ({substituteValue}).ToString() + \"";

                            line.Remove(match.Index, match.Length);
                            line.Insert(match.Index, substituteWith);
                        }
                    }

                    result.AppendLine("__currentLine.Clear();");
                    result.AppendLine($"#line {lineIndex + 1} // {originalLine}");
                    result.AppendLine($"__currentLine.Append(\"{line}\");");
                    result.AppendLine($"__output.AppendLine(__currentLine.ToString());");
                }
            }

            result.Append(@"
                            return __output.ToString();
                        }
                    }
                }
            ");

            return result.ToString();
        }

        private void AddHelperMethods(StringBuilder result)
        {
        }
    }
}
