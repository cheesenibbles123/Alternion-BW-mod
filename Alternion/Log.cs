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
}
