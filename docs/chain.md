# Chain

This data structure consists of a linear sequence of nodes, each connected to the previous and the next node by a link.

Additionally, each link has a version tag, which adds the dimension of time to this structure. Internally, this structure collects nodes and links, and once they are added, they cannot be removed (unless we are using a single version). Any modification is done by adding new nodes and links, which connect or disconnect or reorder the existing nodes.

When the chain is empty, there are no nodes or links. When there is at least one node, there always are links for the head and the tail of the chain. So, the structure, when not empty, always alternates links and nodes, starting and ending with links.

This structure allows to represent different states of a text in time, after operations change it. Each operation introduces a new version (unless you are overwriting the previous version), so that building the text for a version is done by just following the path defined by links among nodes. At any time, you can build the text for any version, while still using a single object to represent all of the versions.

Operations usually add new links, and optionally add new nodes; anyway, both nodes and links are immutable items of an unordered set.

## Chain Operations

Chain operations are abstractions on top of the chain structure, used to represent higher level editing operations on the chain's text. These operations serve as an intermediate layer between client code with user-friendly models and the chain model itself.

Each chain operation derives from `ChainOperation<T>`, which in turn is derived from `VarOperationMetadata`.

All these operations get translated into calls to chain editing methods, which are more low-level and directly work on nodes. The chain operations take care of finding the nodes corresponding to their location arguments.

Also, chain operations work on a chain operations context (`ChainOperationContext<T>`), which includes the chain itself and a set of related data:

- the last version tag being the output of the last operation executed on this context.
- the steps executed on the chain, each including:
  - input version tag;
  - output version tag;
  - reference to the operation object.

The general text-based syntax for these operations includes these components, from left to right:

1. `(IN-OTAG:OUT-OTAG)`: these are the input and/or output tags. These tags are different from the interpretative tags used as filters for applying operations; they belong to the chain's model, and represent the input and the output states in the chain before and after applying an operation. You can specify none, just one, or both.
2. `ATxRUN` or `@ATxRUN` (required): node ID or index (when prefixed by `@`) optionally followed by the run of nodes. If not specified, the run defaults to 1.
3. `OPERATOR` (required)
4. `"VALUE"`
5. `^RANK`
6. `[TAGS...]`: version tags, separated by space, used to filter operations in or out according to the editor's interpretation.
7. `{NAME="VALUE"...}`: name=value pairs representing features. The operator can be `=` or `:=` for unique features. A name containing spaces must be included in double quotes.

The available operations (and their corresponding operator in their text-based representation) are:

- replace (`=`): e.g. `3x2="X"`
- delete (`-`): e.g. `3x2-`
- add left (`+<`): e.g. `3x2+<"X"`
- add right (`+>`): e.g. `3x2+>"X"`
- annotate (`:`): e.g. `3x2: {note=sample}`
- move (`>`): e.g. `3x2>6`
- swap (`<>`): e.g. `3x2<>6`
