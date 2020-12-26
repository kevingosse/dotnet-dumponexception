using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DumpOnException.CLI
{
    class Program
    {
        private static CancellationTokenSource _tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
            
            Parser parser = new Parser(settings =>
            {
                settings.AutoHelp = true;
                settings.AutoVersion = true;
                settings.EnableDashDash = true;
                settings.HelpWriter = null;
            });

            ParserResult<Options> result = parser.ParseArguments<Options>(args);
            result.MapResult(ParsedOptions, errors => ParsedErrors(result, errors));
        }

        private static int ParsedOptions(Options options)
        {
            // Process options
            string asmLocation = typeof(StartupHook.Diagnostics.DiagnosticsClient).Assembly.Location;
            string cmd = options.Value.FirstOrDefault();
            string[] args = options.Value.Skip(1).ToArray();

            Dictionary<string, string> envVars = new Dictionary<string, string>
            {
                ["DOTNET_STARTUP_HOOKS"] = asmLocation,
                ["DOE_FILTER"] = options.Filter,
                ["DOE_DIRECTORY"] = options.Directory
            };
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                envVars["COMPlus_DbgEnableElfDumpOnMacOS"] = "1";
            }
            
            ProcessStartInfo processInfo = Utils.GetProcessStartInfo(cmd, envVars);
            processInfo.Arguments = string.Join(' ', args);
            return Utils.RunProcess(processInfo, _tokenSource.Token);
        }

        private static int ParsedErrors(ParserResult<Options> result, IEnumerable<Error> errors)
        {
            HelpText helpText = null;
            if (errors.IsVersion())
            {
                helpText = HelpText.AutoBuild(result);
            }
            else
            {
                helpText = HelpText.AutoBuild(
                    result,
                    h =>
                    {
                        h.Heading = "DumpOnException";
                        h.AddNewLineBetweenHelpSections = true;
                        h.AdditionalNewLineAfterOption = false;
                        return h;
                    },
                    e =>
                    {
                        return e;
                    });
            }

            Console.WriteLine(helpText);
            return 1;
        }
        
        private static void CurrentDomainOnDomainUnload(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
        }

        private static void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _tokenSource.Cancel();
        }
    }
}
