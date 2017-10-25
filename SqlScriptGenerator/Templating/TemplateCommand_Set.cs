using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{
    class TemplateCommand_Set : TemplateCommand
    {
        private static readonly Regex ArgsRegex = new Regex(@"(?<name>\w\S*?)\s*=\s*(?<value>.*)");

        public override void Process(string args)
        {
            var match = ArgsRegex.Match(args);
            var name = match.Groups["name"].Value ?? "";
            var value = match.Groups["value"].Value ?? "";

            if(name == "") State.ReportParserError($"name missing from set args");
            if(value == "") State.ReportParserError($"value missing from set args");

            var valueObj = State.GetVariableNameObject(value);
            State.AddOrReplaceVariable(name, valueObj);
        }
    }
}
