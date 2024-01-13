# Introduction

## General Architecture

GVE includes two main areas: content creation and its presentation.

Creating content happens in a Cadmus-based editing environment, where each epigram is an item (=a record), composed of several parts. Among them, two core parts are used to represent the text and its genetic apparatus, designed to generate all the variants according to their description and interpretations:

- _base text part_: the base text, stored line by line.
- _spine-modeled apparatus_: this is an apparatus based on the GVE model, which can be represented as a spine. The spine contains operations on the base text, optionally branching into trees. The apparatus contains any number of entries, each including a location (just like any other Cadmus layer fragment) and a GVE base operation.

As GVE uses a simpler and independent model for addressing text, an adapter is used to convert between Cadmus token-based coordinates (as used in most layer parts) and GVE coordinates.

Cadmus token-based coordinates are represented by 1 or 2 text points, each having:

- `Y`: the line ordinal number (1-N).
- `X`: the token ordinal number (1-N) in the line. A token in each line is defined as any sequence of characters separated by space. It is assumed that whitespaces are normalized and lines are trimmed, so in the end you find a single space separating each pair of tokens.
- `A` (at): the optional ordinal number (1-N) of the first character of the token to start the selection from. When 0, this means that the whole token is selected.
- `R` (run): the optional count of characters to select starting from `A`. This defaults to 1 when `A` is greater than 0.

You can have 1 or 2 such coordinates, named A and B. When only A is present, text is selected from a single token. When both A and B are present, text is selected from A to B as a range, which could span many tokens and/or lines. In such ranges you can also use `AR` to start the range from the `A`-th character of A's token, and/or end it with the `R`-th character of B's token.

The epigram may include many other generic or specialized parts in its item, to represent any type of related metadata. Also, bigger entities like collections can be represented as items too, and thus edited in the same centralized environment.
