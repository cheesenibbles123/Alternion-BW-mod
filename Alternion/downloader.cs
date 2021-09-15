using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BWModLoader;
using UnityEngine;
using Alternion.Structs;

namespace Alternion
{
#if EXTRAS
    [Mod]
    public class downloader : MonoBehaviour
    {
        public static downloader Instance;
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
        public void forceUpdate()
        {
            StartCoroutine(updateAttributes(true));
        }
        private IEnumerator updateAttributes(bool downloadAll)
        {
            yield return new WaitForSeconds(0.1f);
            WWW www = new WWW(AlternionSettings.mainUrl + "skinAttributes.json");
            yield return www;

            string[] json = www.text.Split('&');
            for (int i = 0; i < json.Length; i++)
            {
                weaponSkinAttributes skin = JsonUtility.FromJson<weaponSkinAttributes>(json[i]);
                if (TheGreatCacher.Instance.skinAttributes.ContainsKey(skin.weaponSkin))
                {
                    TheGreatCacher.Instance.skinAttributes[skin.weaponSkin] = skin;
                }
                else
                {
                    TheGreatCacher.Instance.skinAttributes.Add(skin.weaponSkin, skin);
                }
            }

            if (downloadAll)
            {
                StartCoroutine(downloadAllTextures());
            }
        }

        private IEnumerator downloadAllTextures()
        {
            yield return new WaitForSeconds(0.1f);
            // Will not work for badges
            foreach (KeyValuePair<string,weaponSkinAttributes> weaponSkin in TheGreatCacher.Instance.skinAttributes)
            {

            }
        }

        private IEnumerator downloadSkin(string skinName, string folder, bool hasAttributes)
        {
            if (AlternionSettings.downloadOnStartup)
            {
                if (hasAttributes && TheGreatCacher.Instance.skinAttributes.TryGetValue(skinName, out weaponSkinAttributes skinAttributes))
                {
                    if (skinAttributes.hasAlb)
                    {
                        WWW www = new WWW($"{AlternionSettings.mainUrl}{folder}{skinName}.png");
                        yield return www;
                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes($"{Application.dataPath}{AlternionSettings.texturesFilePath}{folder}{skinName}.png", bytes);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog(e.Message);
                        }
                    }

                    if (skinAttributes.hasMet)
                    {
                        WWW www = new WWW($"{AlternionSettings.mainUrl}{folder}{skinName}_met.png");
                        yield return www;
                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes($"{Application.dataPath}{AlternionSettings.texturesFilePath}{folder}{skinName}_met.png", bytes);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog(e.Message);
                        }
                    }

                    if (skinAttributes.hasNrm)
                    {
                        WWW www = new WWW($"{AlternionSettings.mainUrl}{folder}{skinName}_nrm.png");
                        yield return www;
                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes($"{Application.dataPath}{AlternionSettings.texturesFilePath}{folder}{skinName}_nrm.png", bytes);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog(e.Message);
                        }
                    }
                }
                else if (!hasAttributes)
                {
                    WWW www = new WWW($"{AlternionSettings.mainUrl}{folder}{skinName}.png");
                    yield return www;
                    try
                    {
                        byte[] bytes = www.texture.EncodeToPNG();
                        File.WriteAllBytes($"{Application.dataPath}{AlternionSettings.texturesFilePath}{folder}{skinName}.png", bytes);
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog(e.Message);
                    }
                }
            }
        }

        void loadTexture(string skinName, string folder, textureType type)
        {
            try
            {
                Vector2 imageRes = imageFormats.getResByType(type);
                Texture2D newSkinTexture = loadTextureFromDrive(skinName, $"{AlternionSettings.texturesFilePath}{folder}", (int)imageRes.x, (int)imageRes.y);
                newSkinTexture.name = skinName;
                addToCache(skinName, newSkinTexture, type);
            }
            catch (Exception e)
            {
                Logger.debugLog("------------------");
                Logger.debugLog("Download Error");
                Logger.debugLog(e.Message);
                Logger.debugLog("------------------");
            }
        }

        void addToCache(string skinName, Texture skinTexture, textureType type)
        {
            switch (type)
            {
                case textureType.BADGE:
                    TheGreatCacher.Instance.badges.Add(skinName, skinTexture);
                    break;
            }
        }

        /// <summary>
        /// Loads a texture based on the name, width and height.
        /// </summary>
        /// <param name="texName">Image Name</param>
        /// <param name="imgWidth">Image Width</param>
        /// <param name="imgHeight">Image Height</param>
        /// <returns>Texture2D</returns>
        public static Texture2D loadTextureFromDrive(string texName, string filePath, int imgWidth, int imgHeight)
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
    }

    public class downloader : MonoBehaviour
    {
        public static downloader Instance;
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

        private IEnumerator loadJsonFile()
        {
            LoadingBar.updatePercentage(0, "Fetching Players");

            WWW www = new WWW(AlternionSettings.mainUrl + AlternionSettings.remoteFile);
            yield return www;
            string[] json = www.text.Split('&');

            for (int i = 0; i < json.Length; i++)
            {
                // I didn't want to do this, but unity has forced my hand
                playerObject player = JsonUtility.FromJson<playerObject>(json[i]);
                try
                {
                    TheGreatCacher.Instance.players.Add(player.steamID, player);
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
                LoadingBar.updatePercentage(0 + (10 * ((float)i / (float)json.Length)), "Downloading players...");
            }
            LoadingBar.updatePercentage(20, "Finished getting players");
            StartCoroutine(loadSkinChecks());
        }

        private IEnumerator loadSkinChecks()
        {
            yield return new WaitForSeconds(0.1f);
            WWW www = new WWW(AlternionSettings.mainUrl + "skinAttributes.json");
            yield return www;

            string[] json = www.text.Split('&');
            for (int i = 0; i < json.Length; i++)
            {
                // I didn't want to do this, but unity has forced my hand
                weaponSkinAttributes skin = JsonUtility.FromJson<weaponSkinAttributes>(json[i]);
                TheGreatCacher.Instance.skinAttributes.Add(skin.weaponSkin, skin);
                LoadingBar.updatePercentage(10 + (10 * ((float)i / (float)json.Length)), "Downloading skinInfo...");
            }
            StartCoroutine(downloadTextures());
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
            int count = 0;
            foreach (KeyValuePair<string, playerObject> player in TheGreatCacher.Instance.players)
            {
                // I don't think I have ever typed the word "default" as much as I did the last few days
                string folder;
                // Badges
                if (player.Value.badgeName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.badgeName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.BADGE);
                        StartCoroutine(downloadSkin(player.Value.badgeName, folder, false));
                        loadTexture(player.Value.badgeName, folder, textureType.BADGE);
                        alreadyDownloaded.Add(player.Value.badgeName);
                    }
                }
                // Masks
                if (player.Value.maskSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.maskSkinName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.MASK);
                        StartCoroutine(downloadSkin(player.Value.maskSkinName, folder, true));
                        loadTexture(player.Value.maskSkinName, folder, textureType.MASK);
                        alreadyDownloaded.Add(player.Value.maskSkinName);
                    }
                }

                // Sails
                if (player.Value.sailSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.sailSkinName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.SECONDARY_SAIL);
                        StartCoroutine(downloadSkin(player.Value.sailSkinName, folder, false));
                        loadTexture(player.Value.sailSkinName, folder, textureType.SECONDARY_SAIL);
                        alreadyDownloaded.Add(player.Value.sailSkinName);
                    }
                }
                if (player.Value.mainSailName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.mainSailName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.MAIN_SAIL);
                        StartCoroutine(downloadSkin(player.Value.mainSailName, folder, false));
                        loadTexture(player.Value.mainSailName, folder, textureType.MAIN_SAIL);
                        alreadyDownloaded.Add(player.Value.mainSailName);
                    }
                }

                // Cannons
                if (player.Value.cannonSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.cannonSkinName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.CANNON);
                        StartCoroutine(downloadSkin(player.Value.cannonSkinName, folder, true));
                        loadTexture(player.Value.cannonSkinName, folder, textureType.CANNON);
                        alreadyDownloaded.Add(player.Value.cannonSkinName);
                    }
                }
                if (player.Value.swivelSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.swivelSkinName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.SWIVEL);
                        StartCoroutine(downloadSkin(player.Value.swivelSkinName, folder, true));
                        loadTexture(player.Value.swivelSkinName, folder, textureType.SWIVEL);
                        alreadyDownloaded.Add(player.Value.swivelSkinName);
                    }
                }
                if (player.Value.mortarSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.mortarSkinName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.MORTAR);
                        StartCoroutine(downloadSkin(player.Value.mortarSkinName, folder, true));
                        loadTexture(player.Value.mortarSkinName, folder, textureType.MORTAR);
                        alreadyDownloaded.Add(player.Value.mortarSkinName);
                    }
                }
                // Flags
                if (player.Value.flagNavySkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.flagNavySkinName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.FLAG_NAVY);
                        StartCoroutine(downloadSkin(player.Value.flagNavySkinName, folder, false));
                        loadTexture(player.Value.flagNavySkinName, folder, textureType.FLAG_NAVY);
                        alreadyDownloaded.Add(player.Value.flagNavySkinName);
                    }
                }
                if (player.Value.flagPirateSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.flagPirateSkinName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.FLAG_PIRATE);
                        StartCoroutine(downloadSkin(player.Value.flagPirateSkinName, folder, false));
                        loadTexture(player.Value.flagPirateSkinName, folder, textureType.FLAG_PIRATE);
                        alreadyDownloaded.Add(player.Value.flagPirateSkinName);
                    }
                }
                string combinedName; 
                // Primary weapons
                if (player.Value.musketSkinName != "default")
                {
                    combinedName = "musket_" + player.Value.musketSkinName;
                    flag = alreadyDownloaded.Contains(combinedName);
                    if (!flag)
                    {
                        folder = imageFormats.getLocationByType(textureType.FLAG_PIRATE);
                        StartCoroutine(downloadSkin(player.Value.flagPirateSkinName, folder, false));
                        loadTexture(player.Value.flagPirateSkinName, folder, textureType.FLAG_PIRATE);
                        alreadyDownloaded.Add(player.Value.flagPirateSkinName);

                        fullWeaponString = "musket_" + player.Value.musketSkinName;
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {
                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {
                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }

                                if (skinInfo.hasMet)
                                {
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

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                        TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo);

                        if (AlternionSettings.downloadOnStartup)
                        {
                            if (skinInfo.hasAlb)
                            {
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
                            }
                            if (skinInfo.hasMet)
                            {
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
                            if (skinInfo.hasNrm)
                            {
                                www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                yield return www;

                                if (string.IsNullOrEmpty(www.error))
                                {
                                    byte[] bytes = www.texture.EncodeToPNG();
                                    File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                }
                                else
                                {
                                    Logger.logLow("No nrm found for " + fullWeaponString);
                                }
                            }
                        }

                        newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
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
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(fullWeaponString, out weaponSkinAttributes skinInfo))
                        {

                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
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
                                }
                                if (skinInfo.hasMet)
                                {
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
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        Logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
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
                            TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }
                        alreadyDownloaded.Add(fullWeaponString);
                    }
                }

                count++;
                float newPercentage = 20.0f + (60.0f * ((float)count / TheGreatCacher.Instance.players.Count));
                LoadingBar.updatePercentage(newPercentage, "Downloading Textures");
            }

            // outputPlayerDict();
            Logger.logLow("Complete download!");
            setupMainMenu();
        }

        private IEnumerator downloadSkin(string skinName, string folder, bool hasAttributes)
        {
            if (AlternionSettings.downloadOnStartup)
            {
                if (hasAttributes && TheGreatCacher.Instance.skinAttributes.TryGetValue(skinName, out weaponSkinAttributes skinAttributes))
                {
                    if (skinAttributes.hasAlb)
                    {
                        WWW www = new WWW($"{AlternionSettings.mainUrl}{folder}{skinName}.png");
                        yield return www;
                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes($"{Application.dataPath}{AlternionSettings.texturesFilePath}{folder}{skinName}.png", bytes);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog(e.Message);
                        }
                    }

                    if (skinAttributes.hasMet)
                    {
                        WWW www = new WWW($"{AlternionSettings.mainUrl}{folder}{skinName}_met.png");
                        yield return www;
                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes($"{Application.dataPath}{AlternionSettings.texturesFilePath}{folder}{skinName}_met.png", bytes);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog(e.Message);
                        }
                    }

                    if (skinAttributes.hasNrm)
                    {
                        WWW www = new WWW($"{AlternionSettings.mainUrl}{folder}{skinName}_nrm.png");
                        yield return www;
                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes($"{Application.dataPath}{AlternionSettings.texturesFilePath}{folder}{skinName}_nrm.png", bytes);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog(e.Message);
                        }
                    }
                }
                else if (!hasAttributes)
                {
                    WWW www = new WWW($"{AlternionSettings.mainUrl}{folder}{skinName}.png");
                    yield return www;
                    try
                    {
                        byte[] bytes = www.texture.EncodeToPNG();
                        File.WriteAllBytes($"{Application.dataPath}{AlternionSettings.texturesFilePath}{folder}{skinName}.png", bytes);
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog(e.Message);
                    }
                }
            }
        }

        void loadTexture(string skinName, string folder, textureType type)
        {
            try
            {
                Vector2 imageRes = imageFormats.getResByType(type);
                Texture2D newSkinTexture = loadTextureFromDrive(skinName, $"{AlternionSettings.texturesFilePath}{folder}", (int)imageRes.x, (int)imageRes.y);
                newSkinTexture.name = skinName;
                addToCache(skinName, newSkinTexture, type);
            }
            catch (Exception e)
            {
                Logger.debugLog("------------------");
                Logger.debugLog("Download Error");
                Logger.debugLog(e.Message);
                Logger.debugLog("------------------");
            }
        }

        void addToCache(string skinName, Texture skinTexture, textureType type)
        {
            switch (type)
            {
                case textureType.BADGE:
                    TheGreatCacher.Instance.badges.Add(skinName, skinTexture);
                    break;
            }
        }

        /// <summary>
        /// Loads a texture based on the name, width and height.
        /// </summary>
        /// <param name="texName">Image Name</param>
        /// <param name="imgWidth">Image Width</param>
        /// <param name="imgHeight">Image Height</param>
        /// <returns>Texture2D</returns>
        public static Texture2D loadTextureFromDrive(string texName, string filePath, int imgWidth, int imgHeight)
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

        public void startupDownload()
        {

        }

        public void downloadAllAssets()
        {
            
        }
    }
#endif
}
