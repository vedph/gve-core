# CLAUDE.md

## Purpose of the Solution

This solution contains models, logic, API and CLI tools to represent the transformations of a base text into multiple versions for philological purposes.

## Projects Overview

Each project has a corresponding XUnit test project with the same name + `.Test`.

- core:
  - `Gve.Text`: core models and logic.
  - `Gve.Migration`: data export (mostly for TEI rendition).
- apps:
  - `Gve.Demo` and `Gve.Demo.Client`: Blazor developer's app for testing core components.
  - `gve`: CLI utility tool.
  - `Gve.Api`: ASP.NET Core Web API for managing GVE data. Infrastructure only, logic is in other API projects.
  - `Gve.Api.Controllers`: logic for API controllers, separated from the infrastructure project to make it reusable.
  - `Gve.Api.Models`: models for API controllers, separated from the infrastructure project to make it reusable.
- Cadmus: libraries to use the GVE model inside an editor (code-named Cadmus):
  - `Cadmus.Gve.Parts`: models for GVE.
  - `Cadmus.Gve.Services`: services to manage GVE data in the editor.
  - `Cadmus.Seed.Gve.Parts`: mock data seeders for GVE parts.
  - `CadmusGveApi`: API client for GVE, to be used in the Cadmus editor frontend.

Some dependencies can be found in these folders:

- Cadmus: `\Projects\Cadmus`
- Fusi.Antiquity: `\Projects\Antiquity`
- Fusi.Tools: `\Projects\Tools`

## Coding Conventions

As for C# code:

- Use modern, idiomatic, robust C#.
- Avoid systematic use of `var`; use explicit types unless required.
- Use nullable reference types (`string?`).
- Apply single responsibility principle.
- Write clear, robust code with appropriate error handling.
- Validate non-null arguments (`ArgumentNullException.ThrowIfNull`).
- Comment public classes and methods.
- Keep lines under ~80 characters; split logically.
