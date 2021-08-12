﻿using Harmony;

namespace Alternion.Fixes
{
    class gameModeChanges
    {

        [HarmonyPatch(typeof(GameMode), "getPlayerInfo")]
        static class getPlayerInfoPatch
        {
            /// <summary>
            /// Patches into getPlayerInfo and removes references to 'game', aka calls for when a bot does something
            /// </summary>
            /// <param name="__instance">Current instance of GameMode</param>
            /// <param name="pname">Player Name</param>
            /// <returns></returns>
            static bool Prefix(ref PlayerInfo __result, string pname)
            {
                if (pname == "game")
                {
                    __result = null;
                    return false; // Block function from running if its a bot
                }
                return true;
            }
        }

    }
}
