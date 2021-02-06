using BWModLoader;

namespace Alternion
{
    /// <summary>
    /// Logs stuff.
    /// </summary>
    internal static class Log
    {
        static readonly public ModLogger logger = new ModLogger("[Alternion]", ModLoader.LogPath + "\\Alternion.txt");
    }
    public class Logger
    {
        /// <summary>
        /// Always logs, no matter the logging level.
        /// </summary>
        /// <param name="message">Message to Log</param>
        public static void debugLog(string message)
        {
            //Just easier to type than Log.logger.Log
            //Will always log, so only use in try{} catch(Exception e) {} when absolutely needed
            Log.logger.Log(message);
        }

        /// <summary>
        /// Logs low priority stuff.
        /// </summary>
        /// <param name="message">Message to Log</param>
        public static void logLow(string message)
        {
            //Just easier to type than Log.logger.Log
            // Also lets me just set logLevel to 0 if I dont want to deal with the spam.
            if (AlternionSettings.loggingLevel > 0)
            {
                Log.logger.Log(message);
            }
        }
    }
}
