using LiaRdlcGen.Core.Models.Rdlc;

namespace LiaRdlcGen.Core.Services;

public static class RdlcSampleFactory
{
    public static RdlcReport Create()
    {
        RdlcReport report = new()
        {
            Language = "en-US",
            EmbeddedImages =
            [
                new RdlcEmbeddedImage
                {
                    Name = "SampleLogo",
                    MimeType = "image/png",
                    ImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
                }
            ]
        };
        report.DataSources.Add(new RdlcDataSource
        {
            Name = "AriData",
            ConnectionProperties = new RdlcConnectionProperties(),
            DataSourceId = "65594ac2-fbfe-4e1d-80c0-f1ad0babdff7"
        });
        report.DataSets.Add(new RdlcDataSet
        {
            Name = "d2",
            Query = new RdlcQuery { DataSourceName = "AriData" },
            Fields =
            [
                new RdlcField { Name = "stt", DataField = "stt", TypeName = "System.Int32" },
                new RdlcField { Name = "ten_vt", DataField = "ten_vt", TypeName = "System.String" }
            ]
        });
        report.ReportParameters.Add(new RdlcReportParameter
        {
            Name = "title",
            DataType = "String",
            Nullable = true,
            AllowBlank = true,
            Prompt = "Report title"
        });

        RdlcRectangle rectangle = new()
        {
            Name = "rectComponentContainer",
            Top = "1.2cm",
            Left = "0cm",
            Height = "2cm",
            Width = "20cm",
            KeepTogether = true,
            Style = new RdlcStyle
            {
                BackgroundColor = "#F3F6FA",
                Border = new RdlcBorder { Color = "#4472C4", Style = "Solid", Width = "1pt" }
            },
            ReportItems =
            [
                new RdlcImage
                {
                    Name = "imgEmbeddedSample",
                    Top = "0.2cm",
                    Left = "0.2cm",
                    Height = "1.6cm",
                    Width = "2cm",
                    Source = "Embedded",
                    Value = "SampleLogo",
                    Sizing = "FitProportional",
                    ToolTip = "Embedded image component"
                },
                CreateTextbox("txtNested", "Textbox nested inside a Rectangle", "2.5cm", "0.2cm", "17cm", "1.6cm", "Left")
            ]
        };

        RdlcLine line = new()
        {
            Name = "lineDivider",
            Top = "3.5cm",
            Left = "0cm",
            Height = "0cm",
            Width = "20cm",
            Style = new RdlcStyle
            {
                Border = new RdlcBorder { Color = "#4472C4", Style = "Solid", Width = "1.5pt" }
            },
            ToolTip = "Line component"
        };

        RdlcTablix tablix = CreateTablix();
        report.ReportSections.Add(new RdlcReportSection
        {
            Body = new RdlcBody
            {
                Height = "8cm",
                ReportItems =
                [
                    CreateTextbox("txtTitle", "&title", "0cm", "0cm", "20cm", "0.8cm", "Center", new RdlcStyle
                    {
                        FontFamily = "Times New Roman",
                        FontSize = "16pt",
                        FontWeight = "Bold",
                        Color = "#1F4E78"
                    }),
                    rectangle,
                    line,
                    tablix
                ],
                Style = new RdlcStyle { BackgroundColor = "White" }
            },
            Width = "20cm",
            Page = new RdlcPage
            {
                PageHeader = new RdlcPageSection
                {
                    Height = "0.6cm",
                    PrintOnFirstPage = true,
                    PrintOnLastPage = true,
                    ReportItems =
                    [
                        CreateTextbox("txtPageHeader", "Component test report", "0cm", "0cm", "20cm", "0.5cm", "Right")
                    ]
                },
                PageFooter = new RdlcPageSection
                {
                    Height = "0.6cm",
                    PrintOnFirstPage = true,
                    PrintOnLastPage = true,
                    ReportItems =
                    [
                        CreateTextbox("txtPageFooter", "=Globals!PageNumber & \" / \" & Globals!TotalPages", "0cm", "0cm", "20cm", "0.5cm", "Center")
                    ]
                }
            }
        });
        return report;
    }

    private static RdlcTextbox CreateTextbox(string name, string value, string left, string top, string width, string height, string textAlign, RdlcStyle? textRunStyle = null)
    {
        return new RdlcTextbox
        {
            Name = name,
            Top = top,
            Left = left,
            Height = height,
            Width = width,
            CanGrow = true,
            KeepTogether = true,
            Paragraphs =
            [
                new RdlcParagraph
                {
                    TextRuns =
                    [
                        new RdlcTextRun { Value = value, Style = textRunStyle }
                    ],
                    Style = new RdlcStyle { TextAlign = textAlign }
                }
            ],
            Style = new RdlcStyle
            {
                VerticalAlign = "Middle",
                PaddingLeft = "2pt",
                PaddingRight = "2pt"
            }
        };
    }

    private static RdlcTablix CreateTablix()
    {
        RdlcStyle cellStyle = new()
        {
            Border = new RdlcBorder { Color = "#808080", Style = "Solid", Width = "0.5pt" },
            VerticalAlign = "Middle",
            PaddingLeft = "2pt",
            PaddingRight = "2pt"
        };
        RdlcTablix tablix = new()
        {
            Name = "tblComponentTest",
            Top = "4cm",
            Left = "0cm",
            Height = "1cm",
            Width = "20cm",
            DataSetName = "d2",
            RepeatRowHeaders = true,
            KeepTogether = true,
            NoRowsMessage = "No detail rows",
            TablixBody = new RdlcTablixBody
            {
                TablixColumns =
                [
                    new RdlcTablixColumn { Width = "3cm" },
                    new RdlcTablixColumn { Width = "17cm" }
                ],
                TablixRows =
                [
                    new RdlcTablixRow
                    {
                        Height = "0.5cm",
                        TablixCells =
                        [
                            CreateCell(CreateTextbox("txtHeaderNumber", "No.", "0cm", "0cm", "3cm", "0.5cm", "Center", new RdlcStyle { FontWeight = "Bold" }), cellStyle),
                            CreateCell(CreateTextbox("txtHeaderName", "Name", "0cm", "0cm", "17cm", "0.5cm", "Center", new RdlcStyle { FontWeight = "Bold" }), cellStyle)
                        ]
                    },
                    new RdlcTablixRow
                    {
                        Height = "0.5cm",
                        TablixCells =
                        [
                            CreateCell(CreateTextbox("txtDetailNumber", "=Fields!stt.Value", "0cm", "0cm", "3cm", "0.5cm", "Center"), cellStyle),
                            CreateCell(CreateTextbox("txtDetailName", "=Fields!ten_vt.Value", "0cm", "0cm", "17cm", "0.5cm", "Left"), cellStyle)
                        ]
                    }
                ]
            },
            TablixColumnHierarchy = new RdlcTablixHierarchy
            {
                TablixMembers =
                [
                    new RdlcTablixMember(),
                    new RdlcTablixMember()
                ]
            },
            TablixRowHierarchy = new RdlcTablixHierarchy
            {
                TablixMembers =
                [
                    new RdlcTablixMember { KeepWithGroup = "After", RepeatOnNewPage = true },
                    new RdlcTablixMember { Group = new RdlcGroup { Name = "DetailGroup" } }
                ]
            },
            SortExpressions =
            [
                new RdlcSortExpression { Value = "=Fields!stt.Value", Direction = "Ascending" }
            ]
        };
        return tablix;
    }

    private static RdlcTablixCell CreateCell(RdlcTextbox textbox, RdlcStyle style)
    {
        textbox.Style = MergeStyles(textbox.Style, style);
        return new RdlcTablixCell { CellContents = new RdlcCellContents { ReportItem = textbox } };
    }

    private static RdlcStyle MergeStyles(RdlcStyle? primary, RdlcStyle secondary)
    {
        primary ??= new RdlcStyle();
        primary.Border = secondary.Border;
        primary.VerticalAlign ??= secondary.VerticalAlign;
        primary.PaddingLeft ??= secondary.PaddingLeft;
        primary.PaddingRight ??= secondary.PaddingRight;
        return primary;
    }
}
