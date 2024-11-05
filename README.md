# GVE

## Docker

🐋 Quick Docker image build (demo app):

```bash
docker build . -t vedph2020/gve-demo:1.0.1 -t vedph2020/gve-demo:latest --build-arg BUILDPLATFORM=linux
```

or for ARM:

```bash
docker build . -t vedph2020/gve-demo:1.0.1 -t vedph2020/gve-demo:latest --build-arg BUILDPLATFORM=arm64
```

(replace with the current version).

To run the demo in Docker at <localhost:8080>, use this command:

```ps1
docker run -it --rm -p 8080:80 --name gve-demo gve-demo
```

To build the API image:

```bash
docker build . -f Dockerfile.api -t vedph2020/gve-api:0.0.1 -t vedph2020/gve-api:latest --build-arg BUILDPLATFORM=linux
```

or for ARM:

```bash
docker build . -f Dockerfile.api -t vedph2020/gve-api:0.0.1 -t vedph2020/gve-api:latest --build-arg BUILDPLATFORM=arm64
```

Real world example for demo:

```txt
Seyd Vernünftige mir g'rade Mitte gest

27x1="' in die" [*log:="replaced e with ' in die"]
@21x24="grad gegen über gestellt" [*log:="replaced g'rad' in die Mitte gest with grad gegen über gestellt" loc=below *version^:=beta]

or better, keeping into account also the requirements of diplomatic annotations:

27x1="'" [*log:="replace e with '"]
28+[" in die" [*log:="insert in die before Mitte'" loc=above]
23- [*log:="delete ' in g'rad'"]
@25x8- [*log:="delete in die"]
29x10="gegen über gestellt" [*log:="replace Mitte gest with gegen über gestellt" loc=below]
```

## History

- 2024-11-05: updated packages.
- 2024-10-26: refactored Blazor demo removing Bootstrap.
- 2024-10-22: updated packages.
- 2024-10-18: updated packages.
- 2024-10-08: updated packages.
- 2024-09-25: updated packages.
- 2024-09-13:
  - updated packages.
  - added `--platform=$BUILDPLATFORM` to Docker build.
- 2024-07-08:
  - added tags endpoint.
  - adjusted annotation operation so that it generates a new version cloning it from the input one and then adding its annotations.
  - added DTOs for snapshot timelines.
- 2024-07-07: updated packages.
- 2024-06-26:
  - updated packages.
  - added Cadmus parts.
- 2024-06-20: fix to non copied tagged node features in context when running an operation.
- 2024-06-17: fix to text controller node feature key.
- 2024-06-09: updated packages.
- 2024-05-31:
  - updated packages.
  - refactored binding model for run operation to accept an array of character objects rather than a string.
- 2024-05-25:
  - minor renamings.
  - updated packages.
- 2024-05-16: added `refNodeIds` to `ChainOperationContextStep`. This provides the reference nodes of each executed operation, which are useful to locate their visuals with reference to the base text.
- 2024-05-05: added nodes to API results.
- 2024-05-03: more controls in demo.

### 1.0.1

- 2024-05-03:
  - fixes to operations.
  - added chars view to demo.
- 2024-04-15: updated packages.
- 2024-03-25: started refactoring infrastructure for chain, removing legacy operation families.
- 2024-03-23: replaced demo.
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
