using DotnetGui.Core.Templating.Models;

namespace DotnetGui.Core.Platforms.Windows.Templating;
internal interface ITemplateRetriever
{
    Task<IReadOnlyList<CompositeTemplateManifest>> RetrieveAllTemplatesAsync();
}
