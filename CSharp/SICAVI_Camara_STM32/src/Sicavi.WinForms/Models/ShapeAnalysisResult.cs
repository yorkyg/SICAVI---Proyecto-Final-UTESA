using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace Sicavi.WinForms.Models;

public sealed class ShapeAnalysisResult : IDisposable
{
    public ShapeAnalysisResult(
        DetectedShape shape,
        int vertices,
        double area,
        double circularity,
        Point[]? contour,
        Mat debugFrame)
    {
        Shape = shape;
        Vertices = vertices;
        Area = area;
        Circularity = circularity;
        Contour = contour;
        DebugFrame = debugFrame;
    }

    public DetectedShape Shape { get; }
    public int Vertices { get; }
    public double Area { get; }
    public double Circularity { get; }
    public Point[]? Contour { get; }
    public Mat DebugFrame { get; }

    public void Dispose() => DebugFrame.Dispose();
}
