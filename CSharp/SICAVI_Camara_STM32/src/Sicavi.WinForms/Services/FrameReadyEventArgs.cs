using OpenCvSharp;

namespace Sicavi.WinForms.Services;

public sealed class FrameReadyEventArgs : EventArgs
{
    public FrameReadyEventArgs(Mat frame, double framesPerSecond)
    {
        Frame = frame;
        FramesPerSecond = framesPerSecond;
    }

    public Mat Frame { get; }
    public double FramesPerSecond { get; }
}
