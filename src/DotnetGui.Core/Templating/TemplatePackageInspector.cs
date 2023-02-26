using DotnetGui.Core.Common.NuGet;
using DotnetGui.Core.Templating.Models;
using System.IO.Compression;
using System.Text.Json;
using static DotnetGui.Core.Common.NuGet.PackageInspector;

namespace TestApp;
public static partial class TemplatePackageInspector
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public static IReadOnlyList<CompositeTemplateManifest> GetTemplateManifestsFromPackage(string packagePath, bool isBuiltIn)
    {
        var (packageName, packageVersion) = PackageInspectorHelper.GetPackageNameAndVersion(packagePath);

        using var file = File.OpenRead(packagePath);
        using var archive = new ZipArchive(file);

        var packageManifest = GetPackageManifest(archive, packageName);
        var base64PackageIcon = TryGetBase64PackageIcon(archive, packageManifest.Metadata.Icon);

        return PackageInspectorHelper
            .GetTemplateConfigDirs(archive.Entries.Select(x => x.FullName))
            .Select(templateConfigDir =>
            {
                var templateManifest = TryGetTemplateManifest(archive, templateConfigDir);
                var ideHostManifest = TryGetIdeHostManifest(archive, templateConfigDir);
                var base64TemplateIcon = TryGetBase64TemplateIcon(archive, templateConfigDir, ideHostManifest?.Icon);

                // templateManifest! -> possibly forcing null into a non-nullable property, but filtering those cases out immediately
                // it makes it easier for the rest of app, so we don't have to check for nulls
                return new CompositeTemplateManifest(packageName, packageVersion, base64TemplateIcon ?? base64PackageIcon, isBuiltIn, templateManifest!, ideHostManifest);
            })
            .Where(x => x.TemplateManifest is not null)
            .ToList();
    }

    private static TemplateManifest? TryGetTemplateManifest(ZipArchive archive, string templateConfigDir)
    {
        var templateManifestPath = PackageInspectorHelper.GetTemplateManifestPath(templateConfigDir);
        var templateManifestFile = archive.Entries.SingleOrDefault(x => string.Equals(x.FullName, templateManifestPath, StringComparison.OrdinalIgnoreCase));
        if (templateManifestFile is not null)
        {
            using var templateManifestFileStream = templateManifestFile.Open();
            return ParseJsonTemplateManifest(templateManifestFileStream);
        }

        return null;
    }

    private static string? TryGetBase64TemplateIcon(ZipArchive archive, string templateConfigDir, string? templateIconRelativePath)
    {
        if (templateIconRelativePath is not null)
        {
            var templateIconPath = PackageInspectorHelper.GetTemplateIconPath(templateConfigDir, templateIconRelativePath);
            var templateIconFile = archive.Entries.Single(x => string.Equals(x.FullName, templateIconPath, StringComparison.OrdinalIgnoreCase));
            return PackageInspectorHelper.GetBase64Icon(templateIconFile.Open(), templateIconFile.Name);
        }

        return null;
    }
       
    private static TemplateIdeHostManifest? TryGetIdeHostManifest(ZipArchive archive, string templateConfigDir)
    {
        var ideHostManifestPath = PackageInspectorHelper.GetIdeHostManifestPath(templateConfigDir);
        var ideHostManifestFile = archive.Entries.SingleOrDefault(x => string.Equals(x.FullName, ideHostManifestPath, StringComparison.OrdinalIgnoreCase));
        if (ideHostManifestFile is not null)
        {
            using var ideHostManifestFileStream = ideHostManifestFile.Open();
            return ParseJsonIdeHostManifest(ideHostManifestFileStream);
        }

        return null;
    }

    private static TemplateManifest ParseJsonTemplateManifest(Stream stream) =>
            JsonSerializer.Deserialize<TemplateManifest>(stream, _jsonSerializerOptions)!;

    private static TemplateIdeHostManifest ParseJsonIdeHostManifest(Stream stream) =>
            JsonSerializer.Deserialize<TemplateIdeHostManifest>(stream, _jsonSerializerOptions)!;
}
