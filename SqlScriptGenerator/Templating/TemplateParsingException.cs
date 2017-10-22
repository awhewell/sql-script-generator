using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator.Templating
{

    [Serializable]
    public class TemplateParsingException : Exception
    {
        public TemplateParsingException() { }
        public TemplateParsingException(string message) : base(message) { }
        public TemplateParsingException(string message, Exception inner) : base(message, inner) { }
        protected TemplateParsingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
