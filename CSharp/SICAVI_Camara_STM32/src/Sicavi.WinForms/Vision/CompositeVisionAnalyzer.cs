using OpenCvSharp;
using Sicavi.WinForms.Configuration;
using Sicavi.WinForms.Models;
using Point = OpenCvSharp.Point;

namespace Sicavi.WinForms.Vision;

public sealed class CompositeVisionAnalyzer
{
    private readonly VisionSettings _settings;
    private readonly CardLocator _cardLocator;
    private readonly ColorAnalyzer _colorAnalyzer;
    private readonly ShapeAnalyzer _shapeAnalyzer;

    public CompositeVisionAnalyzer(VisionSettings settings)
    {
        _settings = settings;
        _cardLocator = new CardLocator(settings);
        _colorAnalyzer = new ColorAnalyzer(settings);
        _shapeAnalyzer = new ShapeAnalyzer(settings);
    }

    public Rect GetRoi(OpenCvSharp.Size frameSize) => _settings.CalculateRoi(frameSize);

    public VisionAnalysisResult Analyze(
        Mat frame,
        ColorTarget expectedColor,
        ShapeTarget expectedShape)
    {
        if (frame.Empty())
        {
            throw new ArgumentException("No se puede analizar un fotograma vacío.", nameof(frame));
        }

        Rect roi = _settings.CalculateRoi(frame.Size());
        using var roiView = new Mat(frame, roi);
        double analysisScale = Math.Min(
            1.0,
            _settings.MaximumAnalysisWidth / (double)Math.Max(1, roiView.Width));
        using var analysisFrame = new Mat();
        if (analysisScale < 0.999)
        {
            Cv2.Resize(
                roiView,
                analysisFrame,
                new OpenCvSharp.Size(),
                analysisScale,
                analysisScale,
                InterpolationFlags.Area);
        }
        else
        {
            roiView.CopyTo(analysisFrame);
        }

        CardLocation? card = _cardLocator.Locate(analysisFrame);
        Rect figureArea = card?.InnerBounds ?? new Rect(0, 0, analysisFrame.Width, analysisFrame.Height);
        using var figureFrame = new Mat(analysisFrame, figureArea);
        using ColorAnalysisResult color = _colorAnalyzer.Analyze(figureFrame);
        using ShapeAnalysisResult shape = _shapeAnalyzer.Analyze(figureFrame);

        (InspectionDecision decision, string reason) = Decide(
            color.Color,
            shape.Shape,
            expectedColor,
            expectedShape);

        var annotatedFrame = frame.Clone();
        Scalar decisionColor = decision switch
        {
            InspectionDecision.Good => new Scalar(0, 210, 0),
            InspectionDecision.Reject => new Scalar(0, 0, 230),
            _ => new Scalar(0, 215, 255)
        };

        Cv2.Rectangle(annotatedFrame, roi, new Scalar(130, 105, 70), 2);

        Rect reportedRoi = roi;
        if (card is not null)
        {
            Rect cardBounds = ToFrameRect(card.Bounds, analysisScale, roi, frame.Size());
            reportedRoi = ToFrameRect(card.InnerBounds, analysisScale, roi, frame.Size());
            Cv2.Rectangle(annotatedFrame, cardBounds, new Scalar(255, 190, 0), 2);
            Cv2.Rectangle(annotatedFrame, reportedRoi, decisionColor, 3);
        }

        if (shape.Contour is not null)
        {
            Point[] translated = shape.Contour
                .Select(point => new Point(
                    (int)Math.Round((point.X + figureArea.X) / analysisScale) + roi.X,
                    (int)Math.Round((point.Y + figureArea.Y) / analysisScale) + roi.Y))
                .ToArray();

            Cv2.DrawContours(
                annotatedFrame,
                new[] { translated },
                -1,
                new Scalar(255, 185, 0),
                3,
                LineTypes.AntiAlias);
        }

        if (card is null && decision == InspectionDecision.Unknown)
        {
            reason += " La tarjeta blanca no pudo aislarse; revise su posición e iluminación.";
        }

        string headline = $"{DecisionText(decision)} | {ColorText(color.Color)} + {ShapeText(shape.Shape)}";
        DrawBanner(annotatedFrame, headline, decisionColor);

        using var colorDebug = color.DebugFrame.Clone();
        using var shapeDebug = shape.DebugFrame.Clone();
        var debugFrame = new Mat();
        Cv2.HConcat(new[] { colorDebug, shapeDebug }, debugFrame);

        return new VisionAnalysisResult(
            color.Color,
            color.GreenPercentage,
            color.RedPercentage,
            shape.Shape,
            shape.Vertices,
            shape.Circularity,
            decision,
            reason,
            reportedRoi,
            annotatedFrame,
            debugFrame);
    }

    private static Rect ToFrameRect(
        Rect analysisRect,
        double analysisScale,
        Rect outerRoi,
        OpenCvSharp.Size frameSize)
    {
        int x = (int)Math.Round(analysisRect.X / analysisScale) + outerRoi.X;
        int y = (int)Math.Round(analysisRect.Y / analysisScale) + outerRoi.Y;
        int width = Math.Max(1, (int)Math.Round(analysisRect.Width / analysisScale));
        int height = Math.Max(1, (int)Math.Round(analysisRect.Height / analysisScale));
        x = Math.Clamp(x, 0, frameSize.Width - 1);
        y = Math.Clamp(y, 0, frameSize.Height - 1);
        width = Math.Clamp(width, 1, frameSize.Width - x);
        height = Math.Clamp(height, 1, frameSize.Height - y);
        return new Rect(x, y, width, height);
    }

    public Mat DrawIdleOverlay(Mat frame)
    {
        var output = frame.Clone();
        Rect roi = _settings.CalculateRoi(frame.Size());
        Cv2.Rectangle(output, roi, new Scalar(255, 190, 0), 2);
        DrawBanner(output, "Coloque la caja dentro de la ROI", new Scalar(255, 190, 0));
        return output;
    }

    private static (InspectionDecision Decision, string Reason) Decide(
        DetectedColor color,
        DetectedShape shape,
        ColorTarget expectedColor,
        ShapeTarget expectedShape)
    {
        if (color == DetectedColor.Unknown && shape == DetectedShape.Unknown)
        {
            return (InspectionDecision.Unknown, "No se detectó un color ni una forma confiables.");
        }

        if (color == DetectedColor.Unknown)
        {
            return (InspectionDecision.Unknown, "El color no pudo determinarse.");
        }

        if (shape == DetectedShape.Unknown)
        {
            return (InspectionDecision.Unknown, "La forma no pudo determinarse.");
        }

        bool colorMatches = expectedColor == ColorTarget.Any ||
            (expectedColor == ColorTarget.Green && color == DetectedColor.Green) ||
            (expectedColor == ColorTarget.Red && color == DetectedColor.Red);

        bool shapeMatches = expectedShape == ShapeTarget.Any ||
            (expectedShape == ShapeTarget.Circle && shape == DetectedShape.Circle) ||
            (expectedShape == ShapeTarget.Square && shape == DetectedShape.Square) ||
            (expectedShape == ShapeTarget.Triangle && shape == DetectedShape.Triangle);

        if (colorMatches && shapeMatches)
        {
            return (InspectionDecision.Good, "El color y la forma cumplen la configuración.");
        }

        if (!colorMatches && !shapeMatches)
        {
            return (InspectionDecision.Reject, "El color y la forma no coinciden.");
        }

        return !colorMatches
            ? (InspectionDecision.Reject, "El color no coincide.")
            : (InspectionDecision.Reject, "La forma no coincide.");
    }

    private static void DrawBanner(Mat image, string text, Scalar color)
    {
        Cv2.Rectangle(image, new Rect(0, 0, image.Width, 44), new Scalar(20, 28, 38), -1);
        Cv2.PutText(
            image,
            text,
            new Point(14, 30),
            HersheyFonts.HersheySimplex,
            0.72,
            color,
            2,
            LineTypes.AntiAlias);
    }

    private static string DecisionText(InspectionDecision decision) => decision switch
    {
        InspectionDecision.Good => "ACEPTADA",
        InspectionDecision.Reject => "RECHAZADA",
        _ => "INDETERMINADA"
    };

    private static string ColorText(DetectedColor color) => color switch
    {
        DetectedColor.Green => "VERDE",
        DetectedColor.Red => "ROJO",
        _ => "COLOR?"
    };

    private static string ShapeText(DetectedShape shape) => shape switch
    {
        DetectedShape.Circle => "CIRCULO",
        DetectedShape.Square => "CUADRADO",
        DetectedShape.Triangle => "TRIANGULO",
        _ => "FORMA?"
    };
}
