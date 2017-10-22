using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace SqlScriptGenerator.Templating
{
    class TemplateState
    {
        public int LineIndex { get; set; }

        public int LineNumber => LineIndex + 1;

        public string[] TemplateLines { get; set; }

        public string Line => TemplateLines[LineIndex];

        public string TrimmedLine => (Line ?? "").Trim();

        public List<string> Output { get; } = new List<string>();

        public Dictionary<string, object> Variables { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Stack<CommandState_Loop> Loops { get; } = new Stack<CommandState_Loop>();

        public void MoveToLineIndex(int lineIndex)
        {
            // This is one less than the required index because the for loop through the lines will increment whatever we
            // set here
            LineIndex = lineIndex - 1;
        }

        public void AddPropertiesAsVariables<T>(T hostObject, int? lineNumberOverride = null)
        {
            foreach(var property in typeof(T).GetProperties()) {
                var name = property.Name;
                var value = property.GetValue(hostObject, null);
                AddVariable(name, value, lineNumberOverride);
            }
        }

        public void AddVariable(string name, object obj, int? lineNumberOverride = null)
        {
            if(!String.IsNullOrEmpty(name)) {
                if(Variables.ContainsKey(name)) {
                    ReportParserError($"Variable {name} has already been declared", lineNumberOverride);
                }

                Variables.Add(name, obj);
            }
        }

        public void AddOrReplaceVariable(string name, object obj, int? lineNumberOverride = null)
        {
            if(!String.IsNullOrEmpty(name)) {
                if(Variables.ContainsKey(name)) {
                    Variables[name] = obj;
                } else {
                    Variables.Add(name, obj);
                }
            }
        }

        public void RemoveVariable(string name, int? lineNumberOverride = null)
        {
            if(!String.IsNullOrEmpty(name)) {
                if(!Variables.ContainsKey(name)) {
                    ReportParserError($"Variable {name} cannot be removed, it does not exist", lineNumberOverride);
                }

                Variables.Remove(name);
            }
        }

        public object GetVariable(string name, int? lineNumberOverride = null)
        {
            if(!Variables.TryGetValue(name, out object result)) {
                ReportParserError($"Variable name {name} does not exist", lineNumberOverride);
            }

            return result;
        }

        public void ReportParserError(string exceptionMessage, int? lineNumberOverride = null)
        {
            throw new TemplateParsingException($"Line {lineNumberOverride ?? LineNumber}: {exceptionMessage}");
        }

        public NameCode SplitVariableNameIntoNameAndCode(string variableName)
        {
            var result = new NameCode();

            var dotIdx = variableName.IndexOf('.');
            result.Name = dotIdx == -1 ? variableName : variableName.Substring(0, dotIdx);
            result.Code = dotIdx == -1 ? null : variableName.Substring(dotIdx + 1);

            return result;
        }

        public object GetVariableNameObject(string substituteValue, int? lineNumberOverride = null)
        {
            var nameCode = SplitVariableNameIntoNameAndCode(substituteValue);

            var value = GetVariable(nameCode.Name, lineNumberOverride);
            var result = nameCode.Code == null ? value : EvaluateCode(value, nameCode.Code);

            return result;
        }

        public string GetVariableName(string substituteValue, int? lineNumberOverride = null)
        {
            return GetVariableNameObject(substituteValue, lineNumberOverride)?.ToString();
        }

        public object EvaluateCode(object host, string code, int? lineNumberOverride = null)
        {
            var type = host.GetType();
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var className = $"Evalulator{guid}";
            var source = @"
                using System;
                using System.Linq;

                namespace EvaluatorNamespace
                {
                    public class " + className + @"
                    {
                        public object Evaluate(" + TypeFormatter.FormatType(type) + @" host)
                        {
                            return host." + code + @";
                        }
                    }
                }
            ";

            var compilerParameters = new CompilerParameters() {
                GenerateExecutable = false,
                GenerateInMemory = true,
            };
            compilerParameters.CompilerOptions = "/t:library";
            compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");

            var codeProvider = new CSharpCodeProvider();
            var compileResults = codeProvider.CompileAssemblyFromSource(compilerParameters, source);

            if(compileResults.Errors.HasErrors) {
                var errors = String.Join(Environment.NewLine, compileResults.Errors.OfType<CompilerError>().Select(r => r.ErrorText));
                ReportParserError($"Could not resolve {host}.{code}: {errors}", lineNumberOverride);
            }

            var assembly = compileResults.CompiledAssembly;
            var instance = assembly.CreateInstance($"EvaluatorNamespace.{className}");
            var result = instance.GetType().InvokeMember("Evaluate", BindingFlags.InvokeMethod, null, instance, new object[] { host });

            return result;
        }
    }
}
