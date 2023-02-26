using DotnetGui.Core.Templating.Models;
using NuGet.Versioning;

namespace DotnetGui.Core.Common;
internal interface IDotNetCli
{
    Task<IReadOnlyList<InstalledSdk>> ListSdksAsync(CancellationToken cancellationToken);

    Task<SemanticVersion> GetSdkVersionAsync(CancellationToken cancellationToken);
}
