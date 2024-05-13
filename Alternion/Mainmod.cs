using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BWModLoader;
using Harmony;
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
        private static Logger logger = new Logger("[Main]");
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

                //Start init process
                StartCoroutine(loadJsonFile());

                //Setup watermark
                StartCoroutine(AssetDownloader.waterMark());
            }
            catch (Exception e)
            {
                logger.debugLog(e.Message);
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
            StartCoroutine(AssetDownloader.downloadTextures());
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

    }
}
