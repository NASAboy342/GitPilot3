using System;

namespace GitPilot3.Models.Graph;

public class MergePointBuffer
{
    public string MergeFromSha { get; set; } = "";
    public int MergeToIndex { get; set; }
    public int RowsAwayFromMergePoint { get; set; }
}
