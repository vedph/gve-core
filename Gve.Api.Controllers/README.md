# Gve.Api.Controllers

Controllers for GVE RESTful API.

## TextController

This controller is used to manage text transformations via chains.

- `POST api/text/operations/parse`: parses a multi-line text where each line is an operation DSL string. Returns a list of `CharChainOperationSource` objects or an error.
- `POST api/text/operations/tags`: given a list of operations, calculates and returns their input and output version tags. Output tags depend on context so this endpoint is needed to resolve them without running the operations.
- `POST api/text/operations/dsl`: given a list of operations, returns their DSL string representation.
- `POST api/text/operations/run`: runs the specified operations on the specified base text and returns the full result (see below).
- `POST api/text/operations/run/chain`: runs the specified operations on the specified base text and returns the raw chain structure (`CharChainModel`). This is for diagnostic purposes.

## Result of `POST api/text/operations/run`

The endpoint returns a `ResultWrapperModel<CharChainResultModel>` where the result has the following structure:

```json
{
  "steps": [
    {
      "operation": { "id": "...", "type": "Delete", ... },
      "inputTag": "v0",
      "outputTag": "v1",
      "result": "BC",
      "refNodeIds": [1]
    }
  ],
  "chainDump": "...",
  "chainTags": ["v0", "v1", "v2"],
  "nodes": [
    {
      "id": 1, "label": "A", "data": "A", "sourceTag": "v0",
      "features": {
        "v1": [{ "name": "del", "value": "op1 v0:v1 1", ... }]
      }
    },
    { "id": 2, "label": "B", "data": "B", "sourceTag": "v0", "features": null }
  ],
  "versionNodeIds": {
    "v0": [1, 2, 3],
    "v1": [2, 3]
  },
  "globalFeatures": {
    "v0": [],
    "v1": [{ "name": "log", "value": "deleted A", "isGlobal": true }]
  }
}
```

### Fields

- `steps`: execution trace — one entry per operation. Each step carries the operation metadata, input/output version tags, the result text, and the IDs of nodes referenced by the operation. Steps do **not** carry a feature snapshot.
- `chainDump`: diagnostic string dump of the raw chain structure.
- `chainTags`: all distinct version tags present in the chain, in version order. Includes `v0`.
- `nodes`: **all** nodes in the chain regardless of which version(s) they belong to, including deleted nodes. Each node's `features` is a dictionary keyed by version tag; the value is the list of features for that node at that version. `features` is `null` when the node has no features in any version.
- `versionNodeIds`: for each version tag (including `v0`), the ordered sequence of node IDs that make up that version's text. Use this together with `nodes` (looked up by `id`) to reconstruct any version.
- `globalFeatures`: for each version tag (including `v0`), the accumulated global (chain-level) features at the end of that version. The `v0` entry is always an empty array.

### How to use the result

**Reconstruct version text:**

```javascript
function getVersionText(result, tag) {
  const ids = result.versionNodeIds[tag] ?? [];
  const nodeMap = Object.fromEntries(result.nodes.map(n => [n.id, n]));
  return ids.map(id => nodeMap[id].data).join('');
}
```

**Get features for a node at a specific version:**

```javascript
function getNodeFeatures(result, nodeId, tag) {
  const node = result.nodes.find(n => n.id === nodeId);
  return node?.features?.[tag] ?? [];
}
```

**Get global features at a specific version:**

```javascript
function getGlobalFeatures(result, tag) {
  return result.globalFeatures[tag] ?? [];
}
```
