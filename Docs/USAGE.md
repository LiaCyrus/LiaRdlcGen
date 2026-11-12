# Using LiaRdlcGen

## Run the Application

Open `LiaRdlcGen.slnx`, set `LiaRdlcGen.WinForm` as the startup project, and run it.

From the command line:

```powershell
dotnet run --project .\LiaRdlcGen.WinForm\LiaRdlcGen.WinForm.csproj
```

## Generate a Report

1. Select an RDLC template.
2. Select **Full report** or **Template items** mode.
3. Enter direct RDLC model JSON or click **Load sample**.
4. Optionally click **Format JSON**.
5. Click **Create RDLC...**.
6. Select the output filename.

The application deserializes the JSON, generates RDLC content, merges it into the selected template, and saves the result.

## Generate JSON from an Image

1. Enter a Gemini API key and model, then click **Save AI settings**.
2. Click **Select image...** and choose a PNG or JPEG report image smaller than 15 MB.
3. Click **Generate JSON**.
4. Review and adjust the generated component JSON.
5. Click **Create RDLC...**.

The default model is `gemini-3.5-flash`. Gemini structured output restricts the result to the supported `title`, `textbox`, and `tablix` component contract. AI-generated dimensions and text can still be inaccurate, so review the JSON before generating RDLC.

The image-analysis instructions are loaded at runtime from `Prompts/ImageToLayout.txt`. Edit or replace that file to refine AI behavior without changing C# code.

Settings are stored for the current Windows user under `%LOCALAPPDATA%\LiaRdlcGen\settings.json`. The API key is stored as plain text in that user-local file; do not use this option on a shared Windows account.

### Generation Modes

- **Template items** is the default. It takes only the report items from the first generated report section's body and replaces the single template report item named `TemplatePlaceholder` with those items in their original order. Their `Top` positions are shifted below the lowest existing item in the same template container, and the container height grows when needed.
- **Full report** replaces matching top-level template elements with the generated report elements.

Template items mode preserves all other template content, including datasets, entity information, logo, page settings, page footer, custom code, and metadata. The selected template must contain exactly one report item whose `Name` is `TemplatePlaceholder`.

Template items mode also accepts concise component JSON:

```json
{
  "components": [
    {
      "type": "title",
      "name": "txtTitle",
      "value": "&title",
      "top": "0cm",
      "left": "0cm",
      "width": "20cm"
    },
    {
      "type": "textbox",
      "name": "txtContent",
      "value": "Normal textbox content",
      "top": "1cm",
      "left": "0cm",
      "width": "20cm"
    }
  ]
}
```

Component types are declared in `ComponentTemplates/*.json`. Each declaration contains a direct RDLC `reportItem` template, including defaults such as fonts, padding, alignment, and dimensions. Add another declaration to create a new AI-facing component type without changing C# code. A component can supply an `overrides` object to replace or extend direct report-item properties.

The included `tablix` component provides the visual defaults from the reference report while the AI supplies the detected columns, widths, labels, values, and alignment:

```json
{
  "type": "tablix",
  "name": "Detail",
  "top": "2cm",
  "left": "0cm",
  "width": "20cm",
  "columns": [
    {
      "name": "itemCode",
      "width": "4cm",
      "header": "Item code",
      "value": "!d2.item_code",
      "textAlign": "Left"
    },
    {
      "name": "quantity",
      "width": "3cm",
      "header": "Quantity",
      "value": "!d2.quantity",
      "textAlign": "Right",
      "format": "round_qty"
    }
  ]
}
```

`dataSetName` is optional. Dataset definitions are not added to the RDLC template; database mapping remains separate from the image-derived visual layout.

**Load sample** loads the concise title and textbox example in template items mode. In full report mode it loads the complete direct-model sample.

## JSON Format

The editor currently accepts the direct RDLC model, not the planned abstract AI component format.

Report item arrays require a `type` discriminator:

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
      ],
      "style": {
        "textAlign": "Center"
      }
    }
  ],
  "style": {
    "verticalAlign": "Middle"
  }
}
```

Supported `type` values:

- `textbox`
- `rectangle`
- `image`
- `line`
- `tablix`

Sizes must include RDLC units, for example `20cm`, `8pt`, or `0.5in`.

Expressions and custom placeholders remain strings:

- `&title`
- `!d1.ten_kh`
- `#Code.RFuns.ReplaceDate(...)`
- `=Fields!so_luong.Value`

## Text Alignment

For rich textboxes:

- Put `textAlign` in the paragraph `style`.
- Put `verticalAlign` in the textbox `style`.
- Put font properties in the text run `style`.

## Add a Template

Add the new `.rdlc` file to:

```text
LiaRdlcGen.Core/Templates
```

Rebuild the application. The template will appear in the combobox automatically.

Templates must use this RDLC namespace:

```text
http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition
```

## Template Merge Rules

- The selected template is loaded first.
- The JSON model is converted into a generated RDLC document.
- Generated top-level elements replace matching elements in the template.
- Generated elements missing from the template are added.
- Template elements absent from the generated document are retained.

This allows template custom code and other baseline metadata to survive generation.

## Current Limitations

Only the initial component subset is supported. Chart, Gauge, Map, Subreport, CustomReportItem, and the abstract AI component format are not implemented yet.
