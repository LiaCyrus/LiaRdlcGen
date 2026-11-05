# LiaRdlcGen – Existing RDLC Technical Reference

## 1. Project Context

Project name: `LiaRdlcGen`

Purpose:
- Generate RDLC report files from structured JSON.
- AI will eventually analyze report images/PDFs and produce the JSON layout.
- C# will convert the JSON into RDLC XML.
- Generated RDLC files will be manually editable in Visual Studio or Report Builder.
- Existing PDF rendering infrastructure must remain compatible.

Technology:
- C# / .NET 10
- WinForms for desktop UI
- .NET Class Library for core logic
- `System.Text.Json` for JSON processing
- `System.Xml.XmlWriter` for RDLC generation
- `System.Xml.Linq` for XML manipulation

## 2. Reference RDLC File

Reference file: `m46_01(1).rdlc`

This file is an existing report from the current reporting system. Use it as the structural and compatibility reference when implementing the generator.

### RDLC namespace

The report uses the following namespaces:

- Default namespace:
  `http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition`
- Report designer namespace:
  `http://schemas.microsoft.com/SQLServer/reporting/reportdesigner`
- Component definition namespace:
  `http://schemas.microsoft.com/sqlserver/reporting/2010/01/componentdefinition`

The report uses the Report Definition Language 2010/01 structure.

Important:
- Preserve the namespace and report structure.
- Do not assume that a generic RDLC example is compatible with this reporting system.
- Generated reports must be tested with the existing PDF rendering engine.

## 3. Page Settings

The reference report uses:

| Property | Value |
|---|---:|
| Page width | 21 cm |
| Page height | 29.7 cm |
| Orientation | Portrait |
| Left margin | 0.5 cm |
| Right margin | 0.5 cm |
| Top margin | 0.5 cm |
| Bottom margin | 0.5 cm |
| Available content width | 20 cm |

Default page configuration for the initial implementation should follow these values, while allowing the JSON to override them.

## 4. Report Structure

The report uses the following hierarchy:

Report
- ReportSections
  - ReportSection
    - Body
      - ReportItems
        - Rectangle: ReportHeader
          - Textboxes
        - Rectangle: LogoHeader
          - Image: image_ulogo
          - Textboxes: Entity_Line1 through Entity_Line4
        - Rectangle: ReportFooter2
          - Signature and approval textboxes
        - Other textboxes
        - Tablix: Detail
    - Page
      - PageFooter
        - Textbox: txtPrintDateTime
        - Textbox: txtPageIndex

Important distinction:

`ReportHeader` and `ReportFooter2` are rectangles inside the report body. They are NOT native RDLC report header/footer sections.

The report has a native PageFooter section.

## 5. Report Elements

The reference report contains:

| Element | Count |
|---|---:|
| Textbox | 38 |
| Rectangle | 3 |
| Image | 1 |
| Tablix | 1 |
| Standalone Line | 0 |

The initial generator should support:

- Textbox
- Rectangle
- Image
- Line
- Tablix

It should also support page header/footer sections and report-level properties.

## 6. Dataset Definitions

The report contains two datasets.

### Dataset d1 – Header

Contains 11 fields:

- id
- so_ct
- ngay_ct
- ma_kh
- ten_kh
- dien_giai
- ma_md
- ten_md
- ngay_kh1
- ngay_kh2
- image_ulogo

### Dataset d2 – Detail

Contains 9 fields:

- stt
- id
- line
- ma_vt
- dvt
- ma_kho
- so_luong
- ten_vt
- ten_dvt

The Detail Tablix uses dataset `d2`.

Dataset definitions and database mappings should be treated separately from visual layout. The initial generator should be able to produce the visual layout without requiring an actual database connection.

## 7. Tablix Structure

The Detail Tablix has:

- 6 columns
- 2 rows
- 6 header cells
- 6 detail cells

Column widths:

| Column | Width |
|---|---:|
| stt | 1.5 cm |
| ma_vt | 3 cm |
| ten_vt | 6.5 cm |
| ten_dvt | 3 cm |
| ma_kho | 3 cm |
| so_luong | 3 cm |
| Total | 20 cm |

Rows:

Header row:
- Height: 0.5 cm
- Contains column labels.

Detail row:
- Height: 0.5 cm
- Contains dataset field references.

The Tablix uses:
- TablixBody
- TablixColumns
- TablixRows
- TablixRowHierarchy
- TablixColumnHierarchy

Important:
A Tablix is not simply an HTML-style table. Its row and column hierarchies must be generated correctly.

## 8. Custom Expression Syntax

The existing report uses both standard RDLC expressions and custom expressions interpreted by the existing PDF/report engine.

These conventions must be preserved.

| Syntax | Example | Meaning |
|---|---|---|
| `&name` | `&title` | Custom multilingual/static field placeholder |
| `!dataset.field` | `!d1.ngay_ct` | Custom dataset field reference |
| `#expression` | `#Code.RFuns.ReplaceDate(...)` | Custom expression |
| `=Fields!field.Value` | `=Fields!so_luong.Value` | Standard RDLC expression |

### XML escaping

Example in RDLC XML:

`<Value>&amp;title</Value>`

After XML parsing, the value is:

`&title`

The generator must use proper XML escaping. Do not manually replace `&` with `&amp;` before passing values to XmlWriter, because this may cause double escaping.

### Custom expression handling

Treat custom expressions as opaque strings.

The generator must:
- Preserve expression text.
- Avoid interpreting or rewriting custom syntax.
- Avoid converting custom expressions into standard RDLC expressions.
- Preserve compatibility with the existing rendering engine.

## 9. Custom VB Code

The report contains a custom `<Code>` section defining a VB.NET class named `RFuns`.

It includes functions for:
- Number formatting.
- String replacement.
- Date formatting.
- Footer date/time.
- Page numbering.

Example:

`Public Shared Function ReplaceDate(ByVal cText As String, d As Date) As String`

This code must eventually be supported as a report-level property.

Do not hardcode the RFuns implementation into every element generator. Keep custom report code separate from visual elements.

## 10. Other Report-Level Components

The reference report also contains:

- One data source: `AriData`
- Two datasets: `d1`, `d2`
- One report parameter: `title`
- One custom VB code section: `RFuns`
- One native PageFooter
- One database image with MIME type `image/png`
- Image sizing mode: `FitProportional`

These should be represented independently from the visual layout.

## 11. Recommended JSON Model Structure

The JSON should be independent of RDLC XML but expressive enough to represent the structures required by the reference report.

Suggested top-level structure:

```json
{
  "version": "1.0",
  "reportSettings": {
    "rdlcNamespace": "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition",
    "pageSettings": {},
    "parameters": [],
    "customCode": ""
  },
  "dataDefinitions": {
    "dataSources": [],
    "dataSets": []
  },
  "body": {
    "width": 20,
    "height": 0,
    "elements": []
  },
  "pageHeader": null,
  "pageFooter": null
}
```

This is a proposed structure, not a finalized schema.

The JSON model should support:

- Report-level settings.
- Page dimensions and margins.
- Report parameters.
- Custom VB code.
- Data source and dataset declarations.
- Textboxes.
- Rectangles.
- Images.
- Lines.
- Tablix with rows, columns and cells.
- Element positioning and sizing.
- Fonts, borders, backgrounds, padding and alignment.
- Native page header/footer sections.

Do not generate the entire RDLC XML directly from AI. AI should generate the JSON model, and C# should generate the RDLC.

## 12. Recommended C# Project Structure

```text
LiaRdlcGen/
│
├── LiaRdlcGen.Core/
│   ├── Models/
│   │   ├── ReportLayout.cs
│   │   ├── ReportSettings.cs
│   │   ├── ReportPage.cs
│   │   ├── ReportElement.cs
│   │   ├── ReportStyle.cs
│   │   ├── Tablix.cs
│   │   └── FieldDefinition.cs
│   │
│   ├── Generators/
│   │   ├── RdlcGenerator.cs
│   │   ├── FieldXmlGenerator.cs
│   │   └── Elements/
│   │       ├── TextboxGenerator.cs
│   │       ├── RectangleGenerator.cs
│   │       ├── ImageGenerator.cs
│   │       ├── LineGenerator.cs
│   │       └── TablixGenerator.cs
│   │
│   ├── Services/
│   │   └── JsonLayoutService.cs
│   │
│   └── Validators/
│       └── LayoutValidator.cs
│
├── LiaRdlcGen.WinForm/
│
└── LiaRdlcGen.Tests/
```

This is a suggested structure. Create classes only when their implementation is required.

## 13. Implementation Priorities

Recommended development sequence:

1. Define C# models for the JSON layout.
2. Define JSON serialization/deserialization.
3. Implement report root and namespace generation.
4. Implement page settings.
5. Implement textbox generation.
6. Implement rectangle generation.
7. Implement image generation.
8. Implement Tablix generation.
9. Implement report parameters and custom code.
10. Implement dataset definitions.
11. Implement native page headers and footers.
12. Validate the generated RDLC using the existing rendering engine.

## 14. Compatibility Requirements

These requirements are important:

- Preserve the 2010/01 RDLC namespace.
- Preserve custom expressions (`&`, `!`, `#` conventions).
- Preserve custom VB code when supplied.
- Generate valid XML with correct escaping.
- Support the Tablix hierarchy correctly.
- Maintain page dimensions and margins.
- Keep database mapping separate from visual layout.
- Generated RDLC must be manually editable in Visual Studio or Report Builder.
- Do not introduce unnecessary features beyond the requested implementation scope.

## 15. First Milestone

The first milestone is NOT AI integration.

Build a C# RDLC generator that:

1. Accepts a manually created JSON layout.
2. Generates a valid RDLC file.
3. Uses the same RDLC namespace and structure as the reference file.
4. Preserves the custom expression conventions.
5. Can be opened and rendered by the existing reporting infrastructure.

Use the uploaded `m46_01(1).rdlc` as the compatibility reference throughout implementation.

Do not reanalyze the reference file unless the original XML needs to be inspected for a specific implementation detail.