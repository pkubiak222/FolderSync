using System;
using System.IO;

namespace FolderSync
{
    internal class Logger : IDisposable
    {
        private readonly StreamWriter? writer;
        private readonly object syncObj = new object();
        private readonly string? logFilePath;

        public Logger(string logFilePath)
        {
            try
            {
                string fullPath = Path.GetFullPath(logFilePath);
                string? directory = Path.GetDirectoryName(fullPath);

                if (string.IsNullOrWhiteSpace(directory))
                    throw new ArgumentException("Invalid log file path – missing directory part.");

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (Directory.Exists(fullPath))
                    throw new IOException($"'{fullPath}' is a directory, not a file. Please specify a valid file path (e.g. C:\\Logs\\sync.txt).");

                writer = new StreamWriter(new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true
                };

                this.logFilePath = fullPath;
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"[Logger] Access denied: cannot write to '{logFilePath}'. Try running as administrator or choose another path.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Logger] Failed to initialize: {ex.Message}");
            }
        }

        public void Log(string message)
        {
            if (writer == null)
            {
                Console.Error.WriteLine($"[Logger] No valid log file. Skipping log: {message}");
                return;
            }

            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            lock (syncObj)
            {
                try
                {
                    writer.WriteLine(entry);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"[Logger] Write error: {e.Message}");
                }
            }
        }

        public void Dispose()
        {
            lock (syncObj)
            {
                writer?.Dispose();
            }
        }
    }
}
