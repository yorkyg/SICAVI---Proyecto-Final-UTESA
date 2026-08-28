using OpenCvSharp;
using Sicavi.WinForms.Configuration;
using Sicavi.WinForms.Models;
using Point = OpenCvSharp.Point;

namespace Sicavi.WinForms.Vision;

public sealed class ColorAnalyzer : VisionAnalyzerBase<ColorAnalysisResult>
{
    public ColorAnalyzer(VisionSettings settings) : base(settings)
    {
    }

    public override ColorAnalysisResult Analyze(Mat image)
    {
        ValidateImage(image);

        using var hsv = new Mat();
        using var greenMask = new Mat();
        using var redLowMask = new Mat();
        using var redHighMask = new Mat();
        using var redMask = new Mat();
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));

        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
        Cv2.InRange(hsv, Settings.Green.Minimum, Settings.Green.Maximum, greenMask);
        Cv2.InRange(hsv, Settings.RedLow.Minimum, Settings.RedLow.Maximum, redLowMask);
        Cv2.InRange(hsv, Settings.RedHigh.Minimum, Settings.RedHigh.Maximum, redHighMask);
        Cv2.BitwiseOr(redLowMask, redHighMask, redMask);

        CleanMask(greenMask, kernel);
        CleanMask(redMask, kernel);

        using Mat relevantGreenMask = ExtractRelevantObject(greenMask);
        using Mat relevantRedMask = ExtractRelevantObject(redMask);

        double totalPixels = image.Rows * image.Cols;
        double greenPercentage = 100.0 * Cv2.CountNonZero(relevantGreenMask) / totalPixels;
        double redPercentage = 100.0 * Cv2.CountNonZero(relevantRedMask) / totalPixels;

        DetectedColor detectedColor = DetermineColor(greenPercentage, redPercentage);

        var debugFrame = Mat.Zeros(image.Rows, image.Cols, MatType.CV_8UC3).ToMat();
        debugFrame.SetTo(new Scalar(0, 190, 0), relevantGreenMask);
        debugFrame.SetTo(new Scalar(0, 0, 220), relevantRedMask);

        string label = $"Verde {greenPercentage:F1}% | Rojo {redPercentage:F1}%";
        Cv2.PutText(
            debugFrame,
            label,
            new Point(12, 28),
            HersheyFonts.HersheySimplex,
            0.65,
            Scalar.White,
            2,
            LineTypes.AntiAlias);

        return new ColorAnalysisResult(
            detectedColor,
            greenPercentage,
            redPercentage,
            debugFrame);
    }

    private DetectedColor DetermineColor(double greenPercentage, double redPercentage)
    {
        double minimum = Settings.MinimumColorPercentage;

        if (greenPercentage < minimum && redPercentage < minimum)
        {
            return DetectedColor.Unknown;
        }

        if (Math.Abs(greenPercentage - redPercentage) < 1.0)
        {
            return DetectedColor.Unknown;
        }

        return greenPercentage > redPercentage
            ? DetectedColor.Green
            : DetectedColor.Red;
    }

    private static void CleanMask(Mat mask, Mat kernel)
    {
        Cv2.MorphologyEx(mask, mask, MorphTypes.Open, kernel, iterations: 1);
        Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel, iterations: 2);
    }

    private Mat ExtractRelevantObject(Mat sourceMask)
    {
        Cv2.FindContours(
            sourceMask,
            out Point[][] contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        double imageArea = sourceMask.Width * sourceMask.Height;
        double minimumArea = Math.Max(120.0, imageArea * Settings.MinimumColorObjectAreaRatio);
        Point[]? bestContour = null;
        double bestScore = 0.0;
        double centerX = sourceMask.Width / 2.0;
        double centerY = sourceMask.Height / 2.0;

        foreach (Point[] contour in contours)
        {
            double area = Math.Abs(Cv2.ContourArea(contour));
            if (area < minimumArea)
            {
                continue;
            }

            Rect bounds = Cv2.BoundingRect(contour);
            if (TouchesImageBorder(bounds, sourceMask.Size()))
            {
                continue;
            }

            Moments moments = Cv2.Moments(contour);
            double objectX = Math.Abs(moments.M00) > double.Epsilon
                ? moments.M10 / moments.M00
                : bounds.X + bounds.Width / 2.0;
            double objectY = Math.Abs(moments.M00) > double.Epsilon
                ? moments.M01 / moments.M00
                : bounds.Y + bounds.Height / 2.0;
            double normalizedDistance = Math.Sqrt(
                Math.Pow((objectX - centerX) / Math.Max(1.0, centerX), 2) +
                Math.Pow((objectY - centerY) / Math.Max(1.0, centerY), 2));
            double score = area / (1.0 + (2.0 * normalizedDistance));

            if (score > bestScore)
            {
                bestScore = score;
                bestContour = contour;
            }
        }

        var filtered = Mat.Zeros(sourceMask.Rows, sourceMask.Cols, MatType.CV_8UC1).ToMat();
        if (bestContour is not null)
        {
            Cv2.DrawContours(filtered, new[] { bestContour }, -1, Scalar.White, -1, LineTypes.AntiAlias);
        }

        return filtered;
    }

    private static bool TouchesImageBorder(Rect bounds, OpenCvSharp.Size imageSize)
    {
        int margin = Math.Max(4, Math.Min(imageSize.Width, imageSize.Height) / 100);
        return bounds.X <= margin ||
               bounds.Y <= margin ||
               bounds.Right >= imageSize.Width - margin ||
               bounds.Bottom >= imageSize.Height - margin;
    }
}
