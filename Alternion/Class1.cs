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

    class cachedShip
    {
        //Format will be OBJECTNAME / OBJECT
        public Dictionary<string, SailHealth> sailDict = new Dictionary<string, SailHealth>();
        public Dictionary<string, SailHealth> mainSailDict = new Dictionary<string, SailHealth>();
        public Dictionary<string, CannonUse> cannonOperationalDict = new Dictionary<string, CannonUse>();
        public Dictionary<string, CannonDestroy> cannonDestroyDict = new Dictionary<string, CannonDestroy>();
    }

    class cachedCannonsAndSails
    {
        //Format will be TEAMNUMBER / SHIP
        public Texture2D defaultSails = null;
        public Texture2D defaultCannons = null;
        public Dictionary<string, cachedShip> ships = new Dictionary<string, cachedShip>();

        public void setDefaultSails(Texture2D newTexture)
        {
            defaultSails = newTexture;
        }
        public void setDefaultCannons(Texture2D newTexture)
        {
            defaultCannons = newTexture;
        }
    }



    [Mod]
    public class Mainmod : MonoBehaviour
    {

        Texture2D watermarkTex;

        static cachedCannonsAndSails cachedGameObjects = new cachedCannonsAndSails();

        static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        static List<string> PlayerID = new List<string>();
        static List<string> badgeName = new List<string>();
        static Dictionary<string, Texture2D> badgeTextures = new Dictionary<string, Texture2D>();

        static List<string> PlayerIDSkins = new List<string>();
        static List<string> SkinNames = new List<string>();
        static Dictionary<string, Texture2D> weaponTextures = new Dictionary<string, Texture2D>();
        static Dictionary<string, string> playerWeaponsList = new Dictionary<string, string>();

        static List<string> PlayerIDSailSkins = new List<string>();
        static List<string> SkinSailNames = new List<string>();
        static Dictionary<string, Texture2D> sailSkinTextures = new Dictionary<string, Texture2D>();

        static List<string> PlayerIDMainSailSkins = new List<string>();
        static List<string> MainSkinSailNames = new List<string>();
        static Dictionary<string, Texture2D> mainSailDict = new Dictionary<string, Texture2D>();

        static List<string> PlayerIDCannonSkins = new List<string>();
        static List<string> CannonSkinNames = new List<string>();
        static Dictionary<string, Texture2D> cannonSkinTextures = new Dictionary<string, Texture2D>();

        static int logLevel = 1;

        static bool showTWBadges = false;
        static bool useWeaponSkins = false;

        static string mainUrl = "http://www.archiesbots.com/BlackwakeStuff/";

        void Start()
        {
            try
            {
                HarmonyInstance harmony = HarmonyInstance.Create("com.github.archie");
                harmony.PatchAll();
                createDirectories();
                StartCoroutine(waterMark());
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

            WWW www = new WWW(mainUrl + "badgeList.txt");
            yield return www;

            string[] badgeFile = www.text.Replace("\r", "").Split('\n');
            string[] splitArrBadge;
            char[] charArray = new char[] { '=' };

            for (int i = 0; i < badgeFile.Length; i++)
            {
                try
                {
                    splitArrBadge = badgeFile[i].Split(charArray);
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
            if (useWeaponSkins)
            {
                StartCoroutine(loadSkinFileIE());
            }
            else
            {
                StartCoroutine(loadSailsFile());
            }
        }
        private IEnumerator loadSkinFileIE()
        {

            WWW www = new WWW(mainUrl + "skinsList.txt");
            yield return www;

            string[] skinsFile = www.text.Replace("\r", "").Split('\n');
            string[] splitArrSkin;
            char[] charArray = new char[] { '=' };

            for (int i = 0; i < skinsFile.Length; i++)
            {
                try
                {
                    splitArrSkin = skinsFile[i].Split(charArray);
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

            WWW www = new WWW(mainUrl + "sailSkins.txt");
            yield return www;

            string[] skinsFile = www.text.Replace("\r", "").Split('\n');
            string[] splitArrSkin;

            for (int i = 0; i < skinsFile.Length; i++)
            {
                try
                {
                    splitArrSkin = skinsFile[i].Split(new char[] { '=' });
                    PlayerIDSailSkins.Add(splitArrSkin[0]);
                    SkinSailNames.Add(splitArrSkin[1]);
                }
                catch (Exception e)
                {
                    logLow("Error loading Normal Sail skin file into program:");
                    logLow(e.Message);
                }
            }
            logLow("Sails File Downloaded!");

            // Main Sails
            www = new WWW(mainUrl + "mainSailSkins.txt");
            yield return www;

            skinsFile = www.text.Replace("\r", "").Split('\n');

            logLow(skinsFile.Length.ToString());

            for (int i = 0; i < skinsFile.Length; i++)
            {
                try
                {
                    logLow($"i: {i}");
                    splitArrSkin = skinsFile[i].Split(new char[] { '=' });
                    PlayerIDMainSailSkins.Add(splitArrSkin[0]);
                    MainSkinSailNames.Add(splitArrSkin[1]);
                }
                catch (Exception e)
                {
                    logLow(i.ToString());
                    logLow("Error loading Main Sail skin file into program:");
                    logLow(e.Message);
                }
            }
            logLow("Main Sails File Downloaded!");
            StartCoroutine(loadCannonsFile());
        }
        private IEnumerator loadCannonsFile()
        {
            WWW www = new WWW(mainUrl + "cannonSkins.txt");
            yield return www;
            string[] skinsFile = www.text.Replace("\r", "").Split('\n');
            //string[] splitArrSkin;
            //char[] charArray = new char[] { '=' };

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
            logLow("Cannon File Downloaded!");
            StartCoroutine(DownloadTexturesFromInternet());
        }
        private IEnumerator waterMark()
        {
            if (!File.Exists(Application.dataPath + texturesFilePath + "pfp.png"))
            {
                WWW www = new WWW(mainUrl + "pfp.png");
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
            WWW www;
            List<string> alreadyDownloaded = new List<string>();
            int i;
            int s;

            for (i = 0; i < badgeName.Count; i++)
            {
                bool flag = alreadyDownloaded.Contains(badgeName[i]);
                if (!flag)
                {
                    www = new WWW(mainUrl + "Badges/" + badgeName[i] + ".png");
                    yield return www;

                    try
                    {
                        byte[] bytes = www.texture.EncodeToPNG();
                        File.WriteAllBytes(Application.dataPath + texturesFilePath + badgeName[i] + ".png", bytes);
                        newTexture = loadTexture(badgeName[i], 110, 47);
                        badgeTextures.Add(PlayerID[i], newTexture);
                        alreadyDownloaded.Add(badgeName[i]);
                    }
                    catch (Exception e)
                    {
                        logLow("Error downloading badge images:");
                        logLow(e.Message);
                    }
                }
            }
            logLow("Badges Downloaded.");
            setMainmenuBadge();

            if (useWeaponSkins)
            {
                List<string> weaponNames = new List<string>()
                {
                "nockGun", "blunderbuss", "musket", "handmortar",
                "duckfoot", "pistol", "shortpistol", "matchlock" , "annelyRevolver",
                "cutlass", "rapier", "twoHandAxe", "dagger", "pike"
                };
                string wpn;
                for (i = 0; i < SkinNames.Count; i++)
                {
                    bool flag = alreadyDownloaded.Contains(SkinNames[i]);
                    if (!flag)
                    {
                        playerWeaponsList.Add(PlayerIDSkins[i], SkinNames[i]);
                        for (s = 0; s < weaponNames.Count; s++)
                        {
                            wpn = weaponNames[s] + '_' + SkinNames[i];
                            www = new WWW(mainUrl + "WeaponSkins/" + wpn + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + wpn + ".png", bytes);
                                newTexture = loadTexture(wpn, 2048, 2048);
                                weaponTextures.Add(wpn, newTexture);
                                alreadyDownloaded.Add(SkinNames[i]);
                            }
                            catch (Exception e)
                            {
                                logLow("Error downloading Weapon Skin images:");
                                logLow(e.Message);
                            }
                        }
                    }
                }
                logLow("Weapons Downloaded.");
            }

            for (i = 0; i < MainSkinSailNames.Count; i++)
            {
                bool flag = alreadyDownloaded.Contains(MainSkinSailNames[i]);
                if (!flag)
                {
                    www = new WWW(mainUrl + "MainSailSkins/" + MainSkinSailNames[i] + ".png");
                    yield return www;

                    try
                    {
                        byte[] bytes = www.texture.EncodeToPNG();
                        File.WriteAllBytes(Application.dataPath + texturesFilePath + MainSkinSailNames[i] + ".png", bytes);
                        newTexture = loadTexture(MainSkinSailNames[i], 2048, 2048);
                        mainSailDict.Add(PlayerIDMainSailSkins[i], newTexture);
                        alreadyDownloaded.Add(MainSkinSailNames[i]);
                    }
                    catch (Exception e)
                    {
                        logLow("Error downloading Main Sail images:");
                        logLow("Failed: " + i.ToString() + $" / {MainSkinSailNames.Count.ToString()}");
                        logLow(e.Message);
                    }
                }
            }
            logLow("Main Sails Downloaded.");

            for (i = 0; i < SkinSailNames.Count; i++)
            {
                bool flag = alreadyDownloaded.Contains(SkinSailNames[i]);
                if (!flag)
                {
                    www = new WWW(mainUrl + "SailSkins/" + SkinSailNames[i] + ".png");
                    yield return www;

                    try
                    {
                        byte[] bytes = www.texture.EncodeToPNG();
                        File.WriteAllBytes(Application.dataPath + texturesFilePath + SkinSailNames[i] + ".png", bytes);
                        newTexture = loadTexture(SkinSailNames[i], 2048, 2048);
                        sailSkinTextures.Add(PlayerIDSailSkins[i], newTexture);
                        alreadyDownloaded.Add(SkinSailNames[i]);
                    }
                    catch (Exception e)
                    {
                        logLow("Error downloading Secondary Sail images:");
                        logLow(e.Message);
                    }
                }
            }
            logLow("Secondary Sails Downloaded.");

            for (i = 0; i < CannonSkinNames.Count; i++)
            {
                bool flag = alreadyDownloaded.Contains(CannonSkinNames[i]);
                if (!flag)
                {
                    www = new WWW(mainUrl + "CannonSkins/" + CannonSkinNames[i] + ".png");
                    yield return www;

                    try
                    {
                        byte[] bytes = www.texture.EncodeToPNG();
                        File.WriteAllBytes(Application.dataPath + texturesFilePath + CannonSkinNames[i] + ".png", bytes);
                        newTexture = loadTexture(CannonSkinNames[i], 2048, 2048);
                        cannonSkinTextures.Add(PlayerIDCannonSkins[i], newTexture);
                        alreadyDownloaded.Add(CannonSkinNames[i]);
                    }
                    catch (Exception e)
                    {
                        logLow("Error downloading Cannon images:");
                        logLow(e.Message);
                    }
                }
            }
            logLow("Cannons Downloaded.");

            logLow("All Textures Downloaded!");
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

        static void resetAllShipsToDefault()
        {
            foreach(KeyValuePair<string, cachedShip> individualShip in cachedGameObjects.ships)
            {
                foreach(KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.sailDict)
                {
                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                }

                foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.mainSailDict)
                {
                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                }

                foreach (KeyValuePair<string, CannonUse> indvidualCannon in individualShip.Value.cannonOperationalDict)
                {
                    indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", cachedGameObjects.defaultCannons);
                }

                foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in individualShip.Value.cannonDestroyDict)
                {
                    indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", cachedGameObjects.defaultCannons);
                }
            }
        }

        // NEEDS FIXING
        static void assignNewTexturesToShips(string steamID, string index)
        {
            try
            {
                if(cachedGameObjects.ships.TryGetValue(index, out cachedShip mightyVessel))
                {
                    Texture2D newTexture;

                    foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                    {
                        if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                        {
                            indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTexture;
                        }
                    }

                    foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                    {
                        if (mainSailDict.TryGetValue(steamID, out newTexture))
                        {
                            indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTexture;
                        }
                    }

                    foreach (KeyValuePair<string, CannonUse> indvidualCannon in mightyVessel.cannonOperationalDict)
                    {
                        if (cannonSkinTextures.TryGetValue(steamID, out newTexture))
                        {
                            indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", newTexture);
                        }
                    }

                    foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in mightyVessel.cannonDestroyDict)
                    {
                        if (cannonSkinTextures.TryGetValue(steamID, out newTexture))
                        {
                            indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newTexture);
                        }
                    }
                }
            } catch (Exception e)
            {
                logLow(e.Message);
                //Ignore Exception
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
                    if (e.Message.Contains("Object reference not set to an instance of an object"))
                    {
                        //Go do one
                    }
                    else
                    {
                        logLow("Failed to assign custom badge to a player:");
                        logLow(e.Message);
                    }
                }

            }
        }

        [HarmonyPatch(typeof(Character), "íëðäêñïçêêñ", new Type[] { typeof(string) })]
        static class weaponSkinPatch3rdPerson
        {
            static void Postfix(Character __instance, string îëðíîïïêñîî)
            {
                if (!useWeaponSkins)
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

                                playerWeaponsList.TryGetValue(steamID, out string skinToUse);

                                string fullSkinName = weaponName + "_" + skinToUse;

                                if (weaponTextures.TryGetValue(fullSkinName, out Texture2D newTexture))
                                {
                                    component.GetComponent<Renderer>().material.SetTexture("_MainTex", newTexture);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Array index is out of range."))
                    {
                        //Do nothing cause this fucking spamms the log
                    }
                    else
                    {
                        logLow("ERROR:");
                        logLow(e.Message);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WeaponRender), "Start")]
        static class weaponSkinpatch1stPerson
        {
            static void Postfix(WeaponRender __instance)
            {
                if (!useWeaponSkins)
                {
                    return;
                }

                if (!__instance.åïääìêêäéèç && __instance.ëæìéäîåçóæí)
                {
                    logLow(__instance.GetComponent<Renderer>().material.name);
                    if (__instance.GetComponent<Renderer>().material.name.StartsWith("wpn_"))
                    {
                        string wpnName = __instance.GetComponent<Renderer>().material.name.Split('_')[1];
                        try
                        {
                            string steamID = SteamUser.GetSteamID().m_SteamID.ToString();

                            playerWeaponsList.TryGetValue(steamID, out string skinToUse);
                            string fullSkinName = wpnName + "_" + skinToUse;
                            if (weaponTextures.TryGetValue(fullSkinName, out Texture2D newTexture))
                            {
                                __instance.GetComponent<Renderer>().material.SetTexture("_MainTex", newTexture);
                            }

                        }
                        catch (Exception e)
                        {
                            if (e.Message.Contains("Array index is out of range."))
                            {
                                //Do nothing cause this fucking spamms the log
                            }
                            else
                            {
                                logLow("ERROR:");
                                logLow(e.Message);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SailHealth), "OnEnable")]
        static class sailSkinPatch
        {
            static void Postfix(SailHealth __instance)
            {
                try
                {
                    Transform shipTransf = __instance.transform.root;
                    if (shipTransf)
                    {
                        Texture2D newTexture;
                        int teamNum = int.Parse(shipTransf.name.Split('m')[1]);

                        string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

                        if (!sailSkinTextures.TryGetValue(steamID, out newTexture))
                        {
                            return;
                        }

                        string shipType = GameMode.Instance.shipTypes[teamNum - 1];
                        shipType = shipType.Remove(shipType.Length - 1);

                        if (cachedGameObjects.defaultSails == null)
                        {
                            cachedGameObjects.setDefaultSails((Texture2D)__instance.GetComponent<Renderer>().material.mainTexture);
                        }


                        switch (shipType)
                        {
                            case "cruiser":
                                if (__instance.name == "hmsSophie_sails08")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }


                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }


                                if (__instance.name != "hmsSophie_sails08")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "galleon":
                                if (__instance.name == "galleon_sails_01")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }


                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "galleon_sails_01")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "brig":
                                if (__instance.name == "hmsSpeedy_sails04")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "hmsSpeedy_sails04")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "xebec":
                                if (__instance.name == "xebec_sail03")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "xebec_sail03")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "bombvessel":
                                if (__instance.name == "bombVessel_sails07")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "bombVessel_sails07")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "gunboat":
                                if (__instance.name == "gunboat_sails02")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "gunboat_sails02")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "cutter":
                                if (__instance.name == "hmsAlert_sails02")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "hmsAlert_sails02")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "bombketch":
                                if (__instance.name == "bombKetch_sails06")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "bombKetch_sails06")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "carrack":
                                if (__instance.name == "carrack_sail03")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "carrack_sail03")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "junk":
                                if (__instance.name == "junk_sails_01")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "junk_sails_01")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "schooner":
                                if (__instance.name == "schooner_sails02" || __instance.name == "schooner_sails00")
                                {

                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (mainSailDict.TryGetValue(steamID, out newTexture))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                    }
                                }
                                else if (sailSkinTextures.TryGetValue(steamID, out newTexture))
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = newTexture;
                                }

                                if (__instance.name != "schooner_sails02" && __instance.name != "schooner_sails00")
                                {
                                    if (cachedGameObjects.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        cachedGameObjects.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    logLow(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(CannonUse), "OnEnable")]
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
                    if (cachedGameObjects.defaultCannons == null)
                    {
                        cachedGameObjects.setDefaultCannons((Texture2D)child.GetComponent<Renderer>().material.mainTexture);
                    }

                    if (cachedGameObjects.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                    {
                        vessel.cannonOperationalDict.Add((vessel.cannonOperationalDict.Count + 1).ToString(), __instance);
                    }
                    else
                    {
                        cachedShip newVessel = new cachedShip();
                        newVessel.cannonOperationalDict.Add("1", __instance);
                        cachedGameObjects.ships.Add(index.ToString(), newVessel);
                    }

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

                if (cachedGameObjects.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                {
                    vessel.cannonDestroyDict.Add((vessel.cannonDestroyDict.Count + 1).ToString(), __instance);
                }
                else
                {
                    cachedShip newVessel = new cachedShip();
                    newVessel.cannonDestroyDict.Add("1", __instance);
                    cachedGameObjects.ships.Add(index.ToString(), newVessel);
                }

                if (cannonSkinTextures.TryGetValue(steamID, out Texture2D newTexture))
                {
                    __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newTexture);
                }
            }
        }

        [HarmonyPatch(typeof(AccoladeItem), "ëîéæìêìëéæï")]
        static class accoladeSetInfoPatch
        {
            static void Postfix(AccoladeItem __instance, string óéíïñîèëëêð, int òææóïíéñåïñ, string çìîñìëðêëéò, CSteamID ìçíêääéïíòç)
            {
                try
                {
                    string steamID = ìçíêääéïíòç.m_SteamID.ToString();
                    if (badgeTextures.TryGetValue(steamID, out Texture2D newTexture))
                    {
                        __instance.äæåéåîèòéîñ.texture = newTexture;
                    }
                }
                catch (Exception e)
                {
                    logLow(e.Message);
                }
            }
        }


        [HarmonyPatch(typeof(GameMode), "newRound")]
        static class newRoundPatch
        {
            static void Postfix(GameMode __instance)
            {
                resetAllShipsToDefault();
            }
        }

        [HarmonyPatch(typeof(PlayerOptions), "passCaptain")]
        static class passCaptainPatch
        {
            static void Prefix(PlayerOptions __instance)
            {
                if (LocalPlayer.îêêæëçäëèñî.äíìíëðñïñéè.isCaptain())
                {
                    PlayerInfo player = GameMode.getPlayerInfo(__instance.êåééóæåñçòì.text);
                    string steamNewCaptainID = player.steamID.ToString();
                    string teamNum = player.team.ToString();
                    assignNewTexturesToShips(steamNewCaptainID, teamNum);
                }
            }
        }

    }
}
