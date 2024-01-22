using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPZebra_test;





public partial class ClientForm : Form
{
    private TcpClientCam_ModbusFC _client;
    public ClientForm()
    {
        InitializeComponent();
    }
    private void BtnConnect_Click(object sender, EventArgs e)
    {
        _client = new TcpClientCam_ModbusFC();

        _client.ConnectAsync();

        _client.MessageReceived += Client_MessageReceived;
    }
    
    private void Client_MessageReceived(object? sender, string e)
    {
        Console.WriteLine("Received: " + e);
    }


    private void Form1_Load(object sender, EventArgs e)
    {

    }
    
    private void BtnSendTriggerStart_Click(object sender, EventArgs e)
    {
        _client?.SendStartTrigger();
    }

    private void BtnSendTriggerStop_Click(object sender, EventArgs e)
    {
        _client?.SendStopTrigger();
    }

    private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _client.Disconnect();
    }
}