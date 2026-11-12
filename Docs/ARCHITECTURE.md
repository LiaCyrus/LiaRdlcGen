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
4. Applies the selected generation mode.
5. Saves the merged document.

Full report mode replaces matching top-level elements by qualified XML name, adds missing generated elements, and retains template-only top-level elements.

Template items mode requires exactly one generated report section and one template report item named `TemplatePlaceholder`. It replaces that placeholder with the generated first section's body report items. All other template content remains unchanged.

Neither mode recursively merges individual datasets or existing report items.

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

## Declarable Components

Template items mode supports a concise AI-facing component format that compiles into the direct model:

```text
Component JSON
    -> ComponentTemplateCompiler
    -> declarations from ComponentTemplates/*.json
    -> direct RdlcReport model
    -> existing generation pipeline
```

Each catalog declaration maps a semantic `type` to a direct `reportItem` template. Component input supplies its name, value, position, dimensions, and optional recursive direct-model overrides. New semantic textbox styles can therefore be added without recompiling the application.
