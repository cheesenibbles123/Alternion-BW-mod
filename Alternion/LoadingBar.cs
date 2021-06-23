using System;
using System.Collections.Generic;
using UnityEngine;
using BWModLoader;

namespace Alternion
{
    /// <summary>
    /// Main loading bar displayed in the centre of the screen on boot.
    /// </summary>
    [Mod]
    public class LoadingBar : MonoBehaviour
    {
        /// <summary>
        /// All loaded textures.
        /// </summary>
        static List<Texture> loadingTextures = new List<Texture>();
        /// <summary>
        /// Currently used texture.
        /// </summary>
        Texture activeTexture;
        /// <summary>
        /// True if the mod is still loading.
        /// </summary>
        static bool isLoading = true;

        /// <summary>
        /// True if the percentage has been changed.
        /// </summary>
        public static bool hasPercentageChanged = false;
        /// <summary>
        /// Text displayed on the loading bar.
        /// </summary>
        public static string loadingText;

        //Display setup
        static Vector2 centreScreen = new Vector2((Screen.width / 2), (Screen.height / 2));
        static Vector2 mainImageSize = new Vector2(800, 300);
        static Vector2 loadingTextSize = new Vector2(200, 40);
        Vector2 startPositionMain = new Vector2(centreScreen.x - mainImageSize.x / 2f, centreScreen.y - mainImageSize.y / 2f);
        Vector2 startPositionText = new Vector2(centreScreen.x - loadingTextSize.x / 2f, centreScreen.y - loadingTextSize.y / 2f);

        void Start()
        {
            var mainTex = Resources.FindObjectsOfTypeAll<Texture>();
            foreach (Texture texture in mainTex)
            {
                if (texture.name.Length >= 1)
                {
                    loadingTextures.Add(texture);
                }
            }
            updatePercentage(0, "Starting bar");
        }

        void Update()
        {
            if (hasPercentageChanged)
            {
                activeTexture = getNewTexture();
                hasPercentageChanged = false;
            }
        }

        void OnGUI()
        {
            if (isLoading)
            {
                GUI.DrawTexture(new Rect(startPositionMain.x, startPositionMain.y, mainImageSize.x, mainImageSize.y), activeTexture, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(startPositionText.x, startPositionText.y, loadingTextSize.x, loadingTextSize.y), loadingText);
            }
        }

        /// <summary>
        /// Gets a new texture to display.
        /// </summary>
        /// <returns>Texture2D</returns>
        Texture getNewTexture()
        {
            try
            {
                UnityEngine.Random.InitState(DateTime.Now.Millisecond);
                int imgToUse = UnityEngine.Random.Range(0, loadingTextures.Count);

                return loadingTextures[imgToUse];
            }catch (Exception e)
            {
                Logger.debugLog("ERROR GETTING LOADING GUI TEXTURE");
                Logger.debugLog(e.Message);
                return Texture2D.whiteTexture;
            }
        }

        /// <summary>
        /// Updates the percentage displayed on the LoadingBar.
        /// </summary>
        public static void updatePercentage(float newPercentage, string newText)
        {
            loadingText = newText + "... " + newPercentage.ToString() + "%";
            hasPercentageChanged = true;
            if (newPercentage >= 100)
            {
                isLoading = false;
            }
        }
    }
}
