using System.Text.Json;
using OpenCvSharp;

namespace Sicavi.WinForms.Configuration;

public sealed class VisionSettings
{
    public HsvRange Green { get; set; } = new(25, 105, 30, 255, 25, 255);
    public HsvRange RedLow { get; set; } = new(0, 20, 30, 255, 25, 255);
    public HsvRange RedHigh { get; set; } = new(150, 179, 30, 255, 25, 255);

    public double MinimumColorPercentage { get; set; } = 2.0;
    public double MinimumColorObjectAreaRatio { get; set; } = 0.0025;
    public double MinimumShapeAreaPixels { get; set; } = 900.0;
    public double MaximumShapeAreaRatio { get; set; } = 0.75;
    public double PolygonEpsilonRatio { get; set; } = 0.03;
    public double MinimumCircleCircularity { get; set; } = 0.68;
    public double SquareAspectRatioMinimum { get; set; } = 0.72;
    public double SquareAspectRatioMaximum { get; set; } = 1.28;
    public int ShapeMinimumSaturation { get; set; } = 28;
    public int ShapeMinimumValue { get; set; } = 25;
    public int MaximumAnalysisWidth { get; set; } = 960;

    public int CardWhiteMaximumSaturation { get; set; } = 105;
    public int CardWhiteMinimumValue { get; set; } = 125;
    public double CardMinimumAreaRatio { get; set; } = 0.035;
    public double CardMaximumAreaRatio { get; set; } = 0.55;
    public double CardAspectRatioMinimum { get; set; } = 1.0;
    public double CardAspectRatioMaximum { get; set; } = 1.60;
    public double CardMinimumExtent { get; set; } = 0.62;
    public double CardInsetRatio { get; set; } = 0.075;

    public double RoiXRatio { get; set; } = 0.15;
    public double RoiYRatio { get; set; } = 0.12;
    public double RoiWidthRatio { get; set; } = 0.70;
    public double RoiHeightRatio { get; set; } = 0.76;

    public Rect CalculateRoi(OpenCvSharp.Size frameSize)
    {
        if (frameSize.Width <= 0 || frameSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameSize), "El fotograma no tiene dimensiones válidas.");
        }

        int x = (int)Math.Round(frameSize.Width * ClampRatio(RoiXRatio));
        int y = (int)Math.Round(frameSize.Height * ClampRatio(RoiYRatio));
        int width = (int)Math.Round(frameSize.Width * ClampRatio(RoiWidthRatio));
        int height = (int)Math.Round(frameSize.Height * ClampRatio(RoiHeightRatio));

        x = Math.Clamp(x, 0, frameSize.Width - 1);
        y = Math.Clamp(y, 0, frameSize.Height - 1);
        width = Math.Clamp(width, 1, frameSize.Width - x);
        height = Math.Clamp(height, 1, frameSize.Height - y);

        return new Rect(x, y, width, height);
    }

    public static VisionSettings LoadOrCreate(string path)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        if (!File.Exists(path))
        {
            var defaults = new VisionSettings();
            File.WriteAllText(path, JsonSerializer.Serialize(defaults, options));
            return defaults;
        }

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<VisionSettings>(json, options)
            ?? throw new InvalidDataException("No se pudo leer vision-settings.json.");
    }

    private static double ClampRatio(double value) => Math.Clamp(value, 0.0, 1.0);
}

public sealed class HsvRange
{
    public HsvRange()
    {
    }

    public HsvRange(int hMin, int hMax, int sMin, int sMax, int vMin, int vMax)
    {
        HMin = hMin;
        HMax = hMax;
        SMin = sMin;
        SMax = sMax;
        VMin = vMin;
        VMax = vMax;
    }

    public int HMin { get; set; }
    public int HMax { get; set; }
    public int SMin { get; set; }
    public int SMax { get; set; }
    public int VMin { get; set; }
    public int VMax { get; set; }

    public Scalar Minimum => new(HMin, SMin, VMin);
    public Scalar Maximum => new(HMax, SMax, VMax);
}
