namespace Sicavi.WinForms.Models;

public enum DetectedColor
{
    Unknown,
    Green,
    Red
}

public enum DetectedShape
{
    Unknown,
    Circle,
    Square,
    Triangle
}

public enum ColorTarget
{
    Any,
    Green,
    Red
}

public enum ShapeTarget
{
    Any,
    Circle,
    Square,
    Triangle
}

public enum InspectionDecision
{
    Unknown,
    Good,
    Reject
}
