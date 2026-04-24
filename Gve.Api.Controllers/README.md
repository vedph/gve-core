# Gve.Api.Controllers

Controllers for GVE RESTful API.

## TextController

This controller is used to manage text transformations via chains.

- `GET api/text/operations/tags`: gets input and output tags for the given operations. Output tags are calculated so this endpoint is required to get them.
- `GET api/text/operations/dsl`: gets the DSL representation for the given operations.
- `POST api/text/operations/run`: runs the specified operations on the specified base text.
