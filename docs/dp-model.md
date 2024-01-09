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

The [philological model](ph-model.md) already provides a tree-based structure for its annotations plane. Diplomatic metadata are just additions on top of this model, as properties of each variant generation operation.

## Metadata

Any variant generation operation has these metadata:

- `rank`
- `groupId`
- `features` (list):
  - `name`
  - `value`
  - `isUnique`
- `taggedOrdinals`
- `sources` (list):
  - `id`
  - `type`
  - `rank`
  - `note`
- `diplomatic`:
  - SVG `textPosition`:
    - text anchor (start, middle, end);
    - dominant baseline (the baseline used to align the box's text: values are like `auto`, `text-bottom`, `middle`, `central`, `mathematical`, `hanging`, `text-top`, etc.);
    - transformations applied to the position.
  - `text`: the optional text representing the variant. This is the original text found attached to the variant, optionally multiline.
  - `features`: generic features grouped as follows (as a dictionary of dictionaries, where the outer key is the feature type, and the inner key is the version tag, the end value being a `Feature` object):
    - functional groups, representing features like _styles_, _classes_ names, or _shapes_, as encoded in SVG.
      - version tag sub-groups: this allows applying the features conditionally, according to the version of the text being generated.
        - `name`
        - `value`
        - `isUnique`

While other properties belong to the philological model, diplomatic metadata are all represented by `diplomatic`.

### SVG Features

The most relevant aspects of SVG you can encode as features are listed here as a quick reference. The general structure of diplomatic features is summarized in this JSON example:

```json
{
  "styles": {
    "x": [
      {
        "name": "style CSS selector",
        "value": "style CSS value"
      }
    ]
  },
  "classes": {
    "x": [
      {
        "name": "class-name"
      }
    ]
  },
  "shapes": {
    "x": [
      {
        "name": "some-name",
        "value": "shape SVG code"
      }
    ]
  },
  "texts": {
    "x": [
      {
        "name": "some-name",
        "value": "text SVG code"
      }
    ]
  },
  "images": {
    "x": [
      {
        "name": "some-name",
        "value": "image SVG code"
      }
    ]
  }
}
```

#### SVG Styles and Classes

In addition to setting attributes on objects, you can also use CSS to style fills and strokes. Anyway, not all attributes can be set via CSS. Attributes that deal with painting and filling are usually available, so fill, stroke, stroke-dasharray, etc... can all be set this way.

You can also wrap several CSS properties into a class and add it to each SVG element.

#### SVG Shapes

- `rect`: `x` `y` `width` `height`, plus `rx` `ry` for radius of the corners.
- `circle`: `cx`, `cy` (center), `r` (radius).
- `ellipse`: `cx`, `cy` (center), `rx`, `ry` (radius).
- `line`: `x1`, `y1`, `x2`, `y2`.
- `polyline`: group of connected straight lines: `points` is a list of points where each number is whitespace/comma separated: e.g. `10 15, 30, 40`.
- `polygon`: a closed `polyline`. The path automatically connects the last point with the first, creating a closed shape.
- `path`: the most general shape, `d` contains drawing instructions. It combines multiple straight lines or curved lines. Each command is a single letter; uppercase for absolute coordinates, lowercase for relative coordinates (with reference to the last point). Coordinates here are always unitless.
  - `M` = move to: `x y`, without drawing a line.
  - `L` = line to: `x y` (from the current position). Abbreviated forms with a single parameter are `H` = horizontal line, `V` = vertical line.
  - `Z` = close path.
  - `C`, `Q`, `S`, `T` = Bezier curves.
  - `A` = arc (section of ellipses).

#### SVG Texts

The `text` element has `x` and `y`, and the text is its content. Other attributes:

- `text-anchor`: `start`, `middle`, `end` or `inherit`.
- `dominant-baseline`: vertical alignment.
- `fill`
- `stroke`
- CSS-like attributes for font: `font-family`, `font-style`, `font-weight`, `font-variant`, `font-stretch`, `font-size`, `font-size-adjust`, `kerning`, `letter-spacing`, `word-spacing` and `text-decoration`.
- `tspan` used inside a `text`, like `span` in HTML.
- `textPath` draws text on a path, e.g.:

```xml
<path id="my_path" d="M 20,20 C 80,60 100,40 120,20" fill="transparent" />
<text>
  <textPath xmlns:xlink="http://www.w3.org/1999/xlink" xlink:href="#my_path">
    A curve.
  </textPath>
</text>
```

#### Images

SVG has an `image` element to embed raster/vector images. The specification requests applications to support at least PNG, JPEG and SVG format files.

The embedded picture becomes a normal SVG element. This means, that you can use clips, masks, filters, rotations and all other tools of SVG on the content.

#### Attributes

Most important attributes:

- `fill`
- `stroke`
- `fill-opacity`
- `stroke-opacity`
- `stroke-width`: the width of the stroke from its path to both edges.
- `stroke-linecap`: `butt`, `square` (stretches the stroke), `round` (rounded at end).
- `stroke-linejoin`: `miter`, `round`, `bevel`.
- `stroke-dasharray`: comma separated numbers (first for the filled area, second for the unfilled area).

#### Transforms

You can use `g` to group elements for transformations. The `transform` attribute can use one or more functions, separated by spaces, like:

- `translate(x,y)`
- `rotate(d)`
- `scale(x,y)`
- `skewX(d)`
- `skewY(d)`
- `matrix`: a 2x3 transformation matrix. To combine several transformations, we can set the resulting matrix directly with the `matrix(a, b, c, d, e, f)` transformation which maps coordinates from a previous coordinate system into a new coordinate system. See [here](https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/transform#General_Transformation) for more.
