# Using LiaRdlcGen

## Run the Application

Open `LiaRdlcGen.slnx`, set `LiaRdlcGen.WinForm` as the startup project, and run it.

From the command line:

```powershell
dotnet run --project .\LiaRdlcGen.WinForm\LiaRdlcGen.WinForm.csproj
```

## Generate a Report

1. Select an RDLC template.
2. Enter direct RDLC model JSON or click **Load sample**.
3. Optionally click **Format JSON**.
4. Click **Create RDLC...**.
5. Select the output filename.

The application deserializes the JSON, generates RDLC content, merges it into the selected template, and saves the result.

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
