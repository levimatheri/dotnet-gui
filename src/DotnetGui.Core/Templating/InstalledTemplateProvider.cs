namespace DotnetGui.Core.Templating;
internal class InstalledTemplateProvider : ITemplateProvider
{
    public Task<IReadOnlyList<string>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var templatesRelativePath = ".templateengine/packages";
        var templatesFolder = Path.Combine(userFolder, templatesRelativePath);

        return Task.FromResult<IReadOnlyList<string>>(
            Directory.EnumerateFiles(templatesFolder, "*.nupkg", SearchOption.TopDirectoryOnly).ToList());
    }
}
