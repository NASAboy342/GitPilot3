using System;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace GitPilot3.Services;

public interface IGraphComponentService
{
    int Height { get; set; }

    Line GetBranchLineCanvas(int index = 0);
    Control GetBranchLineCanvasOnOtherRow(int index, int relativeRow);
    Control GetCheckOutCurveLineCanvas(int index = 0);
    Ellipse GetCommitPointCanvas(int index = 0, bool isAMergeCommit = false);
    Control GetLowerConnectorLineCanvas(int index = 0);
    Control GetMergeCurveLineOnOtherRowCanvas(int index, int relativeRow, int mergeToIndex);
    Control GetUpperConnectorLineCanvas(int index = 0);
    Control HorizontalLineCanvas(int index = 0);
    Control HorizontalLineOnOtherRowCanvas(int index, int relativeRow);
}
