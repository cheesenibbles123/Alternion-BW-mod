using System;
using System.Net;
using System.Threading;
using System.IO;
using Harmony;
using UnityEngine;

namespace Alternion
{
    class ThreadCreationProgram : MonoBehaviour
    {

        public static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        static string mainUrl = "http://www.archiesbots.com/BlackwakeStuff/";

        void Start() {
            debugLog("Started threader object");
        }
        static void debugLog(string message)
        {
            //Just easier to type than Log.logger.Log
            //Will always log, so only use in try{} catch(Exception e) {} when absolutely needed
            Log.logger.Log(message);
        }

        public static Texture2D loadTexture(string texName, byte[] fileData, int imgWidth, int imgHeight)
        {
            try
            {
                Texture2D tex = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24, false);
                tex.LoadImage(fileData);
                return tex;
            }
            catch (Exception e)
            {
                debugLog(string.Format("Error loading texture {0}", texName));
                debugLog(e.Message);
                // Return default white texture on failing to load
                return Texture2D.whiteTexture;
            }
        }

        [HarmonyPatch(typeof(PlayerInfo), "setupRemotePlayer")]
        public static class getPlayerPatch
        {
            static void Postfix(TeamSelect __instance, string pName, int t, int s, int k, int d, int a, bool back, string dStat, int dMedal, int ks, float kd, short wins, float wl, short bm, short sm, short gm)
            {
                if (AlternionSettings.updateDuringRuntime)
                {
                    debugLog("Entered patch");

                    try
                    {
                        debugLog("In Patch: Creating the Child thread");
                        Thread childThread = new Thread(() => ChildThreadJoinIngame(pName));
                        debugLog("Starting thread");
                        childThread.Start();
                        debugLog("Thread Started");

                    }
                    catch (Exception e)
                    {
                        debugLog(e.Message);
                    }
                }
            }
        }

        public static void ChildThreadGetAsset(string filepath, string URL, string assetName, string type, WebClient webCli)
        {
            byte[] bytes = webCli.DownloadData(URL);
            File.WriteAllBytes(Application.dataPath + filepath, bytes);

            Texture2D newTex = loadTexture(assetName, bytes, 100, 40);
            newTex.name = assetName;

            switch (type)
            {
                case "badge":
                    theGreatCacher.badges.Add(assetName, newTex);
                    break;
                case "sail":
                    theGreatCacher.secondarySails.Add(assetName, newTex);
                    break;
                case "mainsail":
                    theGreatCacher.mainSails.Add(assetName, newTex);
                    break;
                case "cannon":
                    theGreatCacher.cannonSkins.Add(assetName, newTex);
                    break;
                case "goldmask":
                    theGreatCacher.maskSkins.Add(assetName, newTex);
                    break;
                case "weaponskin":
                    theGreatCacher.weaponSkins.Add(assetName, newTex);
                    break;
                default:
                    break;
            }
        }

        public static bool checkIfItemIsCached(string type, string assetName)
        {
           switch (type)
           {
                case "badge":
                    if (theGreatCacher.badges[assetName])
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case "sail":
                    if (theGreatCacher.secondarySails[assetName])
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case "mainsail":
                    if (theGreatCacher.mainSails[assetName])
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case "cannon":
                    if (theGreatCacher.cannonSkins[assetName])
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case "goldmask":
                    if (theGreatCacher.maskSkins[assetName])
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case "weaponskin":
                    if (theGreatCacher.weaponSkins[assetName])
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
                    break;
           }
        }

        public static void updateUser(string steamID, playerObject player, WebClient webCli)
        {
            debugLog("Updated user");
            // Overall stuff
            if (theGreatCacher.players[steamID].badgeName != player.badgeName)
            {
                if (!checkIfItemIsCached("badge", player.badgeName))
                {
                    string filePathEnd = "Badges/" + player.badgeName + ".png";
                    Thread childThreadBadges = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.badgeName, "badge", webCli));
                    childThreadBadges.Start();
                }
            }
            if (theGreatCacher.players[steamID].sailSkinName != player.sailSkinName)
            {
                if (!checkIfItemIsCached("sail", player.sailSkinName))
                {
                    string filePathEnd = "SailSkins/" + player.sailSkinName + ".png";
                    Thread childThreadSails = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.sailSkinName, "sail", webCli));
                    childThreadSails.Start();
                }
            }
            if (theGreatCacher.players[steamID].mainSailName != player.mainSailName)
            {
                if (!checkIfItemIsCached("mainsail", player.mainSailName))
                {
                    string filePathEnd = "MainSailSkins/" + player.mainSailName + ".png";
                    Thread childThreadMainSails = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.mainSailName, "mainsail", webCli));
                    childThreadMainSails.Start();
                }
            }
            if (theGreatCacher.players[steamID].cannonSkinName != player.cannonSkinName)
            {
                if (!checkIfItemIsCached("cannon", player.cannonSkinName))
                {
                    string filePathEnd = "CannonSkins/" + player.cannonSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.cannonSkinName, "cannon", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].maskSkinName != player.maskSkinName)
            {
                if (!checkIfItemIsCached("goldmask", player.maskSkinName))
                {
                    string filePathEnd = "MaskSkins/" + player.maskSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.maskSkinName, "goldmask", webCli));
                    childThreadCannons.Start();
                }
            }

            // Weapon skins
            if (theGreatCacher.players[steamID].musketSkinName != player.musketSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "musket_" + player.musketSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.musketSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "musket_" + player.musketSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].blunderbussSkinName != player.blunderbussSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "blunderbuss_" + player.blunderbussSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.blunderbussSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "blunderbuss_" + player.blunderbussSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].nockgunSkinName != player.nockgunSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "nockgun_" + player.nockgunSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.nockgunSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "nockgun_" + player.nockgunSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].handMortarSkinName != player.handMortarSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "handmortar_" + player.handMortarSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.handMortarSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "handmortar_" + player.handMortarSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }

            if (theGreatCacher.players[steamID].standardPistolSkinName != player.standardPistolSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "standardPistol_" + player.standardPistolSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.standardPistolSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "standardPistol_" + player.standardPistolSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].shortPistolSkinName != player.shortPistolSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "shortPistol_" + player.shortPistolSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.shortPistolSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "shortPistol_" + player.shortPistolSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].duckfootSkinName != player.duckfootSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "duckfoot_" + player.duckfootSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.duckfootSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "duckfoot_" + player.duckfootSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].matchlockRevolverSkinName != player.matchlockRevolverSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "matchlock_" + player.matchlockRevolverSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.matchlockRevolverSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "matchlock_" + player.matchlockRevolverSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].annelyRevolverSkinName != player.annelyRevolverSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "annelyRevolver_" + player.annelyRevolverSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.annelyRevolverSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "annelyRevolver_" + player.annelyRevolverSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }

            if (theGreatCacher.players[steamID].axeSkinName != player.axeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "axe_" + player.axeSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.axeSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "axe_" + player.axeSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].rapierSkinName != player.rapierSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "rapier_" + player.rapierSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.rapierSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "rapier_" + player.rapierSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].daggerSkinName != player.daggerSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "dagger_" + player.daggerSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.daggerSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "dagger_" + player.daggerSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].bottleSkinName != player.bottleSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "bottle_" + player.bottleSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.bottleSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "bottle_" + player.bottleSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].cutlassSkinName != player.cutlassSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "cutlass_" + player.cutlassSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.cutlassSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "cutlass_" + player.cutlassSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].pikeSkinName != player.pikeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "pike_" + player.pikeSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.pikeSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "pike_" + player.pikeSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }

            if (theGreatCacher.players[steamID].tomohawkSkinName != player.tomohawkSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "tomohawk_" + player.tomohawkSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.tomohawkSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "tomohawk_" + player.tomohawkSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].spyglassSkinName != player.spyglassSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "spyglass_" + player.spyglassSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.spyglassSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "spyglass_" + player.spyglassSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].grenadeSkinName != player.grenadeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "grenade_" + player.grenadeSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.grenadeSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "grenade_" + player.grenadeSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
            if (theGreatCacher.players[steamID].healItemSkinName != player.healItemSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "healItem_" + player.healItemSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.healItemSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "healItem_" + player.healItemSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }

            if (theGreatCacher.players[steamID].hammerSkinName != player.hammerSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "hammer_" + player.hammerSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.hammerSkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "hammer_" + player.hammerSkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }

            if (theGreatCacher.players[steamID].atlas01SkinName != player.atlas01SkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "atlas01_" + player.atlas01SkinName))
                {
                    string filePathEnd = "WeaponSkins/" + player.atlas01SkinName + ".png";
                    Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "atlas01_" + player.atlas01SkinName, "weaponskin", webCli));
                    childThreadCannons.Start();
                }
            }
        }

        public static void ChildThreadJoinIngame(string pName)
        {
            // Sleep as it takes a while before the steamID is actually assigned to the PlayerInfo
            Thread.Sleep(500);

            string steamID = "0";
            if (GameMode.getPlayerInfo(pName) != null)
            {
                PlayerInfo plrInf = GameMode.getPlayerInfo(pName);
                steamID = plrInf.steamID.ToString();
            }
            debugLog($"Gotten steamID {steamID}");

            WebClient webCli = new WebClient();
            // Fetch all players
            string response = webCli.DownloadString("http://www.archiesbots.com/BlackwakeStuff/playerList.json");
            string[] json = response.Split('&');

            for (int i = 0; i < json.Length; i++)
            {
                playerObject player = JsonUtility.FromJson<playerObject>(json[i]);
                if (player.steamID == steamID) {

                    debugLog($"Found user in json {player.steamID}");
                    // Check if user in dictionary
                    if (theGreatCacher.players.ContainsKey(steamID))
                    {
                        debugLog($"Found user in cached users {player.steamID}");
                        // Only update if actually changed
                        if (theGreatCacher.players[steamID] != player)
                        {
                            debugLog("Ms-match found, updating...");
                            updateUser(steamID, player, webCli);
                            theGreatCacher.players[steamID] = player;
                            debugLog("Updated User");
                        }
                    }
                    else
                    {
                        debugLog("Added user");
                        theGreatCacher.players.Add(player.steamID, player);
                    }
                    debugLog("Fetched player");
                    break;
                }
            }
            debugLog("Finished.");
        }
    }
}
