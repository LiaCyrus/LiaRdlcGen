# LiaRdlcGen

LiaRdlcGen is a .NET 10 WinForms application for generating RDLC 2010/01 reports from JSON.

The current implementation accepts a direct JSON representation of the supported RDLC model, converts it into RDLC XML, and merges that content into a selectable template. Template-only content such as custom VB code is preserved.

## Current Features

- WinForms JSON editor and formatter
- Runtime RDLC template selection
- Direct JSON-to-C# model deserialization
- RDLC XML generation
- Template-based output
- Textbox, Rectangle, Image, Line, and Tablix support
- Data sources, datasets, fields, parameters, filters, sorting, and grouping
- Native page headers and footers
- Embedded images, styles, borders, visibility, and page breaks
- Built-in component test sample

## Run

```powershell
dotnet run --project .\LiaRdlcGen.WinForm\LiaRdlcGen.WinForm.csproj
```

In the application:

1. Select a template.
2. Click **Load sample** or enter direct RDLC model JSON.
3. Click **Create RDLC...**.

## Templates

Place `.rdlc` templates in:

```text
LiaRdlcGen.Core/Templates
```

Rebuild the application to include them in the template selector.

## Documentation

- [Usage](Docs/USAGE.md)
- [Architecture](Docs/ARCHITECTURE.md)
- [Current project status](AGENT/PROJECT_STATUS.md)
- [Direct JSON model](AGENT/RDLC_JSON_MODEL.md)
- [RDLC schema reference](AGENT/REPORT_DEFINITION_REFERENCE.md)

## Current Scope

The abstract AI-friendly component format is planned but not implemented. Chart, Gauge, Map, Subreport, CustomReportItem, and complete XSD coverage are also outside the current milestone.
