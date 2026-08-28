using System.IO.Ports;
using System.Text;

namespace Sicavi.WinForms.Communication;

public sealed class SerialConnectionService : IDisposable
{
    private readonly object _sync = new();
    private readonly StringBuilder _receiveBuffer = new();
    private SerialPort? _serialPort;

    public event EventHandler<string>? LineReceived;
    public event EventHandler<bool>? ConnectionChanged;
    public event EventHandler<string>? CommunicationError;

    public bool IsConnected => _serialPort?.IsOpen == true;

    public void Connect(string portName, int baudRate = 115200)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            throw new ArgumentException("Selecciona un puerto COM.", nameof(portName));
        }

        Disconnect();

        var port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            NewLine = "\n",
            ReadTimeout = 500,
            WriteTimeout = 500,
            DtrEnable = false,
            RtsEnable = false
        };

        port.DataReceived += SerialPort_DataReceived;
        port.ErrorReceived += SerialPort_ErrorReceived;

        try
        {
            port.Open();
            /*
             * El ST-LINK VCP puede conservar telemetria de una ejecucion
             * anterior mientras COM6 esta cerrado. No debe mezclarse con el
             * primer ACK de la sesion nueva.
             */
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
        }
        catch
        {
            port.DataReceived -= SerialPort_DataReceived;
            port.ErrorReceived -= SerialPort_ErrorReceived;
            port.Dispose();
            throw;
        }

        lock (_sync)
        {
            _receiveBuffer.Clear();
            _serialPort = port;
        }

        ConnectionChanged?.Invoke(this, true);
    }

    public void Disconnect()
    {
        SerialPort? port;

        lock (_sync)
        {
            port = _serialPort;
            _serialPort = null;
            _receiveBuffer.Clear();
        }

        if (port is null)
        {
            return;
        }

        port.DataReceived -= SerialPort_DataReceived;
        port.ErrorReceived -= SerialPort_ErrorReceived;

        try
        {
            if (port.IsOpen)
            {
                port.Close();
            }
        }
        finally
        {
            port.Dispose();
            ConnectionChanged?.Invoke(this, false);
        }
    }

    public void SendLine(string line)
    {
        lock (_sync)
        {
            SerialPort? port = _serialPort;
            if (port is null || !port.IsOpen)
            {
                throw new InvalidOperationException("El STM32 no está conectado.");
            }

            /* Evita que el latido y un comando de la interfaz se intercalen. */
            port.Write(line.TrimEnd('\r', '\n') + "\r\n");
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            var port = (SerialPort)sender;
            string received = port.ReadExisting();
            var completedLines = new List<string>();

            lock (_sync)
            {
                _receiveBuffer.Append(received);

                while (true)
                {
                    string text = _receiveBuffer.ToString();
                    int newLineIndex = text.IndexOf('\n');
                    if (newLineIndex < 0)
                    {
                        break;
                    }

                    string line = text[..newLineIndex].TrimEnd('\r');
                    _receiveBuffer.Remove(0, newLineIndex + 1);
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        completedLines.Add(line);
                    }
                }
            }

            foreach (string line in completedLines)
            {
                LineReceived?.Invoke(this, line);
            }
        }
        catch (Exception exception)
        {
            CommunicationError?.Invoke(this, exception.Message);
        }
    }

    private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) =>
        CommunicationError?.Invoke(this, $"Error del puerto serial: {e.EventType}");

    public void Dispose() => Disconnect();
}
