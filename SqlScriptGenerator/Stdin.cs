using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    class StdIn
    {
        private static TextReader _StdIn;

        public static bool Silent { get; set; }

        static StdIn()
        {
            _StdIn = new StreamReader(Console.OpenStandardInput());
        }

        public static void Close()
        {
            if(_StdIn != null) {
                _StdIn.Dispose();
                _StdIn = null;
            }
        }

        public static void InputFromFileName(string fileName)
        {
            if(!File.Exists(fileName)) {
                throw new FileNotFoundException($"Cannot read input from {fileName}, it does not exist");
            }

            Close();

            _StdIn = new StreamReader(fileName);
        }

        public static string ReadLine()
        {
            return _StdIn.ReadLine();
        }
    }
}
