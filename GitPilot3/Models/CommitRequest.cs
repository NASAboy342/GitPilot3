using System;

namespace GitPilot3.Models;

public class CommitRequest
{
    public string Message { get; set; } = "";
    public string Description { get; set; } = "";
    public string Author { get; set; } = "";
    public string Email { get; set; } = "";
}
