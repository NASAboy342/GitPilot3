using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace GitPilot3.Models;

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
            var random = new Random();
            
            // Generate random hue (0-360)
            double hue = random.Next(0, 360);
            
            // Set high saturation (75-100%) for vibrancy
            double saturation = (random.Next(75, 101)) / 100.0;
            
            // Set high brightness (70-100%) for vibrancy
            double brightness = (random.Next(70, 101)) / 100.0;
            
            // Convert HSB to RGB
            return ConvertHsbToRgb(hue, saturation, brightness);
        }

        private RGBColor ConvertHsbToRgb(double hue, double saturation, double brightness)
        {
            double c = brightness * saturation;
            double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            double m = brightness - c;

            double r, g, b;

            if (hue < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (hue < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (hue < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (hue < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (hue < 300)
            {
                r = x; g = 0; b = c;
            }
            else
            {
                r = c; g = 0; b = x;
            }

            return new RGBColor(
                (int)Math.Round((r + m) * 255),
                (int)Math.Round((g + m) * 255),
                (int)Math.Round((b + m) * 255)
            );
        }

    internal IBrush ToAvaloniaColor()
    {
        return new SolidColorBrush(new Color((byte)255, (byte)R, (byte)G, (byte)B));
    }
}
