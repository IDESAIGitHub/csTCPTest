using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPZebra_test;
using CabecerasLib;

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class TcpClientCam_ModbusFC
{
    //private string serverIp = "169.254.222.69"; // Zebra
    private string serverIp = "169.254.163.107"; // HikRobot
    //private string serverIp = "localhost";
    
    //private int _dataPort = 420; // Zebra
    //private int _triggerPort = 107; // Zebra

    private int _dataPort = 2002; // HikRobot
    private int _triggerPort = 2001; // HikRobot
    private string _messagestart = "start";
    private string _messagestop = "stop";

    private TcpClient _dataClient;
    private NetworkStream _dataStream;
    
    private TcpClient _triggerClient;
    private NetworkStream _triggetSteam;
    

    public event EventHandler<string> MessageReceived;

    public void connectModbus()
    {
        List<string> listErrorCabeceras = new List<string>();
        Cabeceras.CreateCabecerasFromINI("FicherosINI\\Cabeceras.ini", ref listErrorCabeceras);
        Cabeceras.ConnectCabeceras(ref listErrorCabeceras);
    }
    private Thread threadReadLoop;
    public async Task ConnectAsync()
    {
        connectModbus();
        bool connected = false;
        try
        {
            _dataClient = new TcpClient(); 
            Console.WriteLine("Connecting with server");
            await _dataClient.ConnectAsync(serverIp, _dataPort);
            _dataStream = _dataClient.GetStream();

            Console.WriteLine("Conection with data server established!");

            if (_dataClient.Connected)
            {
                // Start a separate thread to read messages from the server
                Task.Run(ReceiveMessages);
                connected = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Conection with data server Error: " + ex.Message);
            connected = false;
        }
        try
        {
            _triggerClient = new TcpClient();
            await _triggerClient.ConnectAsync(serverIp, _triggerPort);
            _triggetSteam = _triggerClient.GetStream();
            connected = true;
            Console.WriteLine("Conection with trigger server established!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Conection with trigger server Error: " + ex.Message);
            connected = false;
        }
        if (connected)
        {
            //start thread read loop
            threadReadLoop = new Thread(readLoop);
            threadReadLoop.Start();
        }
    }
    public bool EstadoAnteriorFC = false;
    public bool EstadoActualFC = false;
    public bool activo = true;
    private void readLoop()
    {
        while (activo)
        {
            EstadoActualFC = Cabeceras.ListEntradasGlobal[2].Value;
            //if (EstadoActualFC != EstadoAnteriorFC)
            //{
            //    if (EstadoActualFC)
            //    {
            //        SendStartTrigger();
            //        Console.WriteLine("Trigger Start");
            //    }
            //    EstadoAnteriorFC = EstadoActualFC;
            //}
            if (EstadoActualFC)
            {
                SendStartTrigger();
                Console.WriteLine("Trigger Start");
            }
            Thread.Sleep(10);
        }
    }
    public void SendStopTrigger()
    {
        if (_dataClient.Connected)
        {
            if (_triggetSteam != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(_messagestop);
                _triggetSteam.Write(data, 0, data.Length);
            }
        }
    }
    public void SendStartTrigger()
    {
        if (_dataClient.Connected)
        {
            if (_triggetSteam != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(_messagestart);
                _triggetSteam.Write(data, 0, data.Length);
            }
        }
    }
    private async Task ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (_dataClient.Connected)
        {
            int bytesRead = await _dataStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                MessageReceived?.Invoke(this, receivedMessage);
            }
            Thread.Sleep(50);
        }
    }

    public void Disconnect()
    {
        _dataStream?.Close();
        _dataClient?.Close();
        activo = false;
        Cabeceras.FinalizeCabeceras();
    }
}