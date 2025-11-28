using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace GitPilot3.Services;

public class GraphComponentService : IGraphComponentService
{
    public int Height { get; set; } = 40;

    private int _lineThickness = 6;
    public Ellipse GetCommitPointCanvas(int index = 0)
    {
        var circle = new Ellipse
        {
            Width = Height * 0.8,
            Height = Height * 0.8,
            Fill = Brushes.DodgerBlue,
        };
        var margitLeft = index * circle.Width;
        var adjustedMarginLeft = ((Height / 2) - (circle.Width / 2)) * index;

        Canvas.SetLeft(circle, (Height / 2) - (circle.Width / 2) + margitLeft + adjustedMarginLeft);
        Canvas.SetTop(circle, (Height / 2) - (circle.Height / 2));
        return circle;
    }

    public Line GetBranchLineCanvas(int index = 0)
    {
        index++;
        var circle = GetCommitPointCanvas(index);
        var adjustedMarginLeft = ((Height / 2) - (circle.Width / 2)) * index;
        var startX = (circle.Width * index - circle.Width / 2) + adjustedMarginLeft;
        var startY = 0;
        var endX = (circle.Width * index - circle.Width / 2) + adjustedMarginLeft;
        var endY = Height;

        var line = new Line
        {
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = _lineThickness,
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
        };
        return line;
    }

    public Control GetLowerConnectorLineCanvas(int index = 0)
    {
        index++;
        var circle = GetCommitPointCanvas(index);
        var adjustedMarginLeft = ((Height / 2) - (circle.Width / 2)) * index;
        var startX = (circle.Width * index - circle.Width / 2) + adjustedMarginLeft;
        var startY = Height / 2;
        var endX = (circle.Width * index - circle.Width / 2) + adjustedMarginLeft;
        var endY = Height;

        var line = new Line
        {
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = _lineThickness,
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
        };
        return line;
    }

    public Control GetUpperConnectorLineCanvas(int index = 0)
    {
        index++;
        var circle = GetCommitPointCanvas(index);
        var adjustedMarginLeft = ((Height / 2) - (circle.Width / 2)) * index;
        var startX = (circle.Width * index - circle.Width / 2) + adjustedMarginLeft;
        var startY = 0;
        var endX = (circle.Width * index - circle.Width / 2) + adjustedMarginLeft;
        var endY = Height / 2;

        var line = new Line
        {
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = _lineThickness,
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
        };
        return line;
    }

    public Control GetCheckOutCurveLineCanvas(int index = 0)
    {
        index++;
        var circle = GetCommitPointCanvas(index);
        var adjustedMarginLeft = ((Height / 2) - (circle.Width / 2)) * index;
        var startX = (circle.Width * index - circle.Width / 2) + adjustedMarginLeft;
        var startY = 0;
        var controlX = (circle.Width * index - circle.Width / 2) + adjustedMarginLeft; ;
        var controlY = Height / 2;
        var endX = (circle.Width * index - circle.Width) + adjustedMarginLeft;
        var endY = Height / 2;
        // Define a Path
        var path = new Path
        {
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = _lineThickness
            
        };

        // Geometry: Move to (10,100), then quadratic Bezier to (200,100) with control point (100,10)
        var geometry = new PathGeometry
        {
            Figures = new PathFigures
            {
                new PathFigure
                {
                    StartPoint = new Point(startX, startY),
                    IsClosed = false,
                    Segments = new PathSegments
                    {
                        new QuadraticBezierSegment
                        {
                            Point1 = new Point(controlX, controlY),   // control point
                            Point2 = new Point(endX, endY)   // end point
                        }
                    }
                }
            }
        };

        path.Data = geometry;

        return path;
    }
}
