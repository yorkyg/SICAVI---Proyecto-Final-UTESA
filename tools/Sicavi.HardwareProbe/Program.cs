using System.Collections.Concurrent;
using System.Diagnostics;
using OpenCvSharp;
using Sicavi.WinForms.Communication;
using Sicavi.WinForms.Configuration;
using Sicavi.WinForms.Models;
using Sicavi.WinForms.Services;
using Sicavi.WinForms.Vision;

string workspaceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
string outputFolder = Path.Combine(workspaceRoot, "diagnostico_sensor_camara");
Directory.CreateDirectory(outputFolder);

if (args.Length == 1 && string.Equals(args[0], "--regresion", StringComparison.OrdinalIgnoreCase))
{
    string settingsPath = Path.Combine(AppContext.BaseDirectory, "vision-settings.json");
    VisionSettings settings = VisionSettings.LoadOrCreate(settingsPath);
    var analyzer = new CompositeVisionAnalyzer(settings);
    int failures = 0;

    foreach (ColorTarget color in new[] { ColorTarget.Green, ColorTarget.Red })
    {
        foreach (ShapeTarget shape in new[] { ShapeTarget.Circle, ShapeTarget.Square, ShapeTarget.Triangle })
        {
            using Mat sample = CreateSyntheticSample(color, shape);
            using VisionAnalysisResult analysis = analyzer.Analyze(sample, color, shape);
            bool passed = analysis.Decision == InspectionDecision.Good;
            if (!passed) failures++;
            Console.WriteLine(
                $"{(passed ? "OK" : "FALLO")}|{color}|{shape}|" +
                $"color={analysis.Color}|forma={analysis.Shape}|" +
                $"circularidad={analysis.Circularity:F3}|razon={analysis.Reason}");
        }
    }

    Console.WriteLine($"REGRESION={(failures == 0 ? "CORRECTA" : "FALLIDA")};FALLOS={failures}");
    Environment.ExitCode = failures == 0 ? 0 : 1;
    return;
}

if (args.Length == 2 && string.Equals(args[0], "--analizar", StringComparison.OrdinalIgnoreCase))
{
    string inputPath = Path.GetFullPath(args[1]);
    string settingsPath = Path.Combine(AppContext.BaseDirectory, "vision-settings.json");
    VisionSettings settings = VisionSettings.LoadOrCreate(settingsPath);
    var analyzer = new CompositeVisionAnalyzer(settings);
    using Mat input = Cv2.ImRead(inputPath, ImreadModes.Color);

    Rect searchRoi = settings.CalculateRoi(input.Size());
    using var searchView = new Mat(input, searchRoi);
    double locatorScale = Math.Min(1.0, settings.MaximumAnalysisWidth / (double)Math.Max(1, searchView.Width));
    using var locatorFrame = new Mat();
    if (locatorScale < 0.999)
    {
        Cv2.Resize(searchView, locatorFrame, new OpenCvSharp.Size(), locatorScale, locatorScale, InterpolationFlags.Area);
    }
    else
    {
        searchView.CopyTo(locatorFrame);
    }
    var cardLocator = new CardLocator(settings);
    CardLocation? card = cardLocator.Locate(locatorFrame);
    using Mat cardMask = cardLocator.CreateMask(locatorFrame);
    using VisionAnalysisResult analysis = analyzer.Analyze(input, ColorTarget.Any, ShapeTarget.Circle);

    string analysisStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
    string annotatedPath = Path.Combine(outputFolder, $"analizada-{analysisStamp}.png");
    string maskPath = Path.Combine(outputFolder, $"mascara-{analysisStamp}.png");
    string cardMaskPath = Path.Combine(outputFolder, $"tarjeta-{analysisStamp}.png");
    Cv2.ImWrite(annotatedPath, analysis.AnnotatedFrame);
    Cv2.ImWrite(maskPath, analysis.DebugFrame);
    Cv2.ImWrite(cardMaskPath, cardMask);

    Console.WriteLine($"COLOR={analysis.Color}");
    Console.WriteLine($"VERDE={analysis.GreenPercentage:F2}%");
    Console.WriteLine($"ROJO={analysis.RedPercentage:F2}%");
    Console.WriteLine($"FORMA={analysis.Shape}");
    Console.WriteLine($"VERTICES={analysis.Vertices}");
    Console.WriteLine($"CIRCULARIDAD={analysis.Circularity:F3}");
    Console.WriteLine($"DECISION={analysis.Decision}");
    Console.WriteLine($"RAZON={analysis.Reason}");
    Console.WriteLine($"ROI={analysis.Roi}");
    Console.WriteLine($"TARJETA={(card is null ? "NO_LOCALIZADA" : card.ToString())}");
    Console.WriteLine($"ANOTADA={annotatedPath}");
    Console.WriteLine($"MASCARA={maskPath}");
    Console.WriteLine($"MASCARA_TARJETA={cardMaskPath}");
    return;
}

if (args.Length == 1 && string.Equals(args[0], "--protocolo", StringComparison.OrdinalIgnoreCase))
{
    var protocolLines = new ConcurrentQueue<string>();
    SicaviTelemetry? latestTelemetry = null;

    void ProtocolLog(string text)
    {
        string line = $"{DateTime.Now:HH:mm:ss.fff}  {text}";
        protocolLines.Enqueue(line);
        Console.WriteLine(line);
    }

    using var protocolController = new SicaviSerialController();
    protocolController.RawLineReceived += (_, line) => ProtocolLog("RX " + line);
    protocolController.TelemetryReceived += (_, telemetry) => latestTelemetry = telemetry;
    protocolController.CommunicationError += (_, message) => ProtocolLog("ERROR " + message);

    try
    {
        ProtocolLog("Conectando COM6; prueba sin RUN y con el motor detenido");
        protocolController.Connect("COM6");
        await Task.Delay(1200);

        for (int attempt = 1; attempt <= 10; attempt++)
        {
            ProtocolLog($"RESET {attempt}/10");
            await protocolController.ResetAsync(TimeSpan.FromSeconds(5));
            ProtocolLog($"ACK RESET {attempt}/10");
            await Task.Delay(200);
        }

        await Task.Delay(1100);
        if (latestTelemetry is null)
        {
            throw new InvalidOperationException("No se recibió telemetría del STM32.");
        }

        ProtocolLog(
            $"RESULTADO OK; FW={latestTelemetry.FirmwareVersion}; " +
            $"ESTADO={latestTelemetry.State}; ALARMA={latestTelemetry.Alarm}");

        if (!latestTelemetry.State.Equals("STOPPED", StringComparison.OrdinalIgnoreCase) ||
            !latestTelemetry.Alarm.Equals("NONE", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Estado final inesperado: {latestTelemetry.State}, alarma {latestTelemetry.Alarm}.");
        }
    }
    finally
    {
        protocolController.Disconnect();
        string protocolStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string protocolLogPath = Path.Combine(outputFolder, $"protocolo-reset-{protocolStamp}.log");
        File.WriteAllLines(protocolLogPath, protocolLines);
        Console.WriteLine("LOG=" + protocolLogPath);
    }

    return;
}

string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
string logPath = Path.Combine(outputFolder, $"prueba-{stamp}.log");
string imagePath = Path.Combine(outputFolder, $"captura-sensor-{stamp}.png");
var lines = new ConcurrentQueue<string>();
var detected = new TaskCompletionSource<BoxDetectedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
var alarm = new TaskCompletionSource<AlarmEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
var stopwatch = new Stopwatch();
string result = "SIN_RESULTADO";

void Log(string text)
{
    string line = $"{DateTime.Now:HH:mm:ss.fff}  +{stopwatch.ElapsedMilliseconds,6} ms  {text}";
    lines.Enqueue(line);
    Console.WriteLine(line);
}

using var camera = new CameraService();
using var controller = new SicaviSerialController();

controller.RawLineReceived += (_, line) => Log("RX " + line);
controller.BoxDetected += (_, e) =>
{
    Log($"EVENTO SENSOR: ID={e.Id}; LUZ={e.LightPercentage}%; ADC={e.Adc}");
    if (stopwatch.IsRunning)
    {
        detected.TrySetResult(e);
    }
};
controller.AlarmRaised += (_, e) =>
{
    Log("EVENTO ALARMA: " + e.Code);
    if (stopwatch.IsRunning)
    {
        alarm.TrySetResult(e);
    }
};
controller.CommunicationError += (_, message) => Log("ERROR COMUNICACIÓN: " + message);
camera.CameraError += (_, message) => Log("ERROR CÁMARA: " + message);

try
{
    Log("Abriendo cámara 0 a 1920x1080");
    camera.Start(0, 1920, 1080);
    await Task.Delay(700);
    Log("Cámara activa y estabilizada; no se guardó ninguna foto previa al sensor");

    Log("Conectando COM6");
    controller.Connect("COM6");
    await Task.Delay(300);

    Log("TX RESET");
    await controller.ResetAsync(TimeSpan.FromSeconds(3));
    Log("RX ACK RESET");

    stopwatch.Restart();
    Log("TX RUN - INICIO DE VENTANA DE PRUEBA");
    await controller.RunAsync(TimeSpan.FromSeconds(3));
    Log("RX ACK RUN");

    Task timeout = Task.Delay(TimeSpan.FromSeconds(20));
    Task winner = await Task.WhenAny(detected.Task, alarm.Task, timeout);

    if (winner == detected.Task)
    {
        BoxDetectedEventArgs box = await detected.Task;
        long eventMilliseconds = stopwatch.ElapsedMilliseconds;
        Log("Solicitando captura al primer frame posterior al evento del sensor");

        using Mat snapshot = await camera.CaptureSnapshotAsync(TimeSpan.FromSeconds(4));
        long savedMilliseconds = stopwatch.ElapsedMilliseconds;
        Cv2.ImWrite(imagePath, snapshot);
        Log($"CAPTURA GUARDADA: {imagePath}; {snapshot.Width}x{snapshot.Height}; demora desde evento={savedMilliseconds - eventMilliseconds} ms");
        result = $"DETECTADA|{box.Id}|{eventMilliseconds}|{savedMilliseconds}|{imagePath}|{logPath}";
    }
    else if (winner == alarm.Task)
    {
        AlarmEventArgs alarmEvent = await alarm.Task;
        result = $"ALARMA|{alarmEvent.Code}|{stopwatch.ElapsedMilliseconds}|{logPath}";
        Log("La prueba terminó por alarma antes de detectar la caja");
    }
    else
    {
        result = $"TIMEOUT|{stopwatch.ElapsedMilliseconds}|{logPath}";
        Log("TIMEOUT: no llegó evento de caja ni alarma en 20 segundos");
    }
}
catch (Exception exception)
{
    result = $"ERROR|{exception.GetType().Name}|{exception.Message}|{logPath}";
    Log("EXCEPCIÓN: " + exception);
}
finally
{
    if (controller.IsConnected)
    {
        try
        {
            Log("TX STOP - PARADA DE SEGURIDAD");
            controller.Stop();
        }
        catch (Exception exception)
        {
            Log("ERROR AL ENVIAR STOP: " + exception.Message);
        }
    }

    try
    {
        await camera.StopAsync();
    }
    catch (Exception exception)
    {
        Log("ERROR AL DETENER CÁMARA: " + exception.Message);
    }

    try
    {
        controller.Disconnect();
    }
    catch (Exception exception)
    {
        Log("ERROR AL DESCONECTAR: " + exception.Message);
    }

    File.WriteAllLines(logPath, lines);
}

Console.WriteLine("RESULTADO=" + result);

static Mat CreateSyntheticSample(ColorTarget color, ShapeTarget shape)
{
    var frame = new Mat(new OpenCvSharp.Size(1600, 1200), MatType.CV_8UC3, new Scalar(120, 25, 135));
    Cv2.Rectangle(frame, new Rect(1100, 0, 260, 1200), new Scalar(18, 18, 18), -1);
    var card = new Rect(500, 400, 450, 450);
    Cv2.Rectangle(frame, card, new Scalar(242, 242, 242), -1);
    Scalar ink = color == ColorTarget.Green
        ? new Scalar(75, 155, 80)
        : new Scalar(75, 75, 185);

    switch (shape)
    {
        case ShapeTarget.Circle:
            Cv2.Circle(frame, new Point(725, 625), 130, ink, -1, LineTypes.AntiAlias);
            break;
        case ShapeTarget.Square:
            Cv2.Rectangle(frame, new Rect(595, 495, 260, 260), ink, -1, LineTypes.AntiAlias);
            break;
        case ShapeTarget.Triangle:
            Cv2.FillConvexPoly(
                frame,
                new[] { new Point(725, 485), new Point(580, 760), new Point(870, 760) },
                ink,
                LineTypes.AntiAlias);
            break;
    }

    Cv2.GaussianBlur(frame, frame, new OpenCvSharp.Size(3, 3), 0.7);
    return frame;
}
