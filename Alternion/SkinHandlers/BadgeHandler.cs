using Alternion.Structs;
using Harmony;
using System;
using UnityEngine;
using Steamworks;


namespace Alternion.SkinHandlers
{
    class BadgeHelpers
    {
        public static Logger logger = new Logger("[FlagHandler]");

        /// <summary>
        /// Checks if input badge is a Kickstarter or Tournamentwake badge
        /// </summary>
        /// <param name="__instance">ScoreboardSlot</param> 
        public static bool checkIfTWOrKS(ScoreboardSlot __instance)
        {
            // If TW Badge
            if (__instance.éòëèïòëóæèó.texture.name != "tournamentWake1Badge" ^ (!AlternionSettings.showTWBadges && __instance.éòëèïòëóæèó.texture.name == "tournamentWake1Badge"))
            {
                // IF KS Badge
                if (__instance.éòëèïòëóæèó.texture.name != "KSbadge" ^ (!AlternionSettings.showKSBadges && __instance.éòëèïòëóæèó.texture.name == "KSbadge"))
                {
                    // IF KS Badge
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Checks if input badge is a Kickstarter or Tournamentwake badge
        /// </summary>
        /// <param name="name">Player Name</param> 
        /// <returns>Bool</returns>
        public static bool checkIfTWOrKS(string name)
        {
            PlayerInfo plrInf = GameMode.getPlayerInfo(name);
            // If TW Badge
            if (!plrInf.isTournyWinner ^ (!AlternionSettings.showTWBadges & plrInf.isTournyWinner))
            {
                // IF KS Badge
                if (!plrInf.backer ^ (!AlternionSettings.showKSBadges & plrInf.backer))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

    }

    /// <summary>
    /// Harmony patch to setup badges in the scoreboard
    /// </summary>
    [HarmonyPatch(typeof(ScoreboardSlot), "ñòæëíîêïæîí", new Type[] { typeof(string), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
    class scoreBoardSlotAdjuster
    {
        static void Postfix(ScoreboardSlot __instance, string ìåäòäóëäêèæ, int óîèòèðîðçîì, string ñíçæóñðæéòó, int çïîèïçïñêïæ, int äïóïåòòéðåç, int ìëäòìèçñçìí, int óíïòðíäóïçç, int íîóìóíèíñìå, bool ðèæòðìêóëïð, bool äåîéíèñèììñ, bool æíèòîîìðçóî, int ïîñíñóóåîîñ, int æìíñèéçñîíí, bool òêóçíïåæíîë, bool æåèòðéóçêçó, bool èëçòëæêäêîå, bool ëååííåïäæîè, bool ñîäèñæïîóçó)
        {
            if (AlternionSettings.useBadges)
            {
                try
                {
                    string steamID = GameMode.getPlayerInfo(ìåäòäóëäêèæ).steamID.ToString();
                    if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                    {
                        // if they have a TW OR KS badge, this will dictate if it should or shouldn't override it visually
                        if (BadgeHelpers.checkIfTWOrKS(__instance) && TheGreatCacher.Instance.badges.TryGetValue(player.badgeName, out Texture newTexture))
                        {
                            __instance.éòëèïòëóæèó.texture = newTexture;
                            __instance.éòëèïòëóæèó.SetNativeSize();
                        }
                    }

                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Object reference not set to an instance of an object"))
                    {
                        //Go do one
                    }
                    else
                    {
                        BadgeHelpers.logger.debugLog("Failed to assign custom badge to a player:");
                        BadgeHelpers.logger.debugLog(e.Message);
                    }
                }
            }

        }
    }

    /// <summary>
    /// Harmony patch to set badge in endRound scene
    /// </summary>
    [HarmonyPatch(typeof(AccoladeItem), "ëîéæìêìëéæï")]
    class accoladeSetInfoPatch
    {
        static void Postfix(AccoladeItem __instance, string óéíïñîèëëêð, int òææóïíéñåïñ, string çìîñìëðêëéò, CSteamID ìçíêääéïíòç)
        {
            // Sets win screen badges
            if (AlternionSettings.useBadges)
            {
                string steamID = ìçíêääéïíòç.m_SteamID.ToString();
                if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                {
                    if (BadgeHelpers.checkIfTWOrKS(óéíïñîèëëêð) && TheGreatCacher.Instance.badges.TryGetValue(player.badgeName, out Texture newTex))
                    {
                        __instance.äæåéåîèòéîñ.texture = newTex;
                        __instance.äæåéåîèòéîñ.SetNativeSize();
                    }
                }
            }
        }
    }
}
