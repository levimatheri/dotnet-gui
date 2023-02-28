using DotnetGui.Core.Templating.Models;

namespace DotnetGui.Core.Templating;
internal interface ITemplateProvider
{
    public Task<IReadOnlyList<CompositeTemplateManifest>> GetAllTemplatesAsync(CancellationToken cancellationToken);
}
