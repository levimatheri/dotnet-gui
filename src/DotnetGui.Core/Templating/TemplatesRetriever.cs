using DotnetGui.Core.Platforms.Windows.Templating;
using DotnetGui.Core.Templating.Models;

namespace DotnetGui.Core.Templating;
internal class TemplatesRetriever : ITemplateRetriever
{
    private readonly IEnumerable<ITemplateProvider> _templateProviders;

    public TemplatesRetriever(IEnumerable<ITemplateProvider> templateProviders)
    {
        _templateProviders = templateProviders;
    }

    public Task<IReadOnlyList<CompositeTemplateManifest>> RetrieveAllTemplatesAsync()
    {
        throw new NotImplementedException();
    }
}
