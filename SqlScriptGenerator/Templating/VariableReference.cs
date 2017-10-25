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

            var codeIdx = variable.IndexOf('.');
            var arrayIdx = variable.IndexOf('[');

            if(arrayIdx != -1) {
                codeIdx = codeIdx == -1 ? arrayIdx : Math.Min(codeIdx, arrayIdx);
            }

            if(codeIdx == -1) {
                result.Name = variable;
            } else {
                result.Name = variable.Substring(0, codeIdx);
                result.EvalCode = variable.Substring(codeIdx + 1);
            }

            return result;
        }

        public override string ToString() => $"{Name} {EvalCode}";
    }
}
