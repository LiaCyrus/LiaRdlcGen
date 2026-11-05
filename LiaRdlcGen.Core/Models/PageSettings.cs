using System.Collections.Generic;

namespace LiaRdlcGen.Core.Models;

public class PageSettings
{
    public double WidthCm { get; set; } = 21;
    public double HeightCm { get; set; } = 29.7;
    public double MarginLeftCm { get; set; } = 0.5;
    public double MarginRightCm { get; set; } = 0.5;
    public double MarginTopCm { get; set; } = 0.5;
    public double MarginBottomCm { get; set; } = 0.5;
    public string Orientation { get; set; } = "Portrait";
}

