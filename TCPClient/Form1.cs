using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPZebra_test;





public partial class Form1 : Form
{
    private TcpClientExample _client;
    public Form1()
    {
        InitializeComponent();
        _client = new TcpClientExample();

        _client.ConnectAsync();

        _client.MessageReceived += Client_MessageReceived;
    }

    private void Client_MessageReceived(object? sender, string e)
    {
        MessageBox.Show("Received: " + e);
    }


    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {

    }
}