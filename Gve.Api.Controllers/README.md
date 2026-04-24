# Gve.Api.Controllers

Controllers for GVE RESTful API.

## TextController

This controller is used to manage text transformations via chains.

- `GET api/text/operations/tags`: gets input and output tags for the given operations. Output tags are calculated so this endpoint is required to get them.
- `GET api/text/operations/dsl`: gets the DSL representation for the given operations.
- `POST api/text/operations/run`: runs the specified operations on the specified base text.
- `POST api/text/operations/run/chain`: runs the specified operations on the specified base text returning data from the inner chain structure. This is for diagnostic purposes.

## 🐛 Design Issues

The run operation returns a lot of redundant data (`CharChainResultModel`), which slows it down:

- `Steps`: all the steps, each with its full set of features, both for chain and for nodes.
- `TaggedNodes`: all the nodes with their features extracted from steps, for each version. As each version contains only the nodes used for its text, this makes it difficult to access other nodes; for instance the consumer cannot immediately see a node which was deleted as it's no longer present in the version following its deletion.

Suggestion: if we refactor `Gve.Text` as proposed (features stored in nodes and in chain), we can return something like this:

- `Features` (`IDictionary<string, IList<OperationFeature>>`): the global chain features, grouped by version tag.
- `Nodes` (`IList<CharChainNode>`): all the nodes in chain, with their features. `CharChainNode.Features` will be replaced by a dictionary of features grouped by versions.
- `VersionNodeIds` (`IDictionary<string, IList<int>>`): the sequence of node IDs for each version: e.g. `v0` has node IDs 1,2,3; `v1` has node IDs 1,3 (2 was deleted); etc. This provides the same functionality of `TaggedNodes` to build the effective sequence, but avoids a redundant copy of all the nodes for each version.
