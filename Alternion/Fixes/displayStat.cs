using Harmony;
using Steamworks;

namespace Alternion.Fixes
{
#if DEBUG
    class displayStat
    {
        
        [HarmonyPatch(typeof(PlayerInfo), "setupRemotePlayer")]
        class displayStatPatch
        {
            static void Postfix(PlayerInfo __instance, string pName, int t, int s, int k, int d, int a, bool back, string dStat, int dMedal, int ks, float kd, short wins, float wl, short bm, short sm, short gm)
            {
                Logger.debugLog("Got setup for " + pName);
                Logger.debugLog($"Medal set to -{__instance.displayStat}-{dStat}- ");
                Logger.debugLog($"Level set to -{__instance.displayStatMedal}-{dMedal}- ");
            }
        }

        [HarmonyPatch(typeof(AccoladeItem), "ëîéæìêìëéæï")]
        class accoladeSetupPatch
        {
            static void Postfix(AccoladeItem __instance, string óéíïñîèëëêð, int òææóïíéñåïñ, string çìîñìëðêëéò, CSteamID ìçíêääéïíòç)
            {
                PlayerInfo plrInf = GameMode.getPlayerInfo(óéíïñîèëëêð);
                Logger.debugLog("Got Accolade setup for " + óéíïñîèëëêð);
                Logger.debugLog($"Accolade Medal set to -{plrInf.displayStat}-{plrInf.displayStatMedal}- ");
            }
        }
    }
#endif
}
