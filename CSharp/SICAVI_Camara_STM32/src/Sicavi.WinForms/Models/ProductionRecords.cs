namespace Sicavi.WinForms.Models;

public sealed record InspectionRecord(
    long Id,
    string BoxCode,
    DateTime Timestamp,
    string DetectedColor,
    string DetectedShape,
    double GreenPercentage,
    double RedPercentage,
    int Vertices,
    double Circularity,
    string Result,
    string Reason,
    string OperatorName);

public sealed record DashboardSummary(int Good, int Reject, int Unknown, int Total);

public sealed record HourmeterDailyRecord(
    DateTime Date,
    long WorkSeconds,
    long EjectorCycles,
    DateTime LastUpdate);

public sealed record AlarmHistoryRecord(
    long Id,
    string Code,
    DateTime StartedAt,
    DateTime? ClosedAt,
    string Status,
    string? Detail);
