# History

- 2026-05-14:
  - removed solid features and added source-of features in thesauri `snapshot-feat-names` and `snapshot-feat-values`; removed solid property from hints in profile settings.
  - updated packages.
- 2026-05-13: reordered feature names in thesaurus.

### 6.0.1

- 2026-05-06:
  - added snake-e to settings and `snapshot-feat-values` thesaurus.
  - updated packages.
- 2026-04-26: more entries added to `text-passage-tags` thesaurus.

### 6.0.0

- 2026-04-26: added references to NDP parts for `TextPassagesPart` and its seeder.
- 2026-04-25:
  - ⚠️ [refactored core and API for features](REFACTOR.md).
  - added to profile settings new hints: line-bottom, line-top, line-left, line-right, hamburger, hotdog.
- 2026-04-24:
  - thesauri.
  - updated packages.
- 2026-04-17:
  - updated packages.
  - added scripts to features and more hands in thesauri.

## 5.0.5

- 2026-04-16:
  - updated packages.
  - updated settings for hints to be equal to those in gve-snapshot-rendition `h5-48.settings.json`.
  - added thesauri `snapshot-source-ids`, `snapshot-source-types` and updated `snapshot-feat-names` and `snapshot-feat-values` refactoring colors and adding sheet number for foliation.
  - fixed unified versioning.
- 2026-04-15: added red pencil.
- 2026-04-13: updated packages.
- 2026-04-08: `categories:content` thesaurus entries.
- 2026-04-07: updated packages.
- 2026-03-04: added `proposal` to feature names thesaurus (`snapshot-feat-values`).

## 5.0.4

- 2026-03-25:
  - updated packages.
  - 🆕 added `models` to `settings` for facet editor.

## 5.0.3

- 2026-03-19: updated packages.
- 2026-03-18: updated packages.
- 2026-03-03: updated packages.
- 2026-02-25:
  - updated packages.
  - updated thesaurus `snapshot-feat-values`.

## 5.0.2

- 2026-02-19:
  - 🆕 added get operations DSL endpoint to GVE API.
  - added preset to demo app.
  - updated packages.
- 2026-02-17: updated packages.

## 5.0.1

- 2026-02-16: updated packages (integrating new Cadmus API import controllers).
- 2026-02-13:
  - changed/added thesauri `categories_support`, `physical-state-features`, `cod-shelfmark-libraries`.
  - added settings `it.vedph.codicology.shelfmarks`.
  - changed facets: moved physical states part from carrier facet into snapshot facet.
- 2026-02-05: removed shelfmarks part from snapshot item.
- 2026-02-02: updated packages.

## 5.0.0

- 2026-01-29: fixes to Docker files.
- 2026-01-25: updated profile settings.
- 2026-01-24: updated profile settings.
- 2026-01-22:
  - ⚠️ refactored models to remove visuals from operations.
  - refactored solution by centralizing package management and versions.
  - updated packages.
- 2026-01-19:
  - updated packages.
  - replaced external IDs with links in Cadmus editor profile.

## 4.0.4

- 2026-01-08:
  - updated packages.
  - added `log` to feature names thesaurus.
- 2026-01-06: updated thesauri and hints.
- 2026-01-05: added more hints to Cadmus API profile for GVE.
- 2026-01-02:
  - added settings to Cadmus API profile for GVE hints.
  - updated packages.
- 2025-12-14:
  - refactored rendition features thesauri.
  - updated packages.

## 4.0.2

- 2025-12-10:
  - updated packages.
  - completed preset for demo.
  - Docker image for demo.
- 2025-12-07:
  - in text controller supply `sourceTag` for `TaggedNodes` in result when it is empty because it corresponds to the base text. In this case supply `v0`.
  - in demo updated presets.
- 2025-12-05:
  - fixed preset.
  - updated packages.
- 2025-12-02: multiline text in demo grid.
- 2025-12-01:
  - added "build request" command to demo.
  - fixed ordering of tags in `Chain`.`GetDistinctTags`.
- 2025-12-01:
  - updated packages. Docker image for demo.
- 2025-11-28: adjusted thesauri entries.
- 2025-11-26: thesauri entries.

## 4.0.0

- 2025-11-26: Docker image.
- 2025-11-24: ⚠️ upgraded to NET 10.
- 2025-11-20:
  - thesauri entries.
  - removed sparse lines as items from profile and added sparse flag.
- 2025-11-18: thesauri entries.
- 2025-11-11: thesauri entries.
- 2025-11-06: more thesauri entries.
- 2025-11-05: updated packages.

## 3.0.6

- 2025-10-31: updated packages.
- 2025-10-30:
  - updated packages.
  - refactored profile removing epigram version.
  - changed some thesauri entries.
  - replaced dead service `via.placeholder.com/800x600` with https://placehold.co/800x600.
- 2025-10-24: added features to thesauri.

## 3.0.5

- 2025-10-24: updated packages and generated Docker image for Cadmus API.
- 2025-10-22: updated API model for returned result in run operations so that now the full operation metadata are returned, and not just a subset of them. Also all the operations receive a `dsl` property with their text representation.
- 2025-10-20: moved `Type` to `OperationMetadata` so that it is returned with run operations result by API.
- 2025-10-18:
  - added left/right anchor trace features for delete operations.
  - fixes to trace features for the output version.
  - increased all patch version numbers.
- 2025-10-15: updated packages and facets.
- 2025-10-08: updated packages.
- 2025-10-01: added import configurations to CLI tool.
- 2025-09-23: refactored profile.

## 3.0.4

- 2025-09-22: updated packages.
- 2025-09-19: added sparse lines (with facet equal to epigram version) to profile.
- 2025-09-17: updated packages.

## 3.0.3

- 2025-09-04: added doc references to all facets.
- 2025-09-03:
  - updated packages.
  - added `GveHandsPart`.
  - generated and published image for Cadmus GVE API 3.0.2.
- 2025-08-31: fixes to profile.
- 2025-08-30:
  - updated packages.
  - refactored Cadmus API profiles.
- 2025-08-02: updated packages.
- 2025-07-12: updated packages.
- 2025-07-02:
  - updated packages.
  - added export controller to Cadmus API.

## 3.0.1

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

## 3.0.0

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

## 2.0.4

- 2025-02-20:
  - updated packages.
  - fixed thesauri import in Cadmus API.
- 2025-02-05: updated packages.
- 2025-01-28: updated packages.
- 2025-01-20: updated packages.

## 2.0.3

- 2025-01-15:
  - fix to `ChainOperationContextFeats.GetNodeFeatures` (need to look only at a single tag).
  - updated packages.

## 2.0.2

- 2025-01-12: updated packages.
- 2025-01-10: updated packages.
- 2025-01-07: updated packages.
- 2024-12-22: updated packages.
- 2024-12-11: added endpoint `api/text/operations/run/chain`.
- 2024-12-10: upgraded Gve.Demo to .NET 9 restoring the dependency from project in the same solution.
- 2024-12-03: updated packages.

## 2.0.1

- 2024-11-28: added `isNewTextHidden` to operation diplomatics.
- 2024-11-24: moved controllers to libraries to allow sharing among APIs.
- 2024-11-23: updated packages.

## 2.0.0

- 2024-11-19: ⚠️ upgraded to .NET 9 except for `Gve.Demo` which is being held back by its dependencies. To allow for the upgrade, the `Gve.Demo` project dependency on `Gve.Text` has been moved to a NuGet package rather than to the project in this solution. As soon as the required packages are updated, the project dependency will be restored.

## 1.0.5

- 2024-11-12:
  - added `opid` feature to each node added by an operation (insertion or replacement). This allows to immediately identify the source of each node when it does not belong to the base text. Base text nodes are those whose IDs range from 1 to N where N is the length of the base text.
  - extended behavior of `x` in `ChainOperationContextFeats.UpdateFeatures` to `y` too, under the assumption that an operation adds characters on the same line, where one is after the other. Should you find a corner-case situation where this is not true, you can multiply the operations, each with its own `x` and `y` features.

## 1.0.4

- 2024-11-11: fix to `ChainOperationContextFeats.UpdateFeatures` for `x` feature: when the feature name is `x`, it is set only for the first node in the set of the operation's target nodes. This allows for correct positioning of newly added nodes (via insertion or replacement). When adding a new set of nodes, the first node is the one that actually gets positioned by `x`, while the others are positioned next to it. Otherwise, all the new nodes would get the same `x` coordinate, and thus be displayed one on top of the other.

## 1.0.3

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

## 1.0.1

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
