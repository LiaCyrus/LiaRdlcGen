# LiaRdlcGen Architecture

## Overview

LiaRdlcGen separates JSON processing, the RDLC object model, XML generation, and template merging.

```text
JSON editor
    |
    v
RdlcJsonService
    |
    v
RdlcReport and related models
    |
    v
RdlcContentGenerator
    |
    v
Generated RDLC XML
    |
    + selected template
    |
    v
TemplateRdlcGenerator
    |
    v
Final RDLC file
```

## Direct Model

Classes under `LiaRdlcGen.Core/Models/Rdlc` represent the supported RDLC structures directly.

The model intentionally keeps:

- Unit-bearing sizes as strings.
- Expressions as opaque strings.
- Required XML names as explicit `Name` properties.
- Tablix body and hierarchy structures separate.
- Style scopes attached to Textbox, Paragraph, and TextRun objects.

`RdlcReportItem` is polymorphic. `System.Text.Json` uses the `type` discriminator to instantiate a supported concrete report item.

## JSON Services

### `RdlcJsonService`

- Deserializes direct JSON into `RdlcReport`.
- Serializes `RdlcReport` into camel-case JSON.
- Formats JSON without escaping `&` as `\u0026`.
- Supports comments and trailing commas while reading.

### `RdlcJsonGenerator`

Combines `RdlcJsonService` and `RdlcContentGenerator` when standalone generated XML is needed.

## XML Generation

`RdlcContentGenerator` writes RDLC using `XmlWriter`.

It:

- Uses the 2010/01 report definition namespace.
- Writes report designer metadata in the `rd` namespace.
- Escapes XML values correctly.
- Validates required report sections, report-item names, textbox text runs, and Tablix structures.
- Supports writing to a string or file.

## Template Merge

`TemplateRdlcGenerator`:

1. Deserializes and generates the direct JSON model.
2. Loads the selected template with `XDocument`.
3. Confirms both documents use the same report root namespace.
4. Replaces matching top-level elements by qualified XML name.
5. Adds generated top-level elements that are not already present.
6. Retains template-only elements.
7. Saves the merged document.

The merge is currently top-level. It does not recursively merge individual report items or datasets.

## WinForms Application

`MainForm` provides:

- Direct JSON editing.
- Sample generation from `RdlcSampleFactory`.
- JSON formatting.
- Runtime template discovery.
- Output file selection.
- Error reporting.

Templates are discovered from:

```text
<application output>/Templates/*.rdlc
```

The WinForms project copies every source template from `LiaRdlcGen.Core/Templates`.

## Future Abstract Components

The future AI-facing format should not replace the direct model. It should compile into it:

```text
Abstract component JSON
    -> component expansion and layout
    -> direct RdlcReport model
    -> existing generation pipeline
```

Abstract components should expose named parts with optional direct-model overrides. This keeps ordinary AI output concise while preserving full control when needed.
