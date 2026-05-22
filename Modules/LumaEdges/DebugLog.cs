using System.IO;

namespace LuminaDesktop.Modules.LumaEdges;

public static class DebugLog
{
    private static readonly object LockObject = new();

    public static string LogPath
    {
        get
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "LumaEdges", "debug.log");
        }
    }

    public static void Write(string message)
    {
        try
        {
            var directory = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            lock (LockObject)
            {
                File.AppendAllText(LogPath, $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Debug logging must never break hotkey handling.
        }
    }
}


