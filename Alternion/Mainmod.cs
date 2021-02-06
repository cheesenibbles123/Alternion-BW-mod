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
        /// Main menu character transform.
        /// </summary>
        static Transform menuCharacter;
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
                HarmonyInstance harmony = HarmonyInstance.Create("com.github.archie");
                harmony.PatchAll();

                //Starts asset fetching cycle
                createDirectories();

                //Setup watermark
                StartCoroutine(waterMark());

                //Rotate Character
                setMenuCharacter();

            }
            catch (Exception e)
            {
                debugLog(e.Message);
            }

        }
        void OnGUI()
        {
            if (setWatermark && AlternionSettings.enableWaterMark)
            {
                GUI.DrawTexture(new Rect(10, 10, 64, 52), watermarkTex, ScaleMode.ScaleToFit);
            }
        }
        void Update()
        {
            if (!óèïòòåææäêï.åìçæçìíäåóë.activeSelf && global::Input.GetMouseButton(1) && menuCharacter)
            {
                // If it has been found
                // Rotation code copied from CharacterCustomizationUI
                menuCharacter.Rotate(Vector3.up, 1000f * Time.deltaTime * -global::Input.GetAxisRaw("Mouse X"));

            }

            if (Input.GetKeyUp("`"))
            {
                debugLog("v7.0");
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
                    theGreatCacher.players.Add(player.steamID, player);
                }
                catch (Exception e)
                {
                    debugLog("------------------");
                    debugLog("Loading from JSON error");
                    debugLog("Attempted User: " + player.steamID);
                    debugLog(e.Message);
                    debugLog("------------------");
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
                    debugLog("Error downloading watermark:");
                    debugLog(e.Message);
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
            for (int i = 0; i < theGreatCacher.players.Count; i++)
            {
                foreach (KeyValuePair<string, playerObject> player in theGreatCacher.players)
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
                                    debugLog(e.Message);
                                }
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
                                    debugLog(e.Message);
                                }
                            }

                            try
                            {

                                newTex = loadTexture(player.Value.maskSkinName, texturesFilePath + "MaskSkins/", 1024, 1024);
                                newTex.name = player.Value.maskSkinName;
                                theGreatCacher.maskSkins.Add(player.Value.maskSkinName, newTex);
                                alreadyDownloaded.Add(player.Value.maskSkinName);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Mask Download Error");
                                debugLog(e.Message);
                                debugLog("------------------");
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
                                    debugLog("------------------");
                                    debugLog("Sail Skin Download Error");
                                    debugLog(e.Message);
                                }
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
                                debugLog("Normal Sail Setup Error");
                                debugLog(e.Message);
                                debugLog("------------------");
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
                                    debugLog(e.Message);
                                }
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
                                debugLog("------------------");
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
                                    debugLog(e.Message);
                                }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }

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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("Tomahawk Skin Download Error: " + player.Value.tomahawkSkinName);
                                debugLog(e.Message);
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("Teacup Skin Download Error: " + player.Value.teaCupSkinName);
                                debugLog(e.Message);
                                debugLog("------------------");
                            }
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
                                debugLog("Tea Water Skin Download Error: " + player.Value.teaWaterSkinName);
                                debugLog(e.Message);
                                debugLog("------------------");
                            }
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
                                debugLog("HealItem Skin Download Error: " + player.Value.bucketSkinName);
                                debugLog(e.Message);
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
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
                                debugLog("------------------");
                            }
                        }
                    }
                }

                float newPercentage = 20 + (60 * ((float)i / (float)theGreatCacher.players.Count));
                LoadingBar.updatePercentage(newPercentage, "Downloading Textures");
            }
            // outputPlayerDict();
            logLow("Complete download!");
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
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "MaskSkins/");
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
            setMainMenuBadge();
        }
        /// <summary>
        /// Sets the main menu badge.
        /// </summary>
        static void setMainMenuBadge()
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
                    debugLog($"Got player {player.steamID} => {player.badgeName}");
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
        /// <summary>
        /// Sets the main menu weapon skin.
        /// </summary>
        static void setMainMenuWeaponSkin()
        {
            if (AlternionSettings.useWeaponSkins)
            {
                try
                {
                    string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
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
                    }
                }
                catch (Exception e)
                {
                    debugLog(e.Message);
                }
            }

            LoadingBar.updatePercentage(100, "Finished!");
        }
        /// <summary>
        /// Sets the main menu character transform.
        /// </summary>
        static void setMenuCharacter()
        {
            // Find the musket object
            var musket = GameObject.Find("wpn_standardMusket_LOD1");
            if (musket != null)
            {
                // If it exists, then go to root and find the character model in the heirachy
                Transform rootTransf = musket.transform.root;
                foreach (Transform transform in rootTransf)
                {
                    if (transform.name == "default_character_rig")
                    {
                        // Save it for the rotating in Update()
                        menuCharacter = transform;
                        break;
                    }
                }
            }
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
            foreach (KeyValuePair<string, cachedShip> individualShip in theGreatCacher.ships)
            {
                // Only reset if sail texture has been set
                if (theGreatCacher.defaultSails)
                {
                    foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.sailDict)
                    {
                        indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                    }
                }
                if (theGreatCacher.defaultSails)
                {
                    foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.mainSailDict)
                    {
                        indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                    }
                }

                // Only reset if cannon texture has been set
                if (theGreatCacher.defaultCannons)
                {
                    foreach (KeyValuePair<string, CannonUse> indvidualCannon in individualShip.Value.cannonOperationalDict)
                    {
                        indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
                    }
                }
                if (theGreatCacher.defaultCannons)
                {
                    foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in individualShip.Value.cannonDestroyDict)
                    {
                        indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
                    }
                }
            }
        }

        //NEEDS FIXING
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
                if (theGreatCacher.ships.TryGetValue(index, out cachedShip mightyVessel))
                {
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        // Only apply new texture if config has sail textures enabled
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

                        // Only apply new texture if config has main sail textures enabled
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

                        // Only apply new texture if config has cannon textures enabled
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

                        // Only apply new texture if config has cannon textures enabled
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
        /// <summary>
        /// Assigns the weapon skin to the weapon.
        /// </summary>
        /// <param name="renderer">Renderer to apply to</param>
        /// <param name="weaponSkin">Weapon skin to use</param>
        /// <param name="weapon">Name of the Weapon</param>
        static void assignWeaponToRenderer(Renderer renderer, string weaponSkin, string weapon)
        {
            try
            {
                debugLog($"Applying: {weaponSkin} to {weapon}");
                // If the player Dict contains a reference to the specific weapon, output the texture
                if (weaponSkin != "default")
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
        /// <summary>
        /// Handles the finding of which skin, to apply to the weapon, based off the player setup and weapon equipped.
        /// </summary>
        /// <param name="__instance">WeaponRender Instance</param>
        /// <param name="player">Player loadout</param>
        static void weaponSkinHandler(WeaponRender __instance, playerObject player)
        {

            Renderer renderer = __instance.GetComponent<Renderer>();
            //foreach (Transform transf in __instance.transform.parent)
            //{
            //logLow(transf.name);
            //}
            //logLow("__instance.transform.parent.parent:");
            //foreach (Transform transf in __instance.transform.parent.parent)
            //{
            //    logLow(transf.name);
            //}

            // Needs a rework as the following share the same texture, and so return the same texture name:
            // Axe + Rapier
            // Dagger + Cutlass
            switch (renderer.material.mainTexture.name)
            {
                case "wpn_standardMusket_stock_alb":
                    assignWeaponToRenderer(renderer, player.musketSkinName, "musket");
                    break;
                case "wpn_standardCutlass_alb":
                    if (renderer.name == "Cutlass" || renderer.name == "wpn_cutlass_LOD1")
                    {
                        assignWeaponToRenderer(renderer, player.cutlassSkinName, "cutlass");
                    }
                    else if (renderer.name == "wpn_dagger")
                    {
                        assignWeaponToRenderer(renderer, player.daggerSkinName, "dagger");
                    }
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
                case "wpn_dagger_alb":
                    assignWeaponToRenderer(renderer, player.daggerSkinName, "dagger");
                    break;
                case "wpn_bottle_alb":
                    assignWeaponToRenderer(renderer, player.bottleSkinName, "bottle");
                    break;
                case "wpn_rumHealth_alb":
                    assignWeaponToRenderer(renderer, player.healItemSkinName, "bottleHealth");
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
                case "prp_bucket_alb":
                    assignWeaponToRenderer(renderer, player.bucketSkinName, "bucket");
                    break;
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
                    assignWeaponToRenderer(renderer, player.tomahawkSkinName, "tomahawk");
                    break;
                case "wpn_matchlockRevolver_alb":
                    assignWeaponToRenderer(renderer, player.matchlockRevolverSkinName, "matchlock");
                    break;
                case "wpn_twoHandAxe_alb":
                    if (renderer.name == "wpn_twoHandAxe")
                    {
                        assignWeaponToRenderer(renderer, player.axeSkinName, "axe");
                    }
                    else if (renderer.name == "wpn_rapier")
                    {
                        assignWeaponToRenderer(renderer, player.rapierSkinName, "rapier");
                    }
                    break;
                case "wpn_boardingPike_alb":
                    assignWeaponToRenderer(renderer, player.pikeSkinName, "pike");
                    break;
                case "wpn_spyglass_alb":
                    assignWeaponToRenderer(renderer, player.spyglassSkinName, "spyglass");
                    break;
                case "prp_teaCup_alb":
                    assignWeaponToRenderer(renderer, player.teaCupSkinName, "teaCup");
                    break;
                case "tea_alb":
                    assignWeaponToRenderer(renderer, player.teaWaterSkinName, "teaWater");
                    break;
                default:
                    // If not known, output here
                    //logLow("Type name: -" + renderer.name + "-");
                    //logLow("Default name: -" + renderer.material.mainTexture.name + "-");
                    break;
            }
        }

        /// <summary>
        /// Logs low priority stuff.
        /// </summary>
        /// <param name="message">Message to Log</param>
        static void logLow(string message)
        {
            //Just easier to type than Log.logger.Log
            // Also lets me just set logLevel to 0 if I dont want to deal with the spam.
            if (AlternionSettings.loggingLevel > 0)
            {
                Log.logger.Log(message);
            }
        }
        /// <summary>
        /// Always logs, no matter the logging level.
        /// </summary>
        /// <param name="message">Message to Log</param>
        static void debugLog(string message)
        {
            //Just easier to type than Log.logger.Log
            //Will always log, so only use in try{} catch(Exception e) {} when absolutely needed
            Log.logger.Log(message);
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
        static class scoreBoardSlotAdjuster
        {
            static void Postfix(ScoreboardSlot __instance, string ìåäòäóëäêèæ, int óîèòèðîðçîì, string ñíçæóñðæéòó, int çïîèïçïñêïæ, int äïóïåòòéðåç, int ìëäòìèçñçìí, int óíïòðíäóïçç, int íîóìóíèíñìå, bool ðèæòðìêóëïð, bool äåîéíèñèììñ, bool æíèòîîìðçóî, int ïîñíñóóåîîñ, int æìíñèéçñîíí, bool òêóçíïåæíîë, bool æåèòðéóçêçó, bool èëçòëæêäêîå, bool ëååííåïäæîè, bool ñîäèñæïîóçó)
            {
                if (AlternionSettings.useBadges)
                {
                    try
                    {
                        string steamID = GameMode.getPlayerInfo(ìåäòäóëäêèæ).steamID.ToString();
                        if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                        {
                            // if they have a TW badge, this will dictate if it should or shouldn't override it visually
                            if (__instance.éòëèïòëóæèó.texture.name != "tournamentWake1Badge" ^ (!AlternionSettings.showTWBadges & __instance.éòëèïòëóæèó.texture.name == "tournamentWake1Badge"))
                            {
                                if (theGreatCacher.badges.TryGetValue(player.badgeName, out Texture newTexture))
                                {
                                    __instance.éòëèïòëóæèó.texture = newTexture;
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
        }

        //applyGold()
        [HarmonyPatch(typeof(WeaponRender), "ìæóòèðêççæî")]
        static class goldApplyPatch
        {
            static void Postfix(WeaponRender __instance)
            {
                if (AlternionSettings.useWeaponSkins)
                {
                    try
                    {
                        //PlayerInfo plyrInf = __instance.ìäóêäðçóììî.ìêïòëîåëìòñ.gameObject.transform.parent.parent.GetComponent<PlayerInfo>();
                        string steamID = __instance.ìäóêäðçóììî.ìêïòëîåëìòñ.gameObject.transform.parent.parent.GetComponent<PlayerInfo>().steamID.ToString();
                        if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                        {
                            weaponSkinHandler(__instance, player);
                        }
                    }
                    catch (Exception e)
                    {
                        debugLog("err: " + e.Message);
                    }
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
                                // Renderer renderer = __instance.éäéïéðïåææè.transform.GetComponent<Renderer>();
                                __instance.éäéïéðïåææè.transform.GetComponent<Renderer>().material.mainTexture = newTex;
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
                // Call these so that they set correctly again on returning to the main menu
                setupMainMenu();
                setMenuCharacter();
            }
        }

        [HarmonyPatch(typeof(MainMenu), "toggleKSBadge")]
        static class toggleKSPatch
        {
            static void Postfix(MainMenu __instance, bool on)
            {
                if (AlternionSettings.useBadges)
                {
                    if (!on)
                    {
                        setMainMenuBadge();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WeaponRender), "Start")]
        static class weaponSkinpatch1stPerson
        {
            static void Postfix(WeaponRender __instance)
            {
                if (AlternionSettings.useWeaponSkins)
                {
                    try
                    {
                        if (!__instance.åïääìêêäéèç)
                        {
                            //Grab local steamID
                            string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                            if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                            {
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
        }

        [HarmonyPatch(typeof(SailHealth), "OnEnable")]
        static class sailSkinPatch
        {
            static void Postfix(SailHealth __instance)
            {
                if (AlternionSettings.useMainSails || AlternionSettings.useSecondarySails)
                {
                    try
                    {
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


                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        if (theGreatCacher.defaultSails != null)
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
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


                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        if (theGreatCacher.defaultSails != null)
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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

                                        if (player.mainSailName != "default")
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
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
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
        }

        // Borked
        [HarmonyPatch(typeof(CannonUse), "OnEnable")]
        static class cannonOperationalSkinPatch
        {
            static void Postfix(CannonUse __instance)
            {
                int index = 1000;
                if (AlternionSettings.useCannonSkins)
                {
                    try
                    {
                        Transform child = __instance.transform.FindChild("cannon");
                        int.TryParse(__instance.transform.root.name.Split('m')[1], out index);
                        logLow($"Got Operational index: -{index}-");
                        string steamID = "0";
                        if (GameMode.Instance.teamCaptains[index - 1])
                        {
                            logLow("Team has captain");
                            steamID = GameMode.Instance.teamCaptains[index - 1].steamID.ToString();
                        }
                        else
                        {
                            logLow($"Team has not got a captain at index: -{index}- (position: {index-1})");
                        }
                        if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                        {
                            logLow("Gotten player with steamID: " + steamID);
                            // If vessel is already cached, grab it and add, otherwise create new vessel
                            if (theGreatCacher.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                            {
                                logLow($"Adding to ship at index: -{index}- (position: {index - 1})");
                                vessel.cannonOperationalDict.Add((vessel.cannonOperationalDict.Count + 1).ToString(), __instance);
                                logLow($"Added to ship at index: -{vessel.cannonOperationalDict.Count + 1}- (position: {index - 1})");
                            }
                            else
                            {
                                cachedShip newVessel = new cachedShip();
                                logLow("Generated new ship");
                                newVessel.cannonOperationalDict.Add("0", __instance);
                                logLow($"Added 1st cannon to ship at index: -{index}- (position: {index - 1})");
                                theGreatCacher.ships.Add(index.ToString(), newVessel);
                                logLow($"Added bessel to ship cache: -{index}-");
                            }

                            // If they have a custom texture, use it, else use default skin
                            if (player.cannonSkinName != "default")
                            {
                                if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out Texture newTex))
                                {
                                    logLow("Applying skin: " + newTex.name);
                                    child.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                                else
                                {
                                    if (theGreatCacher.defaultCannons != null)
                                    {
                                        logLow("Applying default to custom");
                                        child.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultCannons;
                                    }

                                }
                            }
                            else
                            {
                                if (theGreatCacher.defaultCannons != null)
                                {
                                    logLow("Applying default as default");
                                    child.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultCannons;
                                }
                            }
                        }
                        else
                        {
                            if (theGreatCacher.defaultCannons != null)
                            {
                                logLow("Applying default as null");
                                child.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultCannons;
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
                            debugLog("Cannon operational error start");
                            debugLog(e.Message);
                            debugLog($"Issue at index: -{index}- (position {index-1})");
                            debugLog($"Team: -{__instance.transform.root.name}-");
                            debugLog($"Num: -{__instance.transform.root.name.Split('m')[1]}-");
                            for (int i = 0; i < GameMode.Instance.teamCaptains.Length; i++)
                            {
                                if (GameMode.Instance.teamCaptains[i])
                                {
                                    debugLog($"Got captain at index -{i}- with steamID -{GameMode.Instance.teamCaptains[i].steamID}-");
                                }
                                else
                                {
                                    debugLog($"No captain at index -{i}-");
                                }
                            }
                            debugLog("Cannon operational error end");
                        }
                    }
                }
            }
        }
        // Borked
        [HarmonyPatch(typeof(CannonDestroy), "Start")]
        static class cannonDestroySkinPatch
        {
            static void Postfix(CannonDestroy __instance)
            {
                if (AlternionSettings.useCannonSkins)
                {
                    try
                    {

                        int index = GameMode.getParentIndex(__instance.æïìçñðåììêç.transform.root);
                        string steamID = "0";
                        if (GameMode.Instance.teamCaptains[index])
                        {
                            steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                        }
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
                            if (player.cannonSkinName != "default")
                            {
                                if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out Texture newTex))
                                {
                                    __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
                                }
                            }
                        }
                        else
                        {
                            if (theGreatCacher.defaultCannons != null)
                            {
                                __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        debugLog("Cannon destroy start");
                        debugLog(e.Message);
                        debugLog("Cannon destroy end");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AccoladeItem), "ëîéæìêìëéæï")]
        static class accoladeSetInfoPatch
        {
            static void Postfix(AccoladeItem __instance, string óéíïñîèëëêð, int òææóïíéñåïñ, string çìîñìëðêëéò, CSteamID ìçíêääéïíòç)
            {
                // Sets win screen badges
                if (AlternionSettings.useBadges)
                {
                    try
                    {
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
                    debugLog("##########################################################");
                    debugLog("Pass captain patch.");
                    debugLog(e.Message);
                    debugLog("##########################################################");
                }
            }
        }

    }
}
