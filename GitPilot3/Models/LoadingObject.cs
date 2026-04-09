using System;

namespace GitPilot3.Models;

public class LoadingObject
{
    public string Id { get; set; }
    public string Message { get; set; }
    public DateTime StartTime { get; set; }

    public LoadingObject(string id, string message)
    {
        Id = id;
        Message = message;
        StartTime = DateTime.Now;
    }
}
