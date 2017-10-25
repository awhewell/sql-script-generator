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
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace SqlScriptGenerator
{
    static class Evaluator_CSharp
    {
        public static EvaluationResult Evaluate(string code, IDictionary<string, object> variables)
        {
            var result = new EvaluationResult();
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var className = $"Evalulator{guid}";

            var propertyDeclarations = String.Join("\r\n", variables.Select(r => {
                var variableType = TypeFormatter.FormatType(r.Value == null ? typeof(object) : r.Value.GetType());
                return $"public {variableType} {r.Key} {{ get; set; }}";
            }));

            var source = @"
                using System;
                using System.Linq;

                namespace EvaluatorNamespace
                {
                    public class " + className + @"
                    {
                        " + propertyDeclarations + @"

                        public object Evaluate()
                        {
                            return " + code + @";
                        }
                    }
                }
            ";

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
                var errors = String.Join(Environment.NewLine, compileResults.Errors.OfType<CompilerError>().Select(r => r.ErrorText));
                result.ParserError = $"Could not resolve {code}: {errors}";
            }

            if(result.ParserError == null) {
                var assembly = compileResults.CompiledAssembly;
                var instance = assembly.CreateInstance($"EvaluatorNamespace.{className}");

                foreach(var variable in variables) {
                    var property = instance.GetType().GetProperty(variable.Key);
                    property.SetValue(instance, variable.Value);
                }

                result.Result = instance.GetType().InvokeMember("Evaluate", BindingFlags.InvokeMethod, null, instance, new object[]{});
            }

            return result;
        }
    }
}
