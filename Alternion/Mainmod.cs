using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BWModLoader;
using Harmony;
using Steamworks;


namespace Alternion
{
    

    [Mod]
    public class Mainmod : MonoBehaviour
    {
        Texture2D watermarkTex;

        public static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        static string mainUrl = "http://www.archiesbots.com/BlackwakeStuff/";

        void Start()
        {
            try
            {
                //Setup harmony patching
                HarmonyInstance harmony = HarmonyInstance.Create("com.github.archie");
                harmony.PatchAll();

                //Starts asset fetching cycle
                createDirectories();

                //Setup watermark
                StartCoroutine(waterMark());

                //Rotate Character
                //InvokeRepeating("rotateMainMenuCharacter", 1, 0.1f);
            }
            catch (Exception e)
            {
                debugLog(e.Message);
            }
        }

        void OnGUI()
        {
            if (watermarkTex != null)
            {
                GUI.DrawTexture(new Rect(10, 10, 64, 52), watermarkTex, ScaleMode.ScaleToFit);
            }
        }

        //Fetching players and textures
        private IEnumerator loadJsonFile()
        {
            LoadingBar.updatePercentage(0, "Fetching Players");

            WWW www = new WWW(mainUrl + "playerObjectList2.json");
            yield return www;

            try
            {
                string[] json = www.text.Split('&');
                for (int i = 0; i < json.Length; i++)
                {
                    playerObject player = JsonUtility.FromJson<playerObject>(json[i]);
                    theGreatCacher.players.Add(player.steamID, player);
                    LoadingBar.updatePercentage(0 + (20 * ((float)i / (float)json.Length)), "Downloading players");
                }
            }
            catch (Exception e)
            {
                debugLog("------------------");
                debugLog("Loading from JSON error");
                debugLog(e.Message);
                debugLog("------------------");
            }
            LoadingBar.updatePercentage(20, "Finished getting players");
            StartCoroutine(DownloadTextures());
        }
        private IEnumerator waterMark()
        {
            if (!File.Exists(Application.dataPath + texturesFilePath + "pfp.png"))
            {
                WWW www = new WWW(mainUrl + "pfp.png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + "pfp.png", bytes);
                }
                catch (Exception e)
                {
                    debugLog("Error downloading watermark:");
                    debugLog(e.Message);
                }

            }

            watermarkTex = loadTexture("pfp", texturesFilePath, 258, 208);
        }
        private IEnumerator DownloadTextures()
        {
            List<string> alreadyDownloaded = new List<string>();
            WWW www;
            bool flag;
            Texture newTex;
            string fullWeaponString;
            LoadingBar.updatePercentage(20, "Preparing to download");
            //Grab UI textures
            logLow("Player Count:" + theGreatCacher.players.Count.ToString());
            //Grab Player textures
            for (int i = 0; i < theGreatCacher.players.Count; i++)
            {
                foreach (KeyValuePair<string, playerObject> player in theGreatCacher.players)
                {
                    // Badges
                    if (player.Value.badgeName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.badgeName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "Badges/" + player.Value.badgeName + ".png");
                            yield return www;
                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "Badges/" + player.Value.badgeName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(player.Value.badgeName, texturesFilePath + "Badges/", 100, 40);
                                newTex.name = player.Value.badgeName;
                                theGreatCacher.badges.Add(player.Value.badgeName, newTex);
                                alreadyDownloaded.Add(player.Value.badgeName);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Badge Download Error");
                                debugLog(e.Message);
                                debugLog("------------------");
                            }

                        }
                    }

                    // Masks
                    if (player.Value.maskSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.maskSkinName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "MaskSkins/" + player.Value.maskSkinName + ".png");
                            yield return www;
                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "MaskSkins/" + player.Value.maskSkinName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }
                            try
                            {

                                newTex = loadTexture(player.Value.maskSkinName, texturesFilePath + "MaskSkins/", 100, 40);
                                newTex.name = player.Value.maskSkinName;
                                theGreatCacher.maskSkins.Add(player.Value.maskSkinName, newTex);
                                alreadyDownloaded.Add(player.Value.maskSkinName);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Mask Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    // Sails
                    if (player.Value.sailSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.sailSkinName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "SailSkins/" + player.Value.sailSkinName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "SailSkins/" + player.Value.sailSkinName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Sail Skin Download Error");
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(player.Value.sailSkinName, texturesFilePath + "SailSkins/", 2048, 2048);
                                newTex.name = player.Value.sailSkinName;
                                theGreatCacher.secondarySails.Add(player.Value.sailSkinName, newTex);
                                alreadyDownloaded.Add(player.Value.sailSkinName);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog(e.Message);
                                debugLog("------------------");
                            }
                        }
                    }

                    if (player.Value.mainSailName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.mainSailName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "MainSailSkins/" + player.Value.mainSailName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "MainSailSkins/" + player.Value.mainSailName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }
                            try
                            {
                                newTex = loadTexture(player.Value.mainSailName, texturesFilePath + "MainSailSkins/", 2048, 2048);
                                newTex.name = player.Value.mainSailName;
                                theGreatCacher.mainSails.Add(player.Value.mainSailName, newTex);
                                alreadyDownloaded.Add(player.Value.mainSailName);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Main Sail Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    // Cannons
                    if (player.Value.cannonSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.cannonSkinName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "CannonSkins/" + player.Value.cannonSkinName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "CannonSkins/" + player.Value.cannonSkinName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(player.Value.cannonSkinName, texturesFilePath + "CannonSkins/", 2048, 2048);
                                newTex.name = player.Value.cannonSkinName;
                                theGreatCacher.cannonSkins.Add(player.Value.cannonSkinName, newTex);
                                alreadyDownloaded.Add(player.Value.cannonSkinName);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Cannon Skin Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    // Primary weapons
                    if (player.Value.musketSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("musket_" + player.Value.musketSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "musket_" + player.Value.musketSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Musket Skin Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.blunderbussSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("blunderbuss_" + player.Value.blunderbussSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "blunderbuss_" + player.Value.blunderbussSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Blunderbuss Skin Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.nockgunSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("nockgun_" + player.Value.nockgunSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "nockgun_" + player.Value.nockgunSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Nockgun Skin Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.handMortarSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("handmortar_" + player.Value.handMortarSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "handmortar_" + player.Value.handMortarSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Handmortar Skin Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    // Secondary Weapons
                    if (player.Value.standardPistolSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("standardPistol_" + player.Value.standardPistolSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "standardPistol_" + player.Value.standardPistolSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Standard Pistol Skin Download Error: " + player.Value.standardPistolSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.shortPistolSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("shortPistol_" +  player.Value.shortPistolSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "shortPistol_" + player.Value.shortPistolSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Short Pistol Skin Download Error: " + player.Value.shortPistolSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.duckfootSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("duckfoot_" + player.Value.duckfootSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "duckfoot_" + player.Value.duckfootSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Duckfoot Skin Download Error: " + player.Value.duckfootSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.matchlockRevolverSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("matchlock_" + player.Value.matchlockRevolverSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "matchlock_" + player.Value.matchlockRevolverSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Matchlock Skin Download Error: " + player.Value.matchlockRevolverSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.annelyRevolverSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("annelyRevolver_" + player.Value.annelyRevolverSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "annelyRevolver_" + player.Value.annelyRevolverSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + "annelyRevolver_" + player.Value.annelyRevolverSkinName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }
                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Annely Skin Download Error: " + player.Value.annelyRevolverSkinName);
                                debugLog(e.Message);
                            }

                        }
                    }

                    // Melee weapons
                    if (player.Value.axeSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("axe_" + player.Value.axeSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "axe_" + player.Value.axeSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Axe Skin Download Error: " + player.Value.axeSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.rapierSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("rapier_" + player.Value.rapierSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "rapier_" + player.Value.rapierSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Rapier Skin Download Error: " + player.Value.rapierSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.daggerSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("dagger_" + player.Value.daggerSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "dagger_" + player.Value.daggerSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Dagger Skin Download Error: " + player.Value.daggerSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.bottleSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("bottle_" + player.Value.bottleSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "bottle_" + player.Value.bottleSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Bottle Skin Download Error: " + player.Value.bottleSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.cutlassSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("cutlass_" + player.Value.cutlassSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "cutlass_" + player.Value.cutlassSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Cutlass Skin Download Error: " + player.Value.cutlassSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.pikeSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("pike_" + player.Value.pikeSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "pike_" + player.Value.pikeSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Pike Skin Download Error: " + player.Value.pikeSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    // Specials
                    if (player.Value.tomohawkSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("tomohawk_" + player.Value.tomohawkSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "tomohawk_" + player.Value.tomohawkSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Tomohawk Skin Download Error: " + player.Value.tomohawkSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.spyglassSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("spyglass_" + player.Value.spyglassSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "spyglass_" + player.Value.spyglassSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Spyglass Skin Download Error: " + player.Value.spyglassSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.grenadeSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("grenade_" + player.Value.grenadeSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "grenade_" + player.Value.grenadeSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Grenade Skin Download Error: " + player.Value.grenadeSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.healItemSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("healItem_" + player.Value.healItemSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "healItem_" + player.Value.healItemSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }
                            
                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("HealItem Skin Download Error: " + player.Value.healItemSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    // Hammer
                    if (player.Value.hammerSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("hammer_" + player.Value.hammerSkinName);
                        if (!flag)
                        {
                            fullWeaponString = "hammer_" + player.Value.hammerSkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Hammer Skin Download Error: " + player.Value.hammerSkinName);
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.atlas01SkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains("atlas01_" + player.Value.atlas01SkinName);
                        if (!flag)
                        {
                            fullWeaponString = "atlas01_" + player.Value.atlas01SkinName;
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                newTex.name = fullWeaponString;
                                theGreatCacher.weaponSkins.Add(fullWeaponString, newTex);
                                alreadyDownloaded.Add(fullWeaponString);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Atlas Skin Download Error: " + player.Value.atlas01SkinName);
                                debugLog(e.Message);
                            }
                        }
                    }
                }

                float newPercentage = 20 + (60 * ((float)i / (float)theGreatCacher.players.Count));
                LoadingBar.updatePercentage(newPercentage, "Downloading Textures");
            }
            // outputPlayerDict();
            setupMainMenu();
        }

        void createDirectories()
        {
            //Create directories prior to downloading all asset files
            if (!File.Exists(Application.dataPath + texturesFilePath))
            {
                Directory.CreateDirectory(Application.dataPath + texturesFilePath);
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "Badges/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "WeaponSkins/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "SailSkins/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "MainSailSkins/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "CannonSkins/"); 
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "MaskSkins/");
            }

            //Grab online JSON file
            logLow("Starting JSON fetch");
            StartCoroutine(loadJsonFile());
        }
        static void setupMainMenu()
        {
            LoadingBar.updatePercentage(90, "Preparing Main Menu");
            if (!AlternionSettings.useWeaponSkins && !AlternionSettings.useBadges)
            {
                LoadingBar.updatePercentage(100, "Finished!");
                return;
            }
            setMainmenuBadge();
        }
        static void setMainmenuBadge()
        {

            if (!AlternionSettings.useBadges)
            {
                LoadingBar.updatePercentage(95, "applying weapon skin");
                setMainMenuWeaponSkin();
                return;
            }

            //Only main menu that you will really see is the one intially started
            //This doesn't work if you return to the main menu from a server
            MainMenu mm = FindObjectOfType<MainMenu>();

            try
            {
                string steamID = Steamworks.SteamUser.GetSteamID().ToString();
                if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                {
                    if (mm.menuBadge.texture.name != "tournamentWake1Badge" ^ (!AlternionSettings.showTWBadges & mm.menuBadge.texture.name == "tournamentWake1Badge"))
                    {
                        if (theGreatCacher.badges.TryGetValue(player.badgeName, out Texture newTex))
                        {
                            mm.menuBadge.texture = newTex;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                debugLog("Failed to assign custom badge to a player:");
                debugLog(e.Message);
            }

            LoadingBar.updatePercentage(95, "Applying weapon skin");
            setMainMenuWeaponSkin();

        }
        static void setMainMenuWeaponSkin()
        {
            try
            {
                if (!AlternionSettings.useWeaponSkins)
                {
                    LoadingBar.updatePercentage(100, "Finished!");
                    return;
                }
                string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                {
                    //if (player.weaponSkins.TryGetValue("musket", out string weaponSkin))
                    //{
                        var musket = GameObject.Find("wpn_standardMusket_LOD1");
                    if (musket != null)
                    {
                        if (theGreatCacher.weaponSkins.TryGetValue("musket_" + player.musketSkinName, out Texture newTex))
                        {
                            musket.GetComponent<Renderer>().material.mainTexture = newTex;
                        }
                    }
                    else
                    {
                        debugLog("Main menu musket not found.");
                    }
                    //}
                }
            }
            catch (Exception e)
            {
                debugLog(e.Message);
            }

            LoadingBar.updatePercentage(100, "Finished!");
        }

        static void resetAllShipsToDefault()
        {
            // Loop through all ships, and set all visuals to defaults in the following order:
            // Sails
            // Main Sails
            // Functioning cannons
            // Destroyed cannons
            foreach (KeyValuePair<string, cachedShip> individualShip in theGreatCacher.ships)
            {
                foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.sailDict)
                {
                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                }

                foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.mainSailDict)
                {
                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                }

                foreach (KeyValuePair<string, CannonUse> indvidualCannon in individualShip.Value.cannonOperationalDict)
                {
                    indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
                }

                foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in individualShip.Value.cannonDestroyDict)
                {
                    indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
                }
            }
        }

        //NEEDS FIXING
        static void assignNewTexturesToShips(string steamID, string index)
        {
            try
            {
                // Loop through all cached vessels and apply new textures in the following order:
                // Sails
                // Main Sails
                // Functional Cannons
                // Destroyed Cannons
                Texture newTex;
                if (theGreatCacher.ships.TryGetValue(index, out cachedShip mightyVessel))
                {
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        if (AlternionSettings.useSecondarySails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                            }
                        }

                        if (AlternionSettings.useMainSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                            }
                        }

                        if (AlternionSettings.useCannonSkins)
                        {
                            foreach (KeyValuePair<string, CannonUse> indvidualCannon in mightyVessel.cannonOperationalDict)
                            {
                                if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                                {
                                    indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
                                }
                            }
                        }

                        if (AlternionSettings.useCannonSkins)
                        {
                            foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in mightyVessel.cannonDestroyDict)
                            {
                                if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                                {
                                    indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                debugLog(e.Message);
                //Ignore Exception
            }
        }

        static void assignWeaponToRenderer(Renderer renderer, string weaponSkin, string weapon)
        {
            try
            {
                // If the player Dict contains a reference to the specific weapon, output the texture
                logLow(weapon + "_" + weaponSkin);
                if (weaponSkin != "null")
                {
                    if (theGreatCacher.weaponSkins.TryGetValue(weapon + "_" + weaponSkin, out Texture newTexture))
                    {
                        renderer.material.mainTexture = newTexture;
                    }
                }
            }
            catch (Exception e)
            {
                debugLog(e.Message);
            }
        }

        static void weaponSkinHandler(WeaponRender __instance, playerObject player)
        {

            Renderer renderer = __instance.GetComponent<Renderer>();

            switch (renderer.material.mainTexture.name)
            {
                case "wpn_standardMusket_stock_alb":
                    assignWeaponToRenderer(renderer, player.musketSkinName, "musket");
                    break;
                case "wpn_standardCutlass_alb":
                    assignWeaponToRenderer(renderer, player.cutlassSkinName, "cutlass");
                    break;
                case "wpn_blunderbuss_alb":
                    assignWeaponToRenderer(renderer, player.blunderbussSkinName, "blunderbuss");
                    break;
                case "wpn_nockGun_stock_alb":
                    assignWeaponToRenderer(renderer, player.nockgunSkinName, "nockgun");
                    break;
                case "wpn_handMortar_alb":
                    assignWeaponToRenderer(renderer, player.handMortarSkinName, "handmortar");
                    break;
                case "wpn_rapier_alb":
                    assignWeaponToRenderer(renderer, player.rapierSkinName, "rapier");
                    break;
                case "wpn_dagger_alb":
                    assignWeaponToRenderer(renderer, player.daggerSkinName, "dagger");
                    break;
                case "wpn_bottle_alb":
                    assignWeaponToRenderer(renderer, player.bottleSkinName, "bottle");
                    break;
                case "wpn_rumHealth_alb":
                    assignWeaponToRenderer(renderer, player.healItemSkinName, "healItem");
                    break;
                case "prp_hammer_alb":
                    assignWeaponToRenderer(renderer, player.hammerSkinName, "hammer");
                    break;
                case "wpn_standardPistol_stock_alb":
                    assignWeaponToRenderer(renderer, player.standardPistolSkinName, "standardPistol");
                    break;
                case "prp_atlas01_alb":
                    assignWeaponToRenderer(renderer, player.atlas01SkinName, "atlas01");
                    break;
                //case "prp_bucket_alb":
                //    assignWeaponToRenderer(renderer, player, "bucket");
                //    break;
                case "wpn_shortpistol_alb":
                    assignWeaponToRenderer(renderer, player.shortPistolSkinName, "shortPistol");
                    break;
                case "wpn_duckfoot_alb":
                    assignWeaponToRenderer(renderer, player.duckfootSkinName, "duckfoot");
                    break;
                case "wpn_annelyRevolver_alb":
                    assignWeaponToRenderer(renderer, player.annelyRevolverSkinName, "annelyRevolver");
                    break;
                case "wpn_tomohawk_alb":
                    assignWeaponToRenderer(renderer, player.tomohawkSkinName, "tomohawk");
                    break;
                case "wpn_matchlockRevolver_alb":
                    assignWeaponToRenderer(renderer, player.matchlockRevolverSkinName, "matchlockRevolver");
                    break;
                case "wpn_twoHandAxe_alb":
                    assignWeaponToRenderer(renderer, player.axeSkinName, "axe");
                    break;
                case "wpn_boardingPike_alb":
                    assignWeaponToRenderer(renderer, player.pikeSkinName, "pike");
                    break;
                case "wpn_spyglass_alb":
                    assignWeaponToRenderer(renderer, player.spyglassSkinName, "spyglass");
                    break;
                default:
                    // If not known, output here
                    //logLow("Default name: -" + renderer.material.mainTexture.name + "-");
                    break;
            }
        }

        //Debugging purposes
        static void logLow(string message)
        {
            //Just easier to type than Log.logger.Log
            // Also lets me just set logLevel to 0 if I dont want to deal with the spam.
            if (AlternionSettings.loggingLevel > 0)
            {
                Log.logger.Log(message);
            }
        }
        
        //ALWAYS RUNS
        static void debugLog(string message)
        {
            //Just easier to type than Log.logger.Log
            //Will always log, so only use in try{} catch(Exception e) {} when absolutely needed
            Log.logger.Log(message);
        }

        public static Texture2D loadTexture(string texName, string filePath, int imgWidth, int imgHeight)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(Application.dataPath + filePath + texName + ".png");

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

        [HarmonyPatch(typeof(ScoreboardSlot), "ñòæëíîêïæîí", new Type[] { typeof(string), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        static class ScoreBoardSlotAdjuster
        {
            static void Postfix(ScoreboardSlot __instance, string ìåäòäóëäêèæ, int óîèòèðîðçîì, string ñíçæóñðæéòó, int çïîèïçïñêïæ, int äïóïåòòéðåç, int ìëäòìèçñçìí, int óíïòðíäóïçç, int íîóìóíèíñìå, bool ðèæòðìêóëïð, bool äåîéíèñèììñ, bool æíèòîîìðçóî, int ïîñíñóóåîîñ, int æìíñèéçñîíí, bool òêóçíïåæíîë, bool æåèòðéóçêçó, bool èëçòëæêäêîå, bool ëååííåïäæîè, bool ñîäèñæïîóçó)
            {
                try
                {
                    if (!AlternionSettings.useBadges)
                    {
                        return;
                    }
                    string steamID = GameMode.getPlayerInfo(ìåäòäóëäêèæ).steamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        if (__instance.éòëèïòëóæèó.texture.name != "tournamentWake1Badge" ^ (!AlternionSettings.showTWBadges & __instance.éòëèïòëóæèó.texture.name == "tournamentWake1Badge"))
                        {
                            if (theGreatCacher.badges.TryGetValue(steamID, out Texture newTexture))
                            {
                                __instance.éòëèïòëóæèó.texture = newTexture; // loadTexture(badgeName[i], 110, 47);
                            }
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
                        debugLog("Failed to assign custom badge to a player:");
                        debugLog(e.Message);
                    }
                }

            }
        }

        //applyGold()
        [HarmonyPatch(typeof(WeaponRender), "ìæóòèðêççæî")]
        static class goldApplyPatch
        {
            static void Postfix(WeaponRender __instance)
            {
                try
                {
                    if (!AlternionSettings.useWeaponSkins)
                    {
                        return;
                    }
                    PlayerInfo plyrInf = __instance.ìäóêäðçóììî.ìêïòëîåëìòñ.gameObject.transform.parent.parent.GetComponent<PlayerInfo>();
                    string steamID = plyrInf.steamID.ToString();
                    logLow("[GA] Gotten: " + steamID);
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        logLow("[GA] Gotten player");
                        weaponSkinHandler(__instance, player);
                    }
                }
                catch (Exception e)
                {
                    debugLog("err: " + e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(Character), "setGoldMask")]
        static class goldMaskPatch
        {
            static void Postfix(Character __instance)
            {
                try
                {
                    if (AlternionSettings.useMaskSkins)
                    {
                        string steamID = __instance.transform.parent.GetComponent<PlayerInfo>().steamID.ToString();
                        if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                        {  
                            if (theGreatCacher.maskSkins.TryGetValue(player.maskSkinName, out Texture newTex))
                            {
                                Renderer renderer = __instance.éäéïéðïåææè.transform.GetComponent<Renderer>();
                                renderer.material.mainTexture = newTex;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    debugLog("err: " + e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenu), "Start")]
        static class mainMenuStuffPatch
        {
            static void Postfix(MainMenu __instance)
            {
                setMainmenuBadge();
                setMainMenuWeaponSkin();
            }
        }

        [HarmonyPatch(typeof(MainMenu), "toggleKSBadge")]
        static class toggleKSPatch
        {
            static void Postfix(MainMenu __instance, bool on)
            {
                if (!AlternionSettings.useBadges)
                {
                    if (!on)
                    {
                        setMainmenuBadge();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WeaponRender), "Start")]
        static class weaponSkinpatch1stPerson
        {
            static void Postfix(WeaponRender __instance)
            {
                try
                {
                    if (!AlternionSettings.useWeaponSkins)
                    {
                        return;
                    }
                    if (!__instance.åïääìêêäéèç)
                    {
                        //Grab local steamID
                        string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                        logLow("[WS] Gotten: " + steamID);
                        if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                        {
                            logLow("[WS] Gotten player");
                            weaponSkinHandler(__instance, player);
                        }
                    }
                }
                catch (Exception e)
                {
                    debugLog(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(SailHealth), "OnEnable")]
        static class sailSkinPatch
        {
            static void Postfix(SailHealth __instance)
            {
                try
                {
                    if (!AlternionSettings.useMainSails && !AlternionSettings.useSecondarySails)
                    {
                        return;
                    }
                    Transform shipTransf = __instance.transform.root;
                    if (shipTransf)
                    {
                        int teamNum = int.Parse(shipTransf.name.Split('m')[1]);

                        string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

                        if (!theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                        {
                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                            return;
                        }

                        string shipType = GameMode.Instance.shipTypes[teamNum - 1];
                        shipType = shipType.Remove(shipType.Length - 1);


                        switch (shipType)
                        {
                            case "cruiser":
                                if (__instance.name == "hmsSophie_sails08" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }


                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }


                                if (__instance.name != "hmsSophie_sails08")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "galleon":
                                if (__instance.name == "galleon_sails_01" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }


                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "galleon_sails_01")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "brig":
                                if (__instance.name == "hmsSpeedy_sails04" && AlternionSettings.useMainSails)
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "hmsSpeedy_sails04")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "xebec":
                                if (__instance.name == "xebec_sail03" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "xebec_sail03")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "bombvessel":
                                if (__instance.name == "bombVessel_sails07" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "bombVessel_sails07")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "gunboat":
                                if (__instance.name == "gunboat_sails02" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "gunboat_sails02")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "cutter":
                                if (__instance.name == "hmsAlert_sails02" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "hmsAlert_sails02")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "bombketch":
                                if (__instance.name == "bombKetch_sails06" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "bombKetch_sails06")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "carrack":
                                if (__instance.name == "carrack_sail03" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "carrack_sail03")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "junk":
                                if (__instance.name == "junk_sails_01" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "junk_sails_01")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "schooner":
                                if ((__instance.name == "schooner_sails02" && AlternionSettings.useMainSails) || (__instance.name == "schooner_sails00" && AlternionSettings.useMainSails))
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "schooner_sails02" && __instance.name != "schooner_sails00")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    debugLog(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(CannonUse), "OnEnable")]
        static class cannonOperationalSkinPatch
        {
            static void Postfix(CannonUse __instance)
            {
                try
                {
                    if (!AlternionSettings.useCannonSkins)
                    {
                        return;
                    }
                    Transform child = __instance.transform.FindChild("cannon");
                    int.TryParse( child.transform.root.name.Split('m')[1] , out int index);
                    string steamID = GameMode.Instance.teamCaptains[index - 1].steamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        // If vessel is already cached, grab it and add, otherwise create new vessel
                        if (theGreatCacher.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                        {
                            vessel.cannonOperationalDict.Add((vessel.cannonOperationalDict.Count + 1).ToString(), __instance);
                        }
                        else
                        {
                            cachedShip newVessel = new cachedShip();
                            newVessel.cannonOperationalDict.Add("1", __instance);
                            theGreatCacher.ships.Add(index.ToString(), newVessel);
                        }

                        // If they have a custom texture, use it, else use default skin
                        if (player.cannonSkinName != "null")
                        {
                            if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out Texture newTex))
                            {
                                child.GetComponent<Renderer>().material.mainTexture = newTex;
                            }
                        }
                        else
                        {
                            if (theGreatCacher.defaultCannons != null)
                            {
                                child.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultCannons;
                            }
                        }
                    }
                    else
                    {
                        child.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultCannons;
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
                        debugLog("Cannon operational error start");
                        debugLog(e.Message);
                        debugLog("Cannon operational error end");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CannonDestroy), "Start")]
        static class cannonDestroySkinPatch
        {
            static void Postfix(CannonDestroy __instance)
            {
                try
                {
                    if (!AlternionSettings.useCannonSkins)
                    {
                        return;
                    }

                    int.TryParse(__instance.æïìçñðåììêç.transform.root.name.Split('m')[1], out int index);
                    string steamID = GameMode.Instance.teamCaptains[index - 1].steamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        // If vessel is cached, add cannon to it, else create new vessel
                        if (theGreatCacher.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                        {
                            vessel.cannonDestroyDict.Add((vessel.cannonDestroyDict.Count + 1).ToString(), __instance);
                        }
                        else
                        {
                            cachedShip newVessel = new cachedShip();
                            newVessel.cannonDestroyDict.Add("1", __instance);
                            theGreatCacher.ships.Add(index.ToString(), newVessel);
                        }


                        // If they have a cannon skin then apply
                        if (player.cannonSkinName != "null")
                        {
                            if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out Texture newTex))
                            {
                                __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
                            }
                        }
                    }else
                    {
                        __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
                    }
                }catch (Exception e)
                {
                    debugLog("Cannon destroy start");
                    debugLog(e.Message);
                    debugLog("Cannon destroy end");
                }
            }
        }

        [HarmonyPatch(typeof(AccoladeItem), "ëîéæìêìëéæï")]
        static class accoladeSetInfoPatch
        {
            static void Postfix(AccoladeItem __instance, string óéíïñîèëëêð, int òææóïíéñåïñ, string çìîñìëðêëéò, CSteamID ìçíêääéïíòç)
            {
                // Sets win screen badges
                try
                {
                    if (!AlternionSettings.useBadges)
                    {
                        return;
                    }
                    string steamID = ìçíêääéïíòç.m_SteamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        if (theGreatCacher.badges.TryGetValue(player.badgeName, out Texture newTex))
                        {
                            __instance.äæåéåîèòéîñ.texture = newTex;
                        }
                    }
                }
                catch (Exception e)
                {
                    debugLog(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(GameMode), "newRound")]
        static class newRoundPatch
        {
            static void Postfix(GameMode __instance)
            {
                // Reset all ship skins that are cached on newRound() to default textures
                resetAllShipsToDefault();
            }
        }

        [HarmonyPatch(typeof(PlayerOptions), "passCaptain")]
        static class passCaptainPatch
        {
            static void Prefix(PlayerOptions __instance)
            {
                // Untested
                if (LocalPlayer.îêêæëçäëèñî.äíìíëðñïñéè.isCaptain())
                {
                    PlayerInfo player = GameMode.getPlayerInfo(__instance.êåééóæåñçòì.text);
                    string steamNewCaptainID = player.steamID.ToString();
                    string teamNum = player.team.ToString();
                    assignNewTexturesToShips(steamNewCaptainID, teamNum);
                }
            }
        }

    }
}
