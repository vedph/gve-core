# GVE

## Demo

Note: for the demo in this solution I use [MS FluentUI ASP.NET Core](https://www.nuget.org/packages/Microsoft.FluentUI.AspNetCore.Templates/); you can see [demo and docs here](https://www.fluentui-blazor.net). If you need this template in your IDE, add it with this command:

```ps1
dotnet new install Microsoft.FluentUI.AspNetCore.Templates
```

The corresponding code repository is at <https://github.com/microsoft/fluentui-blazor>.

Other Razor packages:

- `Microsoft.FluentUI.AspNetCore.Components.Icons`

## Docker

🐋 Quick Docker image build:

```bash
docker build . -t vedph2020/gve-demo:0.0.2 -t vedph2020/gve-demo:latest
```

(replace with the current version).

To run the demo in Docker at <localhost:8080>, use this command:

```ps1
docker run -it --rm -p 8080:80 --name gve-demo gve-demo
```

TODO: consider using [Radzen](https://blazor.radzen.com/) in demo.

## History

- 2024-02-27: new Docker image.
- 2024-02-24: refactored tree node and moved into `Fusi.Tools` version 5.0.0.
- 2024-02-20: changed `VarOperation` ID to string and used [Nanoid](https://github.com/ai/nanoid) to generate IDs equal both on client and server side.
- 2024-02-06: updated packages.
- 2024-01-30: added Docker for demo.
- 2024-01-15:
  - added Cadmus-related packages.
- 2024-01-09:
  - updated packages.
  - make `Feature`'s `Value` optional.
  - more tests.
  - refactored diplomatic models.
- 2023-12-14: updated packages.
