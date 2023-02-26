namespace DotnetGui.Core.Templating;
internal interface ITemplateProvider
{
    public Task<IReadOnlyList<string>> GetAllTemplatesAsync(CancellationToken cancellationToken);
}
