using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{
    class VariableReference
    {
        public string Name { get; set; }
        public string EvalCode { get; set; }

        public static VariableReference Parse(string variable)
        {
            var result = new VariableReference();

            var hasCode = false;
            foreach(var ch in variable) {
                if(Char.IsPunctuation(ch) || Char.IsSeparator(ch) || Char.IsWhiteSpace(ch)) {
                    hasCode = true;
                    break;
                }
            }

            if(hasCode) {
                result.EvalCode = variable;
            } else {
                result.Name = variable;
            }

            return result;
        }

        public override string ToString() => $"{Name}{EvalCode}";
    }
}
