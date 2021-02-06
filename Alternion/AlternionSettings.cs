using System;
using BWModLoader;
using System.IO;
using UnityEngine;
using AlternionGUI;

namespace Alternion
{
    /// <summary>
    /// Stores all settings.
    /// </summary>
    [Mod]
    public class AlternionSettings : MonoBehaviour
    {
        public static int loggingLevel;
        /// <summary>
        /// Use Tournamentwake badges.
        /// </summary>
        public static bool showTWBadges;
        /// <summary>
        /// Display custom badges.
        /// </summary>
        public static bool useBadges;
        /// <summary>
        /// Display Gold mask skins.
        /// </summary>
        public static bool useMaskSkins;
        /// <summary>
        /// Display Main sail skins.
        /// </summary>
        public static bool useMainSails;
        /// <summary>
        /// Display secondary sail skins.
        /// </summary>
        public static bool useSecondarySails;
        /// <summary>
        /// Display weapon skins.
        /// </summary>
        public static bool useWeaponSkins;
        /// <summary>
        /// Display cannon skins.
        /// </summary>
        public static bool useCannonSkins;
        /// <summary>
        /// Download assets on startup.
        /// </summary>
        public static bool downloadOnStartup;
        /// <summary>
        /// Config menu toggle key.
        /// </summary>
        public static string configKeyInput;
        /// <summary>
        /// Current config menu page. 
        /// </summary>
        public static int configMenuPageNumber = 1;
        /// <summary>
        /// Max config menu pages.
        /// </summary>
        public static int configMenuMaxPages = 3;
        /// <summary>
        /// Display watermark.
        /// </summary>
        public static bool enableWaterMark = true;
        /// <summary>
        /// Update players during runtime.
        /// </summary>
        public static bool updateDuringRuntime;
        /// <summary>
        /// Use Tournamentwake badges.
        /// </summary>
        public static bool showFlags;

        /// <summary>
        /// Config file location.
        /// </summary>
        static string configFile = "AlternionConfig.cfg";
        /// <summary>
        /// Website file name.
        /// </summary>
        public static string remoteFile = "playerList.json";

        void Start()
        {
            checkConfig();
            setTextures();
        }

        /// <summary>
        /// Checks if the config file exists or not.
        /// </summary>
        void checkConfig()
        {
            if (!File.Exists(configFile))
            {
                setupDefaults();
            }
            else
            {
                loadSettings();
            }
        }

        /// <summary>
        /// Creates the config file, and sets up the default settings.
        /// </summary>
        void setupDefaults()
        {
            loggingLevel = 0;
            showTWBadges = false;
            useBadges = true;
            useMaskSkins = true;
            useMainSails = true;
            useSecondarySails = true;
            useWeaponSkins = true;
            useCannonSkins = true;
            downloadOnStartup = true;
            updateDuringRuntime = true;
            showFlags = true;
            configKeyInput = "]";

            StreamWriter streamWriter = new StreamWriter("AlternionConfig.cfg");
            streamWriter.WriteLine("[Alternion config file]");
            streamWriter.WriteLine("");
            streamWriter.WriteLine("[General]");
            streamWriter.WriteLine("configMenuHotkey=" + configKeyInput);
            streamWriter.WriteLine("loggingLevel=" + loggingLevel);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("[Visuals]");
            streamWriter.WriteLine("------------");
            streamWriter.WriteLine("Format:");
            streamWriter.WriteLine("1 : Enabled");
            streamWriter.WriteLine("0 : Disabled");
            streamWriter.WriteLine("------------");
            streamWriter.WriteLine("showTWBadges=" + checkBool(showTWBadges));
            streamWriter.WriteLine("showFlags=" + checkBool(showFlags));
            streamWriter.WriteLine("useBadges=" + checkBool(useBadges));
            streamWriter.WriteLine("useMaskSkins=" + checkBool(useMaskSkins));
            streamWriter.WriteLine("useMainSails=" + checkBool(useMainSails));
            streamWriter.WriteLine("useSecondarySails=" + checkBool(useSecondarySails));
            streamWriter.WriteLine("useWeaponSkins=" + checkBool(useWeaponSkins));
            streamWriter.WriteLine("useCannonSkins=" + checkBool(useCannonSkins));
            streamWriter.WriteLine("downloadOnStartup=" + checkBool(downloadOnStartup));
            streamWriter.WriteLine("updateDuringRuntime=" + checkBool(updateDuringRuntime));
            streamWriter.Close();

            Logger.debugLog("No config file found. Created default config file.");
        }

        /// <summary>
        /// Loads the settings from the config file.
        /// </summary>
        void loadSettings()
        {
            if (!File.Exists(configFile))
            {
                Logger.debugLog("No config found!");
                return;
            }
            string[] array = File.ReadAllLines(configFile);
            char splitCharacter = '=';
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains("="))
                {
                    string[] splitArr = array[i].Split(splitCharacter);
                    if (splitArr.Length >= 2)
                    {
                        switch (splitArr[0])
                        {
                            case "configMenuHotkey":
                                configKeyInput = splitArr[1];
                                break;
                            case "loggingLevel":
                                try
                                {
                                    loggingLevel = Convert.ToInt32(splitArr[1]);
                                }catch (Exception e)
                                {
                                    Logger.debugLog("Error loading loggingLevel config option. Setting default of 0.");
                                    Logger.debugLog(e.Message);
                                    loggingLevel = 0;
                                }
                                break;
                            case "showTWBadges":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    showTWBadges = true;
                                }
                                else
                                {
                                    showTWBadges = false;
                                }
                                break;
                            case "useBadges":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useBadges = true;
                                }
                                else
                                {
                                    useBadges = false;
                                }
                                break;
                            case "useMaskSkins":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useMaskSkins = true;
                                }
                                else
                                {
                                    useMaskSkins = false;
                                }
                                break;
                            case "useMainSails":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useMainSails = true;
                                }
                                else
                                {
                                    useMainSails = false;
                                }
                                break;
                            case "useSecondarySails":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useSecondarySails = true;
                                }
                                else
                                {
                                    useSecondarySails = false;
                                }
                                break;
                            case "useWeaponSkins":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useWeaponSkins = true;
                                }
                                else
                                {
                                    useWeaponSkins = false;
                                }
                                break;
                            case "useCannonSkins":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useCannonSkins = true;
                                }
                                else
                                {
                                    useCannonSkins = false;
                                }
                                break;
                            case "downloadOnStartup":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    downloadOnStartup = true;
                                }
                                else
                                {
                                    downloadOnStartup = false;
                                }
                                break;
                            case "updateDuringRuntime":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    updateDuringRuntime = true;
                                }
                                else
                                {
                                    updateDuringRuntime = false;
                                }
                                break;
                            case "showFlags":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    showFlags = true;
                                }
                                else
                                {
                                    showFlags = false;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current runtime settings to the config file.
        /// </summary>
        public static void saveSettings()
        {
            StreamWriter streamWriter = new StreamWriter("AlternionConfig.cfg");
            streamWriter.WriteLine("[Alternion config file]");
            streamWriter.WriteLine("");
            streamWriter.WriteLine("[General]");
            streamWriter.WriteLine("configMenuHotkey=" + configKeyInput);
            streamWriter.WriteLine("loggingLevel=" + loggingLevel);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("[Visuals]");
            streamWriter.WriteLine("------------");
            streamWriter.WriteLine("Format:");
            streamWriter.WriteLine("1 : Enabled");
            streamWriter.WriteLine("0 : Disabled");
            streamWriter.WriteLine("------------");
            streamWriter.WriteLine("showTWBadges=" + checkBool(showTWBadges));
            streamWriter.WriteLine("showFlags=" + checkBool(showFlags));
            streamWriter.WriteLine("useBadges=" + checkBool(useBadges));
            streamWriter.WriteLine("useMaskSkins=" + checkBool(useMaskSkins));
            streamWriter.WriteLine("useMainSails=" + checkBool(useMainSails));
            streamWriter.WriteLine("useSecondarySails=" + checkBool(useSecondarySails));
            streamWriter.WriteLine("useWeaponSkins=" + checkBool(useWeaponSkins));
            streamWriter.WriteLine("useCannonSkins=" + checkBool(useCannonSkins));
            streamWriter.WriteLine("downloadOnStartup=" + checkBool(downloadOnStartup));
            streamWriter.WriteLine("updateDuringRuntime=" + checkBool(updateDuringRuntime));
            streamWriter.Close();

            Logger.debugLog("Saved config to file.");
        }

        /// <summary>
        /// Converts an int (1:true, 0:false) to a bool.
        /// </summary>
        /// <param name="checking">Bool to convert</param>
        static int checkBool(bool checking)
        {
            if (checking)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Checks for all textures, and stores the relevant ones into their respective slots.
        /// </summary>
        void setTextures()
        {
            var mainTex = Resources.FindObjectsOfTypeAll<Texture>();
            //Texture background;
            foreach (Texture texture in mainTex)
            {
                switch (texture.name)
                {
                    case "oldmap1":
                        ModGUI.setMainBoxBackground(texture);
                        break;
                    case "panel_medium":
                        ModGUI.setMainButtonBackground(texture);
                        break;
                    case "Checkmark":
                        ModGUI.setCheckmark(texture);
                        break;
                    case "UISprite":
                        ModGUI.setCheckBox(texture);
                        break;
                    case "prp_cannon_alb":
                        theGreatCacher.setDefaultCannons(texture);
                        break;
                    case "ships_sails_alb":
                        theGreatCacher.setDefaultSails(texture);
                        break;
                    default:
                        //log(texture.name);
                        break;
                }
            }
        }
    }
}
