#nullable enable

namespace Sicavi.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private ComboBox _expectedColorComboBox = null!;
    private ComboBox _expectedShapeComboBox = null!;
    private PictureBox _cameraPictureBox = null!;
    private PictureBox _debugPictureBox = null!;
    private Label _detectedColorValue = null!;
    private Label _colorPercentagesValue = null!;
    private Label _detectedShapeValue = null!;
    private Label _shapeMetricsValue = null!;
    private Label _decisionValue = null!;
    private Label _reasonValue = null!;
    private ToolStripStatusLabel _cameraStatus = null!;
    private ToolStripStatusLabel _serialStatus = null!;
    private ToolStripStatusLabel _fpsStatus = null!;
    private Button _runSystemButton = null!;
    private Button _stopSystemButton = null!;
    private Button _resetSystemButton = null!;
    private Label _systemStateValue = null!;
    private Label _ldrValue = null!;
    private Label _hourmeterValue = null!;
    private Label _ejectorCyclesValue = null!;
    private Label _alarmValue = null!;
    private Button _databaseButton = null!;
    private Button _usersButton = null!;
    private Button _logoutButton = null!;
    private Label _currentUserLabel = null!;
    private Panel _cameraPlaceholderPanel = null!;
    private Panel _debugPlaceholderPanel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var root = new TableLayoutPanel();
        var header = new Panel();
        var brandLabel = new Label();
        var moduleLabel = new Label();
        _currentUserLabel = new Label();
        _databaseButton = new Button();
        _usersButton = new Button();
        _logoutButton = new Button();
        var content = new TableLayoutPanel();
        var controlPanel = new Panel();
        var navigationTitle = new Label();
        var criteriaTitle = new Label();
        var expectedColorLabel = new Label();
        _expectedColorComboBox = new ComboBox();
        var expectedShapeLabel = new Label();
        _expectedShapeComboBox = new ComboBox();
        var resultTitle = new Label();
        var detectedColorLabel = new Label();
        _detectedColorValue = new Label();
        var percentagesLabel = new Label();
        _colorPercentagesValue = new Label();
        var detectedShapeLabel = new Label();
        _detectedShapeValue = new Label();
        var metricsLabel = new Label();
        _shapeMetricsValue = new Label();
        _decisionValue = new Label();
        _reasonValue = new Label();
        var controlTitle = new Label();
        var serialLabel = new Label();
        _runSystemButton = new Button();
        _stopSystemButton = new Button();
        _resetSystemButton = new Button();
        var stateLabel = new Label();
        _systemStateValue = new Label();
        var ldrLabel = new Label();
        _ldrValue = new Label();
        var hourmeterLabel = new Label();
        _hourmeterValue = new Label();
        var cyclesLabel = new Label();
        _ejectorCyclesValue = new Label();
        var alarmLabel = new Label();
        _alarmValue = new Label();
        var settingsHint = new Label();
        var visionPanel = new TableLayoutPanel();
        var cameraTitle = new Label();
        var debugTitle = new Label();
        var cameraHost = new Panel();
        var debugHost = new Panel();
        _cameraPictureBox = new PictureBox();
        _debugPictureBox = new PictureBox();
        _cameraPlaceholderPanel = new Panel();
        _debugPlaceholderPanel = new Panel();
        var cameraPlaceholderLayout = new TableLayoutPanel();
        var cameraPlaceholderTitle = new Label();
        var cameraPlaceholderSubtitle = new Label();
        var debugPlaceholderLayout = new TableLayoutPanel();
        var debugPlaceholderTitle = new Label();
        var debugPlaceholderSubtitle = new Label();
        var statusStrip = new StatusStrip();
        _cameraStatus = new ToolStripStatusLabel();
        _serialStatus = new ToolStripStatusLabel();
        var statusSpacer = new ToolStripStatusLabel();
        _fpsStatus = new ToolStripStatusLabel();
        root.SuspendLayout();
        header.SuspendLayout();
        content.SuspendLayout();
        controlPanel.SuspendLayout();
        visionPanel.SuspendLayout();
        cameraHost.SuspendLayout();
        debugHost.SuspendLayout();
        _cameraPlaceholderPanel.SuspendLayout();
        _debugPlaceholderPanel.SuspendLayout();
        cameraPlaceholderLayout.SuspendLayout();
        debugPlaceholderLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_cameraPictureBox).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_debugPictureBox).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // root
        root.BackColor = Color.FromArgb(16, 23, 32);
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(content, 0, 1);
        root.Controls.Add(statusStrip, 0, 2);
        root.Dock = DockStyle.Fill;
        root.RowCount = 3;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
        // header
        header.BackColor = Color.FromArgb(24, 35, 48);
        header.Controls.Add(brandLabel);
        header.Controls.Add(moduleLabel);
        header.Controls.Add(_currentUserLabel);
        header.Dock = DockStyle.Fill;
        brandLabel.AutoSize = true; brandLabel.Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold); brandLabel.ForeColor = Color.FromArgb(39, 203, 221); brandLabel.Location = new Point(20, 8); brandLabel.Text = "SICAVI";
        moduleLabel.AutoSize = true; moduleLabel.ForeColor = Color.FromArgb(160, 176, 193); moduleLabel.Location = new Point(23, 51); moduleLabel.Text = "INSPECCIÓN VISUAL Y CONTROL DE PLANTA";
        _currentUserLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right; _currentUserLabel.ForeColor = Color.FromArgb(194, 207, 220); _currentUserLabel.Location = new Point(700, 30); _currentUserLabel.Size = new Size(758, 30); _currentUserLabel.Text = "Usuario de diseño · Administrador"; _currentUserLabel.TextAlign = ContentAlignment.MiddleRight;
        // content
        content.BackColor = Color.FromArgb(16, 23, 32); content.ColumnCount = 2; content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 370F)); content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); content.Controls.Add(controlPanel, 0, 0); content.Controls.Add(visionPanel, 1, 0); content.Dock = DockStyle.Fill; content.RowCount = 1; content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // controlPanel
        controlPanel.AutoScroll = true; controlPanel.BackColor = Color.FromArgb(27, 39, 52); controlPanel.Controls.Add(navigationTitle); controlPanel.Controls.Add(_databaseButton); controlPanel.Controls.Add(_usersButton); controlPanel.Controls.Add(_logoutButton); controlPanel.Controls.Add(criteriaTitle); controlPanel.Controls.Add(expectedColorLabel); controlPanel.Controls.Add(_expectedColorComboBox); controlPanel.Controls.Add(expectedShapeLabel); controlPanel.Controls.Add(_expectedShapeComboBox); controlPanel.Controls.Add(resultTitle); controlPanel.Controls.Add(detectedColorLabel); controlPanel.Controls.Add(_detectedColorValue); controlPanel.Controls.Add(percentagesLabel); controlPanel.Controls.Add(_colorPercentagesValue); controlPanel.Controls.Add(detectedShapeLabel); controlPanel.Controls.Add(_detectedShapeValue); controlPanel.Controls.Add(metricsLabel); controlPanel.Controls.Add(_shapeMetricsValue); controlPanel.Controls.Add(_decisionValue); controlPanel.Controls.Add(_reasonValue); controlPanel.Controls.Add(controlTitle); controlPanel.Controls.Add(serialLabel); controlPanel.Controls.Add(_runSystemButton); controlPanel.Controls.Add(_stopSystemButton); controlPanel.Controls.Add(_resetSystemButton); controlPanel.Controls.Add(stateLabel); controlPanel.Controls.Add(_systemStateValue); controlPanel.Controls.Add(ldrLabel); controlPanel.Controls.Add(_ldrValue); controlPanel.Controls.Add(hourmeterLabel); controlPanel.Controls.Add(_hourmeterValue); controlPanel.Controls.Add(cyclesLabel); controlPanel.Controls.Add(_ejectorCyclesValue); controlPanel.Controls.Add(alarmLabel); controlPanel.Controls.Add(_alarmValue); controlPanel.Controls.Add(settingsHint); controlPanel.Dock = DockStyle.Fill;
        // navigation
        navigationTitle.AutoSize = true; navigationTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold); navigationTitle.ForeColor = Color.FromArgb(39, 203, 221); navigationTitle.Location = new Point(20, 18); navigationTitle.Text = "01 · ACCESOS";
        _databaseButton.BackColor = Color.FromArgb(15, 151, 166); _databaseButton.FlatAppearance.BorderSize = 0; _databaseButton.FlatStyle = FlatStyle.Flat; _databaseButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold); _databaseButton.ForeColor = Color.White; _databaseButton.Location = new Point(20, 48); _databaseButton.Size = new Size(326, 34); _databaseButton.Text = "DATOS DE PRODUCCIÓN"; _databaseButton.UseVisualStyleBackColor = false;
        _usersButton.BackColor = Color.FromArgb(57, 75, 94); _usersButton.FlatAppearance.BorderSize = 0; _usersButton.FlatStyle = FlatStyle.Flat; _usersButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold); _usersButton.ForeColor = Color.White; _usersButton.Location = new Point(20, 89); _usersButton.Size = new Size(326, 34); _usersButton.Text = "GESTIÓN DE USUARIOS"; _usersButton.UseVisualStyleBackColor = false;
        _logoutButton.BackColor = Color.FromArgb(119, 55, 66); _logoutButton.FlatAppearance.BorderSize = 0; _logoutButton.FlatStyle = FlatStyle.Flat; _logoutButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold); _logoutButton.ForeColor = Color.White; _logoutButton.Location = new Point(20, 130); _logoutButton.Size = new Size(326, 34); _logoutButton.Text = "CERRAR SESIÓN"; _logoutButton.UseVisualStyleBackColor = false;
        // expected criteria
        criteriaTitle.AutoSize = true; criteriaTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold); criteriaTitle.ForeColor = Color.FromArgb(39, 203, 221); criteriaTitle.Location = new Point(20, 188); criteriaTitle.Text = "02 · CRITERIOS ESPERADOS";
        expectedColorLabel.AutoSize = true; expectedColorLabel.ForeColor = Color.FromArgb(167, 183, 199); expectedColorLabel.Location = new Point(20, 223); expectedColorLabel.Text = "Color esperado";
        _expectedColorComboBox.BackColor = Color.FromArgb(13, 20, 28); _expectedColorComboBox.DropDownStyle = ComboBoxStyle.DropDownList; _expectedColorComboBox.FlatStyle = FlatStyle.Flat; _expectedColorComboBox.ForeColor = Color.White; _expectedColorComboBox.Items.AddRange(new object[] { "Cualquiera", "Verde", "Rojo" }); _expectedColorComboBox.Location = new Point(20, 245); _expectedColorComboBox.SelectedIndex = 0; _expectedColorComboBox.Size = new Size(326, 28);
        expectedShapeLabel.AutoSize = true; expectedShapeLabel.ForeColor = Color.FromArgb(167, 183, 199); expectedShapeLabel.Location = new Point(20, 284); expectedShapeLabel.Text = "Forma esperada";
        _expectedShapeComboBox.BackColor = Color.FromArgb(13, 20, 28); _expectedShapeComboBox.DropDownStyle = ComboBoxStyle.DropDownList; _expectedShapeComboBox.FlatStyle = FlatStyle.Flat; _expectedShapeComboBox.ForeColor = Color.White; _expectedShapeComboBox.Items.AddRange(new object[] { "Cualquiera", "Círculo", "Cuadrado", "Triángulo" }); _expectedShapeComboBox.Location = new Point(20, 306); _expectedShapeComboBox.SelectedIndex = 0; _expectedShapeComboBox.Size = new Size(326, 28);
        // result
        resultTitle.AutoSize = true; resultTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold); resultTitle.ForeColor = Color.FromArgb(39, 203, 221); resultTitle.Location = new Point(20, 357); resultTitle.Text = "03 · RESULTADO";
        detectedColorLabel.AutoSize = true; detectedColorLabel.ForeColor = Color.FromArgb(154, 171, 188); detectedColorLabel.Location = new Point(20, 394); detectedColorLabel.Text = "Color";
        _detectedColorValue.ForeColor = Color.White; _detectedColorValue.Location = new Point(145, 394); _detectedColorValue.Size = new Size(201, 22); _detectedColorValue.Text = "--"; _detectedColorValue.TextAlign = ContentAlignment.TopRight;
        percentagesLabel.AutoSize = true; percentagesLabel.ForeColor = Color.FromArgb(154, 171, 188); percentagesLabel.Location = new Point(20, 422); percentagesLabel.Text = "HSV";
        _colorPercentagesValue.ForeColor = Color.White; _colorPercentagesValue.Location = new Point(110, 422); _colorPercentagesValue.Size = new Size(236, 22); _colorPercentagesValue.Text = "V 0.0% / R 0.0%"; _colorPercentagesValue.TextAlign = ContentAlignment.TopRight;
        detectedShapeLabel.AutoSize = true; detectedShapeLabel.ForeColor = Color.FromArgb(154, 171, 188); detectedShapeLabel.Location = new Point(20, 450); detectedShapeLabel.Text = "Forma";
        _detectedShapeValue.ForeColor = Color.White; _detectedShapeValue.Location = new Point(145, 450); _detectedShapeValue.Size = new Size(201, 22); _detectedShapeValue.Text = "--"; _detectedShapeValue.TextAlign = ContentAlignment.TopRight;
        metricsLabel.AutoSize = true; metricsLabel.ForeColor = Color.FromArgb(154, 171, 188); metricsLabel.Location = new Point(20, 478); metricsLabel.Text = "Métricas";
        _shapeMetricsValue.ForeColor = Color.White; _shapeMetricsValue.Location = new Point(100, 478); _shapeMetricsValue.Size = new Size(246, 22); _shapeMetricsValue.Text = "Vértices 0 / C 0.00"; _shapeMetricsValue.TextAlign = ContentAlignment.TopRight;
        _decisionValue.BackColor = Color.FromArgb(13, 20, 28); _decisionValue.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold); _decisionValue.ForeColor = Color.FromArgb(160, 176, 193); _decisionValue.Location = new Point(20, 512); _decisionValue.Size = new Size(326, 39); _decisionValue.Text = "SIN CLASIFICACIÓN"; _decisionValue.TextAlign = ContentAlignment.MiddleCenter;
        _reasonValue.AutoEllipsis = true; _reasonValue.ForeColor = Color.FromArgb(160, 176, 193); _reasonValue.Location = new Point(20, 562); _reasonValue.Size = new Size(326, 42); _reasonValue.Text = "Esperando la próxima caja.";
        // STM32
        controlTitle.AutoSize = true; controlTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold); controlTitle.ForeColor = Color.FromArgb(39, 203, 221); controlTitle.Location = new Point(20, 620); controlTitle.Text = "04 · CONTROL STM32";
        serialLabel.AutoSize = true; serialLabel.ForeColor = Color.FromArgb(167, 183, 199); serialLabel.Location = new Point(20, 655); serialLabel.Text = "COM6 automático · 115200 baudios · Cámara 0";
        _runSystemButton.BackColor = Color.FromArgb(43, 130, 102); _runSystemButton.Enabled = false; _runSystemButton.FlatAppearance.BorderSize = 0; _runSystemButton.FlatStyle = FlatStyle.Flat; _runSystemButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold); _runSystemButton.ForeColor = Color.White; _runSystemButton.Location = new Point(20, 684); _runSystemButton.Size = new Size(101, 40); _runSystemButton.Text = "INICIAR"; _runSystemButton.UseVisualStyleBackColor = false;
        _stopSystemButton.BackColor = Color.FromArgb(190, 61, 67); _stopSystemButton.Enabled = false; _stopSystemButton.FlatAppearance.BorderSize = 0; _stopSystemButton.FlatStyle = FlatStyle.Flat; _stopSystemButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold); _stopSystemButton.ForeColor = Color.White; _stopSystemButton.Location = new Point(132, 684); _stopSystemButton.Size = new Size(101, 40); _stopSystemButton.Text = "DETENER"; _stopSystemButton.UseVisualStyleBackColor = false;
        _resetSystemButton.BackColor = Color.FromArgb(207, 147, 38); _resetSystemButton.Enabled = false; _resetSystemButton.FlatAppearance.BorderSize = 0; _resetSystemButton.FlatStyle = FlatStyle.Flat; _resetSystemButton.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold); _resetSystemButton.ForeColor = Color.FromArgb(20, 25, 30); _resetSystemButton.Location = new Point(244, 684); _resetSystemButton.Size = new Size(102, 40); _resetSystemButton.Text = "REINICIAR"; _resetSystemButton.UseVisualStyleBackColor = false;
        stateLabel.AutoSize = true; stateLabel.ForeColor = Color.FromArgb(154, 171, 188); stateLabel.Location = new Point(20, 747); stateLabel.Text = "Estado";
        _systemStateValue.ForeColor = Color.White; _systemStateValue.Location = new Point(118, 747); _systemStateValue.Size = new Size(228, 22); _systemStateValue.Text = "SIN CONEXIÓN"; _systemStateValue.TextAlign = ContentAlignment.TopRight;
        ldrLabel.AutoSize = true; ldrLabel.ForeColor = Color.FromArgb(154, 171, 188); ldrLabel.Location = new Point(20, 775); ldrLabel.Text = "Sensor de luz";
        _ldrValue.ForeColor = Color.White; _ldrValue.Location = new Point(112, 775); _ldrValue.Size = new Size(234, 22); _ldrValue.Text = "--"; _ldrValue.TextAlign = ContentAlignment.TopRight;
        hourmeterLabel.AutoSize = true; hourmeterLabel.ForeColor = Color.FromArgb(154, 171, 188); hourmeterLabel.Location = new Point(20, 803); hourmeterLabel.Text = "Horómetro";
        _hourmeterValue.Font = new Font("Consolas", 9.5F, FontStyle.Bold); _hourmeterValue.ForeColor = Color.FromArgb(39, 203, 221); _hourmeterValue.Location = new Point(160, 803); _hourmeterValue.Size = new Size(186, 22); _hourmeterValue.Text = "00:00:00"; _hourmeterValue.TextAlign = ContentAlignment.TopRight;
        cyclesLabel.AutoSize = true; cyclesLabel.ForeColor = Color.FromArgb(154, 171, 188); cyclesLabel.Location = new Point(20, 831); cyclesLabel.Text = "Ciclos del expulsor";
        _ejectorCyclesValue.ForeColor = Color.White; _ejectorCyclesValue.Location = new Point(160, 831); _ejectorCyclesValue.Size = new Size(186, 22); _ejectorCyclesValue.Text = "0"; _ejectorCyclesValue.TextAlign = ContentAlignment.TopRight;
        alarmLabel.AutoSize = true; alarmLabel.ForeColor = Color.FromArgb(154, 171, 188); alarmLabel.Location = new Point(20, 859); alarmLabel.Text = "Alarma";
        _alarmValue.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold); _alarmValue.ForeColor = Color.FromArgb(255, 194, 87); _alarmValue.Location = new Point(118, 859); _alarmValue.Size = new Size(228, 42); _alarmValue.Text = "NINGUNA"; _alarmValue.TextAlign = ContentAlignment.TopRight;
        settingsHint.ForeColor = Color.FromArgb(126, 144, 162); settingsHint.Location = new Point(20, 914); settingsHint.Size = new Size(326, 40); settingsHint.Text = "Conexiones fijas: COM6 y cámara 0.\r\nLos registros se almacenan en SQL Server.";
        // vision panel
        visionPanel.BackColor = Color.FromArgb(16, 23, 32); visionPanel.ColumnCount = 2; visionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 61F)); visionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39F)); visionPanel.Controls.Add(cameraTitle, 0, 0); visionPanel.Controls.Add(debugTitle, 1, 0); visionPanel.Controls.Add(cameraHost, 0, 1); visionPanel.Controls.Add(debugHost, 1, 1); visionPanel.Dock = DockStyle.Fill; visionPanel.Padding = new Padding(14); visionPanel.RowCount = 2; visionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); visionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        cameraTitle.Dock = DockStyle.Fill; cameraTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold); cameraTitle.ForeColor = Color.FromArgb(190, 205, 219); cameraTitle.Text = "VISTA DE CÁMARA / ROI"; cameraTitle.TextAlign = ContentAlignment.MiddleLeft;
        debugTitle.Dock = DockStyle.Fill; debugTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold); debugTitle.ForeColor = Color.FromArgb(190, 205, 219); debugTitle.Text = "MÁSCARA HSV + CONTORNO"; debugTitle.TextAlign = ContentAlignment.MiddleLeft;
        cameraHost.BackColor = Color.FromArgb(7, 12, 17); cameraHost.BorderStyle = BorderStyle.FixedSingle; cameraHost.Controls.Add(_cameraPictureBox); cameraHost.Controls.Add(_cameraPlaceholderPanel); cameraHost.Dock = DockStyle.Fill; cameraHost.Margin = new Padding(0, 0, 10, 0);
        debugHost.BackColor = Color.FromArgb(7, 12, 17); debugHost.BorderStyle = BorderStyle.FixedSingle; debugHost.Controls.Add(_debugPictureBox); debugHost.Controls.Add(_debugPlaceholderPanel); debugHost.Dock = DockStyle.Fill; debugHost.Margin = new Padding(10, 0, 0, 0);
        _cameraPictureBox.BackColor = Color.FromArgb(7, 12, 17); _cameraPictureBox.Dock = DockStyle.Fill; _cameraPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        _debugPictureBox.BackColor = Color.FromArgb(7, 12, 17); _debugPictureBox.Dock = DockStyle.Fill; _debugPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        _cameraPlaceholderPanel.BackColor = Color.FromArgb(7, 12, 17); _cameraPlaceholderPanel.Controls.Add(cameraPlaceholderLayout); _cameraPlaceholderPanel.Dock = DockStyle.Fill;
        cameraPlaceholderLayout.ColumnCount = 1; cameraPlaceholderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); cameraPlaceholderLayout.Controls.Add(cameraPlaceholderTitle, 0, 1); cameraPlaceholderLayout.Controls.Add(cameraPlaceholderSubtitle, 0, 2); cameraPlaceholderLayout.Dock = DockStyle.Fill; cameraPlaceholderLayout.RowCount = 4; cameraPlaceholderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); cameraPlaceholderLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F)); cameraPlaceholderLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F)); cameraPlaceholderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        cameraPlaceholderTitle.Dock = DockStyle.Fill; cameraPlaceholderTitle.Font = new Font("Segoe UI Semibold", 25F, FontStyle.Bold); cameraPlaceholderTitle.ForeColor = Color.FromArgb(39, 203, 221); cameraPlaceholderTitle.Text = "CÁMARA DESCONECTADA"; cameraPlaceholderTitle.TextAlign = ContentAlignment.MiddleCenter;
        cameraPlaceholderSubtitle.Dock = DockStyle.Fill; cameraPlaceholderSubtitle.Font = new Font("Segoe UI", 13F); cameraPlaceholderSubtitle.ForeColor = Color.FromArgb(160, 176, 193); cameraPlaceholderSubtitle.Text = "Pulsa INICIAR para poner el sistema en marcha"; cameraPlaceholderSubtitle.TextAlign = ContentAlignment.TopCenter;
        _debugPlaceholderPanel.BackColor = Color.FromArgb(7, 12, 17); _debugPlaceholderPanel.Controls.Add(debugPlaceholderLayout); _debugPlaceholderPanel.Dock = DockStyle.Fill;
        debugPlaceholderLayout.ColumnCount = 1; debugPlaceholderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); debugPlaceholderLayout.Controls.Add(debugPlaceholderTitle, 0, 1); debugPlaceholderLayout.Controls.Add(debugPlaceholderSubtitle, 0, 2); debugPlaceholderLayout.Dock = DockStyle.Fill; debugPlaceholderLayout.RowCount = 4; debugPlaceholderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); debugPlaceholderLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F)); debugPlaceholderLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F)); debugPlaceholderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        debugPlaceholderTitle.Dock = DockStyle.Fill; debugPlaceholderTitle.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold); debugPlaceholderTitle.ForeColor = Color.FromArgb(39, 203, 221); debugPlaceholderTitle.Text = "ANÁLISIS"; debugPlaceholderTitle.TextAlign = ContentAlignment.MiddleCenter;
        debugPlaceholderSubtitle.Dock = DockStyle.Fill; debugPlaceholderSubtitle.ForeColor = Color.FromArgb(160, 176, 193); debugPlaceholderSubtitle.Text = "Aquí aparecerán la máscara HSV y los contornos"; debugPlaceholderSubtitle.TextAlign = ContentAlignment.TopCenter;
        _cameraPlaceholderPanel.BringToFront();
        _debugPlaceholderPanel.BringToFront();
        // status
        statusStrip.BackColor = Color.FromArgb(10, 16, 23); statusStrip.Dock = DockStyle.Fill; statusStrip.ForeColor = Color.FromArgb(210, 221, 231); statusStrip.Items.AddRange(new ToolStripItem[] { _cameraStatus, _serialStatus, statusSpacer, _fpsStatus }); statusStrip.SizingGrip = false;
        _cameraStatus.Text = "Cámara: desconectada"; _serialStatus.Margin = new Padding(24, 3, 0, 2); _serialStatus.Text = "STM32: desconectado"; statusSpacer.Spring = true; _fpsStatus.Text = "FPS: 0.0";
        // form
        AutoScaleDimensions = new SizeF(7F, 15F); AutoScaleMode = AutoScaleMode.Font; BackColor = Color.FromArgb(16, 23, 32); ClientSize = new Size(1480, 860); Controls.Add(root); Font = new Font("Segoe UI", 9F); ForeColor = Color.White; MinimumSize = new Size(1180, 720); StartPosition = FormStartPosition.CenterScreen; Text = "SICAVI - Visión y control de planta"; WindowState = FormWindowState.Maximized;
        root.ResumeLayout(false); root.PerformLayout(); header.ResumeLayout(false); header.PerformLayout(); content.ResumeLayout(false); controlPanel.ResumeLayout(false); controlPanel.PerformLayout(); visionPanel.ResumeLayout(false); cameraHost.ResumeLayout(false); debugHost.ResumeLayout(false); _cameraPlaceholderPanel.ResumeLayout(false); _debugPlaceholderPanel.ResumeLayout(false); cameraPlaceholderLayout.ResumeLayout(false); debugPlaceholderLayout.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)_cameraPictureBox).EndInit(); ((System.ComponentModel.ISupportInitialize)_debugPictureBox).EndInit(); statusStrip.ResumeLayout(false); statusStrip.PerformLayout(); ResumeLayout(false);
    }
}
