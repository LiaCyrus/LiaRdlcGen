# ReportDefinition.xml Technical Reference

## 1. Purpose and Scope

`ReportDefinition.xml` is a W3C XML Schema (XSD) for Microsoft SQL Server 2008 R2 Report Definition Language (RDL).

It describes the XML structures accepted in an RDL/RDLC report using this namespace:

```text
http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition
```

The namespace version is `2010/01`. Local report elements must be namespace-qualified because the schema uses `elementFormDefault="qualified"`.

This document inventories the schema so that a JSON layout model can be proposed later. It distinguishes:

- XML elements, represented as JSON properties or collections later.
- XML attributes, especially required `Name` attributes.
- Structural rules and cardinality.
- Runtime behavior implied by an element's role.
- Extension points that must be preserved for compatibility.

The schema mostly defines structure, not rendering semantics. It contains no component-level documentation, so behavioral descriptions below follow the component relationships and standard RDL meaning. Rendering must still be verified with the existing report engine.

## 2. Important Schema Conventions

### 2.1 Property bags

Most complex types use an unbounded XSD `choice`. This permits child properties in flexible order. Although the particle is unbounded, normal RDL uses each named property at most once unless the property is explicitly a collection.

These structures should become JSON objects rather than ordered arrays.

### 2.2 Collections

Wrapper types such as `DataSources`, `Fields`, `Filters`, and `TablixMembers` contain repeated child elements. These should become JSON arrays.

### 2.3 Polymorphic collections

`ReportItems` can contain different visual component types. A future JSON representation should use an array with an explicit component discriminator such as `type`.

### 2.4 Required names

Most visual components and many data objects have a required XML `Name` attribute. Names identify objects for expressions, grouping, toggling, repetition, and designer/runtime references.

### 2.5 Expressions

Many properties use `xsd:string` even when they represent a number, color, boolean-like value, or expression. The schema deliberately allows values such as:

```text
=Fields!Quantity.Value
=Parameters!Title.Value
2.5cm
Red
```

The JSON model must not aggressively convert all string-valued properties into primitive JSON numbers or booleans. It must preserve expressions and units.

### 2.6 Extension data

Most types allow:

- Foreign-namespace child elements through `xsd:any namespace="##other"`.
- Foreign-namespace attributes through `xsd:anyAttribute namespace="##other"`.

This is how designer metadata and vendor-specific extensions can coexist with standard RDL. A round-trip-capable JSON model should retain unknown namespaced data.

### 2.7 Type inheritance

The XSD does not define a shared base class for report items. Instead, common properties are repeated in each type. A C# or JSON model may introduce shared base models, but generators must emit the properties valid for each concrete XML type.

## 3. Document Structure

```text
Report
└── ReportSections
    └── ReportSection (one or more)
        ├── Body
        │   └── ReportItems
        └── Page
            ├── PageHeader
            └── PageFooter
```

### 3.1 Report

`Report` is the only global document element.

| Child element | Type | Required | Behavior |
|---|---|---:|---|
| `Description` | Localizable string | No | Human-readable report description. |
| `Author` | String | No | Report author metadata. |
| `AutoRefresh` | String | No | Refresh interval or expression. |
| `InitialPageName` | String | No | Initial rendered page name. |
| `DataSources` | Collection | No | Declares report data connections. |
| `DataSets` | Collection | No | Declares fields and queries. |
| `ReportParameters` | Collection | No | Declares externally supplied or prompted values. |
| `Code` | String | No | Embedded VB report code. |
| `CodeModules` | Collection | No | Referenced custom code assemblies/modules. |
| `Classes` | Collection | No | Custom class instances available to expressions. |
| `CustomProperties` | Collection | No | Arbitrary report-level name/value metadata. |
| `EmbeddedImages` | Collection | No | Named binary images embedded in the report. |
| `Language` | String | No | Report culture or culture expression. |
| `Variables` | Collection | No | Report-scope calculated values. |
| `DeferVariableEvaluation` | Boolean | No | Delays report variable evaluation. |
| `ConsumeContainerWhitespace` | Boolean | No | Controls removal of unused container whitespace. |
| `DataTransform` | String | No | XML transform metadata. |
| `DataSchema` | String | No | Data rendering schema metadata. |
| `DataElementName` | String | No | Name used by data renderers. |
| `DataElementStyle` | Enum | No | `Attribute` or `Element`. |
| `ReportSections` | Collection | **Yes** | Contains one or more report sections. |

`Report` has no standard attributes, but permits foreign-namespace attributes.

### 3.2 ReportSections and ReportSection

`ReportSections` must contain at least one `ReportSection`.

Each `ReportSection` requires:

| Child element | Type | Behavior |
|---|---|---|
| `Body` | `BodyType` | Main report content. |
| `Width` | `SizeType` | Logical report content width. |
| `Page` | `PageType` | Physical and interactive page settings. |

Optional properties are `PageName` and `DataElementName`.

### 3.3 Body

| Child element | Type | Required | Behavior |
|---|---|---:|---|
| `ReportItems` | Polymorphic collection | No | Visual components in the body. |
| `Height` | `SizeType` | **Yes** | Design height of the body. |
| `Style` | `StyleType` | No | Body background and border styling. |

### 3.4 Page

| Child element | Type | Behavior |
|---|---|---|
| `PageHeader` | `PageSectionType` | Repeated native page header. |
| `PageFooter` | `PageSectionType` | Repeated native page footer. |
| `PageHeight`, `PageWidth` | `SizeType` | Physical output page dimensions. |
| `InteractiveHeight`, `InteractiveWidth` | `SizeType` | Pagination dimensions for interactive rendering. |
| `LeftMargin`, `RightMargin`, `TopMargin`, `BottomMargin` | `SizeType` | Physical page margins. |
| `Columns` | Integer | Number of body columns. |
| `ColumnSpacing` | `SizeType` | Space between body columns. |
| `Style` | `StyleType` | Page-level style. |

### 3.5 PageHeader and PageFooter

A page section requires `Height` and may contain:

- `ReportItems`
- `PrintOnFirstPage`
- `PrintOnLastPage`
- `Style`

These are native repeating page sections. A rectangle named like a header or footer inside `Body` does not acquire page-section behavior.

## 4. Supported Report Item Components

`ReportItems` supports ten standard component kinds plus foreign-namespace extension items:

| XML component | Purpose |
|---|---|
| `Line` | Draws a styled line. |
| `Rectangle` | Visual container that can hold nested report items. |
| `Textbox` | Displays static text, rich text, fields, or expressions. |
| `Image` | Displays embedded, external, or database image content. |
| `Subreport` | Renders another report inside the current report. |
| `Chart` | Displays grouped data as a chart. |
| `GaugePanel` | Displays gauges and indicators. |
| `Map` | Displays spatial data and map layers. |
| `Tablix` | Displays table, matrix, or list-style grouped data. |
| `CustomReportItem` | Hosts a renderer-specific custom component. |

Every standard report item has a required `Name` attribute.

## 5. Shared Report Item Properties

The following property set is repeated on most report item types:

| Property | Type | Behavior |
|---|---|---|
| `Top`, `Left` | `SizeType` | Position relative to the containing body, rectangle, or cell. |
| `Height`, `Width` | `SizeType` | Designed component dimensions. |
| `ZIndex` | Unsigned integer | Layering order for overlapping items. |
| `Style` | `StyleType` | Border, background, font, alignment, and padding. |
| `ActionInfo` | `ActionInfoType` | Hyperlink, bookmark, or drill-through action. |
| `Visibility` | `VisibilityType` | Hidden expression and optional toggle source. |
| `ToolTip` | Localizable string | Hover/help text where supported. |
| `DocumentMapLabel` | Localizable string | Label in the document map. |
| `Bookmark` | String | Navigation target identifier or expression. |
| `RepeatWith` | String | Repeats a static item with a named data region. |
| `CustomProperties` | Collection | Arbitrary name/value metadata. |
| `DataElementName` | String | Exported data element name. |
| `DataElementOutput` | Enum | Controls data-renderer output. |

`DataElementOutput` commonly supports:

- `Auto`
- `Output`
- `NoOutput`
- `ContentsOnly`

The exact allowed values vary by component. For example, groups do not include `Auto`.

## 6. Core Visual Components

### 6.1 Line

`Line` is a named report item using the shared layout, style, visibility, action, tooltip, bookmark, and data-output properties.

Its visible appearance is primarily controlled by the border properties in `Style`. Its dimensions and start/end positioning determine line direction.

### 6.2 Rectangle

`Rectangle` adds:

| Property | Behavior |
|---|---|
| `ReportItems` | Contains nested visual components. |
| `PageBreak` | Controls pagination around the rectangle. |
| `KeepTogether` | Requests that the rectangle remain on one page when possible. |
| `OmitBorderOnPageBreak` | Suppresses borders where the rectangle spans pages. |
| `PageName` | Supplies a rendered page name. |

A rectangle is both a visual element and a coordinate container. Child positions are relative to the rectangle.

### 6.3 Textbox

`Textbox` requires `Paragraphs` and adds:

| Property | Behavior |
|---|---|
| `CanGrow` | Allows vertical growth when content does not fit. |
| `CanShrink` | Allows contraction when content uses less space. |
| `HideDuplicates` | Hides repeated values within a named scope. |
| `ToggleImage` | Displays expanded/collapsed state for a toggle target. |
| `UserSort` | Enables interactive sorting. |
| `KeepTogether` | Requests unbroken rendering where possible. |
| `DataElementStyle` | `Auto`, `Attribute`, or `Element` for data output. |

Text content is hierarchical:

```text
Textbox
└── Paragraphs
    └── Paragraph+
        └── TextRuns
            └── TextRun+
                └── Value
```

#### Paragraph

A paragraph may define:

- `TextRuns`
- `LeftIndent`
- `RightIndent`
- `HangingIndent`
- `SpaceBefore`
- `SpaceAfter`
- `ListStyle`: `None`, `Bulleted`, or `Numbered`
- `ListLevel`
- `Style`

#### TextRun

A text run requires `Value` and may define:

- `Style`
- `ActionInfo`
- `ToolTip`
- `MarkupType`

`Value` is a string-like value with optional `DataType` and `EvaluationMode`. `EvaluationMode` supports `Auto` and `Constant`, with `Auto` as the XSD default.

Multiple text runs allow mixed formatting and actions within one textbox.

### 6.4 Image

`Image` requires:

| Property | Values/behavior |
|---|---|
| `Source` | `External`, `Embedded`, or `Database`. |
| `Value` | URI, embedded image name, database expression, or compatible custom expression. |

Optional properties:

| Property | Behavior |
|---|---|
| `MIMEType` | Required by some dynamic/database image scenarios. |
| `Sizing` | `AutoSize`, `Fit`, `FitProportional`, or `Clip`. |

`FitProportional` preserves aspect ratio; `Fit` may scale independently by axis; `Clip` keeps size and clips overflow.

### 6.5 Subreport

`Subreport` requires `ReportName` and may define:

- `Parameters`
- `NoRowsMessage`
- `MergeTransactions`
- Shared report item properties

Each subreport parameter has a required `Name` attribute and contains a `Value` plus an optional `Omit` expression.

### 6.6 CustomReportItem

`CustomReportItem` requires a `Type` string identifying the custom implementation. It may contain:

- `CustomData`
- `AltReportItem`
- Shared layout and output properties

`AltReportItem` provides fallback standard report content when the custom renderer is unavailable.

`CustomData` can contain column and row hierarchies plus data rows/cells. This is a renderer extension mechanism and should not be flattened into ordinary report items.

## 7. Data Region Behavior

`Tablix`, `Chart`, and `GaugePanel` are data regions. Map supports related data-region concepts through map data regions.

Common data-region properties include:

| Property | Behavior |
|---|---|
| `DataSetName` | Selects the dataset used by the region. |
| `Filters` | Filters rows before display/group processing. |
| `SortExpressions` | Controls ordering. |
| `NoRowsMessage` | Content shown when no data is available. |
| `PageBreak` | Controls pagination around the region. |
| `PageName` | Names pages generated by the region. |
| `KeepTogether` | Requests contiguous rendering where possible. |

The schema can allow a missing dataset name, but runtime context determines whether one is required.

## 8. Tablix

A Tablix represents tables, matrices, and lists. It is not an HTML-style grid. Its body and row/column hierarchies must agree.

### 8.1 Tablix properties

Required:

- `TablixColumnHierarchy`
- `TablixRowHierarchy`

Optional:

- `TablixBody`
- `TablixCorner`
- `LayoutDirection`: `LTR` or `RTL`
- `RepeatColumnHeaders`
- `RepeatRowHeaders`
- `FixedColumnHeaders`
- `FixedRowHeaders`
- Common report item and data-region properties

### 8.2 Tablix body

```text
TablixBody
├── TablixColumns
│   └── TablixColumn+
│       └── Width
└── TablixRows
    └── TablixRow+
        ├── Height
        └── TablixCells
            └── TablixCell+
                └── CellContents
```

The body requires exactly one `TablixColumns` and one `TablixRows` collection.

Each body row must have cells corresponding to the effective column structure. Each cell can contain at most one report item and may define `ColSpan` or `RowSpan`.

### 8.3 Tablix hierarchies

Both row and column hierarchies contain recursive `TablixMember` objects.

A member may define:

- `Group`
- `SortExpressions`
- `TablixHeader`
- Nested `TablixMembers`
- `Visibility`
- `KeepTogether`
- `KeepWithGroup`: `None`, `Before`, or `After`
- `RepeatOnNewPage`
- `FixedData`
- `HideIfNoRows`
- `DataElementName`
- `DataElementOutput`

A `TablixHeader` requires:

- `Size`
- `CellContents`

Static members have no group. Dynamic members use a group and repeat for group instances.

### 8.4 Tablix corner

The optional corner is composed of rows and cells used where row headers and column headers intersect, particularly in matrix layouts.

## 9. Chart

`Chart` is a named data region. Its major components are:

- `ChartSeriesHierarchy`
- `ChartCategoryHierarchy`
- `ChartData`
- `ChartAreas`
- `ChartLegends`
- `ChartTitles`
- `Palette`
- `ChartBorderSkin`
- `NoDataMessage`
- Common report item and data-region properties

Series and category hierarchies recursively define static or grouped chart members. Chart data contains series and data points. Areas define axes, grid lines, tick marks, strip lines, scale breaks, 3D behavior, and plotting behavior.

The schema contains approximately 48 chart-related complex types, including:

- Chart areas and category/value axes
- Series and derived series
- Data points, labels, and markers
- Legends, legend columns, custom legend items, and cells
- Titles
- Formula parameters
- Smart labels
- 3D properties
- Border skins

Most named chart subcomponents require a `Name` attribute. A future JSON model should keep chart configuration as a dedicated subtree rather than forcing it into generic report-item properties.

## 10. GaugePanel

`GaugePanel` is a named data region that can contain:

- `RadialGauges`
- `LinearGauges`
- `NumericIndicators`
- `StateIndicators`
- `GaugeImages`
- `GaugeLabels`
- `BackFrame`
- `TopImage`
- `GaugeMember`
- Common report item and data-region properties

`GaugeMember` is a recursive grouping structure and requires a `Group`.

Gauge-specific component families include:

- Radial and linear scales
- Radial and linear pointers
- Scale ranges
- Tick marks, pins, and custom labels
- Pointer caps and thermometers
- Numeric indicator ranges
- Indicator states
- Frames and images

Most gauge components are named. Their values and visual states may be expressions evaluated for each gauge group instance.

## 11. Map

`Map` is a named report item and requires `MapViewport`.

It may contain:

- `MapLayers`
- `MapDataRegions`
- `MapLegends`
- `MapTitles`
- `MapDistanceScale`
- `MapColorScale`
- Shared report item properties

Supported layer kinds:

- `MapTileLayer`
- `MapPolygonLayer`
- `MapPointLayer`
- `MapLineLayer`

Spatial data can come from shapefiles, datasets, or embedded map geometry depending on layer configuration. Rules map data values to colors, sizes, markers, or ranges. Templates define how polygon, point, and line elements render.

The map family contains approximately 61 complex types covering:

- Viewport, views, limits, location, and size
- Tile, polygon, point, and line layers
- Spatial datasets and shapefiles
- Field definitions and binding pairs
- Color, size, marker, and range rules
- Polygon, point, and line templates
- Legends, titles, scales, and appearance
- Grouped map data regions

`MapDataRegion` is named. Its recursive `MapMember` requires a `Group`.

The schema leaves `Map`'s `DataElementOutput` less constrained than the same property on other report items. The generator should use known RDL values but preserve an unrecognized source value when round-tripping.

## 12. Data Sources and Datasets

### 12.1 DataSource

Each `DataSource` has a required `Name` attribute and can use:

- `ConnectionProperties`
- `DataSourceReference`
- `Transaction`

`ConnectionProperties` requires:

- `DataProvider`
- `ConnectString`

It may also define `IntegratedSecurity` and `Prompt`.

An embedded connection and a shared data-source reference are alternative configuration paths in normal usage.

### 12.2 DataSet

Each `DataSet` has a required `Name` attribute and may define:

- `Fields`
- `Query`
- `SharedDataSet`
- `Filters`
- `CaseSensitivity`
- `AccentSensitivity`
- `KanatypeSensitivity`
- `WidthSensitivity`
- `Collation`
- `InterpretSubtotalsAsDetails`

Sensitivity and subtotal properties use string enums such as `True`, `False`, and `Auto`; they are not ordinary XML booleans.

An embedded query and a shared dataset are alternative data acquisition paths in normal usage.

### 12.3 Query

An embedded `Query` requires:

- `DataSourceName`
- `CommandText`

Optional properties:

- `CommandType`: `Text`, `StoredProcedure`, or `TableDirect`
- `QueryParameters`
- `Timeout`

Each query parameter has a required `Name` attribute and a `Value`.

### 12.4 Field

Each field has a required `Name` attribute and may define:

- `DataField`: source column name.
- `Value`: calculated field expression or value with optional data type metadata.

Field definitions describe the dataset contract separately from visual components.

## 13. Report Parameters

Each `ReportParameter` has a required `Name` attribute.

| Property | Values/behavior |
|---|---|
| `DataType` | `Boolean`, `DateTime`, `Integer`, `Float`, or `String`. |
| `Nullable` | Allows null. |
| `AllowBlank` | Allows an empty string where applicable. |
| `Hidden` | Hides prompting UI. |
| `MultiValue` | Allows multiple selected values. |
| `UsedInQuery` | `False`, `True`, or `Auto`. |
| `DefaultValue` | Static values or values obtained from a dataset. |
| `Prompt` | Localizable prompt text. |
| `ValidValues` | Static labels/values or dataset-derived choices. |

Default and valid values support two modes:

- Static value collections.
- `DataSetReference` with dataset, value field, and optional label field.

These report parameters are distinct from subreport or drill-through action parameters.

## 14. Grouping, Sorting, and Filtering

### 14.1 Group

A `Group` has a required `Name` attribute and may contain:

- `GroupExpressions`
- `PageBreak`
- `Filters`
- `Parent`
- `DataElementName`
- `DataElementOutput`
- `Variables`
- Other group pagination and layout controls

Groups create repeated scopes used by Tablix, Chart, Gauge, and Map structures.

### 14.2 SortExpression

A sort expression requires `Value` and may set `Direction` to:

- `Ascending`
- `Descending`

### 14.3 Filter

A filter requires:

- `FilterExpression`
- `Operator`
- `FilterValues`

Supported operators include:

- `Equal`, `NotEqual`
- `GreaterThan`, `GreaterThanOrEqual`
- `LessThan`, `LessThanOrEqual`
- `Like`
- `TopN`, `BottomN`
- `TopPercent`, `BottomPercent`
- `In`
- `Between`

`In` and `Between` require multiple filter values with operator-specific meaning.

## 15. Actions and Navigation

`ActionInfo` contains one or more actions. An action can define:

- `Hyperlink`
- `Drillthrough`
- `BookmarkLink`

A drill-through action requires `ReportName` and may pass named parameters.

These action targets are generally string expressions. They must remain opaque to the generator.

`Bookmark` defines a target; `BookmarkLink` navigates to one. `DocumentMapLabel` adds an entry to the report navigation tree.

## 16. Visibility

`Visibility` can contain:

- `Hidden`
- `ToggleItem`

Although `Hidden` often contains `true` or `false`, its XSD type is string because it can be an expression. `ToggleItem` names a textbox that controls expand/collapse behavior.

The JSON model should therefore represent visibility expressions as expression-capable values, not only booleans.

## 17. Styling

`Style` is a broad property bag. It includes:

- General and per-side borders
- Background color and background image
- Font family, size, style, weight, and text decoration
- Text color
- Format string
- Language, numeral language, numeral variant, and calendar
- Text alignment and vertical alignment
- Line height
- Text direction and writing mode
- Padding on each side
- Other renderer-supported style properties

### 17.1 Border

A border can define:

- `Color`
- `Style`
- `Width`

Separate top, bottom, left, and right border objects can override the default border.

### 17.2 BackgroundImage

A background image requires:

- `Source`: `External`, `Embedded`, or `Database`
- `Value`

It may also define MIME type, repetition, position, and transparency-related settings supported by the schema.

Style values are predominantly strings so they can hold constants, units, or expressions.

## 18. Page Breaks

`PageBreak` requires `BreakLocation`:

- `None`
- `Start`
- `End`
- `StartAndEnd`
- `Between`

It may also contain reset-page-number behavior. The actual pagination result depends on the containing component and renderer.

## 19. Embedded Images, Code, Variables, and Metadata

### 19.1 EmbeddedImage

Each embedded image has a required `Name` attribute and contains:

- `MIMEType`
- `ImageData`

An `Image` with `Source=Embedded` references this name.

### 19.2 Custom code

The report-level `Code` element stores embedded VB code. It must be preserved as a single opaque string.

`CodeModules` lists external modules. `Classes` defines class and instance names available to report expressions.

### 19.3 Variables

Each variable has:

- Required `Name` attribute
- Required `Value`
- Optional `Writable`

Variables can exist at report or group scope. Their values may be expressions.

### 19.4 Custom properties

A custom property contains `Name` and `Value`. It is metadata and should remain separate from standard component properties.

## 20. Typed String Forms

The schema defines several string wrappers:

### `SizeType`

A normalized string without an XSD unit pattern. Preserve the complete value, including units such as `cm`, `mm`, `in`, `pt`, or `pc`.

### `StringWithDataTypeAttribute`

Text content with an optional `DataType` XML attribute:

- `Boolean`
- `DateTime`
- `Integer`
- `Float`
- `String`

### `StringLocIDType`

String content that permits foreign-namespace localization attributes.

### `LocIDStringWithDataTypeAttribute`

Combines typed string behavior with `EvaluationMode`:

- `Auto` (default)
- `Constant`

Future JSON design must decide whether these values use a compact string form or an object form that can retain the XML attributes.

## 21. Required `Name` Attributes

The following categories contain types with required `Name` attributes:

- Data sources, datasets, fields, query parameters, report parameters, and embedded images
- All ten standard report item types
- Groups, variables, subreport/action parameters
- Tablix
- Chart, chart areas, axes, series, legends, titles, and other named chart objects
- Gauge panels, gauges, scales, pointers, indicators, ranges, images, and labels
- Maps, map data regions, map layers, legends, and titles

Names should remain explicit in JSON even if a collection is represented as a dictionary, because the XML generator must emit the attribute and expressions can refer to it.

## 22. Validation and Runtime Caveats

The XSD validates XML shape, not all report validity. Additional constraints must be checked by the generator, a validator, or the rendering engine:

- Names should be unique in their applicable scope.
- Referenced datasets, parameters, toggle items, embedded images, and report items must exist.
- Tablix body dimensions must agree with its row and column structures.
- Dynamic hierarchy members need valid groups and scopes.
- Image source, value, and MIME type combinations must be compatible.
- Expressions are not parsed or type-checked by this XSD.
- Page dimensions, margins, and body width can be individually valid but collectively exceed the printable area.
- The schema can permit combinations that a specific RDLC renderer does not support.
- Foreign-namespace designer properties can be important even though the base schema treats them as extensions.

Generated files must be tested with the project's existing PDF rendering infrastructure.

## 23. Guidance for the Upcoming JSON Proposal

The future JSON model should:

1. Keep report settings, data definitions, and visual layout separate.
2. Represent `ReportSections` as an array, even if the first implementation supports one section.
3. Represent report items as a discriminated union using a `type` property.
4. Put shared layout properties in a reusable model without assuming unsupported XML inheritance.
5. Keep sizes as unit-bearing strings or use a lossless unit/value object.
6. Preserve expressions and custom expression prefixes as opaque strings.
7. Use arrays for ordered and repeating schema collections.
8. Preserve required names explicitly.
9. Model Tablix hierarchies separately from body rows and cells.
10. Keep advanced Chart, Gauge, and Map subtrees extensible even if they are outside the first implementation milestone.
11. Preserve typed-string metadata such as `DataType` and `EvaluationMode`.
12. Preserve foreign-namespace elements and attributes when round-trip compatibility is required.
13. Avoid exposing raw RDLC XML as the primary AI-generated format.

## 24. Recommended Initial Implementation Scope

The XSD supports substantially more than the initial project needs. A practical first JSON schema can focus on:

- Report and one report section
- Page settings, body, page header, and page footer
- Data sources, datasets, fields, and parameters
- Textbox, rectangle, image, line, and Tablix
- Shared style, visibility, action, and layout properties
- Embedded images and custom report code
- Extension storage for unsupported namespaced metadata

Subreport, Chart, GaugePanel, Map, and CustomReportItem should be acknowledged as valid RDL components but can remain deferred until a concrete report requires them.

This keeps the initial model implementable without closing the design against the complete `ReportDefinition.xml` schema.
