namespace Sicavi.WinForms.Models;

public sealed record VisionSummary(
    DetectedColor Color,
    double GreenPercentage,
    double RedPercentage,
    DetectedShape Shape,
    int Vertices,
    double Circularity,
    InspectionDecision Decision,
    string Reason,
    string? InspectionId);
