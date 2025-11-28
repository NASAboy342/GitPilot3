using System;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace GitPilot3.Services;

public interface IGraphComponentService
{
    int Height { get; set; }

    Line GetBranchLineCanvas(int index = 0);
    Control GetCheckOutCurveLineCanvas(int index = 0);
    Ellipse GetCommitPointCanvas(int index = 0);
    Control GetLowerConnectorLineCanvas(int index = 0);
    Control GetUpperConnectorLineCanvas(int index = 0);
}
