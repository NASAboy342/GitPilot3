using System;
using System.Collections.Generic;

namespace GitPilot3.Models.Graph;

public class BranchDrawBuffer
{
    public string Sha { get; set; } = "";
    public List<string> ParentShas { get; set; } = new List<string>();
}
