# Direct RDLC JSON Model

## Purpose

The direct RDLC JSON model is the serialization format for the C# classes under `Models/Rdlc`. It is intentionally close to the RDLC 2010/01 document structure.

The generation pipeline is:

```text
Direct RDLC JSON
    -> RdlcReport model
    -> generated RDLC document
    -> merge into selected RDLC template
    -> final RDLC file
```

The selected template is the baseline. Generated top-level elements replace elements with the same qualified XML name. Elements that are only present in the template remain in the final report. This currently preserves template custom VB code and other template metadata that the direct model does not supply.

The future abstract component format will add a compiler before the direct model:

```text
Abstract component JSON -> template/component compiler -> RdlcReport model -> RDLC XML
```

Both paths share the same `RdlcReport` model and XML generator.

## Direct Model Rules

- JSON properties use camel case.
- XML collections become JSON arrays.
- XML elements become model properties.
- Required XML `Name` attributes become `name` properties.
- Sizes retain their RDLC units, such as `"20cm"` or `"10pt"`.
- Expressions remain opaque strings.
- Optional properties can be omitted.
- Report items use a `type` discriminator because `ReportItems` is polymorphic.

Supported report item discriminator values:

- `line`
- `rectangle`
- `textbox`
- `image`
- `tablix`

Example:

```json
{
  "type": "textbox",
  "name": "txtTitle",
  "top": "0cm",
  "left": "0cm",
  "height": "0.8cm",
  "width": "20cm",
  "paragraphs": [
    {
      "textRuns": [
        {
          "value": "&title"
        }
      ]
    }
  ]
}
```

`type` is JSON serialization metadata. It does not become an RDLC XML element or attribute.

## Reference Sample

`RdlcSampleFactory` creates the executable direct-model example loaded by the WinForms application. `RdlcJsonService` serializes it into the JSON editor.

The sample exercises all currently supported visual components. It is code-generated and has no dependency on an external sample JSON file.

The abstract component format is not implemented. It will be a separate contract and must not be deserialized directly as `RdlcReport`.

## Abstract Components and Customization

Abstract components should behave as reusable templates or macros. For example, an abstract `title` component can generate an RDLC rectangle, image, and several textboxes.

The compiler should apply values in this order:

1. RDLC model defaults.
2. Theme defaults.
3. Abstract component template.
4. Abstract component input.
5. Explicit direct-model overrides.

Conceptual example:

```json
{
  "type": "title",
  "title": "&title",
  "logo": "!d1.image_ulogo",
  "overrides": {
    "titleTextbox": {
      "height": "1cm",
      "style": {
        "fontSize": "18pt",
        "color": "DarkBlue"
      }
    }
  }
}
```

The keys inside `overrides` identify named parts exposed by the component template. Their values use properties from the direct RDLC model.

This design provides concise AI output while retaining direct control over normal RDLC properties.

## Boundaries

The current direct model covers the initial scope:

- Report, sections, body, page, page header, and page footer
- Data sources, datasets, queries, fields, filters, and parameters
- Textbox, Rectangle, Image, Line, and Tablix
- Paragraphs, text runs, styles, borders, visibility, groups, sorting, and page breaks

Chart, Gauge, Map, Subreport, CustomReportItem, actions, and remaining advanced XSD structures can be added to the direct model when required.

## Current Services

| Service | Responsibility |
|---|---|
| `RdlcJsonService` | Serialize, deserialize, and format direct model JSON. |
| `RdlcJsonGenerator` | Convert direct model JSON into a standalone generated RDLC document. |
| `RdlcContentGenerator` | Serialize `RdlcReport` models into 2010/01 RDLC XML. |
| `TemplateRdlcGenerator` | Merge generated RDLC elements into the selected template and save the final report. |
| `RdlcSampleFactory` | Build the component test report shown in the WinForms editor. |

## Template Selection

The WinForms application discovers all `.rdlc` files copied into its `Templates` output directory. The project includes every file matching:

```text
LiaRdlcGen.Core/Templates/*.rdlc
```

Adding a template to that source directory makes it available in the combobox after rebuilding the application.

The default template is `Template201001.rdlc`.
