using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sicavi.WinForms.Communication;
using Sicavi.WinForms.Configuration;
using Sicavi.WinForms.Data;
using Sicavi.WinForms.Exceptions;
using Sicavi.WinForms.Models;
using Sicavi.WinForms.Services;
using Sicavi.WinForms.Utilities;
using Sicavi.WinForms.Vision;
namespace Sicavi.WinForms.Forms;

public sealed partial class MainForm : Form
{
    private const string DefaultSerialPort = "COM6";
    private const int DefaultCameraIndex = 0;
    private const int CameraWidth = 1920;
    private const int CameraHeight = 1080;

    private readonly CameraService _cameraService = new();
    private readonly SicaviSerialController _sicaviController = new();
    private readonly AppLogger _logger = new();
    private readonly AppUser _currentUser;
    private readonly UserRepository? _userRepository;
    private readonly ProductionRepository? _productionRepository;
    private readonly VisionSettings _settings;
    private readonly CompositeVisionAnalyzer _visionAnalyzer;
    private int _frameProcessing;
    private int _holdLastInspectionFrame;
    private int _serialConnectionAttempt;
    private int _runStartupInProgress;
    private long _lastPreviewUpdateTicks;
    private ColorTarget _expectedColor = ColorTarget.Any;
    private ShapeTarget _expectedShape = ShapeTarget.Any;
    private bool _allowClose;
    private DateTime _lastTelemetryPersist = DateTime.MinValue;
    private System.Windows.Forms.Timer? _serialReconnectTimer;

    public bool LogoutRequested { get; private set; }

    public MainForm() : this(AppUser.DesignUser, null, null)
    {
    }

    public MainForm(
        AppUser currentUser,
        UserRepository? userRepository,
        ProductionRepository? productionRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _productionRepository = productionRepository;
        _settings = LoadSettings();
        _visionAnalyzer = new CompositeVisionAnalyzer(_settings);

        InitializeComponent();
        _currentUserLabel.Text = $"{_currentUser.FullName} · {_currentUser.Role}";
        bool designerPreview = _currentUser.Id == AppUser.DesignUser.Id;
        _usersButton.Visible = true;
        _usersButton.Enabled = designerPreview || _userRepository is not null;
        _databaseButton.Enabled = designerPreview || _productionRepository is not null;
        SetSerialControlButtons(false);

        if (designerPreview)
        {
            return;
        }

        ConnectEvents();
        ConfigureAutomaticSerialConnection();

        _logger.Info($"Sesión iniciada por {_currentUser.Username}.");
    }

    private VisionSettings LoadSettings()
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "vision-settings.json");

        try
        {
            return VisionSettings.LoadOrCreate(settingsPath);
        }
        catch (Exception exception)
        {
            _logger.Warning($"No se pudo cargar la configuración; se usarán valores iniciales. {exception.Message}");
            return new VisionSettings();
        }
    }

    private void ConnectEvents()
    {
        _databaseButton.Click += DatabaseButton_Click;
        _usersButton.Click += UsersButton_Click;
        _logoutButton.Click += LogoutButton_Click;
        _runSystemButton.Click += RunSystemButton_Click;
        _stopSystemButton.Click += StopSystemButton_Click;
        _resetSystemButton.Click += ResetSystemButton_Click;

        _expectedColorComboBox.SelectedIndexChanged += (_, _) =>
            _expectedColor = _expectedColorComboBox.SelectedIndex switch
            {
                1 => ColorTarget.Green,
                2 => ColorTarget.Red,
                _ => ColorTarget.Any
            };

        _expectedShapeComboBox.SelectedIndexChanged += (_, _) =>
            _expectedShape = _expectedShapeComboBox.SelectedIndex switch
            {
                1 => ShapeTarget.Circle,
                2 => ShapeTarget.Square,
                3 => ShapeTarget.Triangle,
                _ => ShapeTarget.Any
            };

        _cameraService.FrameReady += CameraService_FrameReady;
        _cameraService.CameraError += (_, message) =>
        {
            _logger.Error(message);
            SafeBeginInvoke(() =>
            {
                _cameraStatus.Text = "Cámara: error";
                if (_sicaviController.IsConnected)
                {
                    ExecuteSerialCommand(_sicaviController.Stop, "DETENCIÓN de seguridad enviada por fallo de cámara.");
                }
                _ = StopCameraAsync();
            });
        };

        _sicaviController.ConnectionChanged += (_, connected) => SafeBeginInvoke(() =>
        {
            SetSerialControlButtons(connected);
            _serialStatus.Text = connected ? $"STM32: {DefaultSerialPort} automático" : $"STM32: reconectando {DefaultSerialPort}";
            _logger.Info(connected ? $"STM32 conectado automáticamente por {DefaultSerialPort}." : "STM32 desconectado.");
            if (connected)
            {
                _serialReconnectTimer?.Stop();
            }
            if (!connected)
            {
                _serialReconnectTimer?.Start();
            }
        });

        _sicaviController.CommunicationError += (_, message) => SafeBeginInvoke(() =>
        {
            _logger.Error($"UART: {message}");
            _sicaviController.Disconnect();
        });

        _sicaviController.RawLineReceived += (_, line) =>
            SafeBeginInvoke(() => _logger.Info($"STM32 > {line}"));

        _sicaviController.TelemetryReceived += (_, telemetry) => SafeBeginInvoke(() =>
        {
            UpdateTelemetry(telemetry);
            if (DateTime.UtcNow - _lastTelemetryPersist >= TimeSpan.FromSeconds(5))
            {
                _lastTelemetryPersist = DateTime.UtcNow;
                PersistSafely(
                    () =>
                    {
                        _productionRepository?.RecordTelemetry(telemetry.WorkTime, telemetry.EjectorCycles);
                        if (telemetry.Alarm.Equals("NONE", StringComparison.OrdinalIgnoreCase))
                        {
                            _productionRepository?.CloseOpenAlarms();
                        }
                    },
                    "telemetría");
            }
        });

        _sicaviController.StateChanged += (_, state) => SafeBeginInvoke(() =>
            _systemStateValue.Text = TranslateSystemState(state));

        _sicaviController.AlarmRaised += (_, alarm) => SafeBeginInvoke(() =>
        {
            _alarmValue.Text = TranslateAlarm(alarm.Code);
            _logger.Warning($"Alarma del STM32: {alarm.Code}");
            PersistSafely(() => _productionRepository?.OpenAlarm(alarm.Code), "alarma");
        });

        _sicaviController.CommandRejected += (_, rejection) => SafeBeginInvoke(() =>
            _logger.Warning(
                $"El STM32 rechazó {rejection.Command}: {TranslateCommandError(rejection.Code)}."));

        _sicaviController.ResetAcknowledged += (_, _) => SafeBeginInvoke(() =>
        {
            _alarmValue.Text = "NINGUNA";
            _systemStateValue.Text = "DETENIDO";
            PersistSafely(() => _productionRepository?.CloseOpenAlarms(), "cierre de alarma por reinicio");
            _logger.Info("REINICIO confirmado por el STM32. Alarma restablecida.");
        });

        _sicaviController.InspectionFinished += (_, inspection) => SafeBeginInvoke(() =>
        {
            _logger.Info($"Caja {inspection.Id}: ciclo físico terminado con resultado {TranslateInspectionResult(inspection.Result)}.");
            PersistSafely(
                () => _productionRepository?.CompleteInspection(inspection.Id, inspection.Result),
                "finalización de inspección");
        });

        _sicaviController.BoxDetected += SicaviController_BoxDetected;

        FormClosing += MainForm_FormClosing;
    }

    private void DatabaseButton_Click(object? sender, EventArgs e)
    {
        if (_productionRepository is null)
        {
            return;
        }

        using var form = new ProductionDataForm(_productionRepository);
        form.ShowDialog(this);
    }

    private void UsersButton_Click(object? sender, EventArgs e)
    {
        if (!_currentUser.IsAdministrator)
        {
            MessageBox.Show(
                this,
                "La gestión de usuarios está disponible únicamente para cuentas con rol Administrador.",
                "Permiso de administrador requerido",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (_userRepository is null)
        {
            return;
        }

        using var form = new UserManagementForm(_userRepository, _currentUser);
        form.ShowDialog(this);
    }

    private void LogoutButton_Click(object? sender, EventArgs e)
    {
        DialogResult confirmation = MessageBox.Show(
            this,
            "Se detendrá el sistema y volverás a la pantalla de inicio de sesión. ¿Deseas continuar?",
            "Cerrar sesión",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        LogoutRequested = true;
        Close();
    }

    private bool StartCameraForRun()
    {
        if (_cameraService.IsRunning)
        {
            return true;
        }

        try
        {
            _cameraService.Start(DefaultCameraIndex, CameraWidth, CameraHeight);
            _cameraStatus.Text = $"Cámara: 0 automática, {CameraWidth}x{CameraHeight}";
            _logger.Info($"Cámara 0 iniciada automáticamente a {CameraWidth}x{CameraHeight}.");
            return true;
        }
        catch (CameraException exception)
        {
            _logger.Error(exception.Message);
            MessageBox.Show(this, exception.Message, "No se pudo abrir la cámara", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        catch (Exception exception)
        {
            _logger.Error(exception.Message);
            MessageBox.Show(this, exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private async void RunSystemButton_Click(object? sender, EventArgs e)
    {
        if (Interlocked.CompareExchange(ref _runStartupInProgress, 1, 0) != 0)
        {
            return;
        }

        _runSystemButton.Enabled = false;

        try
        {
            /*
             * RUN solo se envia despues del ACK real de RESET. Esto elimina la
             * carrera de tiempo en la que uno de los dos comandos podia perderse.
             */
            await _sicaviController.ResetAsync(TimeSpan.FromSeconds(5));

            if (!StartCameraForRun())
            {
                _logger.Warning("INICIO cancelado porque la cámara no pudo iniciarse.");
                return;
            }

            await _sicaviController.RunAsync(TimeSpan.FromSeconds(3));
            _cameraStatus.Text = "Cámara: lista, esperando sensor de caja";
            _logger.Info("REINICIO e INICIO confirmados por el STM32. La captura queda reservada exclusivamente para el sensor de caja.");
        }
        catch (SicaviCommandException exception)
        {
            string message = TranslateCommandError(exception.Code);
            _logger.Warning(message);
            MessageBox.Show(this, message, "Inicio rechazado por el STM32", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (TimeoutException)
        {
            const string message = "El STM32 no confirmó el comando. Verifica COM6 y vuelve a intentar.";
            _logger.Error(message);
            MessageBox.Show(this, message, "Sin confirmación del STM32", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception exception)
        {
            _logger.Error(exception.Message);
            MessageBox.Show(this, exception.Message, "Inicio del sistema", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Interlocked.Exchange(ref _runStartupInProgress, 0);
            _runSystemButton.Enabled = _sicaviController.IsConnected;
        }
    }

    private async void StopSystemButton_Click(object? sender, EventArgs e)
    {
        try
        {
            _sicaviController.Stop();
            _logger.Info("DETENCIÓN enviada al STM32.");
        }
        catch (Exception exception)
        {
            _logger.Error(exception.Message);
            MessageBox.Show(this, exception.Message, "Comunicación STM32", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            await StopCameraAsync();
        }
    }

    private async void ResetSystemButton_Click(object? sender, EventArgs e)
    {
        _resetSystemButton.Enabled = false;
        try
        {
            _alarmValue.Text = "RESTABLECIENDO...";
            _systemStateValue.Text = "REINICIANDO";
            await _sicaviController.ResetAsync(TimeSpan.FromSeconds(5));
            _logger.Info("REINICIO confirmado por el STM32.");
        }
        catch (TimeoutException)
        {
            const string message = "El STM32 no confirmó el reinicio. La alarma no se marcó como cerrada.";
            _logger.Error(message);
            MessageBox.Show(this, message, "Reinicio sin confirmar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception exception)
        {
            _logger.Error(exception.Message);
            MessageBox.Show(this, exception.Message, "Reinicio del STM32", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _resetSystemButton.Enabled = _sicaviController.IsConnected;
        }
    }

    private void ConfigureAutomaticSerialConnection()
    {
        _serialReconnectTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _serialReconnectTimer.Tick += (_, _) => AttemptAutomaticSerialConnection();
        Shown += (_, _) => AttemptAutomaticSerialConnection();
        _serialStatus.Text = $"STM32: esperando {DefaultSerialPort}";
    }

    private void AttemptAutomaticSerialConnection()
    {
        if (_sicaviController.IsConnected || Interlocked.Exchange(ref _serialConnectionAttempt, 1) != 0)
        {
            return;
        }

        try
        {
            _sicaviController.Connect(DefaultSerialPort);
        }
        catch (Exception exception)
        {
            _serialStatus.Text = $"STM32: reconectando {DefaultSerialPort}";
            _logger.Warning($"No se pudo abrir {DefaultSerialPort}; se reintentará automáticamente. {exception.Message}");
            _serialReconnectTimer?.Start();
        }
        finally
        {
            Interlocked.Exchange(ref _serialConnectionAttempt, 0);
        }
    }

    private void ExecuteSerialCommand(Action command, string logMessage)
    {
        try
        {
            command();
            _logger.Info(logMessage);
        }
        catch (Exception exception)
        {
            _logger.Error(exception.Message);
            MessageBox.Show(this, exception.Message, "Comunicación STM32", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SicaviController_BoxDetected(object? sender, BoxDetectedEventArgs detection)
    {
        SafeBeginInvoke(() => _ = CaptureAndValidateBoxAsync(detection));
    }

    private async Task CaptureAndValidateBoxAsync(BoxDetectedEventArgs detection)
    {
        _logger.Info($"Caja {detection.Id} posicionada. LDR={detection.LightPercentage}% ADC={detection.Adc}.");

        if (!_cameraService.IsRunning)
        {
            SendUnknownVisionResult(detection.Id, "La cámara no está activa.");
            return;
        }

        Interlocked.Exchange(ref _holdLastInspectionFrame, 0);
        _cameraStatus.Text = $"Cámara: capturando caja {detection.Id}...";
        _reasonValue.Text = "Caja posicionada; reservando un fotograma estable.";

        bool ownsFrameProcessing = await TryAcquireFrameProcessingAsync(TimeSpan.FromSeconds(2));
        if (!ownsFrameProcessing)
        {
            SendUnknownVisionResult(detection.Id, "La cámara seguía ocupada y no pudo reservarse el fotograma.");
            return;
        }

        Bitmap? cameraBitmap = null;
        Bitmap? debugBitmap = null;
        bool handedToUi = false;
        var candidates = new List<VisionCandidate>();
        VisionCandidate? selected = null;

        try
        {
            ColorTarget expectedColor = _expectedColor;
            ShapeTarget expectedShape = _expectedShape;
            selected = await CaptureVisionCandidateAsync(
                detection.Id,
                expectedColor,
                expectedShape);
            candidates.Add(selected);

            /*
             * Una lectura indeterminada se verifica con dos fotogramas frescos.
             * Solo se reemplaza si ambos reintentos coinciden en color y forma;
             * así se filtra ruido sin aceptar un único falso positivo.
             */
            if (selected.Summary.Decision == InspectionDecision.Unknown)
            {
                _reasonValue.Text = "Resultado dudoso; comprobando dos fotogramas adicionales...";
                await Task.Delay(70);
                VisionCandidate retry1 = await CaptureVisionCandidateAsync(
                    detection.Id,
                    expectedColor,
                    expectedShape);
                candidates.Add(retry1);
                await Task.Delay(70);
                VisionCandidate retry2 = await CaptureVisionCandidateAsync(
                    detection.Id,
                    expectedColor,
                    expectedShape);
                candidates.Add(retry2);

                if (retry1.Summary.Decision != InspectionDecision.Unknown &&
                    retry2.Summary.Decision != InspectionDecision.Unknown &&
                    retry1.Summary.Color == retry2.Summary.Color &&
                    retry1.Summary.Shape == retry2.Summary.Shape)
                {
                    selected = retry1.Summary.Circularity >= retry2.Summary.Circularity
                        ? retry1
                        : retry2;
                    _logger.Info($"Caja {detection.Id}: validación confirmada por dos fotogramas adicionales.");
                }
            }

            cameraBitmap = selected.CameraBitmap;
            debugBitmap = selected.DebugBitmap;
            VisionSummary summary = selected.Summary;

            Interlocked.Exchange(ref _holdLastInspectionFrame, 1);
            QueueFrameForUi(cameraBitmap, debugBitmap, summary, 0.0);
            handedToUi = true;
            cameraBitmap = null;
            debugBitmap = null;
        }
        catch (Exception exception)
        {
            SendUnknownVisionResult(detection.Id, $"No se pudo capturar o analizar la caja: {exception.Message}");
        }
        finally
        {
            foreach (VisionCandidate candidate in candidates)
            {
                if (!handedToUi || !ReferenceEquals(candidate, selected)) candidate.Dispose();
            }
            cameraBitmap = null;
            debugBitmap = null;
            if (!handedToUi)
            {
                Interlocked.Exchange(ref _frameProcessing, 0);
            }
        }
    }

    private async Task<VisionCandidate> CaptureVisionCandidateAsync(
        string inspectionId,
        ColorTarget expectedColor,
        ShapeTarget expectedShape)
    {
        using Mat snapshot = await _cameraService.CaptureSnapshotAsync(TimeSpan.FromSeconds(3));
        return await Task.Run(() =>
        {
            using VisionAnalysisResult result = _visionAnalyzer.Analyze(
                snapshot,
                expectedColor,
                expectedShape);
            return new VisionCandidate(
                BitmapConverter.ToBitmap(result.AnnotatedFrame),
                BitmapConverter.ToBitmap(result.DebugFrame),
                CreateSummary(result, inspectionId));
        });
    }

    private async Task<bool> TryAcquireFrameProcessingAsync(TimeSpan timeout)
    {
        long deadline = Environment.TickCount64 + (long)timeout.TotalMilliseconds;
        while (Environment.TickCount64 < deadline)
        {
            if (Interlocked.CompareExchange(ref _frameProcessing, 1, 0) == 0)
            {
                return true;
            }

            await Task.Delay(15);
        }

        return false;
    }

    private void SendUnknownVisionResult(string inspectionId, string reason)
    {
        _logger.Warning(reason);
        _cameraStatus.Text = $"Cámara: fallo al capturar caja {inspectionId}";
        _reasonValue.Text = reason;

        if (_sicaviController.IsConnected)
        {
            ExecuteSerialCommand(
                () => _sicaviController.SendVisionResult(inspectionId, "UNKNOWN"),
                $"Resultado INDETERMINADO enviado para la caja {inspectionId}.");
        }
    }

    private void UpdateTelemetry(SicaviTelemetry telemetry)
    {
        _serialStatus.Text = $"STM32: {DefaultSerialPort} · firmware {telemetry.FirmwareVersion}";
        _systemStateValue.Text = TranslateSystemState(telemetry.State);
        _ldrValue.Text = $"{telemetry.LightPercentage}% · ADC {telemetry.Adc} · {telemetry.Millivolts} mV";
        _hourmeterValue.Text = FormatElapsed(telemetry.WorkTime);
        _ejectorCyclesValue.Text = telemetry.EjectorCycles.ToString();
        _alarmValue.Text = TranslateAlarm(telemetry.Alarm);
    }

    private static string FormatElapsed(TimeSpan time) =>
        $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";

    private async Task StopCameraAsync()
    {
        if (!_cameraService.IsRunning)
        {
            return;
        }

        await _cameraService.StopAsync();
        _cameraStatus.Text = "Cámara: desconectada";
        _fpsStatus.Text = "FPS: 0.0";
        _logger.Info("Cámara detenida automáticamente al detener el sistema.");
    }

    private void CameraService_FrameReady(object? sender, FrameReadyEventArgs eventArgs)
    {
        if (Interlocked.Exchange(ref _frameProcessing, 1) == 1)
        {
            return;
        }

        Bitmap? cameraBitmap = null;
        bool handedToUi = false;

        try
        {
            /* La ultima clasificacion permanece en pantalla hasta la proxima caja. */
            if (Volatile.Read(ref _holdLastInspectionFrame) != 0)
            {
                return;
            }

            long now = Environment.TickCount64;
            if (now - Interlocked.Read(ref _lastPreviewUpdateTicks) < 66L)
            {
                return;
            }

            Interlocked.Exchange(ref _lastPreviewUpdateTicks, now);
            using Mat idleFrame = _visionAnalyzer.DrawIdleOverlay(eventArgs.Frame);
            cameraBitmap = BitmapConverter.ToBitmap(idleFrame);

            QueueFrameForUi(cameraBitmap, null, null, eventArgs.FramesPerSecond);
            handedToUi = true;
            cameraBitmap = null;
        }
        catch (Exception exception)
        {
            _logger.Error($"Error procesando el fotograma: {exception.Message}");
            Interlocked.Exchange(ref _frameProcessing, 0);
        }
        finally
        {
            cameraBitmap?.Dispose();
            if (!handedToUi)
            {
                Interlocked.Exchange(ref _frameProcessing, 0);
            }
        }
    }

    private void QueueFrameForUi(
        Bitmap cameraBitmap,
        Bitmap? debugBitmap,
        VisionSummary? summary,
        double framesPerSecond)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            cameraBitmap.Dispose();
            debugBitmap?.Dispose();
            Interlocked.Exchange(ref _frameProcessing, 0);
            return;
        }

        try
        {
            BeginInvoke(new Action(() =>
            {
                try
                {
                    SetPictureImage(_cameraPictureBox, cameraBitmap);
                    _cameraPlaceholderPanel.Visible = false;
                    if (debugBitmap is not null)
                    {
                        SetPictureImage(_debugPictureBox, debugBitmap);
                        _debugPlaceholderPanel.Visible = false;
                    }

                    if (summary is not null)
                    {
                        UpdateResult(summary);
                    }

                    _fpsStatus.Text = $"FPS: {framesPerSecond:F1}";
                }
                finally
                {
                    Interlocked.Exchange(ref _frameProcessing, 0);
                }
            }));
        }
        catch (InvalidOperationException)
        {
            cameraBitmap.Dispose();
            debugBitmap?.Dispose();
            Interlocked.Exchange(ref _frameProcessing, 0);
        }
    }

    private void UpdateResult(VisionSummary summary)
    {
        _detectedColorValue.Text = ToSpanish(summary.Color);
        _colorPercentagesValue.Text = $"V {summary.GreenPercentage:F1}% / R {summary.RedPercentage:F1}%";
        _detectedShapeValue.Text = ToSpanish(summary.Shape);
        _shapeMetricsValue.Text = $"Vértices {summary.Vertices} / C {summary.Circularity:F2}";
        _reasonValue.Text = summary.Reason;

        (_decisionValue.Text, _decisionValue.ForeColor) = summary.Decision switch
        {
            InspectionDecision.Good => ("ACEPTADA", Color.FromArgb(61, 220, 132)),
            InspectionDecision.Reject => ("RECHAZADA", Color.FromArgb(255, 91, 91)),
            _ => ("INDETERMINADA", Color.FromArgb(255, 205, 70))
        };

        string? inspectionId = summary.InspectionId;
        if (inspectionId is not null)
        {
            _cameraStatus.Text = $"Cámara: caja {inspectionId} capturada y validada";
            PersistSafely(
                () => _productionRepository?.AddInspection(summary, _currentUser.Id),
                "clasificación");
        }

        if (inspectionId is not null && _sicaviController.IsConnected)
        {
            string result = summary.Decision switch
            {
                InspectionDecision.Good => "GOOD",
                InspectionDecision.Reject => "REJECT",
                _ => "UNKNOWN"
            };

            ExecuteSerialCommand(
                () => _sicaviController.SendVisionResult(inspectionId, result),
                $"Resultado {TranslateInspectionResult(result)} enviado al STM32 para la caja {inspectionId}.");
        }
        else if (inspectionId is not null)
        {
            _logger.Warning($"No se pudo enviar el resultado de la caja {inspectionId}: STM32 desconectado.");
        }
    }

    private void PersistSafely(Action action, string operation)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            _logger.Error($"No se pudo registrar {operation} en SQL Server: {exception.Message}");
        }
    }

    private async void MainForm_FormClosing(object? sender, FormClosingEventArgs eventArgs)
    {
        if (_allowClose)
        {
            return;
        }

        eventArgs.Cancel = true;
        Enabled = false;

        try
        {
            _serialReconnectTimer?.Stop();
            _serialReconnectTimer?.Dispose();
            _serialReconnectTimer = null;

            if (_sicaviController.IsConnected)
            {
                try
                {
                    _sicaviController.Stop();
                    _logger.Info("DETENCIÓN de seguridad enviada antes de cerrar la sesión o salir de SICAVI.");
                }
                catch (Exception exception)
                {
                    _logger.Warning($"No se pudo enviar la DETENCIÓN durante el cierre: {exception.Message}");
                }
            }

            await _cameraService.StopAsync();
            _cameraService.Dispose();
            _sicaviController.Dispose();

            _cameraPictureBox.Image?.Dispose();
            _debugPictureBox.Image?.Dispose();
        }
        finally
        {
            _allowClose = true;
            Close();
        }
    }

    private void SafeBeginInvoke(Action action)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
        catch (InvalidOperationException)
        {
            // El formulario está cerrándose; no se actualiza la interfaz.
        }
    }

    private void SetSerialControlButtons(bool connected)
    {
        _runSystemButton.Enabled = connected;
        _stopSystemButton.Enabled = connected;
        _resetSystemButton.Enabled = connected;
    }

    private static VisionSummary CreateSummary(VisionAnalysisResult result, string? inspectionId = null) => new(
        result.Color,
        result.GreenPercentage,
        result.RedPercentage,
        result.Shape,
        result.Vertices,
        result.Circularity,
        result.Decision,
        result.Reason,
        inspectionId);

    private sealed record VisionCandidate(
        Bitmap CameraBitmap,
        Bitmap DebugBitmap,
        VisionSummary Summary) : IDisposable
    {
        public void Dispose()
        {
            CameraBitmap.Dispose();
            DebugBitmap.Dispose();
        }
    }

    private static void SetPictureImage(PictureBox pictureBox, Image image)
    {
        Image? previous = pictureBox.Image;
        pictureBox.Image = image;
        previous?.Dispose();
    }

    private static string ToSpanish(DetectedColor color) => color switch
    {
        DetectedColor.Green => "Verde",
        DetectedColor.Red => "Rojo",
        _ => "Desconocido"
    };

    private static string ToSpanish(DetectedShape shape) => shape switch
    {
        DetectedShape.Circle => "Círculo",
        DetectedShape.Square => "Cuadrado",
        DetectedShape.Triangle => "Triángulo",
        _ => "Desconocida"
    };

    private static string TranslateSystemState(string state) => state.ToUpperInvariant() switch
    {
        "STOPPED" => "PARADA",
        "TRANSPORTING" => "TRANSPORTANDO",
        "POSITIONING" => "POSICIONANDO CAJA",
        "STABILIZING" => "ESTABILIZANDO",
        "WAITING_RESULT" => "ESPERANDO RESULTADO",
        "PASSING" => "ACEPTADA · AVANZANDO",
        "REJECTING" => "RECHAZADA · EXPULSANDO",
        "ALARM" => "ALARMA",
        _ => "ESTADO DESCONOCIDO"
    };

    private static string TranslateInspectionResult(string result) => result.ToUpperInvariant() switch
    {
        "GOOD" or "BUENA" or "ACEPTADA" => "ACEPTADA",
        "REJECT" or "MALA" or "RECHAZADA" => "RECHAZADA",
        _ => "INDETERMINADA"
    };

    private static string TranslateAlarm(string code) => code.ToUpperInvariant() switch
    {
        "NONE" => "NINGUNA",
        "UART_ERROR" => "ERROR DE COMUNICACIÓN UART",
        "ADC_ERROR" => "ERROR DEL SENSOR DE LUZ",
        "LIGHT_OUT_OF_RANGE" => "ILUMINACIÓN FUERA DE RANGO",
        "VISION_TIMEOUT" => "TIEMPO DE VISIÓN AGOTADO",
        "COMM_TIMEOUT" => "COMUNICACIÓN CON PC PERDIDA",
        "VISION_UNKNOWN" => "VISIÓN INDETERMINADA",
        _ => code.Replace('_', ' ').ToUpperInvariant()
    };

    private static string TranslateCommandError(string code) => code.ToUpperInvariant() switch
    {
        "RESET_REQUIRED" => "Existe una alarma retenida; pulsa REINICIAR y espera su confirmación",
        "BOX_SENSOR_ACTIVE" => "El sensor D8/PA9 está activo. Retira la caja del sensor o revisa si la señal permanece en 0 V",
        "LIGHT_OUT_OF_RANGE" => "La iluminación está fuera del rango permitido por el LDR",
        "NOT_WAITING" => "El STM32 ya no está esperando ese resultado de visión",
        "ID_MISMATCH" => "El identificador de la caja no coincide con el ciclo activo",
        "UNKNOWN" => "El STM32 rechazó el comando sin indicar una causa conocida",
        _ => code.Replace('_', ' ').ToUpperInvariant()
    };
}
