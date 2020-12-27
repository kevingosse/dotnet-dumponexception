using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ArrangeTypeModifiers

namespace DumpOnException.CLI
{
    class Program
    {
        private static readonly CancellationTokenSource TokenSource = new();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
            
            Parser parser = new(settings =>
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
            string asmLocation = typeof(global::StartupHook).Assembly.Location;
            string? cmd = options.Value.FirstOrDefault();
            string args = string.Join(' ', options.Value.Skip(1));
            if (cmd is null)
            {
                return 0;
            }

            // Read previous startup hooks and append the new one
            string stHooks = Utils.GetEnvironmentValue("DOTNET_STARTUP_HOOKS", string.Empty);
            string[] stHooksArray = stHooks.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            stHooks = string.Join(Path.PathSeparator, stHooksArray.Concat(new[] {asmLocation}));
            
            Dictionary<string, string?> envVars = new()
            {
                ["DOTNET_STARTUP_HOOKS"] = stHooks,
                ["DOE_FILTER"] = options.Filter,
                ["DOE_DIRECTORY"] = options.Directory
            };
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                envVars["COMPlus_DbgEnableElfDumpOnMacOS"] = "1";
            }

            ProcessStartInfo processInfo = Utils.GetProcessStartInfo(cmd, envVars);
            processInfo.Arguments = args;
            return Utils.RunProcess(processInfo, TokenSource.Token);
        }

        private static int ParsedErrors(ParserResult<Options> result, IEnumerable<Error> errors)
        {
            HelpText? helpText;
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
        
        private static void CurrentDomainOnDomainUnload(object? sender, EventArgs e)
        {
            TokenSource.Cancel();
        }

        private static void CurrentDomainOnProcessExit(object? sender, EventArgs e)
        {
            TokenSource.Cancel();
        }

        private static void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            TokenSource.Cancel();
        }
    }
}
