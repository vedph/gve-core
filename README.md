# GVE

- [GVE](#gve)
  - [Docker](#docker)
  - [Demo](#demo)
  - [CLI Tool](#cli-tool)
    - [Import List](#import-list)
  - [Adding Hints](#adding-hints)

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
docker buildx build . -f Dockerfile-gve --platform linux/amd64,linux/arm64,windows/amd64,windows/arm64 -t vedph2020/gve-api:5.0.0 -t vedph2020/gve-api:latest --push
```

- 👉 GVE CADMUS API:

```bash
docker buildx build . -f Dockerfile-cadmus --platform linux/amd64,linux/arm64,windows/amd64,windows/arm64 -t vedph2020/cadmus-gve-api:6.0.7 -t vedph2020/cadmus-gve-api:latest --push
```

- 👉 DEMO:

```bash
docker buildx build . --platform linux/amd64,linux/arm64,windows/amd64,windows/arm64 -t vedph2020/gve-demo:6.0.7 -t vedph2020/gve-demo:latest --push

docker buildx build . --platform linux/amd64,linux/arm64 -t vedph2020/gve-demo:6.0.9 -t vedph2020/gve-demo:latest --push
```

## Demo

To run the demo in Docker at <localhost:8080>, use this command:

```ps1
docker run -it --rm -p 8080:80 --name gve-demo gve-demo
```

## CLI Tool

### Import List

🎯 Import a set of items into the Cadmus GVE database from an Excel list. This is typically used to pre-populate a database with entities to be filled with data. The importer will just create items and optionally their metadata part.

Syntax:

```sh
./gve import-list DATA_PATH CONFIG_PATH [-p]
```

- `DATA_PATH`: the path to the Excel file.
- `CONFIG_PATH`: the path to the configuration file for importing data.
- `-p`: add this to preflight, i.e. run the importer without writing data in the database.

Example:

```sh
import-list c:/users/dfusi/desktop/tables.xlsx c:/users/dfusi/desktop/gve-carriers.json
```

The importer configuration file is a JSON file with this structure:

```json
{
  "reader": {
    "sheetName": "the name of the source Excel sheet",
    "columnMappings": {
      "A": "the key for values in column A",
      "BC": "the key for values in column BC"
      /* ... etc. */
    },
    "endColumns": [ "A" ],
    "endMarker": "xxx",
    "startRow": 2,
    "limit": 100
  },
  "facetId": "required facet ID",
  "groupId": "optional group ID",
  "title": "Template for item title, use {{var}} for variables",
  "description": "Template for item description, use {{var}} for variables",
  "metadataKeys": ["..."],
  "flags": "optional flags in hex notation",
  "userId": "optional user ID"
}
```

- `columnMappings` map the coordinate of each column to a data key. For instance, if `A` is mapped to `eid`, this means that for each row read its `eid` data will be extracted from column A.
- `endColumns`: the coordinate(s) of the column(s) which must be checked to determine when the list ends. Usually a list ends when any of these columns is empty. Alternatively, you can specify in `endMarker` the string to match to determine the end. This is required when for some reasons your list includes blanks.
- `startRow`: the row number (1-N) to start reading data from. Usually there is a legend row at the top, so we start reading from row 2 like in this example.
- `limit`: the maximum number of rows to read. This will force a stop once the limit is reached, even if the list continues. This option is used mostly for diagnostic purposes, when you want to import just a small subset of a list and check the results.
- `metadataKeys` are the keys of data read from Excel which should be stored into the metadata part. For instance, if we have `eid`, this means that if a datum with key `eid` is read from Excel, it will be stored in the metadata part with key equal to `eid`.

Example: here we read data from a worksheet named `lists`, and a single column (`A`) representing the EID of each item to be imported.

```json
{
  "reader": {
    "sheetName": "lists",
    "columnMappings": {
      "A": "eid"
    },
    "endColumns": ["A"],
    "startRow": 3
  },
  "facetId": "carrier",
  "title": "{{eid}}",
  "description": "Carrier {{eid}}.",
  "metadataKeys": ["eid"]
}
```

## Adding Hints

If you need to add hints:

1. in `seed-profile.json`, paste the new hints in the configuration object `it.vedph.gve.snapshot` under `settings.hints`.
2. in `seed-profile.json`, add the corresponding entries to thesaurus `snapshot-feat-values`.
3. import settings and thesaurus via the Admin UI in the editor.

- [history](CHANGELOG.md)
