using BWModLoader;

namespace Alternion
{
    /// <summary>
    /// Logs stuff.
    /// </summary>
    
    public class Logger
    {
        /// <summary>
        /// Prefix to be appended to the start of any messages sent from this instance
        /// </summary>
        private string TAG;

        static readonly public ModLogger logger = new ModLogger("[Alternion]", ModLoader.LogPath + "\\Alternion.txt");

        public Logger(string tag = "")
        {
            TAG = tag;
        }

        /// <summary>
        /// Always logs, no matter the logging level.
        /// </summary>
        public void debugLog(string message)
        {
            //Just easier to type than Log.logger.Log
            //Will always log, so only use in try{} catch(Exception e) {} when absolutely needed
            logger.Log(TAG + ": " + message);
        }

        /// <summary>
        /// Logs debug items at logging levels higher than 0.
        /// </summary>
        public void logLow(string message)
        {
            //Just easier to type than Log.logger.Log
            // Also lets me just set logLevel to 0 if I dont want to deal with the spam.
            if (AlternionSettings.loggingLevel > 0)
            {
                debugLog(TAG + ": " + message);
            }
        }
    }
}
