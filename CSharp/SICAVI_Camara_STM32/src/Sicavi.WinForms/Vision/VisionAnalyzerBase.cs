using OpenCvSharp;
using Sicavi.WinForms.Configuration;

namespace Sicavi.WinForms.Vision;

public abstract class VisionAnalyzerBase<TResult>
{
    protected VisionAnalyzerBase(VisionSettings settings)
    {
        Settings = settings;
    }

    protected VisionSettings Settings { get; }

    public abstract TResult Analyze(Mat image);

    protected static void ValidateImage(Mat image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (image.Empty())
        {
            throw new ArgumentException("La imagen está vacía.", nameof(image));
        }
    }
}
