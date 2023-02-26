using NuGet.Versioning;
using CliWrap;
using CliWrap.Buffered;
using System.Text.RegularExpressions;
using TestApp;

var listSdksTask = await Cli.Wrap("dotnet")
    .WithArguments("--list-sdks")
    .ExecuteBufferedAsync();

var currentSdkTask = await Cli.Wrap("dotnet")
    .WithArguments("--version")
    .ExecuteBufferedAsync();

var result = listSdksTask.StandardOutput;
Console.WriteLine(result);

var result2 = currentSdkTask.StandardOutput;

var listSdksResult = DotnetSdkParser.ParseSdks(result);

//foreach (var sdk in listSdksResult)
//{
//    Console.WriteLine($"Version: {sdk.SemanticVersion}, Path: {sdk.InstallationPath}");
//}

var currentSdkSemantic = SemanticVersion.Parse(result2.Trim());
Console.WriteLine($"{currentSdkSemantic.Major}.{currentSdkSemantic.Minor}.{currentSdkSemantic.Patch}");

var currentSdk = listSdksResult.Single(sdk => sdk.SemanticVersion == currentSdkSemantic);

Console.WriteLine(currentSdk.InstallationPath);

var templateFolders = GetTemplateFolders(currentSdkSemantic, currentSdk.InstallationPath)
    .Where(Directory.Exists)
    .SelectMany(folder => Directory.EnumerateFiles(folder, "*.nupkg", SearchOption.TopDirectoryOnly))
    .ToList();

var manifest = PackageInspector.GetTemplateManifestsFromPackage(templateFolders.First(), true);

Console.ReadLine();

static IEnumerable<string> GetTemplateFolders(SemanticVersion sdkVersion, string sdkInstallDir)
{
    var dotnetRootDir = Path.GetDirectoryName(sdkInstallDir)!;

    var templateFolders = new List<string>();

    var globalTemplateFolders = GetGlobalTemplateFolders(dotnetRootDir, sdkVersion);
    if (globalTemplateFolders is not null)
    {
        templateFolders.AddRange(globalTemplateFolders);
    }

    templateFolders.Add(Path.Combine(dotnetRootDir, "sdk", sdkVersion.ToNormalizedString(), "templates"));

    return templateFolders;
}

static IEnumerable<string>? GetGlobalTemplateFolders(string dotnetRootDir, SemanticVersion sdkVersion)
{
    var templatesRootDir = Path.Combine(dotnetRootDir, "templates");

    if (Directory.Exists(templatesRootDir))
    {
        var templateVersionDirs = Directory.EnumerateDirectories(templatesRootDir, "*.*", SearchOption.TopDirectoryOnly);
        var grouping = templateVersionDirs
                .Select(dir => (Dir: dir, Version: SemanticVersion.Parse(Path.GetFileName(dir))))
                .OrderBy(x => x.Version)
                .TakeWhile(x => x.Version <= sdkVersion)
                .GroupBy(x => new Version(x.Version.Major, x.Version.Minor));

        return grouping.Select(g => g.Last().Dir);
                
    }

    return null;
}


record InstalledSdk(SemanticVersion SemanticVersion, string InstallationPath);