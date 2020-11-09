using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BWModLoader;
using UnityEngine.UI;

namespace Alternion
{
    [Mod]
    public class LoadingBar : MonoBehaviour
    {
        static List<Texture> loadingTextures = new List<Texture>();
        Texture activeTexture;
        static bool isLoading = true;

        //max of 100
        public static bool hasPercentageChanged = false;
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
        static void log(string message)
        {
            Log.logger.Log(message);
        }

        void Update()
        {
            if (hasPercentageChanged)
            {
                activeTexture = getNewTexture();
                hasPercentageChanged = false;
            }
        }
        Texture getNewTexture()
        {
            try
            {
                UnityEngine.Random.InitState(DateTime.Now.Millisecond);
                int imgToUse = UnityEngine.Random.Range(0, loadingTextures.Count);

                return loadingTextures[imgToUse];
            }catch (Exception e)
            {
                Log.logger.Log("ERROR GETTING LOADING GUI TEXTURE");
                Log.logger.Log(e.Message);
                return Texture2D.whiteTexture;
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
