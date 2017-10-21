﻿using System;
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
                    case "-database":
                        result.DatabaseName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-metadata":
                        result.Command = ParseCommand(result, Command.DumpMetadata);
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
            Stdout.WriteLine($"usage: SqlScriptGenerator <command> [options]");
            Stdout.WriteLine($"  -metadata      Dump the metadata for a database");
            Stdout.WriteLine();
            Stdout.WriteLine($"  -engine        Database engine [{defaults.DatabaseEngine}]");
            Stdout.WriteLine($"                 ({ListEnum<DatabaseEngine>(exclude: new DatabaseEngine[] { DatabaseEngine.None })})");
            Stdout.WriteLine($"  -connection    The connection string [{defaults.ConnectionString}]");
            Stdout.WriteLine($"  -database      The name of the database [{defaults.DatabaseName}]");
            Stdout.WriteLine($"  -askPassword   Ask for the connection password at runtime");

            if(!String.IsNullOrEmpty(message)) {
                Stdout.WriteLine();
                Stdout.WriteLine(message);
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
            Stdout.WriteLine($"Connection password:");
            return Stdin.ReadLine();
        }
    }
}
