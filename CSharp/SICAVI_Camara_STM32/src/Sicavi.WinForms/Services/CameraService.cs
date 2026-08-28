using System.Diagnostics;
using OpenCvSharp;
using Sicavi.WinForms.Exceptions;

namespace Sicavi.WinForms.Services;

public sealed class CameraService : IDisposable
{
    private readonly object _sync = new();
    private VideoCapture? _capture;
    private CancellationTokenSource? _cancellation;
    private Task? _captureTask;
    private TaskCompletionSource<Mat>? _snapshotRequest;
    private bool _disposed;

    public event EventHandler<FrameReadyEventArgs>? FrameReady;
    public event EventHandler<string>? CameraError;

    public bool IsRunning
    {
        get
        {
            lock (_sync)
            {
                return _captureTask is { IsCompleted: false };
            }
        }
    }

    public void Start(int cameraIndex, int width, int height)
    {
        ThrowIfDisposed();

        lock (_sync)
        {
            if (_captureTask is { IsCompleted: false })
            {
                throw new CameraException("La cámara ya está iniciada.");
            }

            _capture = OpenCamera(cameraIndex);
            _capture.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G'));
            _capture.Set(VideoCaptureProperties.FrameWidth, width);
            _capture.Set(VideoCaptureProperties.FrameHeight, height);
            _capture.Set(VideoCaptureProperties.Fps, 30);
            _capture.Set(VideoCaptureProperties.BufferSize, 1);

            _cancellation = new CancellationTokenSource();
            _captureTask = Task.Run(() => CaptureLoopAsync(_cancellation.Token));
        }
    }

    public async Task<Mat> CaptureSnapshotAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        var request = new TaskCompletionSource<Mat>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_sync)
        {
            if (_captureTask is not { IsCompleted: false })
            {
                throw new CameraException("La cámara no está entregando imágenes.");
            }
            if (_snapshotRequest is not null)
            {
                throw new CameraException("Ya existe una captura de inspección pendiente.");
            }

            _snapshotRequest = request;
        }

        try
        {
            return await request.Task
                .WaitAsync(timeout, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            lock (_sync)
            {
                if (ReferenceEquals(_snapshotRequest, request))
                {
                    _snapshotRequest = null;
                }
            }

            request.TrySetCanceled(cancellationToken);
            throw;
        }
    }

    public async Task StopAsync()
    {
        Task? task;
        CancellationTokenSource? cancellation;
        TaskCompletionSource<Mat>? snapshotRequest;

        lock (_sync)
        {
            task = _captureTask;
            cancellation = _cancellation;
            snapshotRequest = _snapshotRequest;
            _snapshotRequest = null;
        }

        snapshotRequest?.TrySetException(new CameraException("La cámara se detuvo antes de capturar la inspección."));
        cancellation?.Cancel();

        if (task is not null)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // La cancelación es el cierre normal del ciclo de captura.
            }
        }

        lock (_sync)
        {
            _capture?.Release();
            _capture?.Dispose();
            _capture = null;

            _cancellation?.Dispose();
            _cancellation = null;
            _captureTask = null;
        }
    }

    private static VideoCapture OpenCamera(int cameraIndex)
    {
        VideoCapture? capture = null;

        try
        {
            capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            if (capture.IsOpened())
            {
                return capture;
            }

            capture.Dispose();
            capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.ANY);

            if (!capture.IsOpened())
            {
                throw new CameraException(
                    $"No se pudo abrir la cámara con índice {cameraIndex}. " +
                    "Prueba con otro índice y verifica que ninguna otra aplicación la esté usando.");
            }

            return capture;
        }
        catch (CameraException)
        {
            capture?.Dispose();
            throw;
        }
        catch (Exception exception)
        {
            capture?.Dispose();
            throw new CameraException("Ocurrió un error al inicializar la cámara.", exception);
        }
    }

    private async Task CaptureLoopAsync(CancellationToken cancellationToken)
    {
        using var frame = new Mat();
        var fpsClock = Stopwatch.StartNew();
        int frameCounter = 0;
        double framesPerSecond = 0.0;
        int consecutiveFailures = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            VideoCapture? capture;
            lock (_sync)
            {
                capture = _capture;
            }

            if (capture is null)
            {
                return;
            }

            bool readSucceeded;
            try
            {
                readSucceeded = capture.Read(frame);
            }
            catch (Exception exception)
            {
                CameraError?.Invoke(this, $"Error leyendo la cámara: {exception.Message}");
                return;
            }

            if (!readSucceeded || frame.Empty())
            {
                consecutiveFailures++;
                if (consecutiveFailures == 30)
                {
                    CameraError?.Invoke(this, "La cámara dejó de entregar imágenes.");
                }

                await Task.Delay(30, cancellationToken).ConfigureAwait(false);
                continue;
            }

            consecutiveFailures = 0;
            frameCounter++;

            TaskCompletionSource<Mat>? snapshotRequest;
            lock (_sync)
            {
                snapshotRequest = _snapshotRequest;
                _snapshotRequest = null;
            }

            if (snapshotRequest is not null)
            {
                Mat snapshot = frame.Clone();
                if (!snapshotRequest.TrySetResult(snapshot))
                {
                    snapshot.Dispose();
                }
            }

            if (fpsClock.ElapsedMilliseconds >= 1000)
            {
                framesPerSecond = frameCounter * 1000.0 / fpsClock.ElapsedMilliseconds;
                frameCounter = 0;
                fpsClock.Restart();
            }

            /* El evento es sincrono: el receptor termina antes de reutilizar frame. */
            FrameReady?.Invoke(this, new FrameReadyEventArgs(frame, framesPerSecond));
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            StopAsync().GetAwaiter().GetResult();
        }
        finally
        {
            _disposed = true;
        }
    }
}
