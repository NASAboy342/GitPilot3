using System;
using System.Collections.Generic;

namespace GitPilot3.Models;

public class ChangeContent
{
    public string ChangeHead { get; set; }
    public List<string> ChangeLines { get; set; } = new List<string>();
    public int StartLine { get; set; }
    public int LineCount { get; set; }
    public int EndLine => StartLine + LineCount - 1;
}
