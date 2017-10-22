using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{
    class TemplateCommand_EndLoop : TemplateCommand
    {
        public override void Process(string args)
        {
            var outermostLoop = State.Loops.Peek();
            if(outermostLoop == null) {
                State.ReportParserError($"ENDLOOP without matching LOOP");
            }

            outermostLoop.NextIteration(State);
        }
    }
}
