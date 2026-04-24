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

- all the steps.
- all the nodes for each version. This duplicates the nodes in each version just because they might have different features. Also, this is more difficult to consume because the returned nodes are only those used in the version. So, for instance the consumer cannot immediately see a node which was deleted.

Suggestion: if we refactor `Gve.Text` as proposed there (features stored in nodes and in chain), we can return:

- TODO
