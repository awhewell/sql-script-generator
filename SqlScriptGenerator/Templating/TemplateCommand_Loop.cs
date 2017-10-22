using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{
    class TemplateCommand_Loop : TemplateCommand
    {
        private static readonly Regex ArgsRegex = new Regex(@"(?<name>\w\S*?)\s*=\s*(?<enumerable>.*)");

        public override void Process(string args)
        {
            var match = ArgsRegex.Match(args);
            var name = match.Groups["name"].Value ?? "";
            var enumerableCommand = match.Groups["enumerable"].Value ?? "";

            if(name == "") State.ReportParserError($"name missing from loop args");
            if(enumerableCommand == "") State.ReportParserError($"enumerable missing from loop args");

            var enumerableObj = State.GetVariableNameObject(enumerableCommand);
            if(enumerableObj == null) State.ReportParserError($"Cannot resolve {enumerableCommand}");
            var enumerable = enumerableObj as IEnumerable;
            if(enumerable == null) State.ReportParserError($"{enumerableCommand} does not resolve to an enumerable");

            var elements = new List<object>();
            foreach(var element in enumerable) {
                elements.Add(element);
            }

            var commandState = new CommandState_Loop() {
                Name =                  name,
                Elements =              elements.ToArray(),
                StartBlockLineIndex =   State.LineIndex,
                EndBlockLineIndex =     EndBlockLineIndex,
            };

            State.Loops.Push(commandState);
            commandState.NextIteration(State);
        }
    }
}
