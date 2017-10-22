using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SqlScriptGenerator.Models;

namespace SqlScriptGenerator.Templating
{
    class TemplateEngine
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

            var state = new TemplateState() {
                TemplateLines = templateSource.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None),
            };
            state.AddPropertiesAsVariables(Model);

            for(state.LineIndex = 0;state.LineIndex < state.TemplateLines.Length;++state.LineIndex) {
                var line = state.Line;
                var trimmedLine = state.TrimmedLine;

                if(trimmedLine.StartsWith(TemplateSwitchesStorage.Prefix)) {
                    continue;
                } else if(trimmedLine.StartsWith(TemplateCommandParser.Prefix)) {
                    TemplateCommandParser.Parse(state);
                } else {
                    var outputLine = SubstituteVariables(line, state);
                    state.Output.Add(outputLine);
                }
            }

            return String.Join(Environment.NewLine, state.Output);
        }

        private string SubstituteVariables(string line, TemplateState state)
        {
            var result = new StringBuilder(line);

            foreach(var match in SubstituteValueRegex.Matches(line).OfType<Match>().OrderByDescending(r => r.Index)) {
                var substituteValue = match.Groups["value"].Value ?? "";
                if(substituteValue != "") {
                    var substituteWith = state.GetVariableName(substituteValue);

                    result.Remove(match.Index, match.Length);
                    result.Insert(match.Index, substituteWith);
                }
            }

            return result.ToString();
        }
    }
}
