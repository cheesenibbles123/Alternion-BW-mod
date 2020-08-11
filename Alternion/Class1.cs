using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using BWModLoader;
using System.IO;
using Harmony;
using System.Runtime.InteropServices;

namespace CharacterOutfitChanger
{
    internal static class Log
    {
        static readonly public ModLogger logger = new ModLogger("[Alternion]", ModLoader.LogPath + "\\Alternion.txt");
    }

    [Mod]
    public class Mainmod : MonoBehaviour
    {
        Texture2D watermarkTex;

        static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        static List<string> PlayerID = new List<string>();
        static List<string> badgeName = new List<string>();
        static int logLevel = 1;
        static bool showTWBadges = true;

        void Start()
        {
            logHigh("Patching...");
            HarmonyInstance harmony = HarmonyInstance.Create("com.github.archie");
            harmony.PatchAll();
            logHigh("Patched!");
            logHigh("Starting Coroutines...");
            createDirectories();
        }

        void OnGUI()
        {
            if (watermarkTex != null)
            {
                GUI.DrawTexture(new Rect(10, 10, 64, 52), watermarkTex, ScaleMode.ScaleToFit);
            }
        }

        private IEnumerator loadBadgeFileIE()
        {
            logHigh("Downloading BadgeFile...");

            WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/badgeList.txt");
            yield return www;
            logHigh("Return complete");

            string[] badgeFile = www.text.Replace("\r", "").Split('\n');
            logHigh("Split complete");

            for (int i = 0; i < badgeFile.Length; i++)
            {
                logHigh($"For loop run {i} times");
                try
                {
                    string[] splitArr = badgeFile[i].Split(new char[] { '=' });
                    PlayerID.Add(splitArr[0]);
                    badgeName.Add(splitArr[1]);
                }
                catch (Exception e)
                {
                    logLow("Error loading badge file into program:");
                    logLow(e.Message);
                }
            }
            logHigh("BadgeFile Downloaded!");
            StartCoroutine(DownloadTexturesFromInternet());
        }
        private IEnumerator waterMark()
        {
            if (!File.Exists(Application.dataPath + texturesFilePath + "pfp.png"))
            {
                logMed("pfp not found");
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/pfp.png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    logHigh("Encoded bytes");
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + "pfp.png", bytes);
                    logHigh("Written files");
                }
                catch (Exception e)
                {
                    logMed("Error downloading watermark:");
                    logMed(e.Message);
                }

            }
            else
            {
                logMed("pfp found");
            }

            watermarkTex = loadTexture("pfp", 258, 208);
        }
        private IEnumerator DownloadTexturesFromInternet()
        {
            logLow("Downloading Textures...");
            for (int i = 0; i < badgeName.Count; i++)
            {
                logHigh($"for loop run {i}");
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/Badges/" + badgeName[i] + ".png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    logHigh("Encoded bytes");
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + badgeName[i] + ".png", bytes);
                    logHigh("Written files");
                }
                catch (Exception e)
                {
                    logLow("Error downloading images:");
                    logLow(e.Message);
                }
            }
            logLow("Textures Downloaded!");

            setMainmenuBadge();
            StartCoroutine(waterMark());

        }

        void createDirectories()
        {
            if (!File.Exists(Application.dataPath + texturesFilePath))
            {
                Directory.CreateDirectory(Application.dataPath + texturesFilePath);
            }
            StartCoroutine(loadBadgeFileIE());
            logHigh("Finished Coroutines!");
        }
        void setMainmenuBadge()
        {
            logLow("Setting Main Menu badge...");
            MainMenu mm = FindObjectOfType<MainMenu>();

            try
            {
                logHigh("LOGGING GAMEMODE OBJECT");
                logHigh(Steamworks.SteamUser.GetSteamID().ToString());
                string steamID = Steamworks.SteamUser.GetSteamID().ToString();
                logHigh("STEAMID: " + steamID);
                logHigh($"Gotten SteamID = {steamID}");

                for (int i = 0; i < PlayerID.Count; i++)
                {

                    if (steamID == PlayerID[i])
                    {
                        logHigh($"FOUND MATCH {steamID} == {PlayerID[i]}");
                        logHigh($"Badge Name = :{badgeName[i]}:");
                        if (mm.menuBadge.texture.name != "tournamentWake1Badge" ^ (!showTWBadges & mm.menuBadge.texture.name == "tournamentWake1Badge"))
                        {
                            for (int s = 0; s < badgeName[i].Length; s++)
                            {
                                logHigh(badgeName[i][s].ToString());
                            }
                            if (File.Exists(Application.dataPath + texturesFilePath + badgeName[i] + ".png"))
                            {
                                logHigh("Loading texture....");
                                mm.menuBadge.texture = loadTexture(badgeName[i], 110, 47);
                                logHigh("Texture for badge loaded");
                                logLow("Setting Main Menu badge Finished!");
                            }
                            else
                            {
                                Log.logger.Log("Cannot find image for: " + PlayerID[i]);
                            }
                            logMed($"Player {PlayerID[i]} found");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                logLow("Failed to assign custom badge to a player:");
                logLow(e.Message);
            }

        }

        static void logLow(string message)
        {
            if (logLevel > 0)
            {
                Log.logger.Log(message);
            }
        }

        static void logMed(string message)
        {
            if (logLevel > 1)
            {
                Log.logger.Log(message);
            }
        }

        static void logHigh(string message)
        {
            if (logLevel > 2)
            {
                Log.logger.Log(message);
            }
        }

        static Texture2D loadTexture(string texName, int imgWidth, int imgHeight)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(Application.dataPath + texturesFilePath + texName + ".png");

                Texture2D tex = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24, false);
                tex.LoadImage(fileData);
                return tex;

            }
            catch (Exception e)
            {
                logLow(string.Format("Error loading texture {0}", texName));
                logLow(e.Message);
                return Texture2D.whiteTexture;
            }
        }

        [HarmonyPatch(typeof(ScoreboardSlot), "ñòæëíîêïæîí", new Type[] { typeof(string), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        static class ScoreBoardSlotAdjuster
        {
            static void Postfix(ScoreboardSlot __instance, string ìåäòäóëäêèæ, int óîèòèðîðçîì, string ñíçæóñðæéòó, int çïîèïçïñêïæ, int äïóïåòòéðåç, int ìëäòìèçñçìí, int óíïòðíäóïçç, int íîóìóíèíñìå, bool ðèæòðìêóëïð, bool äåîéíèñèììñ, bool æíèòîîìðçóî, int ïîñíñóóåîîñ, int æìíñèéçñîíí, bool òêóçíïåæíîë, bool æåèòðéóçêçó, bool èëçòëæêäêîå, bool ëååííåïäæîè, bool ñîäèñæïîóçó)
            {
                try
                {
                    logMed("Entered Patch");
                    string steamID = GameMode.getPlayerInfo(ìåäòäóëäêèæ).steamID.ToString();
                    logHigh($"Gotten SteamID = {steamID}");

                    for (int i = 0; i < PlayerID.Count; i++)
                    {

                        if (steamID == PlayerID[i])
                        {
                            logHigh($"FOUND MATCH {steamID} == {PlayerID[i]}");
                            logHigh($"Badge Name = :{badgeName[i]}:");
                            if (__instance.éòëèïòëóæèó.texture.name != "tournamentWake1Badge" ^ (!showTWBadges & __instance.éòëèïòëóæèó.texture.name == "tournamentWake1Badge"))
                            {
                                for (int s = 0; s < badgeName[i].Length; s++)
                                {
                                    logHigh(badgeName[i][s].ToString());
                                }
                                if (File.Exists(Application.dataPath + texturesFilePath + badgeName[i] + ".png"))
                                {
                                    logHigh("Loading texture....");
                                    __instance.éòëèïòëóæèó.texture = loadTexture(badgeName[i], 110, 47);
                                    logHigh("Texture for badge loaded");
                                }
                                else
                                {
                                    Log.logger.Log("Cannot find image for: " + PlayerID[i]);
                                }
                                logMed($"Player {PlayerID[i]} found");
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    logLow("Failed to assign custom badge to a player:");
                    logLow(e.Message);
                }

            }
        }

    }

}