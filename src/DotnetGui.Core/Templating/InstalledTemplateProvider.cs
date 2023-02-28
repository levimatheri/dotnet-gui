using DotnetGui.Core.Templating.Models;

namespace DotnetGui.Core.Templating;
internal class InstalledTemplateProvider : ITemplateProvider
{
    public Task<IReadOnlyList<CompositeTemplateManifest>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var templatesRelativePath = ".templateengine/packages";
        var templatesFolder = Path.Combine(userFolder, templatesRelativePath);

        return Task.FromResult<IReadOnlyList<CompositeTemplateManifest>>(
            Directory.EnumerateFiles(templatesFolder, "*.nupkg", SearchOption.TopDirectoryOnly)
            .SelectMany(path => TemplatePackageInspector.GetTemplateManifestsFromPackage(path, false))
            .ToList());
    }
}
