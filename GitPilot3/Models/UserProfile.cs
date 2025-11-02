using System;
using Avalonia.Media;

namespace GitPilot3.Models;

public class UserProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsActive { get; set; } = false;

    public RGBColor Color { get; set; } = new RGBColor(0, 0, 0);

    internal Color ToAvaloniaColor()
    {
        return new Color((byte)255, (byte)Color.R, (byte)Color.G, (byte)Color.B);
    }
}
