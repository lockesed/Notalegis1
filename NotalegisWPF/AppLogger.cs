using NLog;

namespace NotalegisWPF
{
    /// <summary>
    /// Central logger — wraps NLog so all views/services use one entry point.
    /// Usage: AppLogger.Log.Error(ex, "Contexto descriptivo");
    /// </summary>
    public static class AppLogger
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
    }
}
