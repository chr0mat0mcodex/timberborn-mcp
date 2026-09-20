# Dependencies and references

Our source code is MIT-licensed. Dependencies retain their own licenses.
Versions below are pinned in Directory.Packages.props and resolved in package lockfiles.
License identifiers were checked against restored NuGet package metadata.

| Package | Version | License | Copyright / authors |
|---|---|---|---|
| ModelContextProtocol | 2.2.0 | Apache-2.0 | Model Context Protocol, a Series of LF Projects, LLC |
| Microsoft.Extensions.Hosting | 10.0.11 | MIT | Microsoft Corporation |
| Microsoft.NET.Test.Sdk | 18.3.0 | MIT | Microsoft Corporation |
| xunit.v3 | 3.2.2 | Apache-2.0 | .NET Foundation |
| xunit.runner.visualstudio | 3.1.5 | Apache-2.0 | .NET Foundation |

Transitive dependencies are listed in the lockfiles; their package notices must be retained if binaries
are redistributed. This repository does not bundle dependency binaries or a release package.

For the retained legacy adapter, the HTTP contract was studied through the public More HTTP API source in
https://github.com/datvm/TimberbornMods and verified against a user-provided local installation.
The adapter is an independent client; no game DLLs, mod DLLs or third-party source fragments are included.
Timberborn is by Mechanistry. This project is not an official Mechanistry product.

The active native game bridge uses public APIs from the locally installed game.
Game/Unity assemblies are compile references and are not distributed here.
The native mod has no required third-party mods; see docs/references/README.md for
official and community references. Normal .NET/NuGet dependencies remain as listed above.
