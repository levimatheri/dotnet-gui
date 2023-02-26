using DotnetGui.Core.Common;
using DotnetGui.Core.Templating.Models;
using NuGet.Versioning;

namespace DotnetGui.Core.Templating;
internal class BuiltInTemplateProvider : ITemplateProvider
{
    private readonly IDotNetCli _dotNetCli;

    public BuiltInTemplateProvider(IDotNetCli dotNetCli)
    {
        _dotNetCli = dotNetCli;
    }

    public async Task<IReadOnlyList<string>> GetAllTemplatesAsync(CancellationToken cancellationToken)
    {
        var currentInstalledSdk = await GetCurrentSdkInfoAsync(_dotNetCli, cancellationToken).ConfigureAwait(false);

        return GetTemplateFolders(currentInstalledSdk)
            .Where(Directory.Exists)
            .SelectMany(folder => Directory.EnumerateFiles(folder, "*.nupkg", SearchOption.TopDirectoryOnly))
            .ToList();
    }

    private static async Task<InstalledSdk> GetCurrentSdkInfoAsync(IDotNetCli dotNetCli, CancellationToken cancellationToken)
    {
        var sdks = await dotNetCli.ListSdksAsync(cancellationToken).ConfigureAwait(false);
        var currentSdkVersion = await dotNetCli.GetSdkVersionAsync(cancellationToken).ConfigureAwait(false);
        return sdks.Single(x => x.SemanticVersion == currentSdkVersion);
    }

    private static IEnumerable<string> GetTemplateFolders(InstalledSdk installedSdk)
    {
        var dotnetRootDir = BuiltInTemplatePackageProviderHelper.GetDotnetRootDirectory(installedSdk.InstallationPath);

        var templateFolders = new List<string>();

        var globalTemplateFolders = GetGlobalTemplateFolders(dotnetRootDir, installedSdk.SemanticVersion);
        if (globalTemplateFolders is not null)
        {
            templateFolders.AddRange(globalTemplateFolders);
        }

        templateFolders.Add(BuiltInTemplatePackageProviderHelper.GetSdkTemplateDir(dotnetRootDir, installedSdk.SemanticVersion));

        return templateFolders;
    }

    private static IEnumerable<string>? GetGlobalTemplateFolders(string dotnetRootDir, SemanticVersion sdkVersion)
    {
        var templatesRootDir = BuiltInTemplatePackageProviderHelper.GetGlobalTemplatesRootDir(dotnetRootDir);

        if (Directory.Exists(templatesRootDir))
        {
            var templateVersionDirs = Directory.EnumerateDirectories(templatesRootDir, "*.*", SearchOption.TopDirectoryOnly);
            return BuiltInTemplatePackageProviderHelper.SelectAppropriateTemplateDirs(templateVersionDirs, sdkVersion);
        }

        return null;
    }

    internal static class BuiltInTemplatePackageProviderHelper
    {
        public static string GetDotnetRootDirectory(string sdkInstallDir)
            => Path.GetDirectoryName(sdkInstallDir)!;

        public static string GetGlobalTemplatesRootDir(string dotnetRootDir)
            => Path.Combine(dotnetRootDir, "templates");

        public static string GetSdkTemplateDir(string dotnetRootDir, SemanticVersion sdkVersion)
            => Path.Combine(dotnetRootDir, "sdk", sdkVersion.ToNormalizedString(), "templates");

        public static IEnumerable<string> SelectAppropriateTemplateDirs(IEnumerable<string> templateVersionDirs, SemanticVersion sdkVersion)
            => templateVersionDirs
                .Select(dir => (Dir: dir, Version: SemanticVersion.Parse(Path.GetFileName(dir))))
                .OrderBy(x => x.Version)
                .TakeWhile(x => x.Version <= sdkVersion)
                .GroupBy(x => new Version(x.Version.Major, x.Version.Minor))
                .Select(g => g.Last().Dir);
    }
}
