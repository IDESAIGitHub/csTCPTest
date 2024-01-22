using INIGestor;
using LogLib;
using NModbus;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace CabecerasLib;

public class EntradaCabecera : INotifyPropertyChanged
{
    public Cabeceras CabeceraPertenece { get; private set; }
    public int PosGlobal { get; private set; }
    public int PosCabecera { get; private set; }
    private bool _isNegated;
    public bool IsNegated
    { 
        get
        { 
            lock(_negatedLock)
            {
                return _isNegated;
            }
        }
        private set
        {
            { _isNegated = value; }
        }
    }
    private object _negatedLock = new object();
    public int RisingFilter_ms { get; private set; }
    public int FallingFilter_ms { get; private set; }
    public string Name { get; private set; }
    
    private DateTime _timeLastNotify;
    public string TimeLastNotifyString { get { return _timeLastNotify.ToString("HH:mm:ss.fff"); } }
    //private readonly int _minNotifyms = 200;
    private bool _value;
    private bool _bufferValue;
    private DateTime _timeLastRising;
    private DateTime _timeLastFalling;
    public bool Value
    {
        //get { return this.IsNegated ? !_value : _value; }
        get { return _value; }
        set
        {
            //filtro subida
            if (Value == false)
            {
                DateTime now = DateTime.Now;
                //no filtro, confirma directamente
                if (RisingFilter_ms == 0 && value == true)
                {
                    this._value = value;
                    _timeLastNotify = DateTime.Now;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_value)));
                    return;
                }
                //primera lectura de flanco de subida
                if (_bufferValue == false && value == true)
                {
                    _timeLastRising = now;
                    _bufferValue = value;
                }
                //rechaza entrada
                if (_bufferValue == true && value == false)
                {
                    _bufferValue = value;
                }
                //confirma entrada
                if (_bufferValue == true && value == true &&
                    now - _timeLastRising >= TimeSpan.FromMilliseconds(RisingFilter_ms))
                {
                    this._value = value;
                    _timeLastNotify = DateTime.Now;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_value)));
                    return;
                }
            }
            //filtro bajada
            else if (Value == true)
            {
                DateTime now = DateTime.Now;
                //no filtro, confirma directamente
                if (FallingFilter_ms == 0 && value == false)
                {
                    this._value = value;
                    _timeLastNotify = DateTime.Now;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_value)));
                    return;
                }
                //primera lectura de flanco de bajada
                if (_bufferValue == true && value == false)
                {
                    _timeLastFalling = now;
                    _bufferValue = value;
                }
                //rechaza entrada
                if (_bufferValue == false && value == true)
                {
                    _bufferValue = value;
                }
                //confirma entrada
                if (_bufferValue == false && value == false &&
                    now - _timeLastFalling >= TimeSpan.FromMilliseconds(FallingFilter_ms))
                {
                    this._value = value;
                    _timeLastNotify = DateTime.Now;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_value)));
                    return;
                }
            }
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    //private object _lock = new object();
    
    //constructor
    public EntradaCabecera(Cabeceras cabeceraPertenece, int globalPosition, int posCabecera, bool isNegated, int risingFilter_ms, int fallingFilter_ms, string name)
    {
        this.CabeceraPertenece = cabeceraPertenece;
        this.PosGlobal = globalPosition;
        this.PosCabecera = posCabecera;
        this.IsNegated = isNegated;
        this.RisingFilter_ms = risingFilter_ms;
        this.FallingFilter_ms = fallingFilter_ms;
        this.Name = name;
        this._value = false;
    }

}
public class SalidaCabecera : INotifyPropertyChanged
{
    public Cabeceras CabeceraPertenece { get; private set; }
    private object _lockObject = new object();
    public int PosGlobal { get; private set; }
    public int PosCabecera { get; private set; }
    public string Name { get; private set; }
    private bool _value;
    public bool Value
    {
        get {return _value;}
        set
        {
            lock (_lockObject)
            {
                if (this._value != value)
                {
                    this._value = value;
                    _timeLastNotify = DateTime.Now;
                    try 
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_value)));
                    }
                    catch (Exception ex) { }
                }
            }
        }
    }
    public bool IsNegated { get; private set; }
    public event PropertyChangedEventHandler? PropertyChanged;
    private DateTime _timeLastNotify;
    public string TimeLastNotifyString { get { return _timeLastNotify.ToString("HH:mm:ss.fff"); } }

    //constructor
    public SalidaCabecera(Cabeceras cabeceraPertenece, int globalPosition, int posCabecera, bool isNegated, string name)
    {
        this.CabeceraPertenece = cabeceraPertenece;
        this.PosGlobal = globalPosition;
        this.PosCabecera = posCabecera;
        this.IsNegated = isNegated;
        this.Name = name;
        this.Value = false;
    }
}
public class Cabeceras : INotifyPropertyChanged
{
    //Static variables: these are properties shared by all the couplers (belong to the class itself)
    //static public List<Cabeceras> ListCabeceras { get; private set; } = new List<Cabeceras>();
    static public BindingList<Cabeceras> ListCabeceras { get; private set; } = new BindingList<Cabeceras>();
    //static public List<EntradaCabecera> ListEntradasGlobal { get; private set; } = new List<EntradaCabecera>();
    static public BindingList<EntradaCabecera> ListEntradasGlobal { get; private set; } = new BindingList<EntradaCabecera>();
    //static public List<SalidaCabecera> ListSalidasGlobal { get; private set; } = new List<SalidaCabecera>();
    static public BindingList<SalidaCabecera> ListSalidasGlobal { get; private set; } = new BindingList<SalidaCabecera>();
    static private object s_ListSalidasLock = new object();
    static private IniManager? s_IniManager;
    static private LogManager? s_LogManager;
    static public bool s_allConnected { get { return ListCabeceras.All(x => x.CabeceraConectada); } }

    //Properties for each coupler
    private TcpClient? _modbusClient;
    private ModbusFactory? _modbusFactory;
    private IModbusMaster? _modbusMaster;
    private readonly ushort _decStartReadAddress;
    private readonly ushort _decStartWriteAddress;
    private readonly byte _slaveReadAddress = 0;
    private readonly byte _slaveWriteAddress = 1;
    
    private Thread? _readWriteThread;
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string IpAdress { get; private set; }
    public int Port { get; private set; }
    public string Type { get; private set; }
    private bool _cabeceraConectada;
    public bool CabeceraConectada
    {
        get { return _cabeceraConectada; }
        private set
        {
            if (_cabeceraConectada != value)
            {
                _cabeceraConectada = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_cabeceraConectada)));
            }
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public int MsSleep { get; private set; }

    public List<EntradaCabecera> ListEntradas { get; private set; }
    public List<SalidaCabecera> ListSalidas { get; private set; }
    

    

    //constructor
    public Cabeceras(string name, string ipAdress, int port, string type, ushort startReadAddress, ushort startWriteAddress, byte slaveReadAddress, byte slaveWriteAddress, int msSleep)
    {
        this.Name = name;
        this.IpAdress = ipAdress;
        this.Port = port;
        this.Type = type;
        this.MsSleep = msSleep;
        this._decStartReadAddress = startReadAddress;
        this._decStartWriteAddress = startWriteAddress;
        this._slaveReadAddress = slaveReadAddress;
        this._slaveWriteAddress = slaveWriteAddress;
       
        ListEntradas = new List<EntradaCabecera>();
        ListSalidas = new List<SalidaCabecera>();

        CabeceraConectada = false;
    }
    static public bool ConnectCabeceras(ref List<string> refErrorList)
    {
        List<Task> listTask = new List<Task>();
        bool allConnected = true;
        foreach (Cabeceras c in ListCabeceras)
        {
            listTask.Add(Task.Run(() => c.Connect()));
        }
        //wait for all plcs to connect before 200ms
        allConnected = Task.WaitAll(listTask.ToArray(), TimeSpan.FromMilliseconds(400));
        if (allConnected)
        {
            s_LogManager?.AddEntry("All PLCs connected");
            //start read-write loops
            foreach (Cabeceras c in ListCabeceras)
            {
                c._readWriteThread = new Thread(new ThreadStart(c.ReadWriteLoop));
                c._readWriteThread.Priority = ThreadPriority.Highest;
                c._readWriteThread.Start();
            }
        }
        else
        {
            foreach (Cabeceras c in ListCabeceras)
            {
                if (!c.CabeceraConectada)
                {
                    string msg = "error conecting to the PLC " + c.Id.ToString() + ", with name: " + c.Name + " and IP: " + c.IpAdress + " and Port " + c.Port;
                    refErrorList.Add(msg);
                    s_LogManager?.AddEntry(msg);
                }
            }
            return false;
        }
        return true;
    }

    static public bool CreateCabecerasFromINI(string iniFileName, ref List<string> sectionsError)
    {

        List<Cabeceras> listCabecerasToCreate = new List<Cabeceras>();

        Cabeceras.s_IniManager = new IniManager(iniFileName);
        string logDirectory = s_IniManager.GetString("GENERAL", "DirectorioLog", "C:\\IDESAI\\NOMBRECLIENTE\\LOG\\Cabeceras\\", ref sectionsError);
        string logFileName = s_IniManager.GetString("GENERAL", "ArchivoLog", "cabecerasLog", ref sectionsError);
        int daysToKeep = s_IniManager.GetInt("GENERAL", "DiasMantenerLog", 30, ref sectionsError);
        s_LogManager = new LogManager(logFileName, logDirectory, daysToKeep);
        s_LogManager.AddEntry("------------------- Inicio Cabecera ----------------");
        int numcabeceras = s_IniManager.GetInt("GENERAL", "NumeroCabeceras", 1, ref sectionsError);

        int posInputGlobal = 0;
        int posOutputGlobal = 0;
        for (int i = 0; i < numcabeceras; i++)
        {
            string cabeceraName = s_IniManager.GetString("CABECERA_" + i.ToString(), "NombreCabecera", "Error", ref sectionsError);
            string type = s_IniManager.GetString("CABECERA_" + i.ToString(), "Tipo", "Weidmuller/VIPA", ref sectionsError);
            if (type != "Weidmuller" && type != "VIPA")
            {
                s_IniManager.SetValue("CABECERA_" + i.ToString(), "Tipo", "Weidmuller/VIPA");
            }
            string cabeceraIP = s_IniManager.GetString("CABECERA_" + i.ToString(), "Direccion_IP", "192.168.111.125", ref sectionsError);
            int cabeceraPort = s_IniManager.GetInt("CABECERA_" + i.ToString(), "Puerto", 502, ref sectionsError);
            int numModulosEntrada = s_IniManager.GetInt("CABECERA_" + i.ToString(), "NumModulosEntrada", 1, ref sectionsError);
            int numModulosSalida = s_IniManager.GetInt("CABECERA_" + i.ToString(), "NumModulosSalida", 1, ref sectionsError);
            int tiempoSleep_ms = s_IniManager.GetInt("CABECERA_" + i.ToString(), "TiempoSleepMilisegundos", 10, ref sectionsError);

            ushort readStartAddress = 0;
            ushort writeStartAddress = 0;
            byte slaveReadAddress = 0;
            byte slaveWriteAddress = 0;
            if (type == "Weidmuller")
            {
                slaveReadAddress = 0;
                slaveWriteAddress = 0;
                readStartAddress = 0x0000;  //Read bit Address 
                writeStartAddress = 0x8000;	//Write bit Address 
            }
            else if (type == "VIPA")
            {
                slaveReadAddress = 0;
                slaveWriteAddress = 0;
                readStartAddress = 0x0000;  //1x Bit access to input area
                writeStartAddress = 0x0000;	//0x Bit access to output area
            }
            else
            {
                sectionsError.Add("Unknown type of PLC, Input Weidmuller or VIPA. Current type:" + type);
            }
            listCabecerasToCreate.Add(new Cabeceras(cabeceraName, cabeceraIP, cabeceraPort, type, readStartAddress, writeStartAddress, slaveReadAddress, slaveWriteAddress, tiempoSleep_ms));

            int posInputEnCabecera = 0;
            int posOutputEnCabecera = 0;
            for (int j = 0; j < numModulosEntrada; j++)
            {
                string moduleSection = "CABECERA_" + i.ToString() + ",ModuloEntrada_" + j.ToString();
                int numInputs = s_IniManager.GetInt(moduleSection, "NumeroEntradas", 16, ref sectionsError);

                for (int k = 0; k < numInputs; k++)
                {
                    string key = "IN_" + k.ToString();
                    string defaultInput = posInputGlobal.ToString() +", 1, 0, 0, Nombre";
                    string entradaProperties = s_IniManager.GetString(moduleSection, key, defaultInput, ref sectionsError);
                    int globalINPos = 0;
                    int isNegated = 0;
                    int msFilterRising = 0;
                    int msFilterFalling = 0;
                    string inputName = "";
                    IniManager.GetCabeceraInputProperties(moduleSection, key, entradaProperties, out globalINPos, out isNegated, out msFilterRising, out msFilterFalling, out inputName, ref sectionsError);
                    listCabecerasToCreate[i].ListEntradas.Add(new EntradaCabecera(listCabecerasToCreate[i], globalINPos, posInputEnCabecera, isNegated < 0, msFilterRising, msFilterFalling, inputName));
                    posInputEnCabecera++;
                    posInputGlobal++;
                }
            }

            for (int j = 0; j < numModulosSalida; j++)
            {
                string moduleSection = "CABECERA_" + i.ToString() + ",ModuloSalida_" + j.ToString();
                int numOut = s_IniManager.GetInt(moduleSection, "NumeroSalidas", 16, ref sectionsError);

                for (int k = 0; k < numOut; k++)
                {
                    string key = "OUT_" + k.ToString();
                    string defaultOutput = posOutputGlobal.ToString() + ", 1, 0, Nombre";
                    string entradaProperties = s_IniManager.GetString(moduleSection, key, defaultOutput, ref sectionsError);
                    int globalINPos = 0;
                    int isNegated = 0;
                    string outputName = "";
                    IniManager.GetCabeceraOutputProperties(moduleSection, key, entradaProperties, out globalINPos, out isNegated, out outputName, ref sectionsError);
                    listCabecerasToCreate[i].ListSalidas.Add(new SalidaCabecera(listCabecerasToCreate[i], globalINPos, posOutputEnCabecera, isNegated < 0, outputName));
                    posOutputEnCabecera++;
                    posOutputGlobal++;
                }
            }
        }//i Cabeceras, j Modulos de Entradas y Salidas, k Entradas/Salidas

        if (sectionsError.Count <= 0)
        {
            foreach (Cabeceras c in listCabecerasToCreate)
            {
                AddCabecera(c);
            }
        }
        else
        {
            string errorMsg = "";
            foreach (string s in sectionsError)
            {
                errorMsg += s + "\n";
            }
            s_LogManager.AddEntry("Error en la configuracion de Cabeceras.ini :" + errorMsg);
            return false;
        }
        return true;
    }
    static private void AddCabecera(Cabeceras c)
    {
        c.Id = ListCabeceras.Count;
        ListCabeceras.Add(c);
        foreach (EntradaCabecera e in c.ListEntradas)
        {
            ListEntradasGlobal?.Add(e);
        }
        foreach (SalidaCabecera s in c.ListSalidas)
        {
            ListSalidasGlobal?.Add(s);
        }
    }

    static public bool GetInputGlobal(int posGlobal, out bool value)
    {
        value = false;
        if (ListEntradasGlobal == null)
        {
            return false;
        }
        if (posGlobal < 0 || posGlobal >= ListEntradasGlobal.Count)
        {
            return false;
        }
        value = ListEntradasGlobal[posGlobal].Value;
        return true;
    }
    static public bool SetOutputGlobal(int posGlobal, bool value)
    {
        if (ListSalidasGlobal == null)
        {
            return false;
        }
        if (posGlobal < 0 || posGlobal >= ListSalidasGlobal.Count)
        {
            return false;
        }
        //set Value to Global List
        //(parsing values to each individual Cabeceras is done in the readWrite Loop)
        lock (s_ListSalidasLock)
        {
            ListSalidasGlobal[posGlobal].Value = value;
        }
        return true;
    }
    static public bool FlipOutputGlobal(int posGlobal)
    {
        if (ListSalidasGlobal == null)
        {
            return false;
        }
        if (posGlobal < 0 || posGlobal >= ListSalidasGlobal.Count)
        {
            return false;
        }
        lock (s_ListSalidasLock)
        {
            ListSalidasGlobal[posGlobal].Value = !ListSalidasGlobal[posGlobal].Value;
        }
        return true;
    }
    static public void FinalizeCabeceras()
    {
        List<Cabeceras> listConnected = new List<Cabeceras>();
        foreach (Cabeceras c in ListCabeceras)
        {
            if (c.CabeceraConectada)
            {
                listConnected.Add(c);
            }
        }
        DisconnectAll();
        foreach (Cabeceras c in listConnected)
        {
            c._readWriteThread?.Join();
        }
        s_LogManager?.AddEntry("------------------- Finalización Cabecera ----------------\n");
        s_LogManager?.FinalizeLogManager();
    }

    static private void DisconnectAll()
    {
        if (ListCabeceras != null)
        {
            foreach (Cabeceras c in Cabeceras.ListCabeceras)
            {
                c.CabeceraConectada = false;
            }
        }
    }

    static public void AllOutputsOff()
    {
        if (ListSalidasGlobal != null)
        {
            foreach (SalidaCabecera s in ListSalidasGlobal)
            {
                s.Value = false;
            }
        }
    }
    
    private bool Connect()
    {
        try
        {
            _modbusClient = new TcpClient(IpAdress, Port);
            _modbusFactory = new ModbusFactory();
            _modbusMaster = _modbusFactory.CreateMaster(_modbusClient);
            CabeceraConectada = true;
            s_LogManager?.AddEntry("PLC " + Id.ToString() + ", with name: " + Name + " and IP: " + IpAdress + " and Port " + Port+ " Connected");
            return true;
        }
        catch (Exception e)
        {
            CabeceraConectada = false;
            s_LogManager?.AddEntry("error conecting to the PLC " + Id.ToString() + ", with name: " + Name + " and IP: " + IpAdress + " and Port " + Port + ". Error: " + e.Message);
        }
        return false;
    }

    //read/write loop
    private void ReadWriteLoop()
    {
        if (_modbusMaster == null)
        {
            s_LogManager?.AddEntry("Error in PLC: _modbusMaster was null in this context");
            return;
        } 
        try
        {
            while (CabeceraConectada)
            {
                ushort numEntradas = Convert.ToUInt16(ListEntradas.Count);
                bool[] arrayEntradas = new bool[numEntradas];
                int numSalidas = Convert.ToUInt16(ListSalidas.Count);
                bool[] arraySalidas = new bool[numSalidas];

                arrayEntradas = _modbusMaster.ReadInputs(_slaveReadAddress, _decStartReadAddress, numEntradas);
                //parse arrayEntradas and update entradasCabecera and entradasGlobal
                for (int i = 0; i < numEntradas; i++)
                {
                    ListEntradas[i].Value = ListEntradas[i].IsNegated ? !arrayEntradas[i] : arrayEntradas[i];
                    //ListEntradas[i].Value = !arrayEntradas[i];
                    if (ListEntradasGlobal != null)
                    {
                        //ListEntradasGlobal[ListEntradas[i].PosGlobal].Value = !arrayEntradas[i];
                        ListEntradasGlobal[ListEntradas[i].PosGlobal].Value = ListEntradas[i].IsNegated ? !arrayEntradas[i] : arrayEntradas[i];
                    }

                }
                
                lock (s_ListSalidasLock) 
                { 
                    //parse salidasGlobal to salidasCabecera and update arraySalidas
                    for (int i = 0; i < numSalidas; i++)
                    {
                        if (ListSalidasGlobal != null)
                        {
                            ListSalidas[i].Value = ListSalidasGlobal[ListSalidas[i].PosGlobal].Value;
                            arraySalidas[i] = ListSalidas[i].IsNegated ? !ListSalidas[i].Value : ListSalidas[i].Value;
                        }
                    }
                }
                _modbusMaster.WriteMultipleCoils(_slaveWriteAddress, _decStartWriteAddress, arraySalidas);
                if (MsSleep > 0)
                {
                    Thread.Sleep(MsSleep);
                }
            }
        }
        catch (Exception ex)
        {
            s_LogManager?.AddEntry("Error in PLC " + Id.ToString() + ", with name: " + Name + " and IP: " + IpAdress + ". Error: " + ex.Message);
            DisconnectAll();
        }
        AllOutputsOff();
        try
        {
            bool[] arraySalidasZeros = new bool[Convert.ToUInt16(ListSalidas.Count)];
            _modbusMaster.WriteMultipleCoils(_slaveWriteAddress, _decStartWriteAddress, arraySalidasZeros);
        }
        catch (Exception ex)
        {
            
        }
        _modbusClient?.Close();
        s_LogManager?.AddEntry("plc " + Id.ToString() + ", with name: " + Name + " and IP: " + IpAdress+ " and Port " + Port + " Disconnected");
    }
    
}