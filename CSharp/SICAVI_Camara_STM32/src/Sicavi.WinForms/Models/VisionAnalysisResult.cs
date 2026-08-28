using OpenCvSharp;

namespace Sicavi.WinForms.Models;

public sealed class VisionAnalysisResult : IDisposable
{
    public VisionAnalysisResult(
        DetectedColor color,
        double greenPercentage,
        double redPercentage,
        DetectedShape shape,
        int vertices,
        double circularity,
        InspectionDecision decision,
        string reason,
        Rect roi,
        Mat annotatedFrame,
        Mat debugFrame)
    {
        Color = color;
        GreenPercentage = greenPercentage;
        RedPercentage = redPercentage;
        Shape = shape;
        Vertices = vertices;
        Circularity = circularity;
        Decision = decision;
        Reason = reason;
        Roi = roi;
        AnnotatedFrame = annotatedFrame;
        DebugFrame = debugFrame;
    }

    public DetectedColor Color { get; }
    public double GreenPercentage { get; }
    public double RedPercentage { get; }
    public DetectedShape Shape { get; }
    public int Vertices { get; }
    public double Circularity { get; }
    public InspectionDecision Decision { get; }
    public string Reason { get; }
    public Rect Roi { get; }
    public Mat AnnotatedFrame { get; }
    public Mat DebugFrame { get; }

    public void Dispose()
    {
        AnnotatedFrame.Dispose();
        DebugFrame.Dispose();
    }
}
