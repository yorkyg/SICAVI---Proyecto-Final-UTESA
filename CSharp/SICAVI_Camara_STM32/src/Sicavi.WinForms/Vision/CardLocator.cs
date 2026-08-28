using OpenCvSharp;
using Sicavi.WinForms.Configuration;
using Point = OpenCvSharp.Point;

namespace Sicavi.WinForms.Vision;

/// <summary>
/// Localiza la tarjeta blanca que contiene la figura y devuelve una zona
/// interior sin sus bordes. Así la banda, la placa y el contorno de la caja
/// no compiten con la figura durante la clasificación.
/// </summary>
public sealed class CardLocator
{
    private readonly VisionSettings _settings;

    public CardLocator(VisionSettings settings)
    {
        _settings = settings;
    }

    public CardLocation? Locate(Mat image)
    {
        if (image.Empty()) return null;

        using Mat whiteMask = CreateMask(image);

        Cv2.FindContours(
            whiteMask,
            out Point[][] contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        double imageArea = image.Width * image.Height;
        double centerX = image.Width / 2.0;
        double centerY = image.Height / 2.0;
        CardCandidate? best = null;

        foreach (Point[] contour in contours)
        {
            double area = Math.Abs(Cv2.ContourArea(contour));
            double areaRatio = area / Math.Max(1.0, imageArea);
            if (areaRatio < _settings.CardMinimumAreaRatio ||
                areaRatio > _settings.CardMaximumAreaRatio)
            {
                continue;
            }

            Rect bounds = Cv2.BoundingRect(contour);
            if (TouchesImageBorder(bounds, image.Size()) || bounds.Width < 80 || bounds.Height < 80)
            {
                continue;
            }

            RotatedRect rotated = Cv2.MinAreaRect(contour);
            double longSide = Math.Max(rotated.Size.Width, rotated.Size.Height);
            double shortSide = Math.Min(rotated.Size.Width, rotated.Size.Height);
            if (shortSide <= 0.0) continue;

            double aspect = longSide / shortSide;
            if (aspect < _settings.CardAspectRatioMinimum ||
                aspect > _settings.CardAspectRatioMaximum)
            {
                continue;
            }

            double rotatedArea = longSide * shortSide;
            double extent = rotatedArea > 0.0 ? area / rotatedArea : 0.0;
            if (extent < _settings.CardMinimumExtent) continue;

            Moments moments = Cv2.Moments(contour);
            double objectX = Math.Abs(moments.M00) > double.Epsilon
                ? moments.M10 / moments.M00
                : bounds.X + bounds.Width / 2.0;
            double objectY = Math.Abs(moments.M00) > double.Epsilon
                ? moments.M01 / moments.M00
                : bounds.Y + bounds.Height / 2.0;
            double distance = Math.Sqrt(
                Math.Pow((objectX - centerX) / Math.Max(1.0, centerX), 2) +
                Math.Pow((objectY - centerY) / Math.Max(1.0, centerY), 2));
            double score = area * extent / (1.0 + (1.5 * distance));

            if (best is null || score > best.Score)
            {
                best = new CardCandidate(bounds, score, extent);
            }
        }

        if (best is null) return null;

        int insetX = Math.Max(4, (int)Math.Round(best.Bounds.Width * _settings.CardInsetRatio));
        int insetY = Math.Max(4, (int)Math.Round(best.Bounds.Height * _settings.CardInsetRatio));
        var inner = new Rect(
            best.Bounds.X + insetX,
            best.Bounds.Y + insetY,
            best.Bounds.Width - (2 * insetX),
            best.Bounds.Height - (2 * insetY));

        if (inner.Width < 40 || inner.Height < 40) return null;
        return new CardLocation(best.Bounds, inner, best.Extent);
    }

    public Mat CreateMask(Mat image)
    {
        if (image.Empty()) return new Mat();

        using var hsv = new Mat();
        using var openKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(11, 11));
        using var closeKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
        var whiteMask = new Mat();

        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
        Cv2.InRange(
            hsv,
            new Scalar(0, 0, _settings.CardWhiteMinimumValue),
            new Scalar(179, _settings.CardWhiteMaximumSaturation, 255),
            whiteMask);
        Cv2.MorphologyEx(whiteMask, whiteMask, MorphTypes.Open, openKernel, iterations: 1);
        Cv2.MorphologyEx(whiteMask, whiteMask, MorphTypes.Close, closeKernel, iterations: 1);
        return whiteMask;
    }

    private static bool TouchesImageBorder(Rect bounds, OpenCvSharp.Size size)
    {
        int margin = Math.Max(3, Math.Min(size.Width, size.Height) / 200);
        return bounds.X <= margin ||
               bounds.Y <= margin ||
               bounds.Right >= size.Width - margin ||
               bounds.Bottom >= size.Height - margin;
    }

    private sealed record CardCandidate(Rect Bounds, double Score, double Extent);
}

public sealed record CardLocation(Rect Bounds, Rect InnerBounds, double Confidence);
