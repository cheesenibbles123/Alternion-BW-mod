using System;
using BWModLoader;
using System.IO;
using UnityEngine;

namespace Alternion
{
    [Mod]
    public class AlternionSettings : MonoBehaviour
    {
        public static int loggingLevel;
        public static bool showTWBadges;
        public static bool useBadges;
        public static bool useMaskSkins;
        public static bool useMainSails;
        public static bool useSecondarySails;
        public static bool useWeaponSkins;
        public static bool useCannonSkins;
        public static string configKeyInput;
        public static int configMenuPageNumber = 1;
        public static int configMenuMaxPages = 2;

        static string configFile = "AlternionConfig.cfg";

        static void log(string msg)
        {
            Log.logger.Log(msg);
        }

        void Start()
        {
            checkConfig();
            setTextures();
        }

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

        void setupDefaults()
        {
            loggingLevel = 0;
            showTWBadges = false;
            useBadges = true;
            useMaskSkins = true;
            useMainSails = true;
            useSecondarySails = true;
            useWeaponSkins = true;
            useCannonSkins = false;
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
            streamWriter.WriteLine("useBadges=" + checkBool(useBadges));
            streamWriter.WriteLine("useMaskSkins=" + checkBool(useMaskSkins));
            streamWriter.WriteLine("useMainSails=" + checkBool(useMainSails));
            streamWriter.WriteLine("useSecondarySails=" + checkBool(useSecondarySails));
            streamWriter.WriteLine("useWeaponSkins=" + checkBool(useWeaponSkins));
            streamWriter.WriteLine("useCannonSkins=" + checkBool(useCannonSkins));
            streamWriter.Close();

            log("No config file found. Created default config file.");
        }

        void loadSettings()
        {
            if (!File.Exists(configFile))
            {
                log("No config found!");
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
                                    log("Error loading loggingLevel config option. Setting default of 0.");
                                    log(e.Message);
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
                            default:
                                break;
                        }
                    }
                }
            }
        }

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
            streamWriter.WriteLine("useBadges=" + checkBool(useBadges));
            streamWriter.WriteLine("useMaskSkins=" + checkBool(useMaskSkins));
            streamWriter.WriteLine("useMainSails=" + checkBool(useMainSails));
            streamWriter.WriteLine("useSecondarySails=" + checkBool(useSecondarySails));
            streamWriter.WriteLine("useWeaponSkins=" + checkBool(useWeaponSkins));
            streamWriter.WriteLine("useCannonSkins=" + checkBool(useCannonSkins));
            streamWriter.Close();

            log("Saved config to file.");
        }

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
