using UnityEngine;
using BWModLoader;
using System.IO;

namespace Alternion
{
    /// <summary>
    /// Settings menu handler class.
    /// </summary>
    [Mod]
    public class ModGUI : MonoBehaviour
    {
        /// <summary>
        /// Main options menu background texture
        /// </summary>
        static Texture mainBoxBackground;
        /// <summary>
        /// Button background texture
        /// </summary>
        static Texture mainButtonBackground;
        /// <summary>
        /// Check mark texture
        /// </summary>
        static Texture checkMark;
        /// <summary>
        /// Check box texture
        /// </summary>
        static Texture checkBox;
        /// <summary>
        /// Check box texture
        /// </summary>
        public static Texture watermarkTexture;
        /// <summary>
        /// Is options menu visible?
        /// </summary>
        bool isEnabled = false;
        /// <summary>
        /// ModGUI Instance
        /// </summary>
        public static ModGUI Instance;


        //Placeholder Declarations for displayButtons()
        /// <summary>
        /// Stores the background colour of the GUI layout
        /// </summary>
        Color tempHolder;
        /// <summary>
        /// Stores the Text colour of the GUI layout
        /// </summary>
        Color defaultColour;
        /// <summary>
        /// Stores the default background texture of the GUI layout
        /// </summary>
        Texture2D defaultGUIBackground;

        //UI SETUP (Not yet implemented scaling)
        //Centre Horiztonal = 115
        //Centre Vertical = 120

        /// <summary>
        /// Main UI box X,Y Width,Height parameters
        /// </summary>
        static Vector4 boxSettings = new Vector4(20, 60, 250, 400);

        /// <summary>
        /// Start X,Y of Toggle Buttons
        /// </summary>
        Vector2 horizontalButton = new Vector2(30, 90);
        /// <summary>
        /// Width + Height of Toggle Buttons
        /// </summary>
        Vector2 buttonWH = new Vector2(120, 40);


        /// <summary>
        /// Start X,Y  for checkboxes (Set on Start())
        /// </summary>
        Vector2 horizontalCheckBox;
        /// <summary>
        /// Width + Height of checkboxes
        /// </summary>
        Vector2 checkWH = new Vector2(20, 20);

        /// <summary>
        /// Start X,Y coords of the page label
        /// </summary>
        static Vector2 labelBox = new Vector2(30, 70);
        /// <summary>
        /// Width + Height of the page label
        /// </summary>
        Vector2 labelWH = new Vector2(boxSettings.z - boxSettings.x - 20, boxSettings.w - boxSettings.y -210);
        
        // Start X1, Y1, X2, Y2
        /// <summary>
        /// Page switch button coords (Left(X,Y), Right(X,Y))
        /// </summary>
        static Vector4 switchPageForwardsBackwardsStartPositions = new Vector4(220, boxSettings.w, 30, boxSettings.w);
        /// <summary>
        /// Switch page button width + high parameters
        /// </summary>
        static Vector2 switchPageButtonWH = new Vector2(40, 40);

        /// <summary>
        /// Save button X,Y width,height parameters
        /// </summary>
        Vector4 saveButton = new Vector4(90, boxSettings.w, 110,40);

        /// <summary>
        /// Gap between the buttons (px)
        /// </summary>
        int buttonOffset = 50;

        /// <summary>
        /// Sets the main background image.
        /// </summary>
        /// <param name="newTexture">New Background Image</param>
        public static void setMainBoxBackground(Texture newTexture)
        {
            mainBoxBackground = newTexture;
        }
        /// <summary>
        /// Sets the main button image.
        /// </summary>
        /// <param name="newTexture">New Background Image</param>
        public static void setMainButtonBackground(Texture newTexture)
        {
            mainButtonBackground = newTexture;
        }
        /// <summary>
        /// Sets the main checkmark image.
        /// </summary>
        /// <param name="newTexture">New Background Image</param>
        public static void setCheckmark(Texture newTexture)
        {
            checkMark = newTexture;
        }
        /// <summary>
        /// Sets the main checkbox image.
        /// </summary>
        /// <param name="newTexture">New Background Image</param>
        public static void setCheckBox(Texture newTexture)
        {
            checkBox = newTexture;
        }

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
            horizontalCheckBox = new Vector2(horizontalButton.x + buttonWH.x + 40, horizontalButton.y + 10);
            GameObject uiGameobject = new GameObject();
            uiGameobject.AddComponent<ModGUI>();
            DontDestroyOnLoad(uiGameobject);
        }

        void OnGUI()
        {
            if (isEnabled)
            {
                displayGUI();
            }

            if (watermarkTexture && AlternionSettings.enableWaterMark)
            {
                GUI.DrawTexture(new Rect(10, 10, 64, 52), watermarkTexture, ScaleMode.ScaleToFit);
            }
        }

        void Update()
        {
            if (Input.GetKeyUp(AlternionSettings.configKeyInput))
            {
                isEnabled = !isEnabled;
            }
        }

        /// <summary>
        /// Toggles the config menu. Is called by the modloader when you press the "menu" button beside the loaded mod.
        /// </summary>
        void OnSettingsMenu()
        {
            isEnabled = !isEnabled;
        }
        /// <summary>
        /// Draws the main background box.
        /// </summary>
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
        /// <summary>
        /// Displays the buttons and checkboxes of the config menu.
        /// </summary>
        void displayButtons()
        {
            tempHolder = GUI.backgroundColor;
            defaultGUIBackground = GUI.skin.button.normal.background;
            GUI.skin.button.normal.background = (Texture2D)mainButtonBackground;

            if (AlternionSettings.configMenuPageNumber == 1)
            {
                defaultColour = GUI.contentColor;
                GUI.contentColor = Color.white;

                GUI.Label(new Rect(labelBox.x, labelBox.y, labelWH.x, labelWH.y), "Player");

                GUI.contentColor = defaultColour;

                //Badges
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y, buttonWH.x, buttonWH.y), "Badges"))
                {
                    AlternionSettings.useBadges = !AlternionSettings.useBadges;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y, checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useBadges)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y, checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Show TW Badges
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 1), buttonWH.x, buttonWH.y), "TW Badges"))
                {
                    AlternionSettings.showTWBadges = !AlternionSettings.showTWBadges;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 1), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.showTWBadges)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 1), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Show KS Badges
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 2), buttonWH.x, buttonWH.y), "KS Badges"))
                {
                    AlternionSettings.showKSBadges = !AlternionSettings.showKSBadges;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.showKSBadges)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Weapon Skins
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 3), buttonWH.x, buttonWH.y), "Weapon Skins"))
                {
                    AlternionSettings.useWeaponSkins = !AlternionSettings.useWeaponSkins;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useWeaponSkins)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Golden Mask Skins
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 4), buttonWH.x, buttonWH.y), "Gold Mask Skins"))
                {
                    AlternionSettings.useMaskSkins = !AlternionSettings.useMaskSkins;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 4), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useMaskSkins)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 4), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

            } // Player
            else if (AlternionSettings.configMenuPageNumber == 2)
            {
                defaultColour = GUI.contentColor;
                GUI.contentColor = Color.white;

                GUI.Label(new Rect(labelBox.x, labelBox.y, labelWH.x, labelWH.y), "Ship");

                GUI.contentColor = defaultColour;

                //Main Sails
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y, buttonWH.x, buttonWH.y), "Main Sails"))
                {
                    AlternionSettings.useMainSails = !AlternionSettings.useMainSails;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y, checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useMainSails)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y, checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Secondary Sails
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 1), buttonWH.x, buttonWH.y), "Secondary Sails"))
                {
                    AlternionSettings.useSecondarySails = !AlternionSettings.useSecondarySails;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 1), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useSecondarySails)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 1), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Flag Skins
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 2), buttonWH.x, buttonWH.y), "Flag Skins"))
                {
                    AlternionSettings.showFlags = !AlternionSettings.showFlags;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.showFlags)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Cannon Skins
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 3), buttonWH.x, buttonWH.y), "Cannon Skins"))
                {
                    AlternionSettings.useCannonSkins = !AlternionSettings.useCannonSkins;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useCannonSkins)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Swivel Skins
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 4), buttonWH.x, buttonWH.y), "Swivel Skins"))
                {
                    AlternionSettings.useSwivelSkins = !AlternionSettings.useSwivelSkins;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 4), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useSwivelSkins)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 4), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Mortar Skins
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 5), buttonWH.x, buttonWH.y), "Mortar Skins"))
                {
                    AlternionSettings.useMortarSkins = !AlternionSettings.useMortarSkins;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 5), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useMortarSkins)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 5), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }
            } // Ship
            else if (AlternionSettings.configMenuPageNumber == 3)
            {
                defaultColour = GUI.contentColor;
                GUI.contentColor = Color.white;

                GUI.Label(new Rect(labelBox.x, labelBox.y, labelWH.x, labelWH.y), "Setup");

                GUI.contentColor = defaultColour;

                // Force Update
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y, buttonWH.x, buttonWH.y), "Force Update"))
                {
                    TheGreatCacher.forceUpdate();
                }

                // Download On Startup
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 1), buttonWH.x, buttonWH.y), "Startup Download"))
                {
                    AlternionSettings.downloadOnStartup = !AlternionSettings.downloadOnStartup;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 1), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.downloadOnStartup)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 1), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                // Runtime Updates
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 2), buttonWH.x, buttonWH.y), "Runtime Update"))
                {
                    AlternionSettings.updateDuringRuntime = !AlternionSettings.updateDuringRuntime;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.updateDuringRuntime)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                // Toggle Watermark
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 3), buttonWH.x, buttonWH.y), "Watermark"))
                {
                    AlternionSettings.enableWaterMark = !AlternionSettings.enableWaterMark;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.enableWaterMark)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }
            } // Tech

            //Forwards Button
            if (AlternionSettings.configMenuPageNumber < AlternionSettings.configMenuMaxPages)
            {
                if (GUI.Button(new Rect(switchPageForwardsBackwardsStartPositions.x, switchPageForwardsBackwardsStartPositions.y, switchPageButtonWH.x, switchPageButtonWH.y), ">"))
                {
                    AlternionSettings.configMenuPageNumber += 1;
                }
            }

            //Back button
            if (AlternionSettings.configMenuPageNumber > 1)
            {
                if (GUI.Button(new Rect(switchPageForwardsBackwardsStartPositions.z, switchPageForwardsBackwardsStartPositions.w, switchPageButtonWH.x, switchPageButtonWH.y), "<"))
                {
                    AlternionSettings.configMenuPageNumber -= 1;
                }
            }

            //Save Button
            if (GUI.Button(new Rect(saveButton.x, saveButton.y, saveButton.z, saveButton.w), "Save"))
            {
                AlternionSettings.saveConfig();
            }

            GUI.skin.button.normal.background = defaultGUIBackground;
            GUI.backgroundColor = tempHolder;
        }
        /// <summary>
        /// Displays the config menu.
        /// </summary>
        void displayGUI()
        {
            displayBackingBox();
            displayButtons();
        }
    }
}
