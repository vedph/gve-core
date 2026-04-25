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
  // either error message, or the result
  "error": null,
  "result": {
  	// execution steps
    "steps": [
      // sample step
      {
      	// the operation which generated the step
        "operation": {
          "dsl": "",
          "toAsIndex": false,
          "to": 0,
          "toRun": 0,
          "inputTag": "",
          "outputTag": "",
          "atAsIndex": false,
          "at": 0,
          "run": 0,
          "value": "",
          "noTraceFeatures": false,
          "type": 1,
          "id": "",
          "rank": "",
          "groupId": "",
          "features": [
            {
              "isNegated": false,
              "isGlobal": false,
              "isShortLived": false,
              "setPolicy": 0,
              "name": "",
              "value": null
            }
          ],
          "sources": [
            {
              "id": "",
              "type": null,
              "rank": 0,
              "note": null
            }
          ]
        },
        // input and output tags
        "inputTag": "",
        "outputTag": "",
        // result text
        "result": "",
        // IDs of nodes affected by operation
        "refNodeIds": [
          ""
        ]
      }
    ],
    // chain textual dump
    "chainDump": null,
    // chain tags in order
    "chainTags": [
      ""
    ],
    // chain text nodes
    "nodes": [
      {
        "id": 0,
        "index": 0,
        "label": "",
        "data": "",
        "sourceTag": "",
        // node features keyed by version tag
        "features": {
          "version-tag": [
            {
              "setPolicy": 0,
              "name": "",
              "value": ""
            }
          ]
        }
      }
    ],
    // IDs of nodes in the sequence building the version's text,
    // keyed by version tag
    "versionNodeIds": {
      "version-tag": [
        1,2,3
      ]
    },
    // global features, keyed by version tag
    "globalFeatures": {
      "version-tag": [
        {
          "isNegated": false,
          "isGlobal": false,
          "isShortLived": false,
          "setPolicy": 0,
          "name": "",
          "value": ""
        }
      ]
    }
  }
}
```
