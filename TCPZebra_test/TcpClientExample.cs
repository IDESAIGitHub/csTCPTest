using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPZebra_test;

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class TcpClientExample
{
    private string serverIp = "169.254.120.64"; // Replace with the server's IP address
    private int _dataPort = 69; // Replace with the server's port
    private TcpClient _dataClient;
    private NetworkStream _dataStream;
    private int _triggerPort = 107; // Replace with the server's port
    private TcpClient _triggerClient;
    private NetworkStream _triggetSteam;
    

    public event EventHandler<string> MessageReceived;

    public async Task ConnectAsync()
    {
        try
        {
            _dataClient = new TcpClient();
            await _dataClient.ConnectAsync(serverIp, _dataPort);
            _dataStream = _dataClient.GetStream();

            // Start a separate thread to read messages from the server
            Task.Run(ReceiveMessages);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        try
        {
            _triggerClient = new TcpClient();
            await _triggerClient.ConnectAsync(serverIp, _triggerPort);
            _triggetSteam = _triggerClient.GetStream();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

    }

    public void Send(string message)
    {
        if (_triggetSteam != null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _triggetSteam.Write(data, 0, data.Length);
        }
    }

    private async Task ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int bytesRead = await _dataStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                MessageReceived?.Invoke(this, receivedMessage);
            }
            Thread.Sleep(100);
        }
    }

    public void Disconnect()
    {
        _dataStream?.Close();
        _dataClient?.Close();
    }
}