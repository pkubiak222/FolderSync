using System.IO;

namespace FolderSync
{
    internal class Logger : IDisposable
    {
        private readonly StreamWriter writer;
        private readonly object syncObj = new object();
        public Logger(string logFilePath)
        {
            string? directory = Path.GetDirectoryName(Path.GetFullPath(logFilePath));
            if(!string.IsNullOrWhiteSpace(directory))
            {
                writer = new StreamWriter(new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
            }
        }

        public void Log(string logMessage)
        {
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logMessage}";
            lock (syncObj)
            {

                try
                {
                    writer.WriteLine(entry);
                }
                catch (Exception e) {
                    Console.Error.WriteLine($"Logger error: {e.Message}"); 
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
