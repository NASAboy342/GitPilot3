using System;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using GitPilot3.Models;

namespace GitPilot3.Services;

public interface IGraphComponentService
{
    int Height { get; set; }

    Line GetBranchLineCanvas(int index = 0, RGBColor color = null);
    Control GetBranchLineCanvasOnOtherRow(int index, int relativeRow, RGBColor color);
    Control GetCheckOutCurveLineCanvas(int index = 0, RGBColor color = null);
    Ellipse GetCommitPointCanvas(int index = 0, bool isAMergeCommit = false, RGBColor relativeColor = null);
    Control GetLowerConnectorLineCanvas(int index = 0, RGBColor relativeColor = null);
    Control GetMergeCurveLineOnOtherRowCanvas(int index, int relativeRow, int mergeToIndex, RGBColor color);
    Control GetUpperConnectorLineCanvas(int index = 0, RGBColor color = null);
    Control HorizontalLineToLeftCanvas(int index = 0, RGBColor color = null);
    Control HorizontalLineToLeftOnOtherRowCanvas(int index, int relativeRow, RGBColor color);
    Control HorizontalLineToRightOnOtherRowCanvas(int index, int relativeRow, RGBColor color);
}
