# GVE Refactoring Notes

- 2026-04-25, version 6.0.0

This document records all breaking and non-breaking changes introduced by the
feature-storage and API-result refactoring after version 5.0.5.
Frontend/consumer code must be updated accordingly.

---

## 1. New class: `OperationFeatureSet` (`Gve.Text`)

A new class wrapping `Dictionary<string, List<OperationFeature>>` has been
added. It stores operation features keyed by version tag and provides:

- `Get(tag)` — returns the feature list for a tag (empty list when absent).
- `Set(tag, features)` — stores or replaces the list for a tag.
- `HasTag(tag)` — whether an entry exists for a tag.
- `Tags` — all tags present.
- `Clear()` — removes all entries.
- `ToDictionary()` — returns a snapshot as `IDictionary<string, IList<OperationFeature>>`.

---

## 2. `ChainNode<T>` (`Gve.Text`)

**Added:**
```csharp
public OperationFeatureSet Features { get; } = new();
```
Holds per-version features for the node. Populated by
`ChainOperationContext<T>.SyncNodeFeaturesToChain()` after all operations have
been executed. Starts empty; the chain's internal context storage
(`ChainOperationContextFeats.NodeFeatures`) is still the live accumulator
during execution.

---

## 3. `Chain<T>` (`Gve.Text`)

**Added:**
```csharp
public OperationFeatureSet GlobalFeatures { get; } = new();
```
Stores a snapshot of accumulated global (non-node) features after each
operation, keyed by the operation's output version tag. Populated inside
`CharChainOperation.UpdateContext()`.

`v0` must be seeded explicitly by callers after `Chain.Reset()`:
```csharp
context.Chain.GlobalFeatures.Set("v0", []);
```
`TextController.RunOperations` already does this.

**Modified:** `Chain.Clear()` now also calls `GlobalFeatures.Clear()`.

---

## 4. `ChainOperationContextStep` (`Gve.Text`) — BREAKING

**Removed:** `FeatureSet` property (was `ChainOperationContextFeats`).

Steps now carry only:
- `Operation` — the operation metadata.
- `InputTag`, `OutputTag` — version tags.
- `Result` — the text string produced by the operation.
- `RefNodeIds` — IDs of nodes referenced by the operation.

Features are no longer snapshotted per step. Access them via:
- `context.NodeFeatures` / `context.Features` (internal context, unchanged).
- `node.Features.Get(tag)` after calling `SyncNodeFeaturesToChain()`.
- `chain.GlobalFeatures.Get(tag)` for global features.

**Constructor signature changed** (removed `features` parameter):
```csharp
// Before
new ChainOperationContextStep(operation, inputTag, outputTag, featureSet, refNodeIds)
// After
new ChainOperationContextStep(operation, inputTag, outputTag, refNodeIds)
```

---

## 5. `CharChainOperation` (`Gve.Text`)

**`UpdateContext` (internal):** After updating context features, now also
snapshots them into `context.Chain.GlobalFeatures[outputTag]`.

**`AddNodeTraceFeatures`:** The `synchStep` parameter has been **removed**.
All call sites that passed `synchStep: true` have been updated to omit it.
This affects: `CharChainReplaceOperation`, `CharChainAddBeforeOperation`,
`CharChainAddAfterOperation`, `CharChainMoveBeforeOperation`,
`CharChainMoveAfterOperation`, `CharChainSwapOperation`,
`CharChainAnnotateOperation`.

---

## 6. `ChainOperationContext<T>` (`Gve.Text`)

**Added:**
```csharp
public void SyncNodeFeaturesToChain()
```
Iterates `NodeFeatures` and populates each chain node's
`ChainNode<T>.Features` from the context's internal dictionary. Call this once
after all operations have been executed. The internal `NodeFeatures` dictionary
is not modified and remains available to other consumers (e.g.
`GveTextTreeBuilder`).

---

## 7. `CharChainNode` (`Gve.Text.DTO`) — BREAKING

**`Features` property type changed:**
```csharp
// Before
public IList<Feature>? Features { get; set; }

// After
public IDictionary<string, IList<Feature>>? Features { get; set; }
```

The key is the version tag (e.g. `"v1"`); the value is the list of features
for that node at that version.

**Impact on callers:**
- **Input** (sending base-text nodes to the API): unaffected — features are
  ignored when building the chain.
- **Output** (reading the API result): must iterate by tag key. Example:
  ```javascript
  // Before: node.features was [{name, value}, ...]
  // After:  node.features is {"v1": [{name, value}], "v2": [...]}
  const v1Feats = node.features?.["v1"] ?? [];
  ```
- **Snapshot serialisation/deserialisation**: old JSON with a flat
  `"features": [...]` array will fail to deserialise. Migrate to
  `"features": {"v0": [...]}` format.

`Clone()` and `ToString()` have been updated accordingly.

---

## 8. `ChainOperationContextStepDto` (`Gve.Api.Models`) — BREAKING

**Removed:** `FeatureSet` property.

Steps in the API response now contain only: `operation`, `inputTag`,
`outputTag`, `result`, `refNodeIds`.

---

## 9. `CharChainResultModel` (`Gve.Api.Models`) — BREAKING

The entire model has been redesigned. Old and new shapes:

### Before
```json
{
  "steps": [{ "operation": ..., "inputTag": "v0", "outputTag": "v1",
               "result": "BC", "refNodeIds": [1],
               "featureSet": { "features": [...], "nodeFeatures": {...} } }],
  "chainDump": "...",
  "chainTags": ["v0", "v1"],
  "taggedNodes": {
    "v1": [{ "id": 2, "index": 0, "label": "B", "data": "B",
             "sourceTag": "v0", "features": [{"name":"reason","value":"x"}] }]
  }
}
```

### After
```json
{
  "steps": [{ "operation": ..., "inputTag": "v0", "outputTag": "v1",
               "result": "BC", "refNodeIds": [1] }],
  "chainDump": "...",
  "chainTags": ["v0", "v1"],
  "nodes": [
    { "id": 1, "label": "A", "data": "A", "sourceTag": "v0",
      "features": { "v1": [{"name":"del","value":"..."}] } },
    { "id": 2, "label": "B", "data": "B", "sourceTag": "v0",
      "features": { "v1": [{"name":"reason","value":"x"}] } }
  ],
  "versionNodeIds": {
    "v0": [1, 2, 3],
    "v1": [2, 3]
  },
  "globalFeatures": {
    "v0": [],
    "v1": [{"name":"log","value":"deleted A","isGlobal":true,...}]
  }
}
```

**Migration guide for API consumers:**

| Old field | New equivalent |
|---|---|
| `taggedNodes["v1"][i]` | Look up `versionNodeIds["v1"][i]`, then find node by ID in `nodes` |
| `taggedNodes["v1"][i].features` | `nodes[id].features["v1"]` |
| `steps[i].featureSet.features` | `globalFeatures[steps[i].outputTag]` |
| `steps[i].featureSet.nodeFeatures["v1_id"]` | `nodes[id].features["v1"]` |

**Important:** `versionNodeIds` and `globalFeatures` include `v0` (the base
text version). `taggedNodes` previously did NOT include `v0`. Consumers must
now handle `v0` being present in the response.

**To reconstruct a specific version's text:**
```javascript
function getVersionText(result, tag) {
  const ids = result.versionNodeIds[tag] ?? [];
  const nodeMap = Object.fromEntries(result.nodes.map(n => [n.id, n]));
  return ids.map(id => nodeMap[id].data).join('');
}
```

---

## 10. `TextController.RunOperations` (`Gve.Api.Controllers`)

Updated to build the new `CharChainResultModel`. After executing operations it
calls `context.SyncNodeFeaturesToChain()`, then constructs:
- `Nodes` from `context.Chain.Nodes` (all chain nodes with their Features).
- `VersionNodeIds` from `context.Chain.GetDistinctTags()` +
  `context.Chain.GetTagNodes(tag)`.
- `GlobalFeatures` from `context.Chain.GlobalFeatures` (for all tags).
- `Steps` without `FeatureSet`.

The `v0` global-features entry is seeded to an empty list before operations
run.

---

## 11. `GveTextTreeBuilder` (`Gve.Migration`)

**`GetVersionFromTag`** (private): no longer reads from
`step.FeatureSet.Features`. Now uses:
```csharp
context.Chain.GlobalFeatures.Get(tag)
    .FirstOrDefault(f => f.Name == OperationFeature.F_VERSION)?.Value;
```
This requires that `GlobalFeatures` is populated before `Build` is called.
When using `GveTextTreeBuilder.Build(string, IList<CharChainOperation>)`,
this is automatic. When using `Build(ChainOperationContext<char>, ...)` with a
context you constructed manually, call:
```csharp
context.Chain.GlobalFeatures.Set("v0", []);
foreach (var op in operations) op.Execute(context);
// GlobalFeatures for v1, v2, … are populated automatically by UpdateContext.
```

---

## 12. `TreeBuilder` (`Gve.Migration`) — internal adaptation

`TreeBuilder` is `[Obsolete]` and uses `CharChainNode` as a tree-node
container. It was adapted to the new `Features` dictionary type:
- New public helper: `TreeBuilder.GetTagFeatures(CharChainNode)` — returns the
  list of `_tag` features stored under the `TAG_FEATURE_NAME` key.
- Internal helper: `AddTagFeature(CharChainNode, string)` — adds a version-tag
  marker to the node's features dictionary.
- All locations that previously iterated `CharChainNode.Features` as a flat
  list now use these helpers.

---

## Summary of breaking changes

| Component | What changed | How to migrate |
|---|---|---|
| `ChainOperationContextStep.FeatureSet` | Removed | Use `context.NodeFeatures`, `node.Features.Get(tag)`, or `chain.GlobalFeatures.Get(tag)` |
| `CharChainNode.Features` type | `IList<Feature>?` → `IDictionary<string, IList<Feature>>?` | Access as `features["vN"]` |
| `CharChainResultModel.TaggedNodes` | Removed | Use `nodes` + `versionNodeIds` |
| `CharChainResultModel` (new fields) | Added `nodes`, `versionNodeIds`, `globalFeatures` | See migration guide above |
| `ChainOperationContextStepDto.FeatureSet` | Removed | Use `globalFeatures` / `nodes[].features` from result |
| `v0` included in result | Now always present in `versionNodeIds` and `globalFeatures` | Handle `v0` key in response |
