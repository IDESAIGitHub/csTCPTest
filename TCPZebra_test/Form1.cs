namespace TCPZebra_test;





public partial class Form1 : Form
{
    private TcpClientExample _client;
    public Form1()
    {
        InitializeComponent();
        InitializeComponent();
        _client = new TcpClientExample();

        _client.ConnectAsync();

        _client.MessageReceived += Client_MessageReceived;
    }

    private void Client_MessageReceived(object? sender, string e)
    {
        MessageBox.Show("Received: " + e);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        _client.Send("TRIGGER\r");
        //Thread.Sleep(150);
        //_client.Send("TRIGGER\r");
        //Thread.Sleep(150);
        //_client.Send("TRIGGER\r");
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        _client.Disconnect();
    }
}