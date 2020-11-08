using System.IO;
using UnityEngine;
using BWModLoader;
using System;

namespace Alternion
{
    [Mod]
    public class ModGUI : MonoBehaviour
    {
        static ModGUI() { }

        Texture mainBoxBackground;
        Texture mainButtonBackground;
        Texture checkMark;
        Texture checkBox;
        bool isEnabled = false;
        static string configFile = "AlternionConfig.cfg";

        //UI SETUP
        //MAIN BOX
        Vector4 boxSettings = new Vector4(20, 60, 250, 300);
        //BUTTONS
        Vector2 horizontalButton = new Vector2(30, 90);
        Vector2 buttonWH = new Vector2(120, 40);
        Vector2 horizontalCheckBox;
        Vector2 checkWH = new Vector2(20, 20);
        int buttonOffset = 50;

        //ACTUAL VARIABLES TO BE USED
        public static bool useBadges;
        public static bool useMainSails;
        public static bool useSecondarySails;
        public static bool useWeaponSkins;
        public static bool useCannonSkins;

        void log(string msg)
        {
            Log.logger.Log(msg);
        }

        void Start()
        {
            var mainTex = Resources.FindObjectsOfTypeAll<Texture>();
            //Texture background;
            foreach (Texture texture in mainTex)
            {
                if (texture.name == "oldmap1")
                {
                    //background = texture;
                    mainBoxBackground = texture;
                }
                if (texture.name == "panel_medium")
                {
                    mainButtonBackground = texture;
                }
                if (texture.name == "Checkmark")
                {
                    checkMark = texture;
                }
                if (texture.name == "UISprite")
                {
                    checkBox = texture;
                }
            }

            horizontalCheckBox = new Vector2(horizontalButton.x + buttonWH.x + 40, horizontalButton.y + 10);
            checkConfig();
        }
        void OnGUI()
        {
            if (isEnabled)
            {
                displayGUI();
            }
        }
        void Update()
        {
            if (Input.GetKeyUp("c"))
            {
                isEnabled = !isEnabled;
            }
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
            useBadges = true;
            useMainSails = true;
            useSecondarySails = true;
            useWeaponSkins = true;
            useCannonSkins = true;

            StreamWriter streamWriter = new StreamWriter("AlternionConfig.cfg");
            streamWriter.WriteLine("[Alternion config file]");
            streamWriter.WriteLine("----------");
            streamWriter.WriteLine("Format:");
            streamWriter.WriteLine("1 : Enabled");
            streamWriter.WriteLine("0 : Disabled");
            streamWriter.WriteLine("----------");
            streamWriter.WriteLine("useBadges=" + checkBool(useBadges));
            streamWriter.WriteLine("useMainSails=" + checkBool(useBadges));
            streamWriter.WriteLine("useSecondarySails=" + checkBool(useBadges));
            streamWriter.WriteLine("useWeaponSkins=" + checkBool(useBadges));
            streamWriter.WriteLine("useCannonSkins=" + checkBool(useBadges));
            streamWriter.Close();

            log("No config file found. Setup default config file.");
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
                    log("Start: -" + splitArr[0] + "-");
                    log("Value: -" + splitArr[1] + "-");
                    if (splitArr.Length >= 2)
                    {
                        switch (splitArr[0])
                        {
                            case "useBadges":
                                if (Convert.ToInt32(splitArr[1]) == 1)
                                {
                                    useBadges = true;
                                }else
                                {
                                    useBadges = false;
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
        int checkBool(bool checking)
        {
            if (checking)
            {
                return 1;
            }else
            {
                return 0;
            }
        }

        void displayBackingBox()
        {
            if (mainBoxBackground != null)
            {
                GUI.DrawTexture(new Rect(boxSettings.x, boxSettings.y, boxSettings.z, boxSettings.w), mainBoxBackground, ScaleMode.ScaleAndCrop);
            }
            else
            {
                GUI.Box(new Rect(boxSettings.x, boxSettings.y, boxSettings.z, boxSettings.w), "Did not load texture.");
            }
        }
        void displayButtons()
        {
            Color tempHolder = GUI.backgroundColor;
            //GUI.backgroundColor = Color.clear;
            Texture2D defaultGUIBackground = GUI.skin.button.normal.background;
            GUI.skin.button.normal.background = (Texture2D)mainButtonBackground;

            //Badges
            if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y, buttonWH.x, buttonWH.y), "Badges"))
            {
                useBadges = !useBadges;
            }
            GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y, checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
            if (useBadges)
            {
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y, checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
            }

            //Main Sails
            if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + buttonOffset, buttonWH.x, buttonWH.y), "Main Sails"))
            {
                useMainSails = !useMainSails;
            }
            GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + buttonOffset, checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
            if (useMainSails)
            {
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + buttonOffset, checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
            }

            //Secondary Sails
            if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 2), buttonWH.x, buttonWH.y), "Secondary Sails"))
            {
                useSecondarySails = !useSecondarySails;
            }
            GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
            if (useSecondarySails)
            {
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
            }

            //Weapon Skins
            if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 3), buttonWH.x, buttonWH.y), "Weapon Skins"))
            {
                useWeaponSkins = !useWeaponSkins;
            }
            GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
            if (useWeaponSkins)
            {
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
            }

            //Weapon Skins
            if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 4), buttonWH.x, buttonWH.y), "Cannon Skins"))
            {
                useCannonSkins = !useCannonSkins;
            }
            GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 4), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
            if (useCannonSkins)
            {
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 4), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
            }

            GUI.skin.button.normal.background = defaultGUIBackground;
            GUI.backgroundColor = tempHolder;
        }
        void displayGUI()
        {
            displayBackingBox();
            displayButtons();
        }
    }
}
