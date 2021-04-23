using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DumpOnException.CLI
{
    internal class Options
    {
        [Usage(ApplicationAlias = "dotnet dumponexception")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Creating dump for each exception.", UnParserSettings.WithUseEqualTokenOnly(), new Options { Value = new string[] { "dotnet run" } });
                yield return new Example("Filter exception before creating a memory dump", UnParserSettings.WithUseEqualTokenOnly(), new Options { Filter = "^DivideByZeroException$", Value = new string[] { "dotnet run" } });
            }
        }

        [Option('a', "attach", Required = false, HelpText = "Attach the debugger when the exception happens.")]
        public bool AttachDebugger { get; set; } = false;

        [Option('f', "filter", Required = false, HelpText = "Applies a regex exception filter to trigger the dump.")]
        public string Filter { get; set; } = string.Empty;

        [Option('d', "directory", Required = false, HelpText = "Sets the folder destination of the dump.")]
        public string Directory { get; set; } = string.Empty;

        [Option('t', "threshold", Required = false, HelpText = "Sets the memory threshold to create a dump.")]
        public int MemoryThreshold { get; set; } = 0;

        [Value(0, Hidden = true, Required = true, HelpText = "Command to be wrapped by the cli tool.")]
        public IEnumerable<string> Value { get; set; } = Enumerable.Empty<string>();
    }
}
