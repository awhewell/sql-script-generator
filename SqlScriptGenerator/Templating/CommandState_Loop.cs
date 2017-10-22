using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{
    class CommandState_Loop
    {
        public string Name { get; set; }

        public int StartBlockLineIndex { get; set; }

        public int EndBlockLineIndex { get; set; }

        public object[] Elements { get; set; }

        public int ElementIndex { get; set; } = -1;

        public bool IsFirstElement => ElementIndex == 0;

        public bool IsLastElement => ElementIndex == (Elements?.Length ?? 0) - 1;

        public void NextIteration(TemplateState state)
        {
            ++ElementIndex;

            if(ElementIndex >= Elements.Length) {
                if(Elements.Length > 0) {
                    state.RemoveVariable(Name, StartBlockLineIndex);
                }
                state.Loops.Pop();
                state.MoveToLineIndex(EndBlockLineIndex + 1);
            } else {
                var element = Elements[ElementIndex];
                if(ElementIndex > 0) {
                    state.RemoveVariable(Name);
                }
                state.AddVariable(Name, element, StartBlockLineIndex);

                state.MoveToLineIndex(StartBlockLineIndex + 1);
            }
        }
    }
}
