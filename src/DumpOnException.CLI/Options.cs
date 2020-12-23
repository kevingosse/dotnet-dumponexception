using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace DumpOnException.CLI
{
    internal class Options
    {
        [Usage(ApplicationAlias = "dotnet dumponexception")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Example 1", UnParserSettings.WithUseEqualTokenOnly(), new Options { Value = new string[] { "test" } });
                yield return new Example("Example 2", UnParserSettings.WithUseEqualTokenOnly(), new Options { Filter = "(A|B|C)", Value = new string[] { "test" } });
            }
        }

        [Option("filter", Required = false, HelpText = "Applies a regex exception filter to trigger the dump.")]
        public string Filter { get; set; }

        [Value(0, Hidden = true, Required = true, HelpText = "Command to be wrapped by the cli tool.")]
        public IEnumerable<string> Value { get; set; }
    }
}
