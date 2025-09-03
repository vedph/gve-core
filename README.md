# GVE

[VEdition](https://vedph.github.io/gve-doc) backend (code-named `GVE` = Goethe Venetian Epigrams). This solution contains 3 main projects:

- the GVE API to be used with the corresponding [GVE frontend shell](https://github.com/vedph/gve-shell).
- the Cadmus GVE API to be used with the corresponding [Cadmus editor frontend](https://github.com/vedph/cadmus-gve-app).
- the GVE chain core demo, a minimalist web based UI (directly consuming core components via Blazor) to test the chain structure.

## Docker

🐋 Quick Docker image build (replace with the current versions): as you need a `buildx` container, first ensure to have started one like this:

```sh
docker buildx create --use
```

- 👉 GVE API:

```sh
docker buildx build . -f Dockerfile-gve --platform linux/amd64,linux/arm64,windows/amd64,windows/arm64 -t vedph2020/gve-api:3.0.1 -t vedph2020/gve-api:latest --push
```

- 👉 GVE CADMUS API:

```bash
docker buildx build . -f Dockerfile-cadmus --platform linux/amd64,linux/arm64,windows/amd64,windows/arm64 -t vedph2020/cadmus-gve-api:3.0.1 -t vedph2020/cadmus-gve-api:latest --push
```

- 👉 DEMO:

```bash
docker buildx build . --platform linux/amd64,linux/arm64,windows/amd64,windows/arm64 -t vedph2020/gve-demo:3.0.0 -t vedph2020/gve-demo:latest --push
```

## Demo

To run the demo in Docker at <localhost:8080>, use this command:

```ps1
docker run -it --rm -p 8080:80 --name gve-demo gve-demo
```

Real world examples for demo:

```txt
1234567890
he did say

4x4- [*log="delete did"]
10x1="id" [*log="replace say with said" *version^="beta"]
```

```txt
there was an old man with a beard,
who cried: "It is just as I feared!
four larks and a wren,
two swans and a hen,
all built their nests in my beard!"

40x5="said" [*log="said for cried" x="72" y="140" style="font-size:20px;fill:red"]
99x5="crows" [*log="crows for swans" x="30" y="210" style="font-size:20px;fill:red"]
116+["have " [*log="insert have" *version^="alpha" reason="metrical" x="20" y="250" style="font-size:20px;fill:red"]
72x23<>95x21  [*log="swap verses"]
155x5="owls" [*log="owls for crows" *version^="beta" x="110" y="210" style="font-size:20px;fill:red"]
```

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

```txt
he had a red hat for sure,
and long moustache and grey beard.

32+[\"very \" [*log=\"insert 'very ' before 'long'\"]
10x3="green" [*log="replace 'red' with 'green'"]
51x5- [*log="delete grey" *version^="A"]
18x9>[1@fs [*log=\"move 'for sure,' before 'he'\"]
17-@fs [*log="delete tail ' '"]
1+[" "@fs [*log="insert space before 'he'"]
37x9<>56x5 [*log="swap 'moustache' / 'beard'" *version^="B"]
```

## History

- 2025-09-03:
  - updated packages.
  - added `GveHandsPart`.
- 2025-08-31: fixes to profile.
- 2025-08-30:
  - updated packages.
  - refactored Cadmus API profiles.
- 2025-08-02: updated packages.
- 2025-07-12: updated packages.
- 2025-07-02:
  - updated packages.
  - added export controller to Cadmus API. 

### 3.0.1

- 2025-05-27: Docker image.
- 2025-05-21:
  - updated packages.
  - refactored Docfx.
- 2025-05-17:
  - updated packages.
  - fixed some tests after export refactoring.
  - refactored Dockerfile and built Docker image for demo 3.0.0.
- 2025-05-08: updated packages.
- 2025-05-03:
  - updated packages.
  - updated Cadmus GVE preview config for migration V3.
  - removed `Proteus.Rendering` from this solution and added a dependency to this same project once refactored for Cadmus migration V3.
- 2025-04-26: updated packages.

### 3.0.0

- 2025-04-04:
  - ⚠️ refactored to use `StringPair` from `Fusi.Tools` as the base for features removing `NamedValue` which had the same model. This is a breaking change even if the model is the same.
  - temporarily added to this solution a Proteus library project (`Proteus.Rendering`) which has NO dependencies on GVE. It is GVE which, together with Cadmus migration subsystem, will depend on it once this is completed. For this reason most of its code derives from `Cadmus.Migration` with minimal adjustments to make it more generic and independent from both Cadmus and GVE.
- 2025-04-03: fixed bug in chain's delete range.
- 2025-04-01: updated packages.
- 2025-03-29: updated packages.
- 2025-03-20:
  - added no trace features option to demo.
  - updated packages.
- 2025-03-15: updated packages.
- 2025-03-05: updated packages (Fusi.Tools 7 incorporating tree version merging).
- 2025-03-04: updated packages (Cadmus migration V2).

### 2.0.4

- 2025-02-20:
  - updated packages.
  - fixed thesauri import in Cadmus API.
- 2025-02-05: updated packages.
- 2025-01-28: updated packages.
- 2025-01-20: updated packages.

### 2.0.3

- 2025-01-15:
  - fix to `ChainOperationContextFeats.GetNodeFeatures` (need to look only at a single tag).
  - updated packages.

### 2.0.2

- 2025-01-12: updated packages.
- 2025-01-10: updated packages.
- 2025-01-07: updated packages.
- 2024-12-22: updated packages.
- 2024-12-11: added endpoint `api/text/operations/run/chain`.
- 2024-12-10: upgraded Gve.Demo to .NET 9 restoring the dependency from project in the same solution.
- 2024-12-03: updated packages.

### 2.0.1

- 2024-11-28: added `isNewTextHidden` to operation diplomatics.
- 2024-11-24: moved controllers to libraries to allow sharing among APIs.
- 2024-11-23: updated packages.

### 2.0.0

- 2024-11-19: ⚠️ upgraded to .NET 9 except for `Gve.Demo` which is being held back by its dependencies. To allow for the upgrade, the `Gve.Demo` project dependency on `Gve.Text` has been moved to a NuGet package rather than to the project in this solution. As soon as the required packages are updated, the project dependency will be restored.

### 1.0.5

- 2024-11-12:
  - added `opid` feature to each node added by an operation (insertion or replacement). This allows to immediately identify the source of each node when it does not belong to the base text. Base text nodes are those whose IDs range from 1 to N where N is the length of the base text.
  - extended behavior of `x` in `ChainOperationContextFeats.UpdateFeatures` to `y` too, under the assumption that an operation adds characters on the same line, where one is after the other. Should you find a corner-case situation where this is not true, you can multiply the operations, each with its own `x` and `y` features.

### 1.0.4

- 2024-11-11: fix to `ChainOperationContextFeats.UpdateFeatures` for `x` feature: when the feature name is `x`, it is set only for the first node in the set of the operation's target nodes. This allows for correct positioning of newly added nodes (via insertion or replacement). When adding a new set of nodes, the first node is the one that actually gets positioned by `x`, while the others are positioned next to it. Otherwise, all the new nodes would get the same `x` coordinate, and thus be displayed one on top of the other.

### 1.0.3

- 2024-11-05:
  - fix to swap operation.
  - updated packages.
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
- 2024-01-15: added Cadmus-related packages.
- 2024-01-09:
  - updated packages.
  - make `Feature`'s `Value` optional.
  - more tests.
  - refactored diplomatic models.
- 2023-12-14: updated packages.
