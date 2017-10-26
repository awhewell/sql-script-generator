// Copyright © 2017 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    static class OptionsParser
    {
        /// <summary>
        /// Parses the command-line options passed across.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Options Parse(string[] args)
        {
            var result = new Options();

            if(args.Length == 0) {
                Usage(null);
            }

            for(var i = 0;i < args.Length;++i) {
                var arg = (args[i] ?? "");
                var normalisedArg = arg.ToLower();
                var nextArg = i + 1 < args.Length ? args[i + 1] : null;

                switch(normalisedArg) {
                    case "-?":
                    case "/?":
                    case "--help":
                    case "--?":
                        Usage("");
                        break;
                    case "-askpassword":
                        result.AskForPassword = true;
                        break;
                    case "-connection":
                    case "-connectionstring":
                        result.ConnectionString = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-createproj":
                        result.Command = ParseCommand(result, Command.GenerateProjectFile);
                        break;
                    case "-entity":
                        result.EntityName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-generate":
                        result.Command = ParseCommand(result, Command.GenerateScript);
                        break;
                    case "-project":
                        result.ProjectFileName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-script":
                        result.ScriptFileName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-template":
                        result.TemplateFileName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-writesource":
                        result.DebugWriteSourceFileName = UseNextArg(arg, nextArg, ref i);
                        break;
                    default:
                        Usage($"Invalid argument {arg}");
                        break;
                }
            }

            return result;
        }

        private static Command ParseCommand(Options options, Command command)
        {
            if(options.Command != Command.None) {
                Usage($"Cannot specify both {options.Command} and {command} commands");
            }

            return command;
        }

        private static string UseNextArg(string arg, string nextArg, ref int argIndex)
        {
            if(String.IsNullOrWhiteSpace(nextArg)) {
                Usage($"{arg} argument missing");
            }
            ++argIndex;

            return nextArg;
        }

        private static T ParseEnum<T>(string arg, ref int argIndex)
        {
            if(arg == null) {
                Usage($"{typeof(T).Name} value missing");
            }
            ++argIndex;

            try {
                return (T)Enum.Parse(typeof(T), arg ?? "", ignoreCase: true);
            } catch(ArgumentException) {
                Usage($"{arg} is not a recognised {typeof(T).Name} value");
                throw;
            }
        }

        private static int ParseInt(string arg, ref int argIndex)
        {
            if(arg == null) {
                Usage("Numeric parameter missing");
            }
            ++argIndex;

            if(!int.TryParse(arg, out int result)) {
                Usage($"{arg} is not an integer");
            }

            return result;
        }

        public static void Usage(string message)
        {
            var defaults = new Options();

            //                 123456789.123456789.123456789.123456789.123456789.123456789.123456789.123456789
            StdOut.WriteLine($"usage: SqlScriptGenerator <command> [options]");
            StdOut.WriteLine($"  -createProj    Create or update a project file");
            StdOut.WriteLine($"  -generate      Generate a script from a template");
            StdOut.WriteLine();
            StdOut.WriteLine($"PROJECT OPTIONS");
            StdOut.WriteLine($"  -project       Full path to the project file [{defaults.ProjectFileName}]");
            StdOut.WriteLine();
            StdOut.WriteLine($"TEMPLATE OPTIONS");
            StdOut.WriteLine($"  -template      Template file [{defaults.TemplateFileName}]");
            StdOut.WriteLine($"  -script        Script file [{defaults.ScriptFileName}]");
            StdOut.WriteLine($"  -entity        Name of entity to use [{defaults.EntityName}]");
            StdOut.WriteLine();
            StdOut.WriteLine($"DATABASE ENGINE OPTIONS");
            StdOut.WriteLine($"  -engine        Database engine [{defaults.DatabaseEngine}]");
            StdOut.WriteLine($"                 ({ListEnum<DatabaseEngine>(exclude: new DatabaseEngine[] { DatabaseEngine.None })})");
            StdOut.WriteLine($"  -connection    The connection string [{defaults.ConnectionString}]");
            StdOut.WriteLine($"  -askPassword   Ask for the connection password at runtime");
            StdOut.WriteLine();
            StdOut.WriteLine($"MISC OPTIONS");
            StdOut.WriteLine($"  -writeSource   Write generated C# template code to filename provided [{defaults.DebugWriteSourceFileName}]");

            if(!String.IsNullOrEmpty(message)) {
                StdOut.WriteLine();
                StdOut.WriteLine(message);
            }

            Environment.Exit(1);
        }

        private static string ListEnum<T>(T[] exclude = null)
            where T: struct
        {
            return String.Join(", ",
                    Enum.GetNames(typeof(T))
                   .OfType<string>()
                   .OrderBy(r => r.ToLower())
                   .Where(r => exclude == null || !exclude.Contains((T)Enum.Parse(typeof(T), r)))
            );
        }

        public static string AskForPassword()
        {
            StdOut.WriteLine($"Connection password:");
            return StdIn.ReadLine();
        }
    }
}
