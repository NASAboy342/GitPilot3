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

    public class RGBColor
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public RGBColor()
        {
        }

        public RGBColor(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        public RGBColor GetRandomColor()
        {
            var colorsTemplate = new List<RGBColor>
            {
                new RGBColor(174, 2, 37),
                new RGBColor(159, 52, 5),
                new RGBColor(42, 52, 149),
                new RGBColor(1, 107, 49),
                new RGBColor(165, 16, 20),
                new RGBColor(125, 6, 121),
                new RGBColor(83, 39, 139),
                new RGBColor(155, 0, 65),
                new RGBColor(3, 106, 92),
                new RGBColor(2, 89, 140),
                new RGBColor(178, 1, 1),
                new RGBColor(159, 52, 5),
            };

            var random = new Random();

            var randomIndex = random.Next(0, colorsTemplate.Count);

            var resultColor = colorsTemplate[randomIndex];
            return resultColor;
        }
    }
}
