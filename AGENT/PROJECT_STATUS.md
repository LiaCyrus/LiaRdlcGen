# LiaRdlcGen Current Status

## Current Goal

Generate RDLC 2010/01 reports from JSON while preserving compatibility with the existing report/PDF infrastructure.

The current milestone implements a direct, one-to-one JSON representation of the supported RDLC model. An AI-friendly component format is planned but not implemented yet.

## Implemented Pipeline

```text
WinForms JSON editor
    -> RdlcJsonService
    -> RdlcReport model
    -> RdlcContentGenerator
    -> generated RDLC XML
    -> TemplateRdlcGenerator
    -> selected template merged with generated content
    -> saved .rdlc file
```

## Implemented Projects

### `LiaRdlcGen.Core`

Contains:

- Direct RDLC models under `Models/Rdlc`.
- JSON serialization and polymorphic report-item handling.
- RDLC XML generation.
- Template discovery and merge behavior.
- A component-level sample report factory.

### `LiaRdlcGen.WinForm`

Contains:

- Multiline JSON editor.
- Direct-model sample loader.
- JSON formatter that preserves characters such as `&`.
- Template selector.
- Save dialog and RDLC generation action.
- Status and error messages.

## Supported Direct RDLC Model

Report-level support:

- Report sections
- Body
- Page settings
- Native page header and footer
- Report parameters
- Embedded images
- Custom report code
- Data sources
- Datasets
- Queries
- Fields
- Filters
- Sorting
- Groups
- Page breaks

Visual component support:

- Textbox
- Rectangle
- Image
- Line
- Tablix

Supporting structures:

- Paragraphs and text runs
- Styles and borders
- Visibility
- Tablix body, rows, columns, cells, and recursive hierarchies

## Template Behavior

Templates live under:

```text
LiaRdlcGen.Core/Templates
```

All `.rdlc` files in that directory are copied to the WinForms output and shown in the template combobox.

Generation uses the selected template as a baseline. Generated top-level report elements replace matching template elements. Template-only elements are retained. This preserves the custom VB `<Code>` section in `Template201001.rdlc`.

## Component Test Sample

`RdlcSampleFactory` generates the sample loaded by the WinForms application. It covers:

- Standalone textbox
- Line
- Rectangle with nested textbox and image
- Embedded image
- Tablix headers and detail rows
- Dataset grouping and sorting
- Page header and footer
- Data source, dataset, fields, parameter, styles, and borders

## Important RDLC Styling Scope

RDLC rich text uses different style levels:

- Textbox style: container properties such as `VerticalAlign`, padding, background, and border.
- Paragraph style: paragraph properties such as `TextAlign`.
- TextRun style: font family, size, weight, color, and formatting.

Putting `TextAlign` only on the Textbox style does not reliably center rich text.

## Not Implemented

- Abstract AI component compiler
- Themes and reusable semantic components
- Direct-model override merging for abstract components
- Chart
- Gauge
- Map
- Subreport
- CustomReportItem
- Actions and drill-through
- Complete coverage of all 247 XSD complex types
- Automated validation against `ReportDefinition.xml`
- Rendering tests against the existing PDF engine

## Next Recommended Step

Define one abstract component contract, such as `title` or `table`, and compile it into the existing direct `RdlcReport` model. Allow named component parts to accept direct-model overrides so callers can customize ordinary RDLC properties without generating the complete direct JSON.

## Authoritative Documents

- `CODING_CONVENTIONS.md`: code formatting and comment-preservation rules.
- `RDLC_REFERENCE.md`: compatibility details from the existing reporting system.
- `REPORT_DEFINITION_REFERENCE.md`: technical inventory of `ReportDefinition.xml`.
- `RDLC_JSON_MODEL.md`: direct JSON model and template merge design.
- `PROJECT_STATUS.md`: implemented behavior and current limitations.
