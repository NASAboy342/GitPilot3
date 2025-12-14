using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using GitPilot3.Models;

namespace GitPilot3.Services;

public class GraphComponentService : IGraphComponentService
{
    public int Height { get; set; } = 40;

    private int _lineThickness = 3;
    public Ellipse GetCommitPointCanvas(int index = 0, bool isAMergeCommit = false, RGBColor relativeColor = null)
    {
        var x = Height / 2;
        var y = Height / 2;

        x = x + (index * Height);

        var circle = new Ellipse
        {
            Width = Height * (isAMergeCommit ? 0.5 : 0.8),
            Height = Height * (isAMergeCommit ? 0.5 : 0.8),
            Fill = relativeColor.ToAvaloniaColor(),
        };

        Canvas.SetLeft(circle, x - (circle.Width / 2));
        Canvas.SetTop(circle, y - (circle.Height / 2));
        return circle;
    }

    public Line GetBranchLineCanvas(int index = 0, RGBColor color = null)
    {
        var startX = Height / 2;
        var startY = 0;
        var endX = Height / 2;
        var endY = Height;

        startX = startX + (index * Height);
        endX = endX + (index * Height);

        var line = new Line
        {
            Stroke = color.ToAvaloniaColor(),
            StrokeThickness = _lineThickness,
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
        };
        return line;
    }

    public Control GetLowerConnectorLineCanvas(int index = 0, RGBColor relativeColor = null)
    {
        var startX = Height / 2;
        var startY = Height / 2;
        var endX = Height / 2;
        var endY = Height;

        startX = startX + (index * Height);
        endX = endX + (index * Height);

        var line = new Line
        {
            Stroke = relativeColor.ToAvaloniaColor(),
            StrokeThickness = _lineThickness,
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
        };
        return line;
    }

    public Control GetUpperConnectorLineCanvas(int index = 0, RGBColor color = null)
    {
        var startX = Height / 2;
        var startY = 0;
        var endX = Height / 2;
        var endY = Height / 2;

        startX = startX + (index * Height);
        endX = endX + (index * Height);
        
        var line = new Line
        {
            Stroke = color.ToAvaloniaColor(),
            StrokeThickness = _lineThickness,
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
        };
        return line;
    }

    public Control GetCheckOutCurveLineCanvas(int index = 0, RGBColor color = null)
    {
        
        var startX = Height / 2;
        var startY = 0;
        var controlX = Height / 2;
        var controlY = Height / 2;
        var endX = 0;
        var endY = Height / 2;

        startX = startX + (index * Height);
        controlX = controlX + (index * Height);
        endX = endX + (index * Height) - Convert.ToInt32(Height * 0.25);

        // Define a Path
        var path = new Path
        {
            Stroke = color.ToAvaloniaColor(),
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

    public Control HorizontalLineToLeftCanvas(int index = 0, RGBColor color = null)
    {
        var startX = Height;
        var startY = Height / 2;
        var endX = 0;
        var endY = Height / 2;

        startX = startX + (index * Height);
        endX = endX + (index * Height) - (Height / 2);

        var line = new Line
        {
            Stroke = color.ToAvaloniaColor(),
            StrokeThickness = _lineThickness,
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
        };
        return line;
    }

    public Control HorizontalLineToRightCanvas(int index = 0, RGBColor color = null)
    {
        var startX = Height;
        var startY = Height / 2;
        var endX = 0;
        var endY = Height / 2;

        startX = startX + (index * Height) + (Height / 2);
        endX = endX + (index * Height);

        var line = new Line
        {
            Stroke = color.ToAvaloniaColor(),
            StrokeThickness = _lineThickness,
            StartPoint = new Point(startX, startY),
            EndPoint = new Point(endX, endY),
        };
        return line;
    }

    public Control GetBranchLineCanvasOnOtherRow(int index, int relativeRow, RGBColor color)
    {
        var line = GetBranchLineCanvas(index, color);
        line.StartPoint = new Point(line.StartPoint.X, line.StartPoint.Y + (Height * relativeRow));
        line.EndPoint = new Point(line.EndPoint.X, line.EndPoint.Y + (Height * relativeRow) + (Height / 2));
        return line;
    }

    public Control GetMergeCurveLineOnOtherRowCanvas(int index, int relativeRow, int mergeToIndex, RGBColor color = null)
    {
        var isMergeToRight = mergeToIndex > index;
        
        var startX = Height / 2;
        var startY = Height;
        var controlX = Height / 2;
        var controlY = Height / 2;
        var endX = isMergeToRight ? Height : 0;
        var endY = Height / 2;

        startX = startX + (index * Height);
        startY = startY + (relativeRow * Height);

        controlX = controlX + (index * Height);
        controlY = controlY + (relativeRow * Height);

        if(isMergeToRight)
            endX = endX + (index * Height) + Convert.ToInt32(Height * 0.25);
        else
            endX = endX + (index * Height) - Convert.ToInt32(Height * 0.25);
            
        endY = endY + (relativeRow * Height);
        
        
        // Define a Path
        var path = new Path
        {
            Stroke = color.ToAvaloniaColor(),
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

    public Control HorizontalLineToLeftOnOtherRowCanvas(int index, int relativeRow, RGBColor color)
    {
        var line = (Line)HorizontalLineToLeftCanvas(index, color);
        line.StartPoint = new Point(line.StartPoint.X, line.StartPoint.Y + (Height * relativeRow));
        line.EndPoint = new Point(line.EndPoint.X, line.EndPoint.Y + (Height * relativeRow));

        return line;
    }

    public Control HorizontalLineToRightOnOtherRowCanvas(int index, int relativeRow, RGBColor color)
    {
        var line = (Line)HorizontalLineToRightCanvas(index, color);
        line.StartPoint = new Point(line.StartPoint.X, line.StartPoint.Y + (Height * relativeRow));
        line.EndPoint = new Point(line.EndPoint.X, line.EndPoint.Y + (Height * relativeRow));

        return line;
    }
}
