using System;

namespace GitPilot3.Models;

public class UserProfile
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsActive { get; set; } = false;
}
