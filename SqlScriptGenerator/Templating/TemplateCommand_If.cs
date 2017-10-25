using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{
    class TemplateCommand_If : TemplateCommand
    {
        public override void Process(string args)
        {
            var condition = args;
            var evalResult = Evaluator_CSharp.Evaluate(condition, State.Variables);
            if(evalResult.ParserError != null) {
                State.ReportParserError(evalResult.ParserError);
            }
            var outcome = Convert.ToBoolean(evalResult.Result, CultureInfo.InvariantCulture);

            if(!outcome) {
                State.MoveToLineIndex(EndBlockLineIndex + 1);
            }
        }
    }
}
