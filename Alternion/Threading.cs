using System;
using System.Collections.Generic;
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
        static Texture2D tex;

        void Start()
        {
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
                debugLog("Loading Texture");
                tex.Resize(imgWidth, imgHeight);
                debugLog("Resized Texture");
                tex.LoadImage(fileData);
                debugLog("Applied Texture");
                tex.name = texName;
                debugLog("Applied Name");
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
        static class getPlayerPatch
        {
            static void Postfix(TeamSelect __instance, string pName, int t, int s, int k, int d, int a, bool back, string dStat, int dMedal, int ks, float kd, short wins, float wl, short bm, short sm, short gm)
            {
                if (AlternionSettings.updateDuringRuntime)
                {

                    try
                    {
                        tex = new Texture2D(100, 100, TextureFormat.RGB24, false);
                        Thread childThread = new Thread(() => ChildThreadJoinIngame(pName));
                        childThread.Start();
                    }
                    catch (Exception e)
                    {
                        debugLog(e.Message);
                    }
                }
            }
        }

        public static void ChildThreadJoinIngame(string pName)
        {
            // Sleep as it takes a while before the steamID is actually assigned to the PlayerInfo
            Thread.Sleep(500);
            debugLog("Started Thread");
            string steamID = "0";
            if (GameMode.getPlayerInfo(pName) != null)
            {
                PlayerInfo plrInf = GameMode.getPlayerInfo(pName);
                steamID = plrInf.steamID.ToString();
            }

            debugLog($"Gotten => {steamID}");

            WebClient webCli = new WebClient();
            // Fetch all players
            string response = webCli.DownloadString("http://www.archiesbots.com/BlackwakeStuff/playerList.json");
            string[] json = response.Split('&');

            for (int i = 0; i < json.Length; i++)
            {
                playerObject player = JsonUtility.FromJson<playerObject>(json[i]);
                if (player.steamID == steamID)
                {

                    // Check if user in dictionary
                    if (theGreatCacher.players.ContainsKey(steamID))
                    {
                        // Only update if actually changed
                        if (theGreatCacher.players[steamID] != player)
                        {
                            try
                            {
                                debugLog($"Original duckfoot => {theGreatCacher.players[steamID].duckfootSkinName}");
                                updateUser(steamID, player, webCli);
                                debugLog("Updated User");
                                theGreatCacher.players[steamID] = player;
                                debugLog($"Assigned => {player.steamID}");
                                debugLog($"New duckfoot => {theGreatCacher.players[steamID].duckfootSkinName}");
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }
                        }
                    }
                    else
                    {
                        theGreatCacher.players.Add(player.steamID, player);
                    }
                    break;
                }
            }
        }

        public static void updateAllPlayers()
        {
            Thread childThread = new Thread(ChildThreadUpdateAll);
            childThread.Start();
        }

        private static void ChildThreadGetAsset(string filepath, string URL, string assetName, string type, WebClient webCli)
        {
            //debugLog($"Downloading => -{assetName}-");

            byte[] bytes;
            try
            {
                bytes = webCli.DownloadData(URL);
                File.WriteAllBytes(Application.dataPath + filepath, bytes);
            }catch (Exception e)
            {
                debugLog(e.Message);
                bytes = null;
            }

            Texture2D newTex;
            debugLog("Set tex");
            try
            {
                debugLog("Setting " + type);
                switch (type)
                {
                    case "badge":
                        debugLog("Init A");
                        newTex = loadTexture(assetName, bytes, 100, 40);
                        newTex.name = assetName;
                        theGreatCacher.badges.Add(assetName, newTex);
                        debugLog("Downloaded Badge Skin");
                        break;
                    case "sail":
                        debugLog("Init B");
                        newTex = loadTexture(assetName, bytes, 2048, 2048);
                        newTex.name = assetName;
                        theGreatCacher.secondarySails.Add(assetName, newTex);
                        debugLog("Downloaded Sail Skin");
                        break;
                    case "mainsail":
                        debugLog("Init C");
                        newTex = loadTexture(assetName, bytes, 2048, 2048);
                        newTex.name = assetName;
                        theGreatCacher.mainSails.Add(assetName, newTex);
                        debugLog("Downloaded MainSail Skin");
                        break;
                    case "cannon":
                        debugLog("Init D");
                        newTex = loadTexture(assetName, bytes, 2048, 2048);
                        newTex.name = assetName;
                        theGreatCacher.cannonSkins.Add(assetName, newTex);
                        debugLog("Downloaded Cannon Skin");
                        break;
                    case "goldmask":
                        debugLog("Init E");
                        newTex = loadTexture(assetName, bytes, 1024, 1024);
                        newTex.name = assetName;
                        theGreatCacher.maskSkins.Add(assetName, newTex);
                        debugLog("Downloaded Mask Skin");
                        break;
                    case "weaponskin":
                        debugLog("Init F");
                        newTex = loadTexture(assetName, bytes, 2048, 2048);
                        debugLog("loaded texture");
                        newTex.name = assetName;
                        debugLog($"set name => {newTex.name}");
                        theGreatCacher.weaponSkins.Add(assetName, newTex);
                        debugLog("Downloaded Weapon Skin");
                        break;
                    default:
                        break;
                }
            }catch (Exception e)
            {
                debugLog(e.Message);
            }
            debugLog("Complete!");
        }

        private static bool checkIfItemIsCached(string type, string assetName)
        {
            switch (type)
            {
                case "badge":
                    if (theGreatCacher.badges.ContainsKey(assetName))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "sail":
                    if (theGreatCacher.secondarySails.ContainsKey(assetName))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "mainsail":
                    if (theGreatCacher.mainSails.ContainsKey(assetName))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "cannon":
                    if (theGreatCacher.cannonSkins.ContainsKey(assetName))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "goldmask":
                    if (theGreatCacher.maskSkins.ContainsKey(assetName))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "weaponskin":
                    if (theGreatCacher.weaponSkins.ContainsKey(assetName))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
           }
        }

        private static void updateUser(string steamID, playerObject player, WebClient webCli)
        {
            // Overall stuff
            bool enableThread = false;
            debugLog($"Updating user => {steamID}");
            if (theGreatCacher.players[steamID].badgeName != player.badgeName)
            {
                if (!checkIfItemIsCached("badge", player.badgeName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "Badges/" + player.badgeName + ".png";
                        Thread childThreadBadges = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.badgeName, "badge", webCli));
                        childThreadBadges.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].sailSkinName != player.sailSkinName)
            {
                if (!checkIfItemIsCached("sail", player.sailSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "SailSkins/" + player.sailSkinName + ".png";
                        Thread childThreadSails = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.sailSkinName, "sail", webCli));
                        childThreadSails.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].mainSailName != player.mainSailName)
            {
                if (!checkIfItemIsCached("mainsail", player.mainSailName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "MainSailSkins/" + player.mainSailName + ".png";
                        Thread childThreadMainSails = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.mainSailName, "mainsail", webCli));
                        childThreadMainSails.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].cannonSkinName != player.cannonSkinName)
            {
                if (!checkIfItemIsCached("cannon", player.cannonSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "CannonSkins/" + player.cannonSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.cannonSkinName, "cannon", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].maskSkinName != player.maskSkinName)
            {
                if (!checkIfItemIsCached("goldmask", player.maskSkinName))
                {if (enableThread)
                    {
                        string filePathEnd = "MaskSkins/" + player.maskSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.maskSkinName, "goldmask", webCli));
                        childThreadCannons.Start();
                    }
                }
            }

            // Weapon skins
            if (theGreatCacher.players[steamID].musketSkinName != player.musketSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "musket_" + player.musketSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "musket_" + player.musketSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "musket_" + player.musketSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].blunderbussSkinName != player.blunderbussSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "blunderbuss_" + player.blunderbussSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "blunderbuss_" + player.blunderbussSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "blunderbuss_" + player.blunderbussSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].nockgunSkinName != player.nockgunSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "nockgun_" + player.nockgunSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "nockgun_" + player.nockgunSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "nockgun_" + player.nockgunSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].handMortarSkinName != player.handMortarSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "handmortar_" + player.handMortarSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "handmortar_" + player.handMortarSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "handmortar_" + player.handMortarSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }

            if (theGreatCacher.players[steamID].standardPistolSkinName != player.standardPistolSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "standardPistol_" + player.standardPistolSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "standardPistol_" + player.standardPistolSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "standardPistol_" + player.standardPistolSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].shortPistolSkinName != player.shortPistolSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "shortPistol_" + player.shortPistolSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "shortPistol_" + player.shortPistolSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "shortPistol_" + player.shortPistolSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].duckfootSkinName != player.duckfootSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "duckfoot_" + player.duckfootSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "duckfoot_" + player.duckfootSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "duckfoot_" + player.duckfootSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].matchlockRevolverSkinName != player.matchlockRevolverSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "matchlock_" + player.matchlockRevolverSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "matchlock_" + player.matchlockRevolverSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "matchlock_" + player.matchlockRevolverSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].annelyRevolverSkinName != player.annelyRevolverSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "annelyRevolver_" + player.annelyRevolverSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "annelyRevolver_" + player.annelyRevolverSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "annelyRevolver_" + player.annelyRevolverSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }

            if (theGreatCacher.players[steamID].axeSkinName != player.axeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "axe_" + player.axeSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "axe_" + player.axeSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "axe_" + player.axeSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].rapierSkinName != player.rapierSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "rapier_" + player.rapierSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "rapier_" + player.rapierSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "rapier_" + player.rapierSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].daggerSkinName != player.daggerSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "dagger_" + player.daggerSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "dagger_" + player.daggerSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "dagger_" + player.daggerSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].bottleSkinName != player.bottleSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "bottle_" + player.bottleSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "bottle_" + player.bottleSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "bottle_" + player.bottleSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].cutlassSkinName != player.cutlassSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "cutlass_" + player.cutlassSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "cutlass_" + player.cutlassSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "cutlass_" + player.cutlassSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].pikeSkinName != player.pikeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "pike_" + player.pikeSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "pike_" + player.pikeSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "pike_" + player.pikeSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }

            if (theGreatCacher.players[steamID].tomohawkSkinName != player.tomohawkSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "tomohawk_" + player.tomohawkSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "tomohawk_" + player.tomohawkSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "tomohawk_" + player.tomohawkSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].spyglassSkinName != player.spyglassSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "spyglass_" + player.spyglassSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "spyglass_" + player.spyglassSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "spyglass_" + player.spyglassSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].grenadeSkinName != player.grenadeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "grenade_" + player.grenadeSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "grenade_" + player.grenadeSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "grenade_" + player.grenadeSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
            if (theGreatCacher.players[steamID].healItemSkinName != player.healItemSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "healItem_" + player.healItemSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "healItem_" + player.healItemSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "healItem_" + player.healItemSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }

            if (theGreatCacher.players[steamID].hammerSkinName != player.hammerSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "hammer_" + player.hammerSkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "hammer_" + player.hammerSkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "hammer_" + player.hammerSkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }

            if (theGreatCacher.players[steamID].atlas01SkinName != player.atlas01SkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "atlas01_" + player.atlas01SkinName))
                {
                    if (enableThread)
                    {
                        string filePathEnd = "WeaponSkins/" + "atlas01_" + player.atlas01SkinName + ".png";
                        Thread childThreadCannons = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, "atlas01_" + player.atlas01SkinName, "weaponskin", webCli));
                        childThreadCannons.Start();
                    }
                }
            }
        }

        private static void addNewPlayer(playerObject player, WebClient webCli)
        {
            string fullWeaponString;
            List<string> alreadyDownloaded = new List<string>();

            // Badges
            if (player.badgeName != "default")
            {
                bool flag = alreadyDownloaded.Contains(player.badgeName);
                if (!flag)
                {
                    string filePathEnd = "Badges/" + player.badgeName + ".png";
                    Thread childThreadBadges = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.badgeName, "badge", webCli));
                    childThreadBadges.Start();
                    alreadyDownloaded.Add(player.badgeName);
                }
            }

            // Masks
            if (player.maskSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains(player.maskSkinName);
                if (!flag)
                {
                    string filePathEnd = "MaskSkins/" + player.maskSkinName + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.maskSkinName, "goldmask", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(player.maskSkinName);
                }
            }

            // Sails
            if (player.sailSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains(player.sailSkinName);
                if (!flag)
                {
                    string filePathEnd = "SailSkins/" + player.sailSkinName + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.sailSkinName, "sail", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(player.maskSkinName);
                }
            }

            if (player.mainSailName != "default")
            {
                bool flag = alreadyDownloaded.Contains(player.mainSailName);
                if (!flag)
                {
                    string filePathEnd = "MainSailSkins/" + player.mainSailName + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.mainSailName, "mainsail", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(player.mainSailName);
                }
            }

            // Cannons
            if (player.cannonSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains(player.cannonSkinName);
                if (!flag)
                {
                    string filePathEnd = "CannonSkins/" + player.cannonSkinName + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, player.cannonSkinName, "cannon", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(player.cannonSkinName);
                }
            }

            // Primary weapons
            if (player.musketSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("musket_" + player.musketSkinName);
                if (!flag)
                {
                    fullWeaponString = "musket_" + player.musketSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.blunderbussSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("blunderbuss_" + player.blunderbussSkinName);
                if (!flag)
                {
                    fullWeaponString = "blunderbuss_" + player.blunderbussSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.nockgunSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("nockgun_" + player.nockgunSkinName);
                if (!flag)
                {
                    fullWeaponString = "nockgun_" + player.nockgunSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.handMortarSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("handmortar_" + player.handMortarSkinName);
                if (!flag)
                {
                    fullWeaponString = "handmortar_" + player.handMortarSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            // Secondary Weapons
            if (player.standardPistolSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("standardPistol_" + player.standardPistolSkinName);
                if (!flag)
                {
                    fullWeaponString = "standardPistol_" + player.standardPistolSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.shortPistolSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("shortPistol_" + player.shortPistolSkinName);
                if (!flag)
                {
                    fullWeaponString = "shortPistol_" + player.shortPistolSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.duckfootSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("duckfoot_" + player.duckfootSkinName);
                if (!flag)
                {
                    fullWeaponString = "duckfoot_" + player.duckfootSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.matchlockRevolverSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("matchlock_" + player.matchlockRevolverSkinName);
                if (!flag)
                {
                    fullWeaponString = "matchlock_" + player.matchlockRevolverSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.annelyRevolverSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("annelyRevolver_" + player.annelyRevolverSkinName);
                if (!flag)
                {
                    fullWeaponString = "annelyRevolver_" + player.annelyRevolverSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);

                }
            }

            // Melee weapons
            if (player.axeSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("axe_" + player.axeSkinName);
                if (!flag)
                {
                    fullWeaponString = "axe_" + player.axeSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.rapierSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("rapier_" + player.rapierSkinName);
                if (!flag)
                {
                    fullWeaponString = "rapier_" + player.rapierSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.daggerSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("dagger_" + player.daggerSkinName);
                if (!flag)
                {
                    fullWeaponString = "dagger_" + player.daggerSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.bottleSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("bottle_" + player.bottleSkinName);
                if (!flag)
                {
                    fullWeaponString = "bottle_" + player.bottleSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.cutlassSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("cutlass_" + player.cutlassSkinName);
                if (!flag)
                {
                    fullWeaponString = "cutlass_" + player.cutlassSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.pikeSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("pike_" + player.pikeSkinName);
                if (!flag)
                {
                    fullWeaponString = "pike_" + player.pikeSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            // Specials
            if (player.tomohawkSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("tomohawk_" + player.tomohawkSkinName);
                if (!flag)
                {
                    fullWeaponString = "tomohawk_" + player.tomohawkSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.spyglassSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("spyglass_" + player.spyglassSkinName);
                if (!flag)
                {
                    fullWeaponString = "spyglass_" + player.spyglassSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.grenadeSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("grenade_" + player.grenadeSkinName);
                if (!flag)
                {
                    fullWeaponString = "grenade_" + player.grenadeSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.healItemSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("healItem_" + player.healItemSkinName);
                if (!flag)
                {
                    fullWeaponString = "healItem_" + player.healItemSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            // Hammer
            if (player.hammerSkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("hammer_" + player.hammerSkinName);
                if (!flag)
                {
                    fullWeaponString = "hammer_" + player.hammerSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }

            if (player.atlas01SkinName != "default")
            {
                bool flag = alreadyDownloaded.Contains("atlas01_" + player.atlas01SkinName);
                if (!flag)
                {
                    fullWeaponString = "atlas01_" + player.atlas01SkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Thread childThreadMasks = new Thread(() => ChildThreadGetAsset(texturesFilePath + filePathEnd, mainUrl + filePathEnd, fullWeaponString, "weaponskin", webCli));
                    childThreadMasks.Start();
                    alreadyDownloaded.Add(fullWeaponString);
                }
            }
        }

        private static void ChildThreadUpdateAll()
        {
            debugLog("Updating all");
            WebClient webCli = new WebClient();
            string response = webCli.DownloadString("http://www.archiesbots.com/BlackwakeStuff/playerList.json");
            string[] json = response.Split('&');
            for (int i = 0; i < json.Length; i++)
            {
                playerObject player = JsonUtility.FromJson<playerObject>(json[i]);
                if (theGreatCacher.players.ContainsKey(player.steamID))
                {
                    updateUser(player.steamID, player, webCli);
                }
                else
                {
                    Thread childThread = new Thread(() => addNewPlayer(player, webCli));
                    childThread.Start();
                }
            }
        }
    }
}
