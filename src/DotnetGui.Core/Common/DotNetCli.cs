using CliWrap;
using CliWrap.Buffered;
using DotnetGui.Core.Common.Extensions;
using DotnetGui.Core.Templating.Models;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System.Text.RegularExpressions;

namespace DotnetGui.Core.Common;
internal partial class DotNetCli : IDotNetCli
{
    private readonly ILogger<DotNetCli> _logger;
    private const string DOTNET_EXECUTABLE_NAME = "dotnet";

    public DotNetCli(ILogger<DotNetCli> logger) => _logger = logger;

    public async Task<IReadOnlyList<InstalledSdk>> ListSdksAsync(CancellationToken cancellationToken = default)
    {
        var result = await this.RunAsync("--list-sdks").ConfigureAwait(false);
        return DotNetCliHelper.ParseDotNetListSdksOutput(result.Output.Trim());
    }

    public async Task<SemanticVersion> GetSdkVersionAsync(CancellationToken cancellationToken = default)
    {
        var (Output, _) = await this.RunAsync("--version").ConfigureAwait(false);
        return SemanticVersion.Parse(Output.Trim());
    }

    internal static partial class DotNetCliHelper
    {
        public static IReadOnlyList<InstalledSdk> ParseDotNetListSdksOutput(string listSdksOutput)
        {
            var output = listSdksOutput
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Select(outputLine =>
                {
                    var regexMatch = DotnetListSdkRegex().Match(outputLine);
                    var version = regexMatch.Groups["version"].Value;
                    var path = regexMatch.Groups["path"].Value;

                    var sdkVersion = SemanticVersion.Parse(version);

                    return new InstalledSdk(SemanticVersion.Parse(version), path);
                })
                .ToList();

            return output;
        }

        public static string FormatAsCliArguments(IReadOnlyDictionary<string, string?> arguments) =>
            string.Join(' ', arguments
                .Where(x => x.Value is not null)
                .Select(x => $"--{x.Key} \"{x.Value!.Replace('\\', '/')}\""));

        [GeneratedRegex("^(?<version>.*) \\[(?<path>.*)\\]$")]
        private static partial Regex DotnetListSdkRegex();
    }

    private async Task<(string Output, string Error)> RunAsync(string arguments)
    {
        try
        {
            using var cts = new CancellationTokenSource(30_000);
            {
                var result = await Cli.Wrap(DOTNET_EXECUTABLE_NAME)
                    .WithArguments("--list-sdks")
                    .ExecuteBufferedAsync();

                var output = result.StandardOutput;
                var error = result.StandardError;

                _logger.ExecutedSuccessfully(DOTNET_EXECUTABLE_NAME, arguments, output, error);
                return (output, error);
            }
        }
        catch (Exception exception)
        {
            _logger.ExecutedFailed(DOTNET_EXECUTABLE_NAME, arguments, exception);
            throw;
        }
    }
}
