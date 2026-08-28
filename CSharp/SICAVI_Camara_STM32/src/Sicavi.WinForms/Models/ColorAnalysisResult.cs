using OpenCvSharp;

namespace Sicavi.WinForms.Models;

public sealed class ColorAnalysisResult : IDisposable
{
    public ColorAnalysisResult(
        DetectedColor color,
        double greenPercentage,
        double redPercentage,
        Mat debugFrame)
    {
        Color = color;
        GreenPercentage = greenPercentage;
        RedPercentage = redPercentage;
        DebugFrame = debugFrame;
    }

    public DetectedColor Color { get; }
    public double GreenPercentage { get; }
    public double RedPercentage { get; }
    public double DominantPercentage => Math.Max(GreenPercentage, RedPercentage);
    public Mat DebugFrame { get; }

    public void Dispose() => DebugFrame.Dispose();
}
