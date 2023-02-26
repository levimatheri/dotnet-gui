using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestApp;
internal static partial class DotnetSdkParser
{
    [GeneratedRegex("^(?<version>.*) \\[(?<path>.*)\\]$")]
    private static partial Regex DotnetSdkListStringRegex();

    public static IReadOnlyList<InstalledSdk> ParseSdks(string sdkListString)
    {
        return sdkListString
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(item =>
            {
                var regexMatch = DotnetSdkListStringRegex().Match(item);
                var version = regexMatch.Groups["version"].Value;
                var path = regexMatch.Groups["path"].Value;

                var sdkVersion = SemanticVersion.Parse(version);

                return new InstalledSdk(sdkVersion, path);
            })
            .ToList().AsReadOnly();
    }
}
