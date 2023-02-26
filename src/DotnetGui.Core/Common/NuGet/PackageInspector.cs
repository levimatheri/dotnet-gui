using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DotnetGui.Core.Common.NuGet;
internal partial class PackageInspector
{
    public static PackageManifest GetPackageManifest(ZipArchive archive, string packageName)
    {
        var packageManifestPath = PackageInspectorHelper.GetPackageManifestPath(packageName);
        var packageManifestFile = archive.Entries.Single(x => string.Equals(x.FullName, packageManifestPath, StringComparison.OrdinalIgnoreCase));
        using var packageManifestFileStream = packageManifestFile.Open();
        return PackageInspectorHelper.ParseXmlPackageManifest(packageManifestFileStream);
    }

    public static string? TryGetBase64PackageIcon(ZipArchive archive, string? iconPath)
    {
        if (iconPath is not null)
        {
            var packageIconFile = archive.Entries.Single(x => string.Equals(x.FullName, iconPath, StringComparison.OrdinalIgnoreCase));
            using var packageIconFileStream = packageIconFile.Open();
            return PackageInspectorHelper.GetBase64Icon(packageIconFileStream, packageIconFile.Name);
        }

        return null;
    }

    internal static partial class PackageInspectorHelper
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public static (string PackageName, string Version) GetPackageNameAndVersion(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            var match = PackageNameAndVersionRegex().Match(fileName);
            var packageName = match.Groups["packagename"].Value;
            var version = match.Groups["version"].Value;

            return (packageName, version);
        }

        public static string GetPackageManifestPath(string packageName)
            => $"{packageName}.nuspec";

        public static IEnumerable<string> GetTemplateConfigDirs(IEnumerable<string> archive)
        {
            var templateConfigDirRegex = TemplateConfigDirsRegex();

            return archive
                .Select(x => templateConfigDirRegex.Match(x))
                .Where(x => x.Success)
                .Select(x => x.Value)
                .Distinct();
        }

        public static string GetTemplateManifestPath(string templateConfigDir) =>
            Path.Combine(templateConfigDir, "template.json").Replace("\\", "/");

        public static string GetIdeHostManifestPath(string templateConfigDir) =>
            Path.Combine(templateConfigDir, "ide.host.json").Replace("\\", "/");

        public static string GetTemplateIconPath(string templateConfigDir, string iconRelativePath) =>
            Path.Combine(templateConfigDir, iconRelativePath).Replace("\\", "/");

        public static PackageManifest ParseXmlPackageManifest(Stream stream)
        {
            var xDoc = XDocument.Load(stream);
            var packageElement = xDoc.Document!.Elements().First();
            var metadataElement = packageElement!.Elements().First();
            var id = metadataElement!.Elements().First(e => string.Equals(e.Name.LocalName, "id", StringComparison.OrdinalIgnoreCase)).Value;
            var version = metadataElement!.Elements().First(e => string.Equals(e.Name.LocalName, "version", StringComparison.OrdinalIgnoreCase)).Value;
            var icon = metadataElement!.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "icon", StringComparison.OrdinalIgnoreCase))?.Value;
            return new PackageManifest(new PackageMetadata(id, version, icon));
        }

        public static string GetBase64Icon(Stream stream, string fileName)
        {
            var fileType = Path.GetExtension(fileName);

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();
            var base64Icon = Convert.ToBase64String(bytes);

            return $"data:image/{fileType};base64,{base64Icon}";
        }

        [GeneratedRegex("^(?<packagename>.*)\\.(?<version>\\d*\\.\\d*\\.\\d*-?.*)\\.nupkg$")]
        private static partial Regex PackageNameAndVersionRegex();

        [GeneratedRegex("^(content/)?(?<template>.*)/\\.template\\.config")]
        private static partial Regex TemplateConfigDirsRegex();
    }
}
