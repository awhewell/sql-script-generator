using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{
    static class TemplateCommandParser
    {
        public const string Prefix = "--;";
        private static readonly Regex CommandKeyValueRegex = new Regex(Prefix + @"\s*(?<cmd>\S+)(\s*(?<args>.*))?");

        class CommandArgs
        {
            public string Command;
            public string Args;

            public override string ToString() => $"{Command} {Args}";
        }

        public static void Parse(TemplateState state)
        {
            var commandArgs = ParseCommandArgs(state, state.Line, state.LineNumber);

            var isBlock = false;
            TemplateCommand templateCommand = null;
            switch(commandArgs.Command.ToLower()) {
                case "endloop": templateCommand = new TemplateCommand_EndLoop(); break;
                case "loop":    templateCommand = new TemplateCommand_Loop(); isBlock = true; break;
                default:
                    state.ReportParserError($"{commandArgs.Command} is not a valid template command");
                    break;
            }
            templateCommand.State = state;
            templateCommand.EndBlockLineIndex = isBlock ? FindEndBlockLineIndex(state, commandArgs.Command) : -1;
            templateCommand.Process(commandArgs.Args);
        }

        static CommandArgs ParseCommandArgs(TemplateState state, string line, int lineNumber)
        {
            var trimmedLine = line.Trim();
            var match = CommandKeyValueRegex.Match(trimmedLine);
            var command = match.Success ? match.Groups["cmd"].Value ?? "" : "";
            var args = match.Success ? match.Groups["args"].Value ?? "" : "";

            if(command == "") {
                state.ReportParserError($"{trimmedLine} is not a valid template command", lineNumber);
            }

            return new CommandArgs() {
                Command = command,
                Args = args,
            };
        }

        private static int FindEndBlockLineIndex(TemplateState state, string command)
        {
            var result = -1;

            command = command.ToLower();
            var endBlock = "";
            switch(command) {
                case "loop":    endBlock = "endloop"; break;
                default:        throw new NotImplementedException();
            }

            var nestedCount = 0;
            for(var lineIndex = state.LineIndex + 1;lineIndex < state.TemplateLines.Length;++lineIndex) {
                var trimmedLine = state.TemplateLines[lineIndex].Trim();
                if(trimmedLine.StartsWith(Prefix)) {
                    var commandArgs = ParseCommandArgs(state, trimmedLine, lineIndex + 1);
                    var normalisedCommand = commandArgs.Command.ToLower();
                    if(normalisedCommand == command) {
                        ++nestedCount;
                    } else if(normalisedCommand == endBlock) {
                        if(nestedCount-- == 0) {
                            result = lineIndex;
                            break;
                        }
                    }
                }
            }

            if(result == -1) {
                state.ReportParserError($"The {command} command has no end block");
            }

            return result;
        }
    }
}
