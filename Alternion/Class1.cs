using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using BWModLoader;
using Harmony;
using Steamworks;
//using System.Runtime.InteropServices;


namespace Alternion
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
        static List<Texture2D> badgeTextures = new List<Texture2D>();

        static List<string> PlayerIDSkins = new List<string>();
        static List<string> SkinNames = new List<string>();

        static List<string> PlayerIDSailSkins = new List<string>();
        static List<string> SkinSailNames = new List<string>();

        static List<string> PlayerIDCannonSkins = new List<string>();
        static List<string> CannonSkinNames = new List<string>();

        static int logLevel = 1;

        static bool showTWBadges = false;
        static bool useCustomSkins = true;

        void Start()
        {
            try
            {
                logHigh("Patching...");
                HarmonyInstance harmony = HarmonyInstance.Create("com.github.archie");
                logHigh("Created harmony object!");
                harmony.PatchAll();
                logHigh("Patched!");
            }
            catch (Exception e)
            {
                logHigh(e.Message);
            }

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
                logHigh($"Badge File For loop run {i} times");
                try
                {
                    string[] splitArrBadge = badgeFile[i].Split(new char[] { '=' });
                    PlayerID.Add(splitArrBadge[0]);
                    badgeName.Add(splitArrBadge[1]);
                }
                catch (Exception e)
                {
                    logLow("Error loading badge file into program:");
                    logLow(e.Message);
                }
            }
            logHigh("BadgeFile Downloaded!");
            StartCoroutine(loadSkinFileIE());
        }
        private IEnumerator loadSkinFileIE()
        {
            logHigh("Downloading SkinFile...");

            WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/skinsList.txt");
            yield return www;
            logHigh("Return complete");

            string[] skinsFile = www.text.Replace("\r", "").Split('\n');
            logHigh("Split complete");

            for (int i = 0; i < skinsFile.Length; i++)
            {
                logHigh($"Skin File For loop run {i} times");
                try
                {
                    string[] splitArrSkin = skinsFile[i].Split(new char[] { '=' });
                    PlayerIDSkins.Add(splitArrSkin[0]);
                    SkinNames.Add(splitArrSkin[1]);
                }
                catch (Exception e)
                {
                    logLow("Error loading skin file into program:");
                    logLow(e.Message);
                }
            }
            logHigh("SkinFile Downloaded!");
            StartCoroutine(loadSailsFile());
        }
        private IEnumerator loadSailsFile()
        {
            logMed("Downloading Sails...");

            WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/sailSkins.txt");
            yield return www;
            logHigh("Return complete");

            string[] skinsFile = www.text.Replace("\r", "").Split('\n');
            logHigh("Split complete");

            for (int i = 0; i < skinsFile.Length; i++)
            {
                logHigh($"Skin File For loop run {i} times");
                try
                {
                    string[] splitArrSkin = skinsFile[i].Split(new char[] { '=' });
                    PlayerIDSailSkins.Add(splitArrSkin[0]);
                    SkinSailNames.Add(splitArrSkin[1]);
                }
                catch (Exception e)
                {
                    logLow("Error loading skin file into program:");
                    logLow(e.Message);
                }
            }
            logHigh("SailsFile Downloaded!");
            StartCoroutine(loadCannonsFile());
        }
        private IEnumerator loadCannonsFile()
        {
            logMed("Downloading Cannon...");

            WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/cannonSkins.txt");
            yield return www;
            logHigh("Return complete");

            string[] skinsFile = www.text.Replace("\r", "").Split('\n');
            logHigh("Split complete");

            for (int i = 0; i < skinsFile.Length; i++)
            {
                logHigh($"Skin File For loop run {i} times");
                try
                {
                    string[] splitArrSkin = skinsFile[i].Split(new char[] { '=' });
                    PlayerIDCannonSkins.Add(splitArrSkin[0]);
                    CannonSkinNames.Add(splitArrSkin[1]);
                }
                catch (Exception e)
                {
                    logLow("Error loading Cannon file into program:");
                    logLow(e.Message);
                }
            }
            logHigh("SailsFile Downloaded!");
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
            logMed("Downloading Textures...");
            logMed("Downloading Badges...");
            Texture2D newBadgeTexture;
            for (int i = 0; i < badgeName.Count; i++)
            {
                logHigh($"for loop run {i}");
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/Badges/" + badgeName[i] + ".png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    logHigh("Encoded Badges bytes");
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + badgeName[i] + ".png", bytes);
                    logHigh("Written files");
                    newBadgeTexture = loadTexture(badgeName[i], 110, 47);
                    logHigh("Loaded badge Texture");
                    badgeTextures.Add(newBadgeTexture);
                    logHigh("Cached badge Texture");
                }
                catch (Exception e)
                {
                    logLow("Error downloading images:");
                    logLow(e.Message);
                }
            }
            logMed("Badges Downloaded.");

            logMed("Downloading Weapons...");
            List<string> weaponNames = new List<string>()
            {
                "nockGun", "blunderbuss", "musket", "handmortar",
                "duckfoot", "pistol", "shortpistol", "matchlock" , "annelyRevolver",
                "cutlass", "rapier", "twoHandAxe", "dagger", "bottle", "pike"
            };
            for (int i = 0; i < SkinNames.Count; i++)
            {
                logHigh($"for loop run {i}");
                for (int s = 0; s < weaponNames.Count; s++)
                {
                    string wpn = weaponNames[s] + '_' + SkinNames[i];
                    WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/WeaponSkins/" + wpn + ".png");
                    yield return www;

                    try
                    {
                        byte[] bytes = www.texture.EncodeToPNG();
                        logHigh("Encoded wpnSkins bytes");
                        File.WriteAllBytes(Application.dataPath + texturesFilePath + wpn + ".png", bytes);
                        logHigh("Written files");
                    }
                    catch (Exception e)
                    {
                        logLow("Error downloading images:");
                        logLow(e.Message);
                    }
                }
            }
            logMed("Weapons Downloaded.");

            logMed("Downloading SkinSails...");
            for (int i = 0; i < SkinSailNames.Count; i++)
            {
                logHigh($"for loop run {i}");
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/SailSkins/" + SkinSailNames[i] + ".png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    logHigh("Encoded Sail bytes");
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + SkinSailNames[i] + ".png", bytes);
                    logHigh("Written files");
                }
                catch (Exception e)
                {
                    logLow("Error downloading images:");
                    logLow(e.Message);
                }
            }
            logMed("Sails Downloaded.");

            logMed("Downloading Cannons...");
            for (int i = 0; i < CannonSkinNames.Count; i++)
            {
                logHigh($"for loop run {i}");
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/CannonSkins/" + CannonSkinNames[i] + ".png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    logHigh("Encoded Cannon bytes");
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + CannonSkinNames[i] + ".png", bytes);
                    logHigh("Written files");
                }
                catch (Exception e)
                {
                    logLow("Error downloading images:");
                    logLow(e.Message);
                }
            }
            logMed("Cannons Downloaded.");

            logMed("Textures Downloaded!");

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
                                if (logLevel >= 2)
                                {
                                    for (int s = 0; s < badgeName[i].Length; s++)
                                    {
                                        logHigh(badgeName[i][s].ToString());
                                    }
                                }
                                if (File.Exists(Application.dataPath + texturesFilePath + badgeName[i] + ".png"))
                                {
                                    logHigh("Loading texture....");
                                    __instance.éòëèïòëóæèó.texture = badgeTextures[i]; // loadTexture(badgeName[i], 110, 47);
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

        [HarmonyPatch(typeof(Character), "íëðäêñïçêêñ", new Type[] { typeof(string) })]
        static class weaponSkinPatch3rdPerson
        {
            static void Postfix(Character __instance, string îëðíîïïêñîî)
            {
                logMed("Entered Patch");
                try
                {
                    if (useCustomSkins)
                    {
                        logMed("Entered if");
                        if (__instance.ìñíððåñéåèæ == null)
                        {
                            logHigh("Inst = null");
                            return;
                        }
                        for (int i = 0; i < __instance.ìñíððåñéåèæ.childCount; i++)
                        {
                            if (__instance.ìñíððåñéåèæ.GetChild(i).name == îëðíîïïêñîî)
                            {
                                logHigh("If 2 entered");
                                WeaponRender component = __instance.ìñíððåñéåèæ.GetChild(i).GetComponent<WeaponRender>();
                                logHigh("Gotten WeaponRenderer Component");

                                if (component != null)
                                {
                                    logHigh("Component != null (true)");
                                    logHigh("----------COMPONENT START----------");
                                    string weaponName = component.GetComponent<Renderer>().material.name.Split('_')[1];
                                    logHigh("Weapon Name: :" + weaponName + ":");

                                    PlayerInfo plyrInfo = component.ìäóêäðçóììî.transform.parent.GetComponent<PlayerInfo>();
                                    logHigh("Gotten Player Info");

                                    string steamID = plyrInfo.steamID.ToString();
                                    logHigh("SteamID: " + steamID);

                                    string skinToUse = "";
                                    for (int e = 0; e < PlayerIDSkins.Count; e++)
                                    {
                                        logHigh("For count: " + (e + 1));
                                        if (PlayerIDSkins[e] == steamID)
                                        {
                                            logHigh("Found Match: " + steamID);
                                            skinToUse = SkinNames[e];
                                            logHigh("Set skinToUse = " + skinToUse);
                                        }
                                    }

                                    string fullSkinName = weaponName + "_" + skinToUse;
                                    logHigh(fullSkinName);

                                    if (File.Exists(Application.dataPath + texturesFilePath + fullSkinName + ".png"))
                                    {
                                        Texture2D tempTexture = loadTexture(fullSkinName, 2048, 2048);
                                        logHigh("Loaded texture: " + fullSkinName);
                                        component.GetComponent<Renderer>().material.SetTexture("_MainTex", tempTexture);
                                        logHigh("Set Texture");
                                    }

                                    logHigh("-----------COMPONENT END-----------");
                                    logHigh("Set Main Tex");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logHigh("ERROR:");
                    logHigh(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(WeaponRender), "Start")]
        static class weaponSkinpatch1stPerson
        {
            static void Postfix(WeaponRender __instance)
            {

                logMed("Entererd 1st Person Wep Skin Patch");
                if (!__instance.åïääìêêäéèç && __instance.ëæìéäîåçóæí && useCustomSkins)
                {
                    logHigh("Entered 1st Person Wep if");
                    if (__instance.GetComponent<Renderer>().material.name.StartsWith("wpn_"))
                    {
                        logHigh("Is Weapon");

                        string wpnName = __instance.GetComponent<Renderer>().material.name.Split('_')[1];
                        logHigh("Weapon Name: " + wpnName);
                        try
                        {
                            string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                            logHigh(steamID);

                            string skinToUse = "";
                            logHigh("Set skinToUse = ''");

                            for (int i = 0; i < PlayerIDSkins.Count; i++)
                            {
                                logHigh("For count: " + (i + 1));
                                if (PlayerIDSkins[i] == steamID)
                                {
                                    logHigh("Found Match: " + steamID);
                                    skinToUse = SkinNames[i];
                                    logHigh("Set skinToUse = " + skinToUse);
                                }
                            }

                            string fullSkinName = wpnName + "_" + skinToUse;
                            logHigh(fullSkinName);
                            if (File.Exists(Application.dataPath + texturesFilePath + fullSkinName + ".png"))
                            {
                                Texture2D tempTexture = loadTexture(fullSkinName, 2048, 2048);
                                logHigh("Loaded texture: " + fullSkinName);

                                __instance.GetComponent<Renderer>().material.SetTexture("_MainTex", tempTexture);
                                logHigh("Set Texture");
                            }

                        }
                        catch (Exception e)
                        {
                            logHigh("ERROR:");
                            logHigh(e.Message);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SailHealth), "Start")]
        static class sailSkinPatch
        {
            static void Postfix(SailHealth __instance)
            {

                try
                {
                    logMed("SailSkinPatch Postfix called");

                    Transform shipTransf = __instance.transform.root;
                    if (shipTransf)
                    {
                        logHigh(shipTransf.name);
                        int teamNum = int.Parse(shipTransf.name.Split('m')[1]);
                        logHigh($"Team Number: {teamNum}");

                        string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();
                        logHigh($"Gotten captain SteamID = {steamID} for team {teamNum}");
                        for (int i = 0; i < PlayerIDSailSkins.Count; i++)
                        {
                            if (PlayerIDSailSkins[i] == steamID)
                            {
                                logHigh($"Found match for ID: -{steamID}-");
                                Texture2D customSailSkin = loadTexture(SkinSailNames[i], 2048, 2048);
                                logHigh("Loaded custom skin");

                                __instance.GetComponent<Renderer>().material.mainTexture = customSailSkin;
                                logHigh("Set Texture");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logLow(e.Message);
                }
            }
        }
        [HarmonyPatch(typeof(CannonUse), "Start")]
        static class cannonOperationalSkinPatch
        {
            static void Postfix(CannonUse __instance)
            {
                logHigh("Enetered cannon Operational Patch");
                for (int s = 0; s < __instance.transform.childCount; s++)
                {
                    Transform child = __instance.transform.FindChild("cannon");
                    int index = GameMode.getParentIndex(child.transform.root);
                    logHigh($"Gotten index: {index}");
                    string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                    logHigh($"Gotten steamID: {steamID}");
                    for (int i = 0; i < PlayerIDCannonSkins.Count; i++)
                    {
                        if (PlayerIDCannonSkins[i] == steamID)
                        {
                            logHigh("Found match");
                            logHigh($"Attempting to load: {CannonSkinNames[i]}");
                            Texture2D newCannonSkin = loadTexture(CannonSkinNames[i], 2048, 2048);
                            logHigh("Loaded Texture");
                            child.GetComponent<Renderer>().material.SetTexture("_MainTex", newCannonSkin);
                            logHigh("Set Texture");
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CannonDestroy), "Start")]
        static class cannonDestroySkinPatch
        {
            static void Postfix(CannonDestroy __instance)
            {
                logHigh("Enetered cannon Patch");
                int index = GameMode.getParentIndex(__instance.æïìçñðåììêç.transform.root);
                logHigh($"Gotten index: {index}");
                string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                logHigh($"Gotten steamID: {steamID}");
                for (int i = 0; i < PlayerIDCannonSkins.Count; i++)
                {
                    if (steamID == PlayerIDCannonSkins[i])
                    {
                        try
                        {
                            if (__instance.îæïíïíäìéêé.GetComponent<Renderer>())
                            {
                                logHigh("Cannon Destroy has renderer");
                                if (__instance.îæïíïíäìéêé.GetComponent<Renderer>().material)
                                {
                                    logHigh("Cannon Destroy has material");
                                    Texture2D newCannonSkin_alb = loadTexture(CannonSkinNames[i], 2048, 2048);
                                    //Texture2D newCannonSkin_met = loadTexture(CannonSkinNames[i] + "_met", 2048, 2048);
                                    logHigh("Loaded skin");
                                    __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newCannonSkin_alb);
                                    //__instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newCannonSkin_met);
                                    logHigh("Set Cannon Destroy material texture");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logHigh(e.Message);
                        }
                    }
                }
            }
        }
    }
}
