using System;
using BWModLoader;
using System.IO;
using UnityEngine;

namespace Alternion
{
    /// <summary>
    /// Stores all settings.
    /// </summary>
    [Mod]
    public class AlternionSettings : MonoBehaviour
    {
        public static int loggingLevel = 0;
        private static Logger logger = new Logger("[AlternionSettings]");
        /// <summary>
        /// Use Tournamentwake badges.
        /// </summary>
        public static bool showTWBadges = false;
        /// <summary>
        /// Use Kickstarter badges.
        /// </summary>
        public static bool showKSBadges = false;
        /// <summary>
        /// Display custom badges.
        /// </summary>
        public static bool useBadges = true;
        /// <summary>
        /// Display Gold mask skins.
        /// </summary>
        public static bool useMaskSkins = true;
        /// <summary>
        /// Display Main sail skins.
        /// </summary>
        public static bool useMainSails = true;
        /// <summary>
        /// Display secondary sail skins.
        /// </summary>
        public static bool useSecondarySails = true;
        /// <summary>
        /// Display weapon skins.
        /// </summary>
        public static bool useWeaponSkins = true;
        /// <summary>
        /// Display cannon skins.
        /// </summary>
        public static bool useCannonSkins = true;
        /// <summary>
        /// Display swivel skins.
        /// </summary>
        public static bool useSwivelSkins = true;
        /// <summary>
        /// Display mortar skins.
        /// </summary>
        public static bool useMortarSkins = true;
        /// <summary>
        /// Download assets on startup.
        /// </summary>
        public static bool downloadOnStartup = false;
        /// <summary>
        /// Config menu toggle key.
        /// </summary>
        public static string configKeyInput = "]";
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
        public static bool updateDuringRuntime = false;
        /// <summary>
        /// Use Flags.
        /// </summary>
        public static bool showFlags = true;
        /// <summary>
        /// Current version number
        /// </summary>
        public static string version = "v10.2 beta";
        /// <summary>
        /// Button to press to display current version in the log
        /// </summary>
        public static string versionDisplayKey = "-";
        /// <summary>
        /// Button to press to step to the next main menu animation
        /// </summary>
        public static string mainMenuAnimationStepKey = "1";
        /// <summary>
        /// Button to press to step to the next main menu weapon
        /// </summary>
        public static string mainMenuWeaponStepKey = "2";
        /// <summary>
        /// Config file Name.
        /// </summary>
        public static readonly string configFile = "AlternionConfig.cfg";
        /// <summary>
        /// Alternative config file location.
        /// </summary>
        public static readonly string altConfigFile = "Blackwake_Data/Managed/Mods/Configs/AlternionConfig.cfg";
        /// <summary>
        /// Website file name.
        /// </summary>
        public static string remoteFile = "playerList2.json";
        /// <summary>
        /// Filepath to the textures.
        /// </summary>
        public static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        /// <summary>
        /// Filepath to the models.
        /// </summary>
        public static string modelsFilePath = "/Managed/Mods/Assets/Archie/Models/";
        /// <summary>
        /// Website URL.
        /// </summary>
        public static string mainUrl = "http://www.archiesbots.com/BlackwakeStuff/";

        void Start()
        {
            checkConfig();
        }

        void Update()
        {
            // Display output of current version on keypress (Good for checking version numbers)
            if (Input.GetKeyUp(versionDisplayKey))
            {
                logger.debugLog("Version: " + version);
            }
        }

        /// <summary>
        /// Checks if the config file exists or not.
        /// </summary>
        void checkConfig()
        {
            if (File.Exists(altConfigFile))
            {
                loadSettings(altConfigFile);
            }else
            if (File.Exists(configFile))
            {
                loadSettings(configFile);
            }
            else
            {
                setupDefaults();
            }
        }

        public static void saveConfig()
        {
            saveSettings(false, configFile);
            if (File.Exists(altConfigFile))
            {
                saveSettings(false, altConfigFile);
            }
        }

        /// <summary>
        /// Creates the config file, and sets up the default settings.
        /// </summary>
        void setupDefaults()
        {
            /*
             * Initializations are mostly redundant as values are setup above to prevent a typo or incorrect parse to fail to populate a variable.
             * (Mostly serves as a backup)
             */
            loggingLevel = 0;
            showTWBadges = false;
            showKSBadges = false;
            useBadges = true;
            useMaskSkins = true;
            useMainSails = true;
            useSecondarySails = true;
            useWeaponSkins = true;
            useCannonSkins = true;
            useMortarSkins = true;
            downloadOnStartup = true;
            updateDuringRuntime = false;
            showFlags = true;
            configKeyInput = "]";
            versionDisplayKey = "-";
            mainMenuAnimationStepKey = "1";
            mainMenuWeaponStepKey = "2";
            saveSettings(true, configFile);
        }

        /// <summary>
        /// Loads the settings from the config file.
        /// PASS REFERENCE WITHOUT Application.dataPath APPENDED.
        /// </summary>
        /// <param name="configToLoad">Config file path relative to common/Blackwake/</param>
        void loadSettings(string configToLoad)
        {
            if (!File.Exists(configToLoad))
            {
                if (configToLoad != configFile)
                {
                    loadSettings(configFile);
                    logger.debugLog("No custom config found! Defaulting to standard.");
                }
                else
                {
                    setupDefaults();
                    loadSettings(configFile);
                    logger.debugLog("No config found! Generating new config file.");
                }
                return;
            }
            string[] array = File.ReadAllLines(configToLoad);
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
                            case "versionDisplayKey":
                                versionDisplayKey = splitArr[1];
                                break;
                            case "mainMenuAnimationStepKey":
                                mainMenuAnimationStepKey = splitArr[1];
                                break;
                            case "mainMenuWeaponStepKey":
                                mainMenuWeaponStepKey = splitArr[1];
                                break;
                            case "texturesFilePath":
                                texturesFilePath = splitArr[1];
                                break;
                            case "loggingLevel":
                                try
                                {
                                    loggingLevel = Convert.ToInt32(splitArr[1]);
                                }catch (Exception e)
                                {
                                    logger.debugLog("Error loading loggingLevel config option. Setting to default of 0.");
                                    logger.debugLog(e.Message);
                                    loggingLevel = 0;
                                }
                                break;
                            case "showTWBadges":
                                showTWBadges = stringIntToBool(splitArr[1]);
                                break;
                            case "showKSBadges":
                                showKSBadges = stringIntToBool(splitArr[1]);
                                break;
                            case "useBadges":
                                useBadges = stringIntToBool(splitArr[1]);
                                break;
                            case "useMaskSkins":
                                useMaskSkins = stringIntToBool(splitArr[1]);
                                break;
                            case "useMainSails":
                                useMainSails = stringIntToBool(splitArr[1]);
                                break;
                            case "useSecondarySails":
                                useSecondarySails = stringIntToBool(splitArr[1]);
                                break;
                            case "useWeaponSkins":
                                useWeaponSkins = stringIntToBool(splitArr[1]);
                                break;
                            case "useCannonSkins":
                                useCannonSkins = stringIntToBool(splitArr[1]);
                                break;
                            case "useSwivelSkins":
                                useSwivelSkins = stringIntToBool(splitArr[1]);
                                break;
                            case "useMortarSkins":
                                useMortarSkins = stringIntToBool(splitArr[1]);
                                break;
                            case "downloadOnStartup":
                                downloadOnStartup = stringIntToBool(splitArr[1]);
                                break;
                            case "updateDuringRuntime":
                                updateDuringRuntime = stringIntToBool(splitArr[1]);
                                break;
                            case "showFlags":
                                showFlags = stringIntToBool(splitArr[1]);
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
        /// <param name="isNew">New config generated or updating existing config file</param>
        public static void saveSettings(bool isNew, string config)
        {
            
            StreamWriter streamWriter = new StreamWriter(config);
            streamWriter.WriteLine("[Alternion config file]");
            streamWriter.WriteLine("");
            streamWriter.WriteLine("[General]");
            streamWriter.WriteLine("configMenuHotkey=" + configKeyInput);
            streamWriter.WriteLine("versionDisplayKey=" + versionDisplayKey);
            streamWriter.WriteLine("mainMenuAnimationStepKey=" + mainMenuAnimationStepKey);
            streamWriter.WriteLine("mainMenuWeaponStepKey=" + mainMenuWeaponStepKey);
            streamWriter.WriteLine("texturesFilePath=" + texturesFilePath);
            streamWriter.WriteLine("loggingLevel=" + loggingLevel);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("[Visuals]");
            streamWriter.WriteLine("------------");
            streamWriter.WriteLine("Format:");
            streamWriter.WriteLine("1 : Enabled");
            streamWriter.WriteLine("0 : Disabled");
            streamWriter.WriteLine("------------");
            streamWriter.WriteLine("showTWBadges=" + boolToInt(showTWBadges));
            streamWriter.WriteLine("showKSBadges=" + boolToInt(showKSBadges));
            streamWriter.WriteLine("showFlags=" + boolToInt(showFlags));
            streamWriter.WriteLine("useBadges=" + boolToInt(useBadges));
            streamWriter.WriteLine("useMaskSkins=" + boolToInt(useMaskSkins));
            streamWriter.WriteLine("useMainSails=" + boolToInt(useMainSails));
            streamWriter.WriteLine("useSecondarySails=" + boolToInt(useSecondarySails));
            streamWriter.WriteLine("useWeaponSkins=" + boolToInt(useWeaponSkins));
            streamWriter.WriteLine("useCannonSkins=" + boolToInt(useCannonSkins));
            streamWriter.WriteLine("useSwivelSkins=" + boolToInt(useSwivelSkins));
            streamWriter.WriteLine("useMortarSkins=" + boolToInt(useMortarSkins));
            streamWriter.WriteLine("downloadOnStartup=" + boolToInt(downloadOnStartup));
            streamWriter.WriteLine("updateDuringRuntime=" + boolToInt(updateDuringRuntime));
            streamWriter.Close();

            if (isNew)
            {
                logger.debugLog("Generated default config file.");
            }
            else
            {
                logger.debugLog("Saved config to file.");
            }
        }

        /// <summary>
        /// Converts an bool to an int (1:true, 0:false).
        /// </summary>
        /// <param name="checking">Bool to convert</param>
        static int boolToInt(bool checking)
        {
            return checking ? 1 : 0;
        }

        static bool stringIntToBool(string intInput)
        {
            return Convert.ToInt32(intInput) == 1;
        }
    }
}
