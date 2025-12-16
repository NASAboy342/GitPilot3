using System;

namespace GitPilot3.Models;

public class OpenedRepository
{
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime LastOpened { get; set; } = DateTime.Now;
}
