using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var success = false;

            try {
                var options = OptionsParser.Parse(args);

                CommandRunner commandRunner = null;
                switch(options.Command) {
                    case Command.DumpMetadata:          commandRunner = new CommandRunner_DumpMetadata(); break;
                    case Command.GenerateProjectFile:   commandRunner = new CommandRunner_GenerateProjectFile(); break;
                    case Command.None:                  break;
                }

                if(commandRunner == null) {
                    OptionsParser.Usage("Missing command");
                } else {
                    commandRunner.Options = options;
                    success = commandRunner.Run();
                }
            } catch(Exception ex) {
                StdOut.WriteLine($"Caught exception {ex.Message}:\r\n{ex}");
                Environment.Exit(2);
            }

            Environment.Exit(success ? 0 : 1);
        }
    }
}
