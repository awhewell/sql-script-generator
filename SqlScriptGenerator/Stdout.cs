using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    static class Stdout
    {
        private static TextWriter _StdOut;

        public static bool Silent { get; set; }

        static Stdout()
        {
            _StdOut = new StreamWriter(Console.OpenStandardOutput());
        }

        public static void Close()
        {
            if(_StdOut != null) {
                _StdOut.Dispose();
                _StdOut = null;
            }
        }

        public static void OutputToFileName(string fileName)
        {
            var folder = Path.GetDirectoryName(fileName);
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            Close();

            _StdOut = new StreamWriter(fileName, append: false);
        }

        public static void WriteLine()
        {
            if(!Silent) {
                _StdOut.WriteLine();
                _StdOut.Flush();
            }
        }

        public static void WriteLine(string message)
        {
            if(!Silent) {
                _StdOut.WriteLine(message);
                _StdOut.Flush();
            }
        }

        public static void WriteLine(string format, params object[] args)
        {
            WriteLine(String.Format(format, args));
        }

        public static void Write(string message)
        {
            if(!Silent) {
                _StdOut.Write(message);
                _StdOut.Flush();
            }
        }

        public static void Write(string format, params object[] args)
        {
            Write(String.Format(format, args));
        }
    }
}
