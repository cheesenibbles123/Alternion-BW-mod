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
        public static string version = "a9.1";
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
        static string configFile = "AlternionConfig.cfg";
        /// <summary>
        /// Website file name.
        /// </summary>
        public static string remoteFile = "playerList2.json";
        /// <summary>
        /// Filepath to the textures.
        /// </summary>
        public static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        /// <summary>
        /// Website URL.
        /// </summary>
        public static string mainUrl = "http://www.archiesbots.com/BlackwakeStuff/";

        void Start()
        {
            checkConfig();
            setTextures();
        }

        void Update()
        {
            // Display output of current version on keypress (Good for checking version numbers)
            if (Input.GetKeyUp(versionDisplayKey))
            {
                // Useful response that I totally always remember to keep up-to-date ;)
                Logger.debugLog(version);
            }
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
            updateDuringRuntime = true;
            showFlags = true;
            configKeyInput = "]";
            versionDisplayKey = "-";
            mainMenuAnimationStepKey = "1";
            mainMenuWeaponStepKey = "2";
            saveSettings(true);
        }

        /// <summary>
        /// Loads the settings from the config file.
        /// </summary>
        void loadSettings()
        {
            if (!File.Exists(configFile))
            {
                Logger.debugLog("No config found! Generating new config file.");
                setupDefaults();
                loadSettings();
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
                            case "versionDisplayKey":
                                versionDisplayKey = splitArr[1];
                                break;
                            case "mainMenuAnimationStepKey":
                                mainMenuAnimationStepKey = splitArr[1];
                                break;
                            case "mainMenuWeaponStepKey":
                                mainMenuWeaponStepKey = splitArr[1];
                                break;
                            case "loggingLevel":
                                try
                                {
                                    loggingLevel = Convert.ToInt32(splitArr[1]);
                                }catch (Exception e)
                                {
                                    Logger.debugLog("Error loading loggingLevel config option. Setting to default of 0.");
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
                            case "showKSBadges":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    showKSBadges = true;
                                }
                                else
                                {
                                    showKSBadges = false;
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
                            case "useSwivelSkins":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useSwivelSkins = true;
                                }
                                else
                                {
                                    useSwivelSkins = false;
                                }
                                break;
                            case "useMortarSkins":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useMortarSkins = true;
                                }
                                else
                                {
                                    useMortarSkins = false;
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
        /// <param name="isNew">New config generated or updating existing config file</param>
        public static void saveSettings(bool isNew)
        {
            StreamWriter streamWriter = new StreamWriter(configFile);
            streamWriter.WriteLine("[Alternion config file]");
            streamWriter.WriteLine("");
            streamWriter.WriteLine("[General]");
            streamWriter.WriteLine("configMenuHotkey=" + configKeyInput);
            streamWriter.WriteLine("versionDisplayKey=" + versionDisplayKey);
            streamWriter.WriteLine("mainMenuAnimationStepKey=" + mainMenuAnimationStepKey);
            streamWriter.WriteLine("mainMenuWeaponStepKey=" + mainMenuWeaponStepKey);
            streamWriter.WriteLine("loggingLevel=" + loggingLevel);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("[Visuals]");
            streamWriter.WriteLine("------------");
            streamWriter.WriteLine("Format:");
            streamWriter.WriteLine("1 : Enabled");
            streamWriter.WriteLine("0 : Disabled");
            streamWriter.WriteLine("------------");
            streamWriter.WriteLine("showTWBadges=" + checkBool(showTWBadges));
            streamWriter.WriteLine("showKSBadges=" + checkBool(showKSBadges));
            streamWriter.WriteLine("showFlags=" + checkBool(showFlags));
            streamWriter.WriteLine("useBadges=" + checkBool(useBadges));
            streamWriter.WriteLine("useMaskSkins=" + checkBool(useMaskSkins));
            streamWriter.WriteLine("useMainSails=" + checkBool(useMainSails));
            streamWriter.WriteLine("useSecondarySails=" + checkBool(useSecondarySails));
            streamWriter.WriteLine("useWeaponSkins=" + checkBool(useWeaponSkins));
            streamWriter.WriteLine("useCannonSkins=" + checkBool(useCannonSkins));
            streamWriter.WriteLine("useSwivelSkins=" + checkBool(useSwivelSkins));
            streamWriter.WriteLine("useMortarSkins=" + checkBool(useMortarSkins));
            streamWriter.WriteLine("downloadOnStartup=" + checkBool(downloadOnStartup));
            streamWriter.WriteLine("updateDuringRuntime=" + checkBool(updateDuringRuntime));
            streamWriter.Close();

            if (isNew)
            {
                Logger.debugLog("Generated default config file.");
            }
            else
            {
                Logger.debugLog("Saved config to file.");
            }
        }

        /// <summary>
        /// Converts an bool to an int (1:true, 0:false).
        /// </summary>
        /// <param name="checking">Bool to convert</param>
        static int checkBool(bool checking)
        {
            return checking ? 1 : 0;
        }

        /// <summary>
        /// Checks for all textures, and stores the relevant ones into their respective slots.
        /// Mostly serves to fill default textures, as well as config menu backgrounds and images.
        /// </summary>
        void setTextures()
        {
            Texture[] mainTex = Resources.FindObjectsOfTypeAll<Texture>();
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
                        TheGreatCacher.setDefaultCannons(texture);
                        break;
                    case "ships_sails_alb":
                        TheGreatCacher.setDefaultSails(texture);
                        break;

                    case "wpn_nockGun_stock_alb":
                        TheGreatCacher.primaryWeaponsDefault[0].alb = texture;
                        break;
                    case "wpn_nockGun_stock_met":
                        TheGreatCacher.primaryWeaponsDefault[0].met = texture;
                        break;
                    case "wpn_nockGun_stock_nrm":
                        TheGreatCacher.primaryWeaponsDefault[0].nrm = texture;
                        break;
                    case "wpn_nockGun_stock_ao":
                        TheGreatCacher.primaryWeaponsDefault[0].ao = texture;
                        break;

                    case "wpn_handMortar_alb":
                        TheGreatCacher.primaryWeaponsDefault[1].alb = texture;
                        break;
                    case "wpn_handMortar_met":
                        TheGreatCacher.primaryWeaponsDefault[1].met = texture;
                        break;
                    case "wpn_handMortar_nrm":
                        TheGreatCacher.primaryWeaponsDefault[1].nrm = texture;
                        break;
                    case "wpn_handMortar_ao":
                        TheGreatCacher.primaryWeaponsDefault[1].ao = texture;
                        break;

                    case "wpn_blunderbuss_alb":
                        TheGreatCacher.primaryWeaponsDefault[2].alb = texture;
                        break;
                    case "wpn_blunderbuss_met":
                        TheGreatCacher.primaryWeaponsDefault[2].met = texture;
                        break;
                    case "wpn_blunderbuss_nrm":
                        TheGreatCacher.primaryWeaponsDefault[2].nrm = texture;
                        break;
                    case "wpn_blunderbuss_ao":
                        TheGreatCacher.primaryWeaponsDefault[2].ao = texture;
                        break;

                    case "wpn_standardMusket_stock_alb":
                        TheGreatCacher.primaryWeaponsDefault[3].alb = texture;
                        break;
                    case "wpn_standardMusket_stock_met":
                        TheGreatCacher.primaryWeaponsDefault[3].met = texture;
                        break;
                    case "wpn_standardMusket_stock_nrm":
                        TheGreatCacher.primaryWeaponsDefault[3].nrm = texture;
                        break;
                    case "wpn_standardMusket_stock_ao":
                        TheGreatCacher.primaryWeaponsDefault[3].ao = texture;
                        break;
                    default:
                        //log(texture.name);
                        break;
                }
            }
        }
    }
}
