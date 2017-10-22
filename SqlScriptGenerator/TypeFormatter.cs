using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    static class TypeFormatter
    {
        public static string FormatType(Type t)
        {
            var name = t.FullName;
            var result = new StringBuilder(name);

            if(t.IsGenericType) {
                var backTickIdx = name.IndexOf('`');
                if(backTickIdx != -1) {
                    result.Remove(backTickIdx, result.Length - backTickIdx);
                }

                result.Append('<');
                result.Append(String.Join(", ", t.GetGenericArguments().Select(r => FormatType(r))));
                result.Append('>');
            }

            return result.ToString();
        }

        public static string FormatType<T>()
        {
            return FormatType(typeof(T));
        }
    }
}
