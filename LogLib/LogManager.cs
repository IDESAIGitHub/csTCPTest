namespace LogLib;

public struct LogEntry
{
    public DateTimeOffset TimeStamp { get; private set; }
    public string _message { get; private set; }
    public LogEntry(DateTimeOffset timeStamp, string message)
    {
        this.TimeStamp = timeStamp;
        this._message = message;
    }
}
public class LogManager
{
    public string LogName { get; private set; }
    public Queue<LogEntry> LogEntries { get; private set; }
    static public bool s_FinalizeLogManager;
    public int DaysToKeepLogs { get; private set; } = 7;
    public string LogFullPath { get; private set; }
    private readonly object _lock = new object();
    private readonly AutoResetEvent _logEvent = new AutoResetEvent(false);
    private readonly Thread _logThread;

    //constructor --- call example: logManager = new LogManager("test", "C:\\IDESAI\\SEUR\\LOG\\")
    public LogManager(string _logName, string _logFolderPath, int _daysToKeepLogs)
    {
        this.LogName = _logName;
        this.DaysToKeepLogs = _daysToKeepLogs;
        LogEntries = new Queue<LogEntry>();

        //create log name with date
        LogName = DateTime.Now.ToString("yyyy-MM-dd") + "_" + LogName + ".txt";

        this.LogFullPath = Path.Combine(_logFolderPath, LogName);
        CreateLogFile(_logFolderPath);
        DeleteOldLogs(_logFolderPath);
        
        _logThread = new Thread(this.LogThread);
        _logThread.Start();
        
    }
    private void CreateLogFile(string _logFolderPath)
    {
        //create log folder if it doesnt exist
        if (!Directory.Exists(_logFolderPath))
        {
            Directory.CreateDirectory(_logFolderPath);
        }
        //create the file if it doesnt exist
        if (!File.Exists(LogFullPath))
        {
            File.Create(LogFullPath).Close();
        }
    }
    private void DeleteOldLogs(string _logFolderPath)
    {
        //delete logs older than N days
        string[] files = Directory.GetFiles(Path.GetDirectoryName(_logFolderPath));
        foreach (string file in files)
        {
            FileInfo fi = new FileInfo(file);
            //fetch log the files that are older than N days and contain the logName
            if (fi.CreationTime < DateTime.Now.AddDays(-DaysToKeepLogs) && fi.Name.Contains(LogName))
                fi.Delete();
        }
    }
    public void AddEntry(string _message)
    {
        lock (_lock)
        {
            LogEntries.Enqueue(new LogEntry(DateTime.Now, _message));
        }
        _logEvent.Set();
    }
    public void SaveLog()
    {
        using (StreamWriter sw = File.AppendText(LogFullPath))
        {
            List<LogEntry> logEntriesCopy = new List<LogEntry>();
            lock (_lock)
            {
                while (LogEntries.Count > 0)
                {
                    LogEntry logEntry = LogEntries.Dequeue();
                    logEntriesCopy.Add(logEntry);
                }
            }
            foreach (LogEntry logEntry in logEntriesCopy)
            {
                sw.WriteLine(logEntry.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + logEntry._message);
            }
        }
    }

    
    private void LogThread()
    {
        while (!s_FinalizeLogManager)
        {
            _logEvent.WaitOne();
            SaveLog();
        }
    }
    public void FinalizeLogManager()
    {
        s_FinalizeLogManager = true;
        _logEvent.Set();
        _logThread.Join();
    }
    
}