using System.IO;

namespace FastLink.Utils
{
    public static class Logger
    {
        public static void Debug(string message)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string logDirectory = Path.Combine(currentDirectory, "Logs");
            string logFile = Path.Combine(logDirectory, $"Debug_{DateTime.Today:yyyyMMdd}.log");

            try
            {
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{message}";
                using StreamWriter sw = new(logFile, true);
                sw.WriteLine(logEntry);
            }
            catch
            { }
        }

        public static void Error(Exception ex, string message = null)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string logDirectory = Path.Combine(currentDirectory, "Logs");
            string logFile = Path.Combine(logDirectory, $"Error_{DateTime.Today:yyyyMMdd}.log");

            try
            {
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{ex.GetType().FullName}\t{ex.Message}\n{ex.StackTrace}";
                if (message != null) logEntry += $"\n=> {message}";
                using StreamWriter sw = new(logFile, true);
                sw.WriteLine(logEntry);
            }
            catch
            { }
        }
    }
}
