# GVE

## Docker

🐋 Quick Docker image build (demo app):

```bash
docker build . -t vedph2020/gve-demo:1.0.1 -t vedph2020/gve-demo:latest
```

(replace with the current version).

To run the demo in Docker at <localhost:8080>, use this command:

```ps1
docker run -it --rm -p 8080:80 --name gve-demo gve-demo
```

To build API image:

```bash
docker build . -f Dockerfile.api -t vedph2020/gve-api:0.0.1 -t vedph2020/gve-api:latest
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
