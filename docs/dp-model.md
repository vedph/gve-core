# Diplomatic Model

The diplomatic model represents the graphical layout of the variants attached to the base text. Essentially, it is a set of metadata optionally attached to each operation, whatever its plane.

As for the general layout, the main assumptions are:

- the **base text tokens** ("words" in a graphical sense, i.e. sequences of characters delimited by whitespace) are laid out linearly on a single row, until line end; after line end, they continue on the next row. Line end is represented by a newline character in the base text. So, the base text tokens position is automatically determined by the layout algorithm.
- the **annotations** on top of each base text selection are always positioned _relative_ to the starting point of that selection.

So, consider this picture, where the right side is just a more detailed legend about positioning tokens. Here the origin tokens are colored in green, and are distributed in two lines, according to the original layout.

![diplomatic tokens](img/diplo-token.png)

Child tokens are colored in magenta, and grand-children tokens in red. In this example, we have:

- an origin token, `two`;
- two child tokens of `two`:
  - `c1`, in turn having a child token:
    - `c1.c1`
  - `c2`, sibling of `c1`.

So, descriptively this means that:

- the original token was `two`;
- then two variants came out from `two`, `c1` and `c2`, not necessarily in this order.
- then, another variant `c1.c1` came out of `c1`.

The underlying structure for this description is a tree model. Each origin token is the root of a tree which may indefinitely branch; so that we have variants, but also variants of variants.

So, the **diplomatic metadata** attached to each operation have these properties:

- an SVG **text position**, including any of these properties:
  - text anchor (start, middle, end);
  - dominant baseline (the baseline used to align the box's text: values are like `auto`, `text-bottom`, `middle`, `central`, `mathematical`, `hanging`, `text-top`, etc.);
  - transformations applied to the position.
- a set of generic **features**. These are grouped as follows (as a dictionary of dictionaries, where the outer key is the feature type, and the inner key is the version tag, the end value being a `Feature` object):
  - functional groups, representing features like _styles_, _classes_ names, or _shapes_, as encoded in SVG.
    - version tag sub-groups: this allows applying the features conditionally, according to the version of the text being generated.
