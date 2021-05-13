using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BWModLoader;
using Harmony;
using Steamworks;


namespace Alternion
{

    /// <summary>
    /// Main class.
    /// </summary>
    [Mod]
    public class Mainmod : MonoBehaviour
    {
        /// <summary>
        /// Watermark Texture
        /// </summary>
        Texture2D watermarkTex;
        /// <summary>
        /// True if watermark has been setup.
        /// </summary>
        bool setWatermark = false;
        /// <summary>
        /// Filepath to the textures.
        /// </summary>
        public static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        /// <summary>
        /// Website URL.
        /// </summary>
        static string mainUrl = "http://www.archiesbots.com/BlackwakeStuff/";

        public static Mainmod Instance;

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
        void Start()
        {
            try
            {
                //Setup harmony patching
                try
                {

                    HarmonyInstance harmony = HarmonyInstance.Create("com.github.archie");
                    harmony.PatchAll();
                }catch(Exception e)
                {
                    Logger.debugLog(e.Message);
                }

                //Starts asset fetching cycle
                createDirectories();

                //Setup watermark
                StartCoroutine(waterMark());
            }
            catch (Exception e)
            {
                Logger.debugLog(e.Message);
            }

        }
        void OnGUI()
        {
            if (setWatermark && AlternionSettings.enableWaterMark)
            {
                GUI.DrawTexture(new Rect(10, 10, 64, 52), watermarkTex, ScaleMode.ScaleToFit);
            }
        }

        //Fetching players and textures

        /// <summary>
        /// Loads the json file from the website.
        /// </summary>
        private IEnumerator loadJsonFile()
        {
            LoadingBar.updatePercentage(0, "Fetching Players");

            WWW www = new WWW(mainUrl + AlternionSettings.remoteFile);
            yield return www;
            string[] json = www.text.Split('&');

            for (int i = 0; i < json.Length; i++)
            {
                // I didn't want to do this, but unity has forced my hand
                playerObject player = JsonUtility.FromJson<playerObject>(json[i]);
                try
                {
                    theGreatCacher.Instance.players.Add(player.steamID, player);
                }
                catch (Exception e)
                {
                    Logger.debugLog("------------------");
                    Logger.debugLog("Loading from JSON error");
                    Logger.debugLog("Attempted User: " + player.steamID);
                    Logger.debugLog(e.Message);
                    Logger.debugLog(json[i]);
                    Logger.debugLog("------------------");
                }
                LoadingBar.updatePercentage(0 + (20 * ((float)i / (float)json.Length)), "Downloading players...");
            }
            LoadingBar.updatePercentage(20, "Finished getting players");
            StartCoroutine(downloadTextures());
        }
        /// <summary>
        /// Fetches and loads the watermark.
        /// </summary>
        private IEnumerator waterMark()
        {
            byte[] bytes = null;
            // Check if pfp is already downloaded or not
            if (!File.Exists(Application.dataPath + texturesFilePath + "pfp.png"))
            {
                WWW www = new WWW(mainUrl + "pfp.png");
                yield return www;

                try
                {
                    bytes = www.texture.EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + "pfp.png", bytes);
                }
                catch (Exception e)
                {
                    Logger.debugLog("Error downloading watermark:");
                    Logger.debugLog(e.Message);
                }

            }

            watermarkTex = loadTexture("pfp", texturesFilePath, 258, 208);
            if (watermarkTex != null)
            {
                setWatermark = true;
            }
        }
        /// <summary>
        /// Downloads all the textures.
        /// </summary>
        /// 
        private IEnumerator downloadTextures()
        {
            List<string> alreadyDownloaded = new List<string>();
            // pre-declare so I don't create lots of new objects each loop, and to keep it readable
            WWW www;
            bool flag;
            Texture newTex;
            string fullWeaponString;
            // Update loading image
            LoadingBar.updatePercentage(20, "Downloading and assigning assets...");

            //Grab Player textures
            int count = 0;
            foreach (KeyValuePair<string, playerObject> player in theGreatCacher.Instance.players)
            {
                // I don't think I have ever typed the word "default" as much as I did the last few days

                // Badges
                if (player.Value.badgeName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.badgeName);
                    if (!flag)
                    {
                        if (AlternionSettings.downloadOnStartup)
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
                                Logger.debugLog(e.Message);
                            }
                        }

                        try
                        {
                            newTex = loadTexture(player.Value.badgeName, texturesFilePath + "Badges/", 100, 40);
                            newTex.name = player.Value.badgeName;
                            theGreatCacher.Instance.badges.Add(player.Value.badgeName, newTex);
                            alreadyDownloaded.Add(player.Value.badgeName);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog("------------------");
                            Logger.debugLog("Badge Download Error");
                            Logger.debugLog(e.Message);
                            Logger.debugLog("------------------");
                        }

                    }
                }

                // Masks
                if (player.Value.maskSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.maskSkinName);
                    if (!flag)
                    {
                        if (AlternionSettings.downloadOnStartup)
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
                                Logger.debugLog(e.Message);
                            }
                        }

                        try
                        {

                            newTex = loadTexture(player.Value.maskSkinName, texturesFilePath + "MaskSkins/", 1024, 1024);
                            newTex.name = player.Value.maskSkinName;
                            theGreatCacher.Instance.maskSkins.Add(player.Value.maskSkinName, newTex);
                            alreadyDownloaded.Add(player.Value.maskSkinName);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog("------------------");
                            Logger.debugLog("Mask Download Error");
                            Logger.debugLog(e.Message);
                            Logger.debugLog("------------------");
                        }
                    }
                }

                // Sails
                if (player.Value.sailSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.sailSkinName);
                    if (!flag)
                    {
                        if (AlternionSettings.downloadOnStartup)
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
                                Logger.debugLog("------------------");
                                Logger.debugLog("Sail Skin Download Error");
                                Logger.debugLog(e.Message);
                            }
                        }

                        try
                        {
                            newTex = loadTexture(player.Value.sailSkinName, texturesFilePath + "SailSkins/", 2048, 2048);
                            newTex.name = player.Value.sailSkinName;
                            theGreatCacher.Instance.secondarySails.Add(player.Value.sailSkinName, newTex);
                            alreadyDownloaded.Add(player.Value.sailSkinName);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog("------------------");
                            Logger.debugLog("Normal Sail Setup Error");
                            Logger.debugLog(e.Message);
                            Logger.debugLog("------------------");
                        }
                    }
                }
                if (player.Value.mainSailName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.mainSailName);
                    if (!flag)
                    {
                        if (AlternionSettings.downloadOnStartup)
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
                                Logger.debugLog(e.Message);
                            }
                        }

                        try
                        {
                            newTex = loadTexture(player.Value.mainSailName, texturesFilePath + "MainSailSkins/", 2048, 2048);
                            newTex.name = player.Value.mainSailName;
                            theGreatCacher.Instance.mainSails.Add(player.Value.mainSailName, newTex);
                            alreadyDownloaded.Add(player.Value.mainSailName);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog("------------------");
                            Logger.debugLog("Main Sail Download Error");
                            Logger.debugLog(e.Message);
                            Logger.debugLog("------------------");
                        }
                    }
                }

                // Cannons
                if (player.Value.cannonSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.cannonSkinName);
                    if (!flag)
                    {
                        if (AlternionSettings.downloadOnStartup)
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
                                Logger.debugLog(e.Message);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "CannonSkins/" + player.Value.cannonSkinName + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "CannonSkins/" + player.Value.cannonSkinName + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + player.Value.cannonSkinName);
                            }
                        }
                        newTex = loadTexture(player.Value.cannonSkinName, texturesFilePath + "CannonSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.cannonSkins.Add(player.Value.cannonSkinName, newTex);
                        }
                        newTex = loadTexture(player.Value.cannonSkinName + "_met", texturesFilePath + "CannonSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.cannonSkins.Add(player.Value.cannonSkinName + "_met", newTex);
                        }
                        alreadyDownloaded.Add(player.Value.cannonSkinName);
                    }
                }
                if (player.Value.swivelSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.swivelSkinName);
                    if (!flag)
                    {
                        if (AlternionSettings.downloadOnStartup)
                        {
                            www = new WWW(mainUrl + "SwivelSkins/" + player.Value.swivelSkinName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "SwivelSkins/" + player.Value.swivelSkinName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                Logger.debugLog(e.Message);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "SwivelSkins/" + player.Value.swivelSkinName + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "SwivelSkins/" + player.Value.swivelSkinName + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + player.Value.swivelSkinName);
                            }
                        }
                        newTex = loadTexture(player.Value.swivelSkinName, texturesFilePath + "SwivelSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.swivels.Add(player.Value.swivelSkinName, newTex);
                        }
                        newTex = loadTexture(player.Value.swivelSkinName + "_met", texturesFilePath + "SwivelSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.swivels.Add(player.Value.swivelSkinName + "_met", newTex);
                        }
                        alreadyDownloaded.Add(player.Value.swivelSkinName);
                    }
                }

                // Flags
                if (player.Value.flagNavySkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.flagNavySkinName);
                    if (!flag)
                    {
                        if (AlternionSettings.downloadOnStartup)
                        {
                            www = new WWW(mainUrl + "Flags/" + player.Value.flagNavySkinName + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "Flags/" + player.Value.flagNavySkinName + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + player.Value.flagNavySkinName);
                            }

                        }
                        newTex = loadTexture(player.Value.flagNavySkinName, texturesFilePath + "Flags/", 1024, 512);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.flags.Add(player.Value.flagNavySkinName, newTex);
                            Logger.logLow($"Added -{player.Value.flagNavySkinName}-");
                        }
                        alreadyDownloaded.Add(player.Value.flagNavySkinName);
                    }
                }
                if (player.Value.flagPirateSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.flagPirateSkinName);
                    if (!flag)
                    {
                        if (AlternionSettings.downloadOnStartup)
                        {
                            www = new WWW(mainUrl + "Flags/" + player.Value.flagPirateSkinName + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "Flags/" + player.Value.flagPirateSkinName + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + player.Value.flagPirateSkinName);
                            }

                        }
                        newTex = loadTexture(player.Value.flagPirateSkinName, texturesFilePath + "Flags/", 1024, 512);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.flags.Add(player.Value.flagPirateSkinName, newTex);
                            Logger.logLow($"Added -{player.Value.flagPirateSkinName}-");
                        }
                        alreadyDownloaded.Add(player.Value.flagPirateSkinName);
                    }
                }

                // Primary weapons
                if (player.Value.musketSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("musket_" + player.Value.musketSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "musket_" + player.Value.musketSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }



                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.blunderbussSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("blunderbuss_" + player.Value.blunderbussSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "blunderbuss_" + player.Value.blunderbussSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB Maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }
                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.nockgunSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("nockgun_" + player.Value.nockgunSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "nockgun_" + player.Value.nockgunSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.handMortarSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("handmortar_" + player.Value.handMortarSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "handmortar_" + player.Value.handMortarSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }

                // Secondary Weapons
                if (player.Value.standardPistolSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("standardPistol_" + player.Value.standardPistolSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "standardPistol_" + player.Value.standardPistolSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.shortPistolSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("shortPistol_" + player.Value.shortPistolSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "shortPistol_" + player.Value.shortPistolSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.duckfootSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("duckfoot_" + player.Value.duckfootSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "duckfoot_" + player.Value.duckfootSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.matchlockRevolverSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("matchlock_" + player.Value.matchlockRevolverSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "matchlock_" + player.Value.matchlockRevolverSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.annelyRevolverSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("annelyRevolver_" + player.Value.annelyRevolverSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "annelyRevolver_" + player.Value.annelyRevolverSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }

                // Melee weapons
                if (player.Value.axeSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("axe_" + player.Value.axeSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "axe_" + player.Value.axeSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.rapierSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("rapier_" + player.Value.rapierSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "rapier_" + player.Value.rapierSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.daggerSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("dagger_" + player.Value.daggerSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "dagger_" + player.Value.daggerSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.bottleSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("bottle_" + player.Value.bottleSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "bottle_" + player.Value.bottleSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.cutlassSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("cutlass_" + player.Value.cutlassSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "cutlass_" + player.Value.cutlassSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.pikeSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("pike_" + player.Value.pikeSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "pike_" + player.Value.pikeSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }

                // Specials
                if (player.Value.tomahawkSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("tomahawk_" + player.Value.tomahawkSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "tomahawk_" + player.Value.tomahawkSkinName;

                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.spyglassSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("spyglass_" + player.Value.spyglassSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "spyglass_" + player.Value.spyglassSkinName;
                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.grenadeSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("grenade_" + player.Value.grenadeSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "grenade_" + player.Value.grenadeSkinName;
                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.healItemSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("healItem_" + player.Value.healItemSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "healItem_" + player.Value.healItemSkinName;
                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.teaCupSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("teaCup_" + player.Value.teaCupSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "teaCup_" + player.Value.teaCupSkinName;
                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.teaWaterSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("teaWater_" + player.Value.teaWaterSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "teaWater_" + player.Value.teaWaterSkinName;
                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }
                if (player.Value.bucketSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("bucket_" + player.Value.bucketSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "bucket_" + player.Value.bucketSkinName;
                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }

                // Hammer
                if (player.Value.hammerSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("hammer_" + player.Value.hammerSkinName);
                    if (!flag)
                    {
                        fullWeaponString = "hammer_" + player.Value.hammerSkinName;
                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }

                if (player.Value.atlas01SkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("atlas01_" + player.Value.atlas01SkinName);
                    if (!flag)
                    {
                        fullWeaponString = "atlas01_" + player.Value.atlas01SkinName;
                        if (AlternionSettings.downloadOnStartup)
                        {
                            // ALB maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                Logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            theGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }

                count++;
                float newPercentage = 20.0f + (60.0f * ((float)count / theGreatCacher.Instance.players.Count));
                LoadingBar.updatePercentage(newPercentage, "Downloading Textures");
            }
            // outputPlayerDict();
            Logger.logLow("Complete download!");
            setupMainMenu();
        }

        /// <summary>
        /// Creates required directories.
        /// </summary>
        public void createDirectories()
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
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "SwivelSkins/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "MaskSkins/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "Flags/");
            }

            //Grab online JSON file
            StartCoroutine(loadJsonFile());
        }

        /// <summary>
        /// Begins the main menu setup.
        /// </summary>
        public static void setupMainMenu()
        {
            LoadingBar.updatePercentage(90, "Preparing Main Menu");
            if (!AlternionSettings.useWeaponSkins && !AlternionSettings.useBadges)
            {
                LoadingBar.updatePercentage(100, "Finished!");
                return;
            }
            MainMenuCL.setMainMenuBadge();
        }

        /// <summary>
        /// Resets all ship assets to default textures. Cannons + Sails
        /// </summary>
        static void resetAllShipsToDefault()
        {
            // Loop through all ships, and set all visuals to defaults in the following order:
            // Sails
            // Main Sails
            // Functioning cannons
            // Destroyed cannons
            foreach (KeyValuePair<string, cachedShip> individualShip in theGreatCacher.Instance.ships)
            {
                // Only reset if sail texture has been set
                if (theGreatCacher.Instance.defaultSails)
                {
                    foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.sailDict)
                    {
                        indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.Instance.defaultSails;
                    }

                    foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.mainSailDict)
                    {
                        indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.Instance.defaultSails;
                    }

                    foreach (Renderer renderer in individualShip.Value.closedSails)
                    {
                        renderer.material.mainTexture = theGreatCacher.Instance.defaultSails;
                    }
                    individualShip.Value.hasChangedSails = false;
                }

                // Only reset if cannon texture has been set
                if (theGreatCacher.Instance.defaultCannons)
                {
                    foreach (KeyValuePair<string, CannonUse> indvidualCannon in individualShip.Value.cannonOperationalDict)
                    {
                        indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.Instance.defaultCannons);
                    }
                    foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in individualShip.Value.cannonDestroyDict)
                    {
                        indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.Instance.defaultCannons);
                    }

                    individualShip.Value.cannonLOD.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                    individualShip.Value.cannonLOD.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                    individualShip.Value.hasChangedCannons = false;
                }

                // Only reset if swivel texture has been set
                if (theGreatCacher.Instance.setSwivelDefaults)
                {
                    foreach (Renderer swivel in individualShip.Value.Swivels)
                    {
                        swivel.material.SetTexture("_MainTex", theGreatCacher.Instance.defaultSwivel);
                        swivel.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSwivelMet);
                    }
                }

            }
        }

        /// <summary>
        /// Sets the new textures for cached ships.
        /// </summary>
        /// <param name="steamID">Captain SteamID</param>
        /// <param name="index">Ship Index</param>
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
                if (theGreatCacher.Instance.ships.TryGetValue(index, out cachedShip mightyVessel))
                {
                    if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                    {
                        // Only apply new texture if config has sail textures enabled
                        if (AlternionSettings.useSecondarySails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                if (theGreatCacher.Instance.secondarySails.TryGetValue(player.sailSkinName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                                else
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.Instance.defaultSails;
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.Instance.defaultSails;
                            }
                        }

                        // Only apply new texture if config has main sail textures enabled
                        if (AlternionSettings.useMainSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                if (theGreatCacher.Instance.mainSails.TryGetValue(player.mainSailName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                                else
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.Instance.defaultSails;
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.Instance.defaultSails;
                            }
                            mightyVessel.hasChangedSails = false;
                        }

                        // Only apply new texture if config has cannon textures enabled
                        if (AlternionSettings.useCannonSkins)
                        {
                            foreach (KeyValuePair<string, CannonUse> indvidualCannon in mightyVessel.cannonOperationalDict)
                            {
                                Renderer renderer = indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>();
                                if (theGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                                {
                                    renderer.material.mainTexture = newTex;
                                }
                                else
                                {
                                    renderer.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                                }

                                if (theGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                                {
                                    renderer.material.SetTexture("_Metallic", newTex);
                                }
                                else
                                {
                                    renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                                }
                            }

                            if (theGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                            {
                                mightyVessel.cannonLOD.material.mainTexture = newTex;
                            }
                            else
                            {
                                mightyVessel.cannonLOD.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                            }

                            if (theGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_Metallic", newTex);
                            }
                            else
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                            }
                        }
                        else if (mightyVessel.hasChangedCannons)
                        {
                            foreach (KeyValuePair<string, CannonUse> indvidualCannon in mightyVessel.cannonOperationalDict)
                            {
                                Renderer renderer = indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>();
                                renderer.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                                renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                            }

                            mightyVessel.cannonLOD.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                            mightyVessel.cannonLOD.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                            mightyVessel.hasChangedCannons = false;
                        }

                        // Only apply new texture if config has cannon textures enabled
                        if (AlternionSettings.useCannonSkins)
                        {
                            foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in mightyVessel.cannonDestroyDict)
                            {
                                Renderer renderer = indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>();
                                if (theGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                                {
                                    renderer.material.mainTexture = newTex;
                                }
                                else
                                {
                                    renderer.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                                }

                                if (theGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                                {
                                    renderer.material.SetTexture("_Metallic", newTex);
                                }
                                else
                                {
                                    renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedCannons)
                        {
                            foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in mightyVessel.cannonDestroyDict)
                            {
                                Renderer renderer = indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>();
                                renderer.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                                renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                            }
                        }

                        // Only apply new texture if config has flag textures enabled
                        if (AlternionSettings.showFlags)
                        {
                            string flagSkin = mightyVessel.isNavy ? player.flagNavySkinName : player.flagPirateSkinName;
                            if (flagSkin != "default" && theGreatCacher.Instance.flags.TryGetValue(flagSkin, out newTex))
                            {
                                flagHandler.Instance.setFlagsToSkin(mightyVessel, newTex);
                            }
                            else
                            {
                                flagHandler.Instance.resetFlag(mightyVessel);
                            }
                        }
                        else if (mightyVessel.hasChangedFlag)
                        {
                            flagHandler.Instance.resetFlag(mightyVessel);
                        }

                        // Only apply new texture if config has swivel textures enabled
                        if (AlternionSettings.useSwivelSkins)
                        {
                            foreach (Renderer swivel in mightyVessel.Swivels)
                            {
                                if (theGreatCacher.Instance.swivelSkins.TryGetValue(player.swivelSkinName, out newTex))
                                {
                                    swivel.material.mainTexture = newTex;
                                }
                                else
                                {
                                    swivel.material.mainTexture = theGreatCacher.Instance.defaultSwivel;
                                }

                                if (theGreatCacher.Instance.swivelSkins.TryGetValue(player.swivelSkinName + "_met", out newTex))
                                {
                                    swivel.material.SetTexture("_Metallic", newTex);
                                }
                                else
                                {
                                    swivel.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSwivelMet);
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedSwivels)
                        {
                            foreach (Renderer swivel in mightyVessel.Swivels)
                            {
                                swivel.material.mainTexture = theGreatCacher.Instance.defaultSwivel;
                                swivel.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSwivelMet);
                            }
                            mightyVessel.hasChangedSwivels = false;
                        }
                    }
                    else
                    {
                        if (mightyVessel.hasChangedCannons)
                        {
                            foreach (KeyValuePair<string, CannonUse> indvidualCannon in mightyVessel.cannonOperationalDict)
                            {
                                if (theGreatCacher.Instance.setCannonDefaults)
                                {
                                    Renderer renderer = indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>();
                                    renderer.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                                    renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                                }
                            }
                            foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in mightyVessel.cannonDestroyDict)
                            {
                                if (theGreatCacher.Instance.setCannonDefaults)
                                {
                                    Renderer renderer = indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>();
                                    renderer.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                                    renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                                }
                            }
                            
                            mightyVessel.cannonLOD.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                            mightyVessel.cannonLOD.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                            
                            mightyVessel.hasChangedCannons = false;
                        }
                        if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                if (theGreatCacher.Instance.setSailDefaults)
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.Instance.defaultSails;
                                }
                            }
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                if (theGreatCacher.Instance.setSailDefaults)
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.Instance.defaultSails;
                                }
                            }
                            mightyVessel.hasChangedSails = false;
                        }
                        if (mightyVessel.hasChangedSwivels)
                        {
                            foreach (Renderer swivel in mightyVessel.Swivels)
                            {
                                swivel.material.mainTexture = theGreatCacher.Instance.defaultSwivel;
                                swivel.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSwivelMet);
                            }
                            mightyVessel.hasChangedSwivels = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.logLow(e.Message);
                //Ignore Exception
            }
        }

        /// <summary>
        /// Loads a texture based on the name, width and height.
        /// </summary>
        /// <param name="texName">Image Name</param>
        /// <param name="imgWidth">Image Width</param>
        /// <param name="imgHeight">Image Height</param>
        /// <returns>Texture2D</returns>
        public static Texture2D loadTexture(string texName, string filePath, int imgWidth, int imgHeight)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(Application.dataPath + filePath + texName + ".png");
                Texture2D tex = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24, false);
                tex.LoadImage(fileData);
                tex.name = texName;
                return tex;
            }
            catch (Exception e)
            {
                Logger.debugLog("Error loading texture {0}" + texName);
                Logger.debugLog(e.Message);
                // Return default white texture on failing to load
                Texture2D tex = Texture2D.whiteTexture;
                tex.name = "FAILED";
                return tex;
            }
        }

        /// <summary>
        /// Checks if input badge is a Kickstarter or Tournamentwake badge
        /// </summary>
        /// <param name="__instance">ScoreboardSlot</param> 
        /// /// <returns>Bool</returns>
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
        /// /// <returns>Bool</returns>
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

        /// <summary>
        /// Harmony patch to setup badges in the scoreboard
        /// </summary>
        [HarmonyPatch(typeof(ScoreboardSlot), "ñòæëíîêïæîí", new Type[] { typeof(string), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        static class scoreBoardSlotAdjuster
        {
            static void Postfix(ScoreboardSlot __instance, string ìåäòäóëäêèæ, int óîèòèðîðçîì, string ñíçæóñðæéòó, int çïîèïçïñêïæ, int äïóïåòòéðåç, int ìëäòìèçñçìí, int óíïòðíäóïçç, int íîóìóíèíñìå, bool ðèæòðìêóëïð, bool äåîéíèñèììñ, bool æíèòîîìðçóî, int ïîñíñóóåîîñ, int æìíñèéçñîíí, bool òêóçíïåæíîë, bool æåèòðéóçêçó, bool èëçòëæêäêîå, bool ëååííåïäæîè, bool ñîäèñæïîóçó)
            {
                if (AlternionSettings.useBadges)
                {
                    try
                    {
                        string steamID = GameMode.getPlayerInfo(ìåäòäóëäêèæ).steamID.ToString();
                        if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                        {
                            // if they have a TW OR KS badge, this will dictate if it should or shouldn't override it visually
                            if (checkIfTWOrKS(__instance) && theGreatCacher.Instance.badges.TryGetValue(player.badgeName, out Texture newTexture))
                            {
                                __instance.éòëèïòëóæèó.texture = newTexture;
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
                            Logger.debugLog("Failed to assign custom badge to a player:");
                            Logger.debugLog(e.Message);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Harmony patch to set badge in endRound scene
        /// </summary>
        [HarmonyPatch(typeof(AccoladeItem), "ëîéæìêìëéæï")]
        static class accoladeSetInfoPatch
        {
            static void Postfix(AccoladeItem __instance, string óéíïñîèëëêð, int òææóïíéñåïñ, string çìîñìëðêëéò, CSteamID ìçíêääéïíòç)
            {
                // Sets win screen badges
                if (AlternionSettings.useBadges)
                {
                    string steamID = ìçíêääéïíòç.m_SteamID.ToString();
                    if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                    {
                        if (checkIfTWOrKS(óéíïñîèëëêð) && theGreatCacher.Instance.badges.TryGetValue(player.badgeName, out Texture newTex))
                        {
                            __instance.äæåéåîèòéîñ.texture = newTex;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Harmony patch to trigger resetting of ships
        /// </summary>
        [HarmonyPatch(typeof(GameMode), "newRound")]
        static class newRoundPatch
        {
            static void Postfix(GameMode __instance)
            {
                // Reset all ship skins that are cached on newRound() to default textures
                resetAllShipsToDefault();
            }
        }

        /// <summary>
        /// Harmony patch to setup ships on captain pass
        /// </summary>
        [HarmonyPatch(typeof(PlayerOptions), "passCaptain")]
        static class passCaptainPatch
        {
            static void Prefix(PlayerOptions __instance)
            {
                // Untested
                try
                {
                    if (LocalPlayer.îêêæëçäëèñî.äíìíëðñïñéè.isCaptain())
                    {
                        PlayerInfo player = GameMode.getPlayerInfo(__instance.êåééóæåñçòì.text);
                        string steamNewCaptainID = player.steamID.ToString();
                        string teamNum = player.team.ToString();
                        assignNewTexturesToShips(steamNewCaptainID, teamNum);
                    }
                }catch (Exception e)
                {
                    Logger.debugLog("##########################################################");
                    Logger.debugLog("Pass captain patch.");
                    Logger.debugLog(e.Message);
                    Logger.debugLog("##########################################################");
                }
            }
        }
#if DEBUG
        private IEnumerator handleBird(PlayerInfo __instance)
        {
            yield return new WaitForSeconds(.5f);
            __instance.isTournyWinner = true;
            Logger.logLow($"{!__instance.character.æïðèñìæêêñç}");
            __instance.character.óððêäóäîçñè.SetActive(!__instance.character.æïðèñìæêêñç); // Only enable if not owner
            Logger.logLow("IsBird");
            if (__instance.character.æïðèñìæêêñç)
            {
                __instance.character.óððêäóäîçñè.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly; // Sets to only use shadows, so it still shows in FP
                Logger.logLow("Removed from FP");
            }
            if (__instance.lobbyPlayer != null)
            {
                __instance.lobbyPlayer.ñéçåäçëñåæê = true;
            }
        }
        /// <summary>
        /// Harmony patch to disable bird in first person
        /// </summary>
        [HarmonyPatch(typeof(PlayerInfo), "setWinner")]
        static class isWinnerPatch
        {
            static bool Prefix(PlayerInfo __instance, ïçîìäîóäìïæ.åéðñðçîîïêç info)
            {
                if (info.åéñëîíèðòçé)
                {
                    Instance.StartCoroutine(Instance.handleBird(__instance));
                }

                return false;
            }
        }
#endif

    }
}
