using System;
using System.IO;
using System.Text;

namespace ReportDataMaker.Infrastructure;

/// <summary>
/// 写入日志到 logs/{启动时间戳}.log，从程序启动到退出。
/// 调用 FileLogger.Initialize() 初始化，之后用 FileLogger.Instance 写入。
/// </summary>
public class FileLogger : IDisposable
{
    private static FileLogger? _instance;
    private static readonly object _lock = new();
    private readonly StreamWriter _writer;
    private readonly string _filePath;
    private bool _disposed;

    private FileLogger()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var logsDir = Path.Combine(baseDir, "logs");
        Directory.CreateDirectory(logsDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _filePath = Path.Combine(logsDir, $"{timestamp}.log");

        _writer = new StreamWriter(_filePath, append: false, encoding: Encoding.UTF8)
        {
            AutoFlush = true
        };

        WriteLine($"[LOG] FileLogger initialized: {_filePath}");
    }

    /// <summary>初始化文件日志（应用启动时调用一次）</summary>
    public static void Initialize()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                _instance ??= new FileLogger();
            }
        }
    }

    /// <summary>全局 FileLogger 实例</summary>
    public static FileLogger Instance
    {
        get
        {
            if (_instance == null)
                Initialize();
            return _instance!;
        }
    }

    /// <summary>写入一行日志（含时间戳前缀）</summary>
    public void WriteLine(string message)
    {
        if (_disposed) return;
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        lock (_lock)
        {
            _writer.WriteLine($"[{timestamp}] {message}");
        }
    }

    /// <summary>关闭日志文件</summary>
    public void Shutdown()
    {
        if (_disposed) return;
        lock (_lock)
        {
            WriteLine("[LOG] FileLogger shutting down");
            _writer.Flush();
            _writer.Close();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Shutdown();
    }
}
