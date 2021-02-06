using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using Harmony;
using UnityEngine;
using System.Collections;

namespace Alternion
{
    /// <summary>
    /// Originally for multithreading stuff.
    /// </summary>
    class ThreadCreationProgram : MonoBehaviour
    {
        /// <summary>
        /// Textures file path.
        /// </summary>
        public static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        static string mainUrl = "http://www.archiesbots.com/BlackwakeStuff/";
        public static ThreadCreationProgram Instance;
        static Texture2D tex;

        void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        public static Texture2D loadTexture(string texName, byte[] fileData, int imgWidth, int imgHeight)
        {
            try
            {
                tex.Resize(imgWidth, imgHeight);
                tex.LoadImage(fileData);
                tex.name = texName;
                return tex;
            }
            catch (Exception e)
            {
                Logger.debugLog(string.Format("Error loading texture {0}", texName));
                Logger.debugLog(e.Message);
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
                        Instance.StartCoroutine(Instance.userJoined(pName));
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog(e.Message);
                    }
                }
            }
        }

        private IEnumerator textureSetup(string path, string tName, string type)
        {
            WWW www = new WWW(mainUrl + path);
            yield return www;

            if (!File.Exists(Application.dataPath + texturesFilePath + tName))
            {
                File.WriteAllBytes(Application.dataPath + texturesFilePath + tName, www.texture.EncodeToPNG());
            }

            Texture newTex = www.texture;
            newTex.name = tName;

            switch (type)
            {
                case "badge":
                    theGreatCacher.badges.Add(tName, newTex);
                    break;
                case "sail":
                    theGreatCacher.secondarySails.Add(tName, newTex);
                    break;
                case "mainsail":
                    theGreatCacher.mainSails.Add(tName, newTex);
                    break;
                case "cannon":
                    theGreatCacher.cannonSkins.Add(tName, newTex);
                    break;
                case "goldmask":
                    theGreatCacher.maskSkins.Add(tName, newTex);
                    break;
                case "weaponskin":
                    theGreatCacher.weaponSkins.Add(tName, newTex);
                    break;
                default:
                    break;
            }
        }

        public IEnumerator userJoined(string pName)
        {
            // Sleep as it takes a while before the steamID is actually assigned to the PlayerInfo
            yield return new WaitForSeconds(.1f);

            string steamID = "0";
            if (GameMode.getPlayerInfo(pName) != null)
            {
                PlayerInfo plrInf = GameMode.getPlayerInfo(pName);
                steamID = plrInf.steamID.ToString();
            }


            WebClient webCli = new WebClient();
            // Fetch all players
            string response = webCli.DownloadString(mainUrl + AlternionSettings.remoteFile);
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
                                updateUser(steamID, player);
                                theGreatCacher.players[steamID] = player;
                            }
                            catch (Exception e)
                            {
                                Logger.debugLog(e.Message);
                            }
                        }
                    }
                    else
                    {
                        addNewPlayer(player);
                        theGreatCacher.players.Add(player.steamID, player);
                    }
                    break;
                }
            }
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

        private static void updateUser(string steamID, playerObject player)
        {
            // Overall stuff
            if (theGreatCacher.players[steamID].badgeName != player.badgeName)
            {
                if (!checkIfItemIsCached("badge", player.badgeName))
                {
                    string filePathEnd = "Badges/" + player.badgeName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.badgeName, "badge"));
                }
            }
            if (theGreatCacher.players[steamID].sailSkinName != player.sailSkinName)
            {
                if (!checkIfItemIsCached("sail", player.sailSkinName))
                {
                    string filePathEnd = "SailSkins/" + player.sailSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.sailSkinName, "sail"));
                }
            }
            if (theGreatCacher.players[steamID].mainSailName != player.mainSailName)
            {
                if (!checkIfItemIsCached("mainsail", player.mainSailName))
                {
                    string filePathEnd = "MainSailSkins/" + player.mainSailName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.mainSailName, "mainsail"));
                }
            }
            if (theGreatCacher.players[steamID].cannonSkinName != player.cannonSkinName)
            {
                if (!checkIfItemIsCached("cannon", player.cannonSkinName))
                {
                    string filePathEnd = "CannonSkins/" + player.cannonSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.cannonSkinName, "cannon"));
                }
            }
            if (theGreatCacher.players[steamID].maskSkinName != player.maskSkinName)
            {
                if (!checkIfItemIsCached("goldmask", player.maskSkinName))
                {
                    string filePathEnd = "MaskSkins/" + player.maskSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.maskSkinName, "goldmask"));
                }
            }

            // Weapon skins
            if (theGreatCacher.players[steamID].musketSkinName != player.musketSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "musket_" + player.musketSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "musket_" + player.musketSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.musketSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].blunderbussSkinName != player.blunderbussSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "blunderbuss_" + player.blunderbussSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "blunderbuss_" + player.blunderbussSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.blunderbussSkinName, "weaponskin"));

                }
            }
            if (theGreatCacher.players[steamID].nockgunSkinName != player.nockgunSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "nockgun_" + player.nockgunSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "nockgun_" + player.nockgunSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.nockgunSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].handMortarSkinName != player.handMortarSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "handmortar_" + player.handMortarSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "handmortar_" + player.handMortarSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.handMortarSkinName, "weaponskin"));
                }
            }

            if (theGreatCacher.players[steamID].standardPistolSkinName != player.standardPistolSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "standardPistol_" + player.standardPistolSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "standardPistol_" + player.standardPistolSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.standardPistolSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].shortPistolSkinName != player.shortPistolSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "shortPistol_" + player.shortPistolSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "shortPistol_" + player.shortPistolSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.standardPistolSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].duckfootSkinName != player.duckfootSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "duckfoot_" + player.duckfootSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "duckfoot_" + player.duckfootSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.duckfootSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].matchlockRevolverSkinName != player.matchlockRevolverSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "matchlock_" + player.matchlockRevolverSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "matchlock_" + player.matchlockRevolverSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.matchlockRevolverSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].annelyRevolverSkinName != player.annelyRevolverSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "annelyRevolver_" + player.annelyRevolverSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "annelyRevolver_" + player.annelyRevolverSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.annelyRevolverSkinName, "weaponskin"));
                }
            }

            if (theGreatCacher.players[steamID].axeSkinName != player.axeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "axe_" + player.axeSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "axe_" + player.axeSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.axeSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].rapierSkinName != player.rapierSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "rapier_" + player.rapierSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "rapier_" + player.rapierSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.rapierSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].daggerSkinName != player.daggerSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "dagger_" + player.daggerSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "dagger_" + player.daggerSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.daggerSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].bottleSkinName != player.bottleSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "bottle_" + player.bottleSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "bottle_" + player.bottleSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.bottleSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].cutlassSkinName != player.cutlassSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "cutlass_" + player.cutlassSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "cutlass_" + player.cutlassSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.cutlassSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].pikeSkinName != player.pikeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "pike_" + player.pikeSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "pike_" + player.pikeSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.pikeSkinName, "weaponskin"));
                }
            }

            if (theGreatCacher.players[steamID].tomahawkSkinName != player.tomahawkSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "tomahawk_" + player.tomahawkSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "tomahawk_" + player.tomahawkSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.tomahawkSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].spyglassSkinName != player.spyglassSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "spyglass_" + player.spyglassSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "spyglass_" + player.spyglassSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.spyglassSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].grenadeSkinName != player.grenadeSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "grenade_" + player.grenadeSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "grenade_" + player.grenadeSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.grenadeSkinName, "weaponskin"));
                }
            }
            if (theGreatCacher.players[steamID].healItemSkinName != player.healItemSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "healItem_" + player.healItemSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "healItem_" + player.healItemSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.healItemSkinName, "weaponskin"));
                }
            }

            if (theGreatCacher.players[steamID].hammerSkinName != player.hammerSkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "hammer_" + player.hammerSkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "hammer_" + player.hammerSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.hammerSkinName, "weaponskin"));
                }
            }

            if (theGreatCacher.players[steamID].atlas01SkinName != player.atlas01SkinName)
            {
                if (!checkIfItemIsCached("weaponskin", "atlas01_" + player.atlas01SkinName))
                {
                    string filePathEnd = "WeaponSkins/" + "atlas01_" + player.atlas01SkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.atlas01SkinName, "weaponskin"));
                }
            }
        }

        private static void addNewPlayer(playerObject player)
        {
            string fullWeaponString;

            // Badges
            if (player.badgeName != "default")
            {
                if (!theGreatCacher.badges.ContainsKey(player.badgeName))
                {
                    string filePathEnd = "Badges/" + player.badgeName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.atlas01SkinName, "badge"));
                }
            }

            // Masks
            if (player.maskSkinName != "default")
            {
                if (!theGreatCacher.maskSkins.ContainsKey(player.maskSkinName))
                {
                    string filePathEnd = "MaskSkins/" + player.maskSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.maskSkinName, "goldmask"));
                }
            }

            // Sails
            if (player.sailSkinName != "default")
            {
                if (!theGreatCacher.secondarySails.ContainsKey(player.sailSkinName))
                {
                    string filePathEnd = "SailSkins/" + player.sailSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.sailSkinName, "sail"));
                }
            }

            if (player.mainSailName != "default")
            {
                if (!theGreatCacher.mainSails.ContainsKey(player.mainSailName))
                {
                    string filePathEnd = "MainSailSkins/" + player.mainSailName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.mainSailName, "mainsail"));
                }
            }

            // Cannons
            if (player.cannonSkinName != "default")
            {
                if (!theGreatCacher.cannonSkins.ContainsKey(player.cannonSkinName))
                {
                    string filePathEnd = "CannonSkins/" + player.cannonSkinName + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.cannonSkinName, "cannon"));
                }
            }

            // Primary weapons
            if (player.musketSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.musketSkinName))
                {
                    fullWeaponString = "musket_" + player.musketSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.musketSkinName, "weapon"));
                }
            }

            if (player.blunderbussSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.blunderbussSkinName))
                {
                    fullWeaponString = "blunderbuss_" + player.blunderbussSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.blunderbussSkinName, "weapon"));
                }
            }

            if (player.nockgunSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.nockgunSkinName))
                {
                    fullWeaponString = "nockgun_" + player.nockgunSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.nockgunSkinName, "weapon"));
                }
            }

            if (player.handMortarSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.handMortarSkinName))
                {
                    fullWeaponString = "handmortar_" + player.handMortarSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.handMortarSkinName, "weapon"));
                }
            }

            // Secondary Weapons
            if (player.standardPistolSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.standardPistolSkinName))
                {
                    fullWeaponString = "standardPistol_" + player.standardPistolSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.standardPistolSkinName, "weapon"));
                }
            }

            if (player.shortPistolSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.shortPistolSkinName))
                {
                    fullWeaponString = "shortPistol_" + player.shortPistolSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.shortPistolSkinName, "weapon"));
                }
            }

            if (player.duckfootSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.duckfootSkinName))
                {
                    fullWeaponString = "duckfoot_" + player.duckfootSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.duckfootSkinName, "weapon"));
                }
            }

            if (player.matchlockRevolverSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.matchlockRevolverSkinName))
                {
                    fullWeaponString = "matchlock_" + player.matchlockRevolverSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.matchlockRevolverSkinName, "weapon"));
                }
            }

            if (player.annelyRevolverSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.annelyRevolverSkinName))
                {
                    fullWeaponString = "annelyRevolver_" + player.annelyRevolverSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.annelyRevolverSkinName, "weapon"));

                }
            }

            // Melee weapons
            if (player.axeSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.axeSkinName))
                {
                    fullWeaponString = "axe_" + player.axeSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.axeSkinName, "weapon"));
                }
            }

            if (player.rapierSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.rapierSkinName))
                {
                    fullWeaponString = "rapier_" + player.rapierSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.rapierSkinName, "weapon"));
                }
            }

            if (player.daggerSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.daggerSkinName))
                {
                    fullWeaponString = "dagger_" + player.daggerSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.daggerSkinName, "weapon"));
                }
            }

            if (player.bottleSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.bottleSkinName))
                {
                    fullWeaponString = "bottle_" + player.bottleSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.bottleSkinName, "weapon"));
                }
            }

            if (player.cutlassSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.cutlassSkinName))
                {
                    fullWeaponString = "cutlass_" + player.cutlassSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.cutlassSkinName, "weapon"));
                }
            }

            if (player.pikeSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.pikeSkinName))
                {
                    fullWeaponString = "pike_" + player.pikeSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.pikeSkinName, "weapon"));
                }
            }

            // Specials
            if (player.tomahawkSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.tomahawkSkinName))
                {
                    fullWeaponString = "tomohawk_" + player.tomahawkSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.tomahawkSkinName, "weapon"));
                }
            }

            if (player.spyglassSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.spyglassSkinName))
                {
                    fullWeaponString = "spyglass_" + player.spyglassSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.spyglassSkinName, "weapon"));
                }
            }

            if (player.grenadeSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.grenadeSkinName))
                {
                    fullWeaponString = "grenade_" + player.grenadeSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.grenadeSkinName, "weapon"));
                }
            }

            if (player.healItemSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.healItemSkinName))
                {
                    fullWeaponString = "healItem_" + player.healItemSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.healItemSkinName, "weapon"));
                }
            }

            // Hammer
            if (player.hammerSkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.hammerSkinName))
                {
                    fullWeaponString = "hammer_" + player.hammerSkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.hammerSkinName, "weapon"));
                }
            }

            if (player.atlas01SkinName != "default")
            {
                if (!theGreatCacher.weaponSkins.ContainsKey(player.atlas01SkinName))
                {
                    fullWeaponString = "atlas01_" + player.atlas01SkinName;
                    string filePathEnd = "WeaponSkins/" + fullWeaponString + ".png";
                    Instance.StartCoroutine(Instance.textureSetup(filePathEnd, player.atlas01SkinName, "weapon"));
                }
            }
        }
        
    }
}
