using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Alternion.Structs;
using UnityEngine;

namespace Alternion
{
    public class AssetDownloader
    {
        private static Logger logger = new Logger("[AssetLoader]");

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
                logger.debugLog("Error loading texture: " + texName);
                logger.debugLog(e.Message);
                // Return default white texture on failing to load
                Texture2D tex = Texture2D.whiteTexture;
                tex.name = "FAILED";
                return tex;
            }
        }

        public static IEnumerator waterMark()
        {
            byte[] bytes = null;
            // Check if pfp is already downloaded or not
            if (!File.Exists(Application.dataPath + AlternionSettings.texturesFilePath + "pfp.png"))
            {
                WWW www = new WWW(AlternionSettings.mainUrl + "pfp.png");
                yield return www;

                try
                {
                    bytes = www.texture.EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + AlternionSettings.texturesFilePath + "pfp.png", bytes);
                }
                catch (Exception e)
                {
                    logger.debugLog("Error downloading watermark: " + e.Message);
                }

            }

            Texture2D loadedTexture = loadTexture("pfp", AlternionSettings.texturesFilePath, 258, 208);
            if (loadedTexture.name != "FAILED")
            {
                ModGUI.watermarkTexture = loadedTexture;
            }
        }

        /// <summary>
        /// Downloads all the textures.
        /// Could do with a cleanup someday... maybe... I'll get round to it...
        /// </summary>
        public static IEnumerator downloadTextures()
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
                            www = new WWW(AlternionSettings.mainUrl + "Badges/" + player.Value.badgeName + ".png");
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
                                www = new WWW(AlternionSettings.mainUrl + "MaskSkins/" + fullString + ".png");
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
                                www = new WWW(AlternionSettings.mainUrl + "MaskSkins/" + fullString + "_met.png");
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
                                www = new WWW(AlternionSettings.mainUrl + "MaskSkins/" + fullString + "_nrm.png");
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
                                www = new WWW(AlternionSettings.mainUrl + "MaskModels/" + fullString + ".obj");
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
                            www = new WWW(AlternionSettings.mainUrl + "SailSkins/" + player.Value.sailSkinName + ".png");
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
                            www = new WWW(AlternionSettings.mainUrl + "MainSailSkins/" + player.Value.mainSailName + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "CannonSkins/" + player.Value.cannonSkinName + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "CannonSkins/" + player.Value.cannonSkinName + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "CannonSkins/" + player.Value.cannonSkinName + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "CannonModels/" + player.Value.cannonSkinName + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "SwivelSkins/" + player.Value.swivelSkinName + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "SwivelSkins/" + player.Value.swivelSkinName + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "SwivelSkins/" + player.Value.swivelSkinName + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "SwivelModels/" + player.Value.swivelSkinName + "_top.obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "SwivelModels/" + player.Value.swivelSkinName + "_connector.obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "SwivelModels/" + player.Value.swivelSkinName + "_base.obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "MortarSkins/" + player.Value.mortarSkinName + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "MortarSkins/" + player.Value.mortarSkinName + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "MortarSkins/" + player.Value.mortarSkinName + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "MortarModels/" + player.Value.mortarSkinName + ".obj");
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
                            www = new WWW(AlternionSettings.mainUrl + "Flags/" + player.Value.flagNavySkinName + ".png");
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
                            www = new WWW(AlternionSettings.mainUrl + "Flags/" + player.Value.flagPirateSkinName + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_nrm.png");
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
                                    www = new WWW(AlternionSettings.mainUrl + "WeaponModels/" + fullWeaponString + ".obj");
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
                            www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + ".png");
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
                            www = new WWW(AlternionSettings.mainUrl + "WeaponSkins/" + fullWeaponString + "_met.png");
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
            Mainmod.setupMainMenu();
        }

    }
}
