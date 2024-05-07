using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BWModLoader;
using Harmony;
using Steamworks;
using Alternion.SkinHandlers;
using Alternion.Structs;

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
        private static Logger logger = new Logger("[Main]");
        /// <summary>
        /// True if watermark has been setup.
        /// </summary>
        bool setWatermark = false;
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
                }
                catch (Exception e)
                {
                    logger.debugLog(e.Message);
                }

                //Starts asset fetching cycle
                createDirectories();

                //Setup watermark
                StartCoroutine(waterMark());
            }
            catch (Exception e)
            {
                logger.debugLog(e.Message);
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
                    TheGreatCacher.Instance.players.Add(player.steamID, player);
                }
                catch (Exception e)
                {
                    logger.debugLog("------------------");
                    logger.debugLog("Loading from JSON error");
                    logger.debugLog("Attempted User: " + player.steamID);
                    logger.debugLog(e.Message);
                    logger.debugLog(json[i]);
                    logger.debugLog("------------------");
                }
                LoadingBar.updatePercentage(0 + (10 * ((float)i / (float)json.Length)), "Downloading players...");
            }
            LoadingBar.updatePercentage(20, "Finished getting players");
            StartCoroutine(loadSkinChecks());
        }

        private IEnumerator loadSkinChecks()
        {
            yield return new WaitForSeconds(0.1f);
            WWW www = new WWW(mainUrl + "skinAttributes.json");
            yield return www;

            string[] json = www.text.Split('&');
            for (int i = 0; i < json.Length; i++)
            {
                // I didn't want to do this, but unity has forced my hand
                weaponSkinAttributes skin = JsonUtility.FromJson<weaponSkinAttributes>(json[i]);
                if (TheGreatCacher.Instance.skinAttributes.ContainsKey(skin.weaponSkin)) { logger.debugLog("Got duplicate skin key entry: " + skin.weaponSkin); }
                else { TheGreatCacher.Instance.skinAttributes.Add(skin.weaponSkin, skin); }
                LoadingBar.updatePercentage(10 + (10 * ((float)i / (float)json.Length)), "Downloading skinInfo...");
            }
            StartCoroutine(downloadTextures());
        }

        private IEnumerator waterMark()
        {
            byte[] bytes = null;
            // Check if pfp is already downloaded or not
            if (!File.Exists(Application.dataPath + AlternionSettings.texturesFilePath + "pfp.png"))
            {
                WWW www = new WWW(mainUrl + "pfp.png");
                yield return www;

                try
                {
                    bytes = www.texture.EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "pfp.png", bytes);
                }
                catch (Exception e)
                {
                    logger.debugLog("Error downloading watermark:");
                    logger.debugLog(e.Message);
                }

            }

            watermarkTex = loadTexture("pfp", AlternionSettings.texturesFilePath, 258, 208);
            if (watermarkTex != null)
            {
                setWatermark = true;
            }
        }

        /// <summary>
        /// Downloads all the textures.
        /// Could do with a cleanup someday... maybe... I'll get round to it...
        /// </summary>
        private IEnumerator downloadTextures()
        {
            List<string> alreadyDownloaded = new List<string>();
            // pre-declare so I don't create lots of new objects each loop, and to keep it readable
            WWW www;
            bool flag;
            Texture newTex;
            string fullWeaponString;
            ObjImporter meshLoader = new ObjImporter();
            // Update loading image
            LoadingBar.updatePercentage(20, "Downloading and assigning assets...");

            //Grab Player textures
            int count = 0;
            foreach (KeyValuePair<string, playerObject> player in TheGreatCacher.Instance.players)
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
                                File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "Badges/" + player.Value.badgeName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                logger.debugLog(e.Message);
                            }
                        }

                        try
                        {
                            newTex = loadTexture(player.Value.badgeName, AlternionSettings.texturesFilePath + "Badges/", 100, 40);
                            newTex.name = player.Value.badgeName;
                            TheGreatCacher.Instance.badges.Add(player.Value.badgeName, newTex);
                            alreadyDownloaded.Add(player.Value.badgeName);
                        }
                        catch (Exception e)
                        {
                            logger.debugLog("------------------");
                            logger.debugLog("Badge Download Error");
                            logger.debugLog(e.Message);
                            logger.debugLog("------------------");
                        }

                    }
                }

                // Masks
                if (player.Value.maskSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains("mask_" + player.Value.maskSkinName);
                    if (!flag)
                    {
                        string fullString = "mask_" + player.Value.maskSkinName;
                        TheGreatCacher.Instance.skinAttributes.TryGetValue("mask_" + player.Value.maskSkinName, out weaponSkinAttributes skinInfo);
                        if (AlternionSettings.downloadOnStartup)
                        {
                            if (skinInfo.hasAlb)
                            {
                                www = new WWW(mainUrl + "MaskSkins/" + fullString + ".png");
                                yield return www;
                                try
                                {
                                    byte[] bytes = www.texture.EncodeToPNG();
                                    File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "MaskSkins/" + fullString + ".png", bytes);
                                }
                                catch (Exception e)
                                {
                                    logger.debugLog(e.Message);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                www = new WWW(mainUrl + "MaskSkins/" + fullString + "_met.png");
                                yield return www;
                                try
                                {
                                    byte[] bytes = www.texture.EncodeToPNG();
                                    File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "MaskSkins/" + fullString + "_met.png", bytes);
                                }
                                catch (Exception e)
                                {
                                    logger.debugLog(e.Message);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                www = new WWW(mainUrl + "MaskSkins/" + fullString + "_nrm.png");
                                yield return www;
                                try
                                {
                                    byte[] bytes = www.texture.EncodeToPNG();
                                    File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "MaskSkins/" + fullString + "_nrm.png", bytes);
                                }
                                catch (Exception e)
                                {
                                    logger.debugLog(e.Message);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                www = new WWW(mainUrl + "MaskModels/" + fullString + ".obj");
                                yield return www;

                                if (string.IsNullOrEmpty(www.error))
                                {
                                    byte[] bytes = www.texture.EncodeToPNG();
                                    File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "MaskModels/" + player.Value.maskSkinName + ".obj", bytes);
                                }
                                else
                                {
                                    logger.logLow("No obj found for " + player.Value.maskSkinName);
                                }
                            }
                        }
                        if (skinInfo.hasAlb)
                        {
                            newTex = loadTexture(fullString, AlternionSettings.texturesFilePath + "MaskSkins/", 2048, 2048);
                            if (newTex.name != "FAILED")
                            {
                                TheGreatCacher.Instance.maskSkins.Add(fullString, newTex);
                            }
                        }
                        if (skinInfo.hasMet)
                        {
                            newTex = loadTexture(fullString + "_met", AlternionSettings.texturesFilePath + "MaskSkins/", 2048, 2048);
                            if (newTex.name != "FAILED")
                            {
                                TheGreatCacher.Instance.maskSkins.Add(fullString + "_met", newTex);
                            }
                        }
                        if (skinInfo.hasNrm)
                        {
                            newTex = loadTexture(fullString + "_nrm", AlternionSettings.texturesFilePath + "MaskSkins/", 2048, 2048);
                            if (newTex.name != "FAILED")
                            {
                                TheGreatCacher.Instance.maskSkins.Add(fullString + "_nrm", newTex);
                            }
                        }
                        if (skinInfo.hasMesh)
                        {
                            if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "MaskModels/" + fullString + ".obj"))
                            {
                                TheGreatCacher.Instance.maskModels.Add(fullString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "MaskModels/" + fullString + ".obj"));
                            }
                            else
                            {
                                logger.debugLog("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "MaskModels/" + fullString + ".obj");
                            }
                        }
                        alreadyDownloaded.Add("mask_" + player.Value.maskSkinName);
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
                                File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "SailSkins/" + player.Value.sailSkinName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                logger.debugLog("------------------");
                                logger.debugLog("Sail Skin Download Error");
                                logger.debugLog(e.Message);
                            }
                        }

                        try
                        {
                            newTex = loadTexture(player.Value.sailSkinName, AlternionSettings.texturesFilePath + "SailSkins/", 2048, 2048);
                            newTex.name = player.Value.sailSkinName;
                            TheGreatCacher.Instance.secondarySails.Add(player.Value.sailSkinName, newTex);
                            alreadyDownloaded.Add(player.Value.sailSkinName);
                        }
                        catch (Exception e)
                        {
                            logger.debugLog("------------------");
                            logger.debugLog("Normal Sail Setup Error");
                            logger.debugLog(e.Message);
                            logger.debugLog("------------------");
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
                                File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "MainSailSkins/" + player.Value.mainSailName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                logger.debugLog(e.Message);
                            }
                        }

                        try
                        {
                            newTex = loadTexture(player.Value.mainSailName, AlternionSettings.texturesFilePath + "MainSailSkins/", 2048, 2048);
                            newTex.name = player.Value.mainSailName;
                            TheGreatCacher.Instance.mainSails.Add(player.Value.mainSailName, newTex);
                            alreadyDownloaded.Add(player.Value.mainSailName);
                        }
                        catch (Exception e)
                        {
                            logger.debugLog("------------------");
                            logger.debugLog("Main Sail Download Error");
                            logger.debugLog(e.Message);
                            logger.debugLog("------------------");
                        }
                    }
                }
                // Cannons
                if (player.Value.cannonSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.cannonSkinName);
                    if (!flag)
                    {
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue("cannon_" + player.Value.cannonSkinName, out weaponSkinAttributes skinInfo))
                        {
                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
                                    www = new WWW(mainUrl + "CannonSkins/" + player.Value.cannonSkinName + ".png");
                                    yield return www;

                                    try
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "CannonSkins/" + player.Value.cannonSkinName + ".png", bytes);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.debugLog(e.Message);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "CannonSkins/" + player.Value.cannonSkinName + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "CannonSkins/" + player.Value.cannonSkinName + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + player.Value.cannonSkinName);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "CannonSkins/" + player.Value.cannonSkinName + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "CannonSkins/" + player.Value.cannonSkinName + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + player.Value.cannonSkinName);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "CannonModels/" + player.Value.cannonSkinName + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "CannonModels/" + player.Value.cannonSkinName + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + player.Value.cannonSkinName);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(player.Value.cannonSkinName, AlternionSettings.texturesFilePath + "CannonSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.cannonSkins.Add(player.Value.cannonSkinName, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(player.Value.cannonSkinName + "_met", AlternionSettings.texturesFilePath + "CannonSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.cannonSkins.Add(player.Value.cannonSkinName + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(player.Value.cannonSkinName + "_nrm", AlternionSettings.texturesFilePath + "CannonSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.cannonSkins.Add(player.Value.cannonSkinName + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "CannonModels/" + player.Value.cannonSkinName + ".obj"))
                                {
                                    TheGreatCacher.Instance.cannonModels.Add(player.Value.cannonSkinName, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "CannonModels/" + player.Value.cannonSkinName + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "CannonModels/" + player.Value.cannonSkinName + ".obj");
                                }
                            }
                        }
                        alreadyDownloaded.Add(player.Value.cannonSkinName);
                    }
                }
                if (player.Value.swivelSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.swivelSkinName);
                    if (!flag)
                    {
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue("swivel_" + player.Value.swivelSkinName, out weaponSkinAttributes skinInfo))
                        {
                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
                                    www = new WWW(mainUrl + "SwivelSkins/" + player.Value.swivelSkinName + ".png");
                                    yield return www;

                                    try
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "SwivelSkins/" + player.Value.swivelSkinName + ".png", bytes);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.debugLog(e.Message);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "SwivelSkins/" + player.Value.swivelSkinName + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "SwivelSkins/" + player.Value.swivelSkinName + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + player.Value.swivelSkinName);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "SwivelSkins/" + player.Value.swivelSkinName + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "SwivelSkins/" + player.Value.swivelSkinName + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + player.Value.swivelSkinName);
                                    }
                                }
                                if (skinInfo.hasMeshTop != null && (bool)skinInfo.hasMeshTop)
                                {
                                    www = new WWW(mainUrl + "SwivelModels/" + player.Value.swivelSkinName + "_top.obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_top.obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + player.Value.swivelSkinName + "_top");
                                    }
                                }
                                if (skinInfo.hasMeshConnector != null && (bool)skinInfo.hasMeshConnector)
                                {
                                    www = new WWW(mainUrl + "SwivelModels/" + player.Value.swivelSkinName + "_connector.obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_connector.obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + player.Value.swivelSkinName + "_connector");
                                    }
                                }
                                if (skinInfo.hasMeshBase != null && (bool)skinInfo.hasMeshBase)
                                {
                                    www = new WWW(mainUrl + "SwivelModels/" + player.Value.swivelSkinName + "_base.obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_base.obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + player.Value.swivelSkinName + "_base");
                                    }
                                }
                            }
                            newTex = loadTexture(player.Value.swivelSkinName, AlternionSettings.texturesFilePath + "SwivelSkins/", 2048, 2048);
                            if (newTex.name != "FAILED")
                            {
                                TheGreatCacher.Instance.swivels.Add(player.Value.swivelSkinName, newTex);
                            }
                            newTex = loadTexture(player.Value.swivelSkinName + "_met", AlternionSettings.texturesFilePath + "SwivelSkins/", 2048, 2048);
                            if (newTex.name != "FAILED")
                            {
                                TheGreatCacher.Instance.swivels.Add(player.Value.swivelSkinName + "_met", newTex);
                            }
                            if (skinInfo.hasMeshTop != null && (bool)skinInfo.hasMeshTop)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_top.obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(player.Value.swivelSkinName, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_top.obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_top.obj");
                                }
                            }
                            if (skinInfo.hasMeshConnector != null && (bool)skinInfo.hasMeshConnector)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_connector.obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(player.Value.swivelSkinName, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_connector.obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_connector.obj");
                                }
                            }
                            if (skinInfo.hasMeshBase != null && (bool)skinInfo.hasMeshBase)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_base.obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(player.Value.swivelSkinName, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_base.obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "SwivelModels/" + player.Value.swivelSkinName + "_base.obj");
                                }
                            }
                        }
                        alreadyDownloaded.Add(player.Value.swivelSkinName);
                    }
                }
                if (player.Value.mortarSkinName != "default")
                {
                    flag = alreadyDownloaded.Contains(player.Value.mortarSkinName);
                    if (!flag)
                    {
                        if (TheGreatCacher.Instance.skinAttributes.TryGetValue(player.Value.mortarSkinName, out weaponSkinAttributes skinInfo))
                        {
                            if (AlternionSettings.downloadOnStartup)
                            {
                                if (skinInfo.hasAlb)
                                {
                                    www = new WWW(mainUrl + "MortarSkins/" + player.Value.mortarSkinName + ".png");
                                    yield return www;

                                    try
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "MortarSkins/" + player.Value.mortarSkinName + ".png", bytes);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.debugLog(e.Message);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "MortarSkins/" + player.Value.mortarSkinName + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "MortarSkins/" + player.Value.mortarSkinName + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + player.Value.mortarSkinName);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "MortarSkins/" + player.Value.mortarSkinName + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "MortarSkins/" + player.Value.mortarSkinName + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + player.Value.mortarSkinName);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "MortarModels/" + player.Value.mortarSkinName + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "MortarModels/" + player.Value.mortarSkinName + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + player.Value.mortarSkinName);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(player.Value.mortarSkinName, AlternionSettings.texturesFilePath + "MortarSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.mortarSkins.Add(player.Value.mortarSkinName, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(player.Value.mortarSkinName + "_met", AlternionSettings.texturesFilePath + "MortarSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.mortarSkins.Add(player.Value.mortarSkinName + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(player.Value.mortarSkinName + "_nrm", AlternionSettings.texturesFilePath + "MortarSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.mortarSkins.Add(player.Value.mortarSkinName + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "MortarModels/" + player.Value.mortarSkinName + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(player.Value.mortarSkinName, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "MortarModels/" + player.Value.mortarSkinName + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "MortarModels/" + player.Value.mortarSkinName + ".obj");
                                }
                            }
                        }
                        alreadyDownloaded.Add(player.Value.mortarSkinName);
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
                                File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "Flags/" + player.Value.flagNavySkinName + ".png", bytes);
                            }
                            else
                            {
                                logger.logLow("No alb found for " + player.Value.flagNavySkinName);
                            }

                        }
                        newTex = loadTexture(player.Value.flagNavySkinName, AlternionSettings.texturesFilePath + "Flags/", 1024, 512);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.flags.Add(player.Value.flagNavySkinName, newTex);
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
                                File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "Flags/" + player.Value.flagPirateSkinName + ".png", bytes);
                            }
                            else
                            {
                                logger.logLow("No alb found for " + player.Value.flagPirateSkinName);
                            }

                        }
                        newTex = loadTexture(player.Value.flagPirateSkinName, AlternionSettings.texturesFilePath + "Flags/", 1024, 512);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.flags.Add(player.Value.flagPirateSkinName, newTex);
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }

                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }

                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }

                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }

                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                }
                                else
                                {
                                    logger.logLow("No alb found for " + fullWeaponString);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                yield return www;

                                if (string.IsNullOrEmpty(www.error))
                                {
                                    byte[] bytes = www.texture.EncodeToPNG();
                                    File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                }
                                else
                                {
                                    logger.logLow("No met found for " + fullWeaponString);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                yield return www;

                                if (string.IsNullOrEmpty(www.error))
                                {
                                    byte[] bytes = www.texture.EncodeToPNG();
                                    File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                }
                                else
                                {
                                    logger.logLow("No nrm found for " + fullWeaponString);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                yield return www;

                                if (string.IsNullOrEmpty(www.error))
                                {
                                    File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                }
                                else
                                {
                                    logger.logLow("No obj found for " + fullWeaponString);
                                }
                            }
                        }

                        newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                        }

                        if (skinInfo.hasMesh)
                        {
                            if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                            {
                                TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                            }
                            else
                            {
                                logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
                            }
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
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No alb found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMet)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No met found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasNrm)
                                {
                                    www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        byte[] bytes = www.texture.EncodeToPNG();
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_nrm.png", bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No nrm found for " + fullWeaponString);
                                    }
                                }
                                if (skinInfo.hasMesh)
                                {
                                    www = new WWW(mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
                                    yield return www;

                                    if (string.IsNullOrEmpty(www.error))
                                    {
                                        File.WriteAllBytes(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj", www.bytes);
                                    }
                                    else
                                    {
                                        logger.logLow("No obj found for " + fullWeaponString);
                                    }
                                }
                            }
                            if (skinInfo.hasAlb)
                            {
                                newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                                }
                            }
                            if (skinInfo.hasMet)
                            {
                                newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_met", newTex);
                                }
                            }
                            if (skinInfo.hasNrm)
                            {
                                newTex = loadTexture(fullWeaponString + "_nrm", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                                if (newTex.name != "FAILED")
                                {
                                    TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString + "_nrm", newTex);
                                }
                            }
                            if (skinInfo.hasMesh)
                            {
                                if (File.Exists(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"))
                                {
                                    TheGreatCacher.Instance.weaponModels.Add(fullWeaponString, meshLoader.ImportFile(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj"));
                                }
                                else
                                {
                                    logger.logLow("Unable to find mesh at location: " + Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/" + fullWeaponString + ".obj");
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
                                File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + ".png", bytes);
                            }
                            else
                            {
                                logger.logLow("No alb found for " + fullWeaponString);
                            }

                            // MET maps
                            www = new WWW(mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
                            yield return www;

                            if (string.IsNullOrEmpty(www.error))
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/" + fullWeaponString + "_met.png", bytes);
                            }
                            else
                            {
                                logger.logLow("No met found for " + fullWeaponString);
                            }
                        }

                        newTex = loadTexture(fullWeaponString, AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
                        if (newTex.name != "FAILED")
                        {
                            TheGreatCacher.Instance.weaponSkins.Add(fullWeaponString, newTex);
                        }

                        newTex = loadTexture(fullWeaponString + "_met", AlternionSettings.texturesFilePath + "WeaponSkins/", 2048, 2048);
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
            logger.logLow("Complete download!");
            setupMainMenu();
        }

        /// <summary>
        /// Creates required directories.
        /// </summary>
        public void createDirectories()
        {
            //Create directories prior to downloading all asset files
            if (!File.Exists(Application.dataPath + AlternionSettings.texturesFilePath))
            {
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath);
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "Badges/");
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "WeaponSkins/");
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "SailSkins/");
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "MainSailSkins/");
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "CannonSkins/");
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "SwivelSkins/");
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "MortarSkins/");
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "MaskSkins/");
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.texturesFilePath + "Flags/");
            }

            if (!File.Exists(Application.dataPath + AlternionSettings.texturesFilePath))
            {
                Directory.CreateDirectory(Application.dataPath + AlternionSettings.modelsFilePath + "WeaponModels/");
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
            MainMenuCL.setMainMenuBadge();
            MainMenuCL.setMenuFlag();
            MainMenuCL.setGoldMask();
            LoadingBar.updatePercentage(100, "Finished!");
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
            // Swivels
            // Mortars
            foreach (KeyValuePair<string, cachedShip> individualShip in TheGreatCacher.Instance.ships)
            {
                if (individualShip.Value.hasChangedSails)
                {
                    foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.sailDict)
                    {
                        indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                        indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                    }

                    foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.mainSailDict)
                    {
                        indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                        indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                    }

                    individualShip.Value.hasChangedSails = false;
                }

                if (individualShip.Value.hasChangedCannons)
                {
                    for (int i = individualShip.Value.cannons.Count - 1; i >= 0; i--)
                    {
                        Renderer rend = individualShip.Value.cannons[i];
                        if (rend)
                        {
                            rend.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                            rend.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                            rend.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                        }
                        else
                        {
                            individualShip.Value.cannons.RemoveAt(i);
                        }
                    }

                    if (individualShip.Value.cannonLOD != null)
                    {
                        individualShip.Value.cannonLOD.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                        individualShip.Value.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                        individualShip.Value.cannonLOD.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                    }
                    individualShip.Value.hasChangedCannons = false;
                }

                if (individualShip.Value.hasChangedCannonModels)
                {
                    for (int i = individualShip.Value.cannons.Count - 1; i >= 0; i--)
                    {
                        MeshFilter rend = individualShip.Value.cannonModels[i];
                        if (rend)
                        {
                            rend.mesh = TheGreatCacher.Instance.defaultCannonMesh;
                        }
                        else
                        {
                            individualShip.Value.cannonModels.RemoveAt(i);
                        }
                    }
                    individualShip.Value.hasChangedCannonModels = false;
                }

                if (individualShip.Value.hasChangedFlag)
                {
                    individualShip.Value.flag.material.mainTexture = individualShip.Value.isNavy ? TheGreatCacher.Instance.navyFlag : TheGreatCacher.Instance.pirateFlag;
                    individualShip.Value.hasChangedFlag = false;
                }
                
                if (individualShip.Value.hasChangedSwivels)
                {
                    foreach (Renderer swivel in individualShip.Value.Swivels)
                    {
                        if (swivel != null)
                        {
                            swivel.material.mainTexture = TheGreatCacher.Instance.defaultSwivel;
                            swivel.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultSwivelMet);
                            swivel.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultSwivelNrm);
                        }
                    }
                    individualShip.Value.hasChangedSwivels = false;
                }

                if (individualShip.Value.hasChangedMortars)
                {
                    foreach (Renderer mortar in individualShip.Value.mortars)
                    {
                        if (mortar != null)
                        {
                            mortar.material.mainTexture = TheGreatCacher.Instance.defaultMortar;
                            mortar.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMortarMet);
                            mortar.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                        }
                    }
                    individualShip.Value.hasChangedMortars = false;
                }

            }
        }

        /// <summary>
        /// Sets the new textures for a cached ship.
        /// </summary>
        /// <param name="steamID">Captain SteamID</param>
        /// <param name="index">Ship Index</param>
        static void assignNewTexturesToShips(string steamID, string index)
        {
            try
            {
                // Loop through the cached vessel and apply new textures in the following order:
                // Sails
                // Main Sails
                // Flags
                // Functional Cannons
                // Destroyed Cannons
                Texture newTex;
                if (TheGreatCacher.Instance.ships.TryGetValue(index, out cachedShip mightyVessel))
                {
                    if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                    {
                        // Only apply new texture if config has sail textures enabled
                        if (AlternionSettings.useSecondarySails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                if (TheGreatCacher.Instance.secondarySails.TryGetValue(player.sailSkinName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex; 
                                    indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                                else
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                    indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                            }
                        }

                        // Only apply new texture if config has main sail textures enabled
                        if (AlternionSettings.useMainSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                if (TheGreatCacher.Instance.mainSails.TryGetValue(player.mainSailName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                    indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                                else
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                    indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                            }
                        }

                        if (AlternionSettings.useMainSails || AlternionSettings.useSecondarySails)
                        {
                            sailHandler.Instance.handleClosedSails(mightyVessel, player);
                        }
                        else {
                            sailHandler.Instance.resetClosedSails(mightyVessel);
                        }

                        // Only apply new texture if config has cannon textures enabled
                        if (AlternionSettings.useCannonSkins)
                        {
                            TheGreatCacher.Instance.skinAttributes.TryGetValue("cannon_" + player.cannonSkinName, out weaponSkinAttributes attrib);

                            Texture mainTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex)){
                                mainTex = newTex;
                            }
                            else { mainTex = TheGreatCacher.Instance.defaultCannons; }
                            Texture metTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                            {
                                metTex = newTex;
                            }
                            else { metTex = TheGreatCacher.Instance.defaultCannonsMet; }
                            Texture nrmTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_nrm", out newTex))
                            {
                                nrmTex = newTex;
                            }
                            else { nrmTex = TheGreatCacher.Instance.defaultCannonsNrm; }

                            for (int i = mightyVessel.cannons.Count - 1; i >= 0; i--)
                            {
                                Renderer rend = mightyVessel.cannons[i];
                                if (rend)
                                {
                                    rend.material.mainTexture = mainTex;
                                    rend.material.SetTexture("_Metallic", metTex);
                                    rend.material.SetTexture("_BumpMap", nrmTex);
                                }
                                else
                                {
                                    mightyVessel.cannons.RemoveAt(i);
                                    i--;
                                }
                            }

                            bool hasAppliedNewSkin = false;

                            // LOD
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                            {
                                mightyVessel.cannonLOD.material.mainTexture = newTex;
                                hasAppliedNewSkin = true;
                            }
                            else
                            {
                                mightyVessel.cannonLOD.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                            }

                            if (attrib.hasMet && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_Metallic", newTex);
                                hasAppliedNewSkin = true;
                            }
                            else
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                            }

                            if (attrib.hasNrm && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_nrm", out newTex))
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_BumpMap", newTex);
                                hasAppliedNewSkin = true;
                            }
                            else
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultCannonsNrm);
                            }
                            mightyVessel.hasChangedCannons = hasAppliedNewSkin;
                        }
                        else if (mightyVessel.hasChangedCannons)
                        {
                            foreach (Renderer cannon in mightyVessel.cannons)
                            {
                                cannon.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                                cannon.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                                cannon.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultCannonsNrm);
                            }

                            mightyVessel.cannonLOD.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                            mightyVessel.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                            mightyVessel.hasChangedCannons = false;
                        }

                        if (AlternionSettings.useCannonSkins)
                        {
                            Mesh cannon;
                            if (TheGreatCacher.Instance.cannonModels.TryGetValue(player.cannonSkinName, out Mesh cannonMesh))
                            {
                                cannon = cannonMesh;
                            }else { cannon = TheGreatCacher.Instance.defaultCannonMesh; }

                            for (int i = mightyVessel.cannonModels.Count - 1; i >= 0; i--)
                            {
                                MeshFilter meshFilter = mightyVessel.cannonModels[i];
                                if (meshFilter)
                                {
                                    meshFilter.mesh = cannon;
                                }
                                else
                                {
                                    mightyVessel.cannonModels.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedCannonModels)
                        {
                            foreach (MeshFilter meshFilter in mightyVessel.cannonModels)
                            {
                                meshFilter.mesh = TheGreatCacher.Instance.defaultCannonMesh;
                            }

                            mightyVessel.hasChangedCannonModels = false;
                        }

                        // Only apply new texture if config has flag textures enabled
                        if (AlternionSettings.showFlags)
                        {
                            string flagSkin = mightyVessel.isNavy ? player.flagNavySkinName : player.flagPirateSkinName;
                            if (flagSkin != "default" && TheGreatCacher.Instance.flags.TryGetValue(flagSkin, out newTex))
                            {
                                Instance.StartCoroutine(flagHandler.Instance.setFlag(int.Parse(index), mightyVessel.flag, newTex));
                                flagHandler.setFlagTexture(mightyVessel, newTex);
                            }
                            else
                            {
                                flagHandler.resetFlag(mightyVessel);
                            }
                        }
                        else if (mightyVessel.hasChangedFlag)
                        {
                            flagHandler.resetFlag(mightyVessel);
                        }

                        // Only apply new texture if config has swivel textures enabled
                        if (AlternionSettings.useSwivelSkins)
                        {
                            swivelHandler.Instance.updateSwivels(mightyVessel, player);
                        }
                        else
                        {
                            swivelHandler.Instance.resetSwivels(mightyVessel);
                        }

                        // Only apply new texture if config has mortar textures enabled
                        if (AlternionSettings.useMortarSkins)
                        {
                            TheGreatCacher.Instance.skinAttributes.TryGetValue("cannon_" + player.cannonSkinName, out weaponSkinAttributes attrib);
                            Texture mainTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                            {
                                mainTex = newTex;
                            }
                            else { mainTex = TheGreatCacher.Instance.defaultMortar; }
                            Texture metTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                            {
                                metTex = newTex;
                            }
                            else { metTex = TheGreatCacher.Instance.defaultMortarMet; }
                            Texture nrmTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_nrm", out newTex))
                            {
                                nrmTex = newTex;
                            }
                            else { nrmTex = TheGreatCacher.Instance.defaultMortarNrm; }

                            for (int i = 0; i < mightyVessel.mortars.Count - 1; i++)
                            {
                                if (!mightyVessel.mortars[i]) { continue; }
                                mightyVessel.mortars[i].material.mainTexture = mainTex;
                                mightyVessel.mortars[i].material.SetTexture("_Metallic", metTex);
                                mightyVessel.mortars[i].material.SetTexture("_BumpMap", nrmTex);

                            }
                        }
                        else if (mightyVessel.hasChangedMortars )
                        {
                            for (int i = 0; i < mightyVessel.mortars.Count; i++)
                            {
                                if (!mightyVessel.mortars[i]) { continue; }
                                mightyVessel.mortars[i].material.mainTexture = TheGreatCacher.Instance.defaultMortar;
                                mightyVessel.mortars[i].material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMortarMet);
                                mightyVessel.mortars[i].material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                            }

                            mightyVessel.hasChangedMortars = false;
                        }
                    }
                    else
                    {
                        if (mightyVessel.hasChangedCannons)
                        {
                            for (int i = mightyVessel.cannons.Count - 1; i >= 0; i--)
                            {
                                Renderer rend = mightyVessel.cannons[i];
                                if (rend)
                                {
                                    rend.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                                    rend.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                                    rend.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultCannonsNrm);
                                }
                                else
                                {
                                    mightyVessel.cannons.RemoveAt(i);
                                    i--;
                                }
                            }

                            mightyVessel.cannonLOD.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                            mightyVessel.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                            mightyVessel.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsNrm);

                            mightyVessel.hasChangedCannons = false;
                        }
                        if (mightyVessel.hasChangedCannonModels)
                        {
                            for (int i = mightyVessel.cannonModels.Count - 1; i >= 0; i--)
                            {
                                if (mightyVessel.cannonModels[i])
                                {
                                    mightyVessel.cannonModels[i].mesh = TheGreatCacher.Instance.defaultCannonMesh;
                                }
                            }
                        }
                        if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                            }
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                            }

                            sailHandler.Instance.resetClosedSails(mightyVessel);

                            mightyVessel.hasChangedSails = false;
                        }
                        flagHandler.resetFlag(mightyVessel);
                        swivelHandler.Instance.resetSwivels(mightyVessel);
                        if (mightyVessel.hasChangedMortars)
                        {
                            for (int i=0; i < mightyVessel.mortars.Count; i++)
                            {
                                if (!mightyVessel.mortars[i]) { continue; }
                                mightyVessel.mortars[i].material.mainTexture = TheGreatCacher.Instance.defaultMortar;
                                mightyVessel.mortars[i].material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMortarMet);
                                mightyVessel.mortars[i].material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                            }

                            mightyVessel.hasChangedMortars = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.logLow(e.Message);
            }
        }

        /// <summary>
        /// Loads a texture based on the name, width and height.
        /// </summary>
        /// <param name="texName">Image Name</param>
        /// <param name="imgWidth">Image Width</param>
        /// <param name="imgHeight">Image Height</param>
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
                logger.debugLog("Error loading texture {0}" + texName);
                logger.debugLog(e.Message);
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
                            if (checkIfTWOrKS(__instance) && TheGreatCacher.Instance.badges.TryGetValue(player.badgeName, out Texture newTexture))
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
                            logger.debugLog("Failed to assign custom badge to a player:");
                            logger.debugLog(e.Message);
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
                        if (checkIfTWOrKS(óéíïñîèëëêð) && TheGreatCacher.Instance.badges.TryGetValue(player.badgeName, out Texture newTex))
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
        class newRoundPatch
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
        class passCaptainPatch
        {
            static void Prefix(PlayerOptions __instance)
            {
                // Untested
                try
                {
                    PlayerInfo player = GameMode.getPlayerInfo(__instance.êåééóæåñçòì.text);
                    assignNewTexturesToShips(player.steamID.ToString(), player.team.ToString());
                }
                catch (Exception e)
                {
                    logger.debugLog("[passCaptainPatch]: " + e.Message);
                }
            }
        }

    }
}
