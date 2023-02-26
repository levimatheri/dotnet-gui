using NuGet.Versioning;

namespace DotnetGui.Core.Templating.Models;
internal record InstalledSdk(SemanticVersion SemanticVersion, string InstallationPath);
