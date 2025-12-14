using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace GitPilot3.Models.Graph;

public class BranchDrawBuffer
{
    public string Sha { get; set; } = "";
    public List<string> ParentShas { get; set; } = new List<string>();

    public RGBColor Color { get; set; } = new RGBColor(0, 0, 0);

    internal Color ToAvaloniaColor()
    {
        return new Color((byte)255, (byte)Color.R, (byte)Color.G, (byte)Color.B);
    }
}
