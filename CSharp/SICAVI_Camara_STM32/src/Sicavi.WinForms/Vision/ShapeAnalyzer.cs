using OpenCvSharp;
using Sicavi.WinForms.Configuration;
using Sicavi.WinForms.Models;
using Point = OpenCvSharp.Point;

namespace Sicavi.WinForms.Vision;

public sealed class ShapeAnalyzer : VisionAnalyzerBase<ShapeAnalysisResult>
{
    public ShapeAnalyzer(VisionSettings settings) : base(settings)
    {
    }

    public override ShapeAnalysisResult Analyze(Mat image)
    {
        ValidateImage(image);

        using var hsv = new Mat();
        using var binary = new Mat();
        using var kernelOpen = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3));
        using var kernelClose = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(7, 7));

        /*
         * CompositeVisionAnalyzer entrega el interior de la tarjeta blanca. En
         * esa región la saturación separa la figura impresa sin depender de que
         * su tono ya haya sido reconocido como rojo o verde. Esto evita que una
         * pequeña variación de iluminación vuelva desconocidos color y forma a
         * la vez.
         */
        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
        Cv2.InRange(
            hsv,
            new Scalar(0, Settings.ShapeMinimumSaturation, Settings.ShapeMinimumValue),
            new Scalar(179, 255, 255),
            binary);
        Cv2.MorphologyEx(binary, binary, MorphTypes.Open, kernelOpen, iterations: 1);
        Cv2.MorphologyEx(binary, binary, MorphTypes.Close, kernelClose, iterations: 2);

        Cv2.FindContours(
            binary,
            out Point[][] contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        ShapeCandidate? selected = FindBestCandidate(contours, image.Size());
        var debugFrame = image.Clone();

        if (selected is null)
        {
            Cv2.PutText(
                debugFrame,
                "Forma: DESCONOCIDA",
                new Point(12, 28),
                HersheyFonts.HersheySimplex,
                0.65,
                new Scalar(0, 215, 255),
                2,
                LineTypes.AntiAlias);

            return new ShapeAnalysisResult(
                DetectedShape.Unknown,
                0,
                0,
                0,
                null,
                debugFrame);
        }

        Cv2.DrawContours(
            debugFrame,
            new[] { selected.Approximation },
            -1,
            new Scalar(255, 180, 0),
            3,
            LineTypes.AntiAlias);

        Moments moments = Cv2.Moments(selected.Approximation);
        if (Math.Abs(moments.M00) > double.Epsilon)
        {
            var center = new Point(
                (int)(moments.M10 / moments.M00),
                (int)(moments.M01 / moments.M00));
            Cv2.Circle(debugFrame, center, 5, new Scalar(0, 255, 255), -1);
        }

        string text = $"{ToSpanish(selected.Shape)} | V={selected.Vertices} | C={selected.Circularity:F2}";
        Cv2.PutText(
            debugFrame,
            text,
            new Point(12, 28),
            HersheyFonts.HersheySimplex,
            0.62,
            new Scalar(255, 255, 255),
            2,
            LineTypes.AntiAlias);

        return new ShapeAnalysisResult(
            selected.Shape,
            selected.Vertices,
            selected.Area,
            selected.Circularity,
            selected.Approximation,
            debugFrame);
    }

    private ShapeCandidate? FindBestCandidate(IEnumerable<Point[]> contours, OpenCvSharp.Size imageSize)
    {
        double imageArea = imageSize.Width * imageSize.Height;
        double maximumArea = imageArea * Settings.MaximumShapeAreaRatio;
        var candidates = new List<ShapeCandidate>();

        foreach (Point[] contour in contours)
        {
            double area = Math.Abs(Cv2.ContourArea(contour));
            if (area < Settings.MinimumShapeAreaPixels || area > maximumArea)
            {
                continue;
            }

            double perimeter = Cv2.ArcLength(contour, true);
            if (perimeter <= 0)
            {
                continue;
            }

            Point[] approximation = Cv2.ApproxPolyDP(
                contour,
                Settings.PolygonEpsilonRatio * perimeter,
                true);

            if (approximation.Length < 3 || !Cv2.IsContourConvex(approximation))
            {
                continue;
            }

            Rect bounds = Cv2.BoundingRect(approximation);
            if (bounds.Width < 20 || bounds.Height < 20 || TouchesImageBorder(bounds, imageSize))
            {
                continue;
            }

            RotatedRect rotatedBounds = Cv2.MinAreaRect(contour);
            double rotatedWidth = Math.Max(rotatedBounds.Size.Width, rotatedBounds.Size.Height);
            double rotatedHeight = Math.Min(rotatedBounds.Size.Width, rotatedBounds.Size.Height);
            if (rotatedHeight <= 0.0)
            {
                continue;
            }

            double aspectRatio = rotatedWidth / rotatedHeight;
            double rotatedArea = rotatedWidth * rotatedHeight;
            double extent = rotatedArea > 0.0 ? area / rotatedArea : 0.0;
            double circularity = 4.0 * Math.PI * area / (perimeter * perimeter);
            DetectedShape shape = Classify(approximation.Length, aspectRatio, extent, circularity);
            if (shape == DetectedShape.Unknown)
            {
                continue;
            }

            candidates.Add(new ShapeCandidate(
                shape,
                approximation.Length,
                area,
                circularity,
                approximation));
        }

        double centerX = imageSize.Width / 2.0;
        double centerY = imageSize.Height / 2.0;
        return candidates
            .OrderByDescending(candidate =>
            {
                Rect bounds = Cv2.BoundingRect(candidate.Approximation);
                double objectX = bounds.X + bounds.Width / 2.0;
                double objectY = bounds.Y + bounds.Height / 2.0;
                double distance = Math.Sqrt(
                    Math.Pow((objectX - centerX) / Math.Max(1.0, centerX), 2) +
                    Math.Pow((objectY - centerY) / Math.Max(1.0, centerY), 2));
                return candidate.Area / (1.0 + (2.0 * distance));
            })
            .FirstOrDefault();
    }

    private DetectedShape Classify(int vertices, double aspectRatio, double extent, double circularity)
    {
        if (vertices == 3 ||
            (vertices is >= 4 and <= 6 && extent <= 0.70 && circularity <= 0.75))
        {
            return DetectedShape.Triangle;
        }

        if (vertices >= 6 && circularity >= Settings.MinimumCircleCircularity && extent <= 0.90)
        {
            return DetectedShape.Circle;
        }

        if (vertices is >= 4 and <= 8 &&
            aspectRatio >= Settings.SquareAspectRatioMinimum &&
            aspectRatio <= Settings.SquareAspectRatioMaximum &&
            extent >= 0.78)
        {
            return DetectedShape.Square;
        }

        return DetectedShape.Unknown;
    }

    private static bool TouchesImageBorder(Rect bounds, OpenCvSharp.Size imageSize)
    {
        const int margin = 4;
        return bounds.X <= margin ||
               bounds.Y <= margin ||
               bounds.Right >= imageSize.Width - margin ||
               bounds.Bottom >= imageSize.Height - margin;
    }

    private static string ToSpanish(DetectedShape shape) => shape switch
    {
        DetectedShape.Circle => "CIRCULO",
        DetectedShape.Square => "CUADRADO",
        DetectedShape.Triangle => "TRIANGULO",
        _ => "DESCONOCIDA"
    };

    private sealed record ShapeCandidate(
        DetectedShape Shape,
        int Vertices,
        double Area,
        double Circularity,
        Point[] Approximation);
}
