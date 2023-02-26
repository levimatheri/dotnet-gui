using DotnetGui.Core.Platforms.Windows.Templating;
using DotnetGui.Core.Templating.Models;

namespace DotnetGui.Core.Templating;
internal class TemplatesRetriever : ITemplateRetriever
{
    public Task<IReadOnlyList<CompositeTemplateManifest>> RetrieveAllTemplatesAsync()
    {
        throw new NotImplementedException();
    }
}
