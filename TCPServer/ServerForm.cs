using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPZebra_test;





public partial class ServerForm : Form
{
    private TcpListener _server = null;
    private TcpClient _client;

    //private TcpClientExample _client;
    public ServerForm()
    {
        InitializeComponent();

        //_client = new TcpClientExample();

        //_client.ConnectAsync();

        //_client.MessageReceived += Client_MessageReceived;
    }

    private void Client_MessageReceived(object? sender, string e)
    {
        MessageBox.Show("Received: " + e);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        //_client.Send("TRIGGER\r");

        if (_client != null)
        {
            NetworkStream stream = _client.GetStream();
            string tb = textBox1.Text;
            string message = (char)2 + tb + (char)3;
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        else
        {
            MessageBox.Show("No client connected");
        }
    }



    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        //_client.Disconnect();
        _server?.Stop();
        _client?.Close();
    }

    private void CreateServer_Click(object sender, EventArgs e)
    {
        _server = new TcpListener(IPAddress.Any, 420);
        _server.Start();
        _client = _server.AcceptTcpClient();
    }
}