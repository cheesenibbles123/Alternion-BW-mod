using UnityEngine;
using BWModLoader;
using Alternion;

namespace AlternionGUI
{
    /// <summary>
    /// GUI class.
    /// </summary>
    [Mod]
    public class ModGUI : MonoBehaviour
    {
        static Texture mainBoxBackground;
        static Texture mainButtonBackground;
        static Texture checkMark;
        static Texture checkBox;
        bool isEnabled = false;

        //Placeholder Declarations for displayButtons()
        Color tempHolder;
        Color defaultColour;
        Texture2D defaultGUIBackground;

        //UI SETUP (Not yet implemented scaling)
        //MAIN BOX
        //Centre Horiztonal = 115
        //Centre Vertical = 120
        static Vector4 boxSettings = new Vector4(20, 60, 250, 300);
        //BUTTONS
        //Start X, Y
        //Width, Height
        Vector2 horizontalButton = new Vector2(30, 90);
        Vector2 buttonWH = new Vector2(120, 40);

        //Start X, Y (assigned on startup)
        //Width, Height
        Vector2 horizontalCheckBox;
        Vector2 checkWH = new Vector2(20, 20);

        //Start X, Y
        //Width, Height
        static Vector2 labelBox = new Vector2(30, 70);
        Vector2 labelWH = new Vector2(boxSettings.z - boxSettings.x - 20, boxSettings.w - boxSettings.y -210);
        
        // Start X1, Y1, X2, Y2
        static Vector4 switchPageForwardsBackwardsStartPositions = new Vector4(220, 300, 30, 300);
        static Vector2 switchPageButtonWH = new Vector2(40, 40);

        // Format X, Y, Width, Height
        Vector4 saveButton = new Vector4(90,300,110,40);

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

        void Start()
        {
            horizontalCheckBox = new Vector2(horizontalButton.x + buttonWH.x + 40, horizontalButton.y + 10);
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

                //Weapon Skins
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 2), buttonWH.x, buttonWH.y), "Weapon Skins"))
                {
                    AlternionSettings.useWeaponSkins = !AlternionSettings.useWeaponSkins;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useWeaponSkins)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 2), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
                }

                //Golden Mask Skins
                if (GUI.Button(new Rect(horizontalButton.x, horizontalButton.y + (buttonOffset * 3), buttonWH.x, buttonWH.y), "Gold Mask Skins"))
                {
                    AlternionSettings.useMaskSkins = !AlternionSettings.useMaskSkins;
                }
                GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkBox, ScaleMode.ScaleToFit);
                if (AlternionSettings.useMaskSkins)
                {
                    GUI.DrawTexture(new Rect(horizontalCheckBox.x, horizontalCheckBox.y + (buttonOffset * 3), checkWH.x, checkWH.y), checkMark, ScaleMode.ScaleToFit);
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
                    theGreatCacher.forceUpdate();
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
                AlternionSettings.saveSettings();
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
