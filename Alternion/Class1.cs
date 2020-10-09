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


namespace Alternion
{
    internal static class Log
    {
        static readonly public ModLogger logger = new ModLogger("[Alternion]", ModLoader.LogPath + "\\Alternion.txt");
    }

    [Mod]
    public class Mainmod : MonoBehaviour
    {
        //Dictionary<ulong, SomePlayerObject> players = new Dictionary<ulong, SomePlayerObject>();
        //players.Add(steamId, player);
        //players[steamId].DoSomeShit();

        Texture2D watermarkTex;

        static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        static List<string> PlayerID = new List<string>();
        static List<string> badgeName = new List<string>();
        static Dictionary<string, Texture2D> badgeTextures = new Dictionary<string, Texture2D>();

        static List<string> PlayerIDSkins = new List<string>();
        static List<string> SkinNames = new List<string>();

        static List<string> PlayerIDSailSkins = new List<string>();
        static List<string> SkinSailNames = new List<string>();
        static Dictionary<string, Texture2D> sailSkinTextures = new Dictionary<string, Texture2D>();

        static List<string> PlayerIDCannonSkins = new List<string>();
        static List<string> CannonSkinNames = new List<string>();
        static Dictionary<string, Texture2D> cannonSkinTextures = new Dictionary<string, Texture2D>();

        static int logLevel = 3;

        static bool showTWBadges = false;
        static bool useCustomSkins = false;

        void Start()
        {
            try
            {
                HarmonyInstance harmony = HarmonyInstance.Create("com.github.archie");
                harmony.PatchAll();
                createDirectories();
            }
            catch (Exception e)
            {
                logLow(e.Message);
            }
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

            WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/badgeList.txt");
            yield return www;

            string[] badgeFile = www.text.Replace("\r", "").Split('\n');

            for (int i = 0; i < badgeFile.Length; i++)
            {
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
            logLow("Badge skins File Downloaded!");
            StartCoroutine(loadSkinFileIE());
        }
        private IEnumerator loadSkinFileIE()
        {

            WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/skinsList.txt");
            yield return www;

            string[] skinsFile = www.text.Replace("\r", "").Split('\n');

            for (int i = 0; i < skinsFile.Length; i++)
            {
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
            logLow("Weapon skins File Downloaded!");
            StartCoroutine(loadSailsFile());
        }
        private IEnumerator loadSailsFile()
        {

            WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/sailSkins.txt");
            yield return www;

            string[] skinsFile = www.text.Replace("\r", "").Split('\n');

            for (int i = 0; i < skinsFile.Length; i++)
            {
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
            logLow("Sails File Downloaded!");
            StartCoroutine(loadCannonsFile());
        }
        private IEnumerator loadCannonsFile()
        {

            WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/cannonSkins.txt");
            yield return www;

            string[] skinsFile = www.text.Replace("\r", "").Split('\n');

            for (int i = 0; i < skinsFile.Length; i++)
            {
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
            logLow("SailsFile Downloaded!");
            StartCoroutine(DownloadTexturesFromInternet());
        }
        private IEnumerator waterMark()
        {
            if (!File.Exists(Application.dataPath + texturesFilePath + "pfp.png"))
            {
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/pfp.png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + "pfp.png", bytes);
                }
                catch (Exception e)
                {
                    logLow("Error downloading watermark:");
                    logLow(e.Message);
                }

            }

            watermarkTex = loadTexture("pfp", 258, 208);
        }
        private IEnumerator DownloadTexturesFromInternet()
        {
            Texture2D newTexture;
            for (int i = 0; i < badgeName.Count; i++)
            {
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/Badges/" + badgeName[i] + ".png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + badgeName[i] + ".png", bytes);
                    newTexture = loadTexture(badgeName[i], 110, 47);
                    badgeTextures.Add(PlayerID[i],newTexture);
                }
                catch (Exception e)
                {
                    logLow("Error downloading images:");
                    logLow(e.Message);
                }
            }
            logLow("Badges Downloaded.");

            List<string> weaponNames = new List<string>()
            {
                "nockGun", "blunderbuss", "musket", "handmortar",
                "duckfoot", "pistol", "shortpistol", "matchlock" , "annelyRevolver",
                "cutlass", "rapier", "twoHandAxe", "dagger", "bottle", "pike"
            };
            for (int i = 0; i < SkinNames.Count; i++)
            {
                for (int s = 0; s < weaponNames.Count; s++)
                {
                    string wpn = weaponNames[s] + '_' + SkinNames[i];
                    WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/WeaponSkins/" + wpn + ".png");
                    yield return www;

                    try
                    {
                        byte[] bytes = www.texture.EncodeToPNG();
                        File.WriteAllBytes(Application.dataPath + texturesFilePath + wpn + ".png", bytes);
                    }
                    catch (Exception e)
                    {
                        logLow("Error downloading images:");
                        logLow(e.Message);
                    }
                }
            }
            logLow("Weapons Downloaded.");

            for (int i = 0; i < SkinSailNames.Count; i++)
            {
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/SailSkins/" + SkinSailNames[i] + ".png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + SkinSailNames[i] + ".png", bytes);
                    newTexture = loadTexture(SkinSailNames[i], 2048, 2048);
                    sailSkinTextures.Add(PlayerIDSailSkins[i],newTexture);

                }
                catch (Exception e)
                {
                    logLow("Error downloading images:");
                    logLow(e.Message);
                }
            }
            logLow("Sails Downloaded.");

            for (int i = 0; i < CannonSkinNames.Count; i++)
            {
                WWW www = new WWW("http://www.archiesbots.com/BlackwakeStuff/CannonSkins/" + CannonSkinNames[i] + ".png");
                yield return www;

                try
                {
                    byte[] bytes = www.texture.EncodeToPNG();
                    File.WriteAllBytes(Application.dataPath + texturesFilePath + CannonSkinNames[i] + ".png", bytes);
                    newTexture = loadTexture(CannonSkinNames[i], 2048, 2048);
                    cannonSkinTextures.Add(PlayerIDCannonSkins[i],newTexture);
                }
                catch (Exception e)
                {
                    logLow("Error downloading images:");
                    logLow(e.Message);
                }
            }
            logLow("Cannons Downloaded.");

            logLow("All Textures Downloaded!");

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
        }
        void setMainmenuBadge()
        {
            logLow("Setting Main Menu badge...");
            MainMenu mm = FindObjectOfType<MainMenu>();

            try
            {
                string steamID = Steamworks.SteamUser.GetSteamID().ToString();
                if (mm.menuBadge.texture.name != "tournamentWake1Badge" ^ (!showTWBadges & mm.menuBadge.texture.name == "tournamentWake1Badge"))
                {
                    if (badgeTextures.TryGetValue(steamID, out Texture2D newTexture))
                    { 
                        mm.menuBadge.texture = newTexture;
                        logLow("Setting Main Menu badge Finished!");
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

        static void logDebug(string message)
        {
            if (logLevel > 1)
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
                    string steamID = GameMode.getPlayerInfo(ìåäòäóëäêèæ).steamID.ToString();
                    if (__instance.éòëèïòëóæèó.texture.name != "tournamentWake1Badge" ^ (!showTWBadges & __instance.éòëèïòëóæèó.texture.name == "tournamentWake1Badge"))
                    {
                        if (badgeTextures.TryGetValue(steamID, out Texture2D newTexture))
                        {
                            __instance.éòëèïòëóæèó.texture = newTexture; // loadTexture(badgeName[i], 110, 47);
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
                if (!useCustomSkins)
                {
                    return;
                }
                try
                {
                    if (__instance.ìñíððåñéåèæ == null)
                    {
                        return;
                    }
                    for (int i = 0; i < __instance.ìñíððåñéåèæ.childCount; i++)
                    {
                        if (__instance.ìñíððåñéåèæ.GetChild(i).name == îëðíîïïêñîî)
                        {
                            WeaponRender component = __instance.ìñíððåñéåèæ.GetChild(i).GetComponent<WeaponRender>();

                            if (component != null)
                            {
                                string weaponName = component.GetComponent<Renderer>().material.name.Split('_')[1];

                                PlayerInfo plyrInfo = component.ìäóêäðçóììî.transform.parent.GetComponent<PlayerInfo>();
                                string steamID = plyrInfo.steamID.ToString();

                                string skinToUse = "";
                                for (int e = 0; e < PlayerIDSkins.Count; e++)
                                {
                                    if (PlayerIDSkins[e] == steamID)
                                    {
                                        skinToUse = SkinNames[e];
                                    }
                                }

                                string fullSkinName = weaponName + "_" + skinToUse;

                                if (File.Exists(Application.dataPath + texturesFilePath + fullSkinName + ".png"))
                                {
                                    Texture2D tempTexture = loadTexture(fullSkinName, 2048, 2048);
                                    component.GetComponent<Renderer>().material.SetTexture("_MainTex", tempTexture);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logLow("ERROR:");
                    logLow(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(WeaponRender), "Start")]
        static class weaponSkinpatch1stPerson
        {
            static void Postfix(WeaponRender __instance)
            {
                if (!useCustomSkins)
                {
                    return;
                }

                if (!__instance.åïääìêêäéèç && __instance.ëæìéäîåçóæí && useCustomSkins)
                {
                    if (__instance.GetComponent<Renderer>().material.name.StartsWith("wpn_"))
                    {
                        string wpnName = __instance.GetComponent<Renderer>().material.name.Split('_')[1];
                        try
                        {
                            string steamID = SteamUser.GetSteamID().m_SteamID.ToString();

                            string skinToUse = "";

                            for (int i = 0; i < PlayerIDSkins.Count; i++)
                            {
                                if (PlayerIDSkins[i] == steamID)
                                {
                                    skinToUse = SkinNames[i];
                                }
                            }

                            string fullSkinName = wpnName + "_" + skinToUse;
                            if (File.Exists(Application.dataPath + texturesFilePath + fullSkinName + ".png"))
                            {
                                Texture2D tempTexture = loadTexture(fullSkinName, 2048, 2048);

                                __instance.GetComponent<Renderer>().material.SetTexture("_MainTex", tempTexture);
                            }

                        }
                        catch (Exception e)
                        {
                            logLow("ERROR:");
                            logLow(e.Message);
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
                    Transform shipTransf = __instance.transform.root;
                    if (shipTransf)
                    {
                        int teamNum = int.Parse(shipTransf.name.Split('m')[1]);

                        string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

                        if (sailSkinTextures.TryGetValue(steamID, out Texture2D newTexture))
                        {
                            __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
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
                Transform child;
                int index;
                string steamID;
                for (int s = 0; s < __instance.transform.childCount; s++)
                {
                    child = __instance.transform.FindChild("cannon");
                    index = GameMode.getParentIndex(child.transform.root);
                    steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();

                    if (cannonSkinTextures.TryGetValue(steamID, out Texture2D newTexture))
                    {
                        child.GetComponent<Renderer>().material.SetTexture("_MainTex", newTexture);
                    }

                }
            }
        }

        [HarmonyPatch(typeof(CannonDestroy), "Start")]
        static class cannonDestroySkinPatch
        {
            static void Postfix(CannonDestroy __instance)
            {
                int index = GameMode.getParentIndex(__instance.æïìçñðåììêç.transform.root);
                string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                if (cannonSkinTextures.TryGetValue(steamID, out Texture2D newTexture))
                {
                    __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newTexture);
                }
            }
        }
    }
}
