using System;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace GitPilot3.Models;

public class GitBranch
{
    public bool IsRemote { get; set; }
    public string Name { get; set; } = "";
    public int InComming { get; set; }
    public int OutGoing { get; set; }
    public bool IsCurrent { get; set; }
    public RGBColor Color { get; set; } = new RGBColor(0, 0, 0);

    internal Color ToAvaloniaColor()
    {
        return new Color((byte)255, (byte)Color.R, (byte)Color.G, (byte)Color.B);
    }
}
