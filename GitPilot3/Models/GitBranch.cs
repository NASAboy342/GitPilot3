using System;
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
        return new Color((byte)Color.R, (byte)Color.G, (byte)Color.B, (byte)255);
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
            Random rand = new Random();

            var oneToThree = rand.Next(1, 4);

            R = rand.Next(oneToThree == 1 ? 200 : 0, 256);
            G = rand.Next(oneToThree == 2 ? 200 : 0, 256);
            B = rand.Next(oneToThree == 3 ? 200 : 0, 256);
            return this;
        }
    }
}
