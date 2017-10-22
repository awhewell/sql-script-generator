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
        private static readonly Regex SubstituteFormatRegex = new Regex(@"(\<(?<format>.*?)\>)");
        private static readonly Regex FormatLoopTernaryRegex = new Regex(@"^(?<first>.*?)(?<!\\)\|(?<other>.*?)(?<!\\)\|(?<last>.*?)$");

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
                    outputLine = SubstituteFormatting(outputLine, state);
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

        private string SubstituteFormatting(string line, TemplateState state)
        {
            var result = new StringBuilder(line);

            foreach(var match in SubstituteFormatRegex.Matches(line).OfType<Match>().OrderByDescending(r => r.Index)) {
                var format = match.Groups["format"].Value ?? "";
                if(format != "") {
                    var replaceWith = "";

                    var formatMatch = FormatLoopTernaryRegex.Match(format);
                    if(formatMatch.Success) {
                        replaceWith = GetLoopTernaryReplacement(match, formatMatch, line, state);
                    } else {
                        replaceWith = null;
                    }

                    if(replaceWith != null) {
                        result.Remove(match.Index, match.Length);
                        result.Insert(match.Index, replaceWith);
                    }
                }
            }

            return result.ToString();
        }

        private string GetLoopTernaryReplacement(Match match, Match formatMatch, string line, TemplateState state)
        {
            var result = "";

            var closestLoop = state.Loops.Peek();
            if(closestLoop != null) {
                var group = closestLoop.IsFirstElement ? "first" : closestLoop.IsLastElement ? "last" : "other";
                result = (formatMatch.Groups[group].Value ?? "").Replace(@"\|", "|");
            }

            return result;
        }
    }
}
