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
            object result = null;

            if(!String.IsNullOrEmpty(name) && !Variables.TryGetValue(name, out result)) {
                ReportParserError($"Variable name {name} does not exist", lineNumberOverride);
            }

            return result;
        }

        public void ReportParserError(string exceptionMessage, int? lineNumberOverride = null)
        {
            throw new TemplateParsingException($"Line {lineNumberOverride ?? LineNumber}: {exceptionMessage}");
        }

        public object GetVariableNameObject(string substituteValue, int? lineNumberOverride = null)
        {
            var variableRef = VariableReference.Parse(substituteValue);

            var value = GetVariable(variableRef.Name, lineNumberOverride);
            var result = value;
            if(variableRef.EvalCode != null) {
                var executionResult = Evaluator_CSharp.Evaluate(substituteValue, Variables);
                if(executionResult.ParserError != null) {
                    ReportParserError(executionResult.ParserError, lineNumberOverride);
                } else {
                    result = executionResult.Result;
                }
            }

            return result;
        }

        public string GetVariableName(string substituteValue, int? lineNumberOverride = null)
        {
            return GetVariableNameObject(substituteValue, lineNumberOverride)?.ToString();
        }
    }
}
