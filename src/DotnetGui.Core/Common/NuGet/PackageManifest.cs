using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetGui.Core.Common.NuGet;
public record class PackageManifest(PackageMetadata Metadata);

public record class PackageMetadata(string Id, string Version, string? Icon);
