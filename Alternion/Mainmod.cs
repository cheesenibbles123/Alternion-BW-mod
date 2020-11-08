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
    

    [Mod]
    public class Mainmod : MonoBehaviour
    {
        Texture2D watermarkTex;

        static cachedCannonsAndSails cachedGameObjects = new cachedCannonsAndSails();
        static Dictionary<string, playerObject> playerDictionary = new Dictionary<string, playerObject>();

        public static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
        static int logLevel = 1;
        static bool showTWBadges = false;
        static string mainUrl = "http://www.archiesbots.com/BlackwakeStuff/";

        void Start()
        {
            try
            {
                //Setup harmony patching
                HarmonyInstance harmony = HarmonyInstance.Create("com.github.archie");
                harmony.PatchAll();

                //Starts asset fetching cycle
                createDirectories();

                //Setup watermark
                StartCoroutine(waterMark());
            }
            catch (Exception e)
            {
                debugLog(e.Message);
            }
        }

        void OnGUI()
        {
            if (watermarkTex != null)
            {
                GUI.DrawTexture(new Rect(10, 10, 64, 52), watermarkTex, ScaleMode.ScaleToFit);
            }
        }

        private IEnumerator loadJsonFile()
        {
            LoadingBar.updatePercentage(0, "Fetching Players");
            List<webPlayerObject> webPlayers = new List<webPlayerObject>();

            WWW www = new WWW(mainUrl + "playerObjectList.json");
            yield return www;

            try
            {
                string[] json = www.text.Split('&');
                for (int i = 0; i < json.Length; i++)
                {
                    webPlayerObject player = JsonUtility.FromJson<webPlayerObject>(json[i]);
                    webPlayers.Add(player);
                }
            }
            catch (Exception e)
            {
                debugLog("------------------");
                debugLog("Loading from JSON error");
                debugLog(e.Message);
                debugLog("------------------");
            }
            logLow("Finished loading Json");
            StartCoroutine(DownloadTextures(webPlayers));
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
                    debugLog("Error downloading watermark:");
                    debugLog(e.Message);
                }

            }

            watermarkTex = loadTexture("pfp", texturesFilePath, 258, 208);
        }
        private IEnumerator DownloadTextures(List<webPlayerObject> players)
        {
            List<string> alreadyDownloaded = new List<string>();
            WWW www;
            bool flag;

            LoadingBar.updatePercentage(20, "Downloading Textures");
            //Grab UI textures

            //Grab Player textures
            for (int i = 0; i < players.Count; i++)
            {
                playerObject finalPlayer = new playerObject();
                finalPlayer.Init(players[i].steamID);

                if (players[i].badgeName != "null")
                {
                    flag = alreadyDownloaded.Contains(players[i].badgeName);
                    if (!flag)
                    {
                        www = new WWW(mainUrl + "Badges/" + players[i].badgeName + ".png");
                        yield return www;
                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes(Application.dataPath + texturesFilePath + "Badges/" + players[i].badgeName + ".png", bytes);
                        }
                        catch (Exception e)
                        {
                            debugLog(e.Message);
                        }
                    }
                    try
                    {

                        finalPlayer.badgeTexture = loadTexture(players[i].badgeName, texturesFilePath + "Badges/", 100, 40);
                        finalPlayer.badgeTexture.name = players[i].badgeName;
                    }
                    catch (Exception e)
                    {
                        debugLog("------------------");
                        debugLog("Badge Download Error");
                        debugLog(e.Message);
                    }
                }

                if (players[i].sailSkinName != "null")
                {
                    flag = alreadyDownloaded.Contains(players[i].sailSkinName);
                    if (!flag)
                    {
                        www = new WWW(mainUrl + "SailSkins/" + players[i].sailSkinName + ".png");
                        yield return www;

                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes(Application.dataPath + texturesFilePath + "SailSkins/" + players[i].sailSkinName + ".png", bytes);
                        }
                        catch (Exception e)
                        {
                            debugLog("------------------");
                            debugLog("Sail Skin Download Error");
                            debugLog(e.Message);
                        }
                    }
                    try
                    {
                        finalPlayer.sailSkinTexture = loadTexture(players[i].sailSkinName, texturesFilePath + "SailSkins/", 2048, 2048);
                        finalPlayer.sailSkinTexture.name = players[i].sailSkinName;
                    }
                    catch (Exception e)
                    {
                        debugLog("------------------");
                        debugLog(e.Message);
                        debugLog("------------------");
                    }
                }

                if (players[i].weaponSkinName != "null")
                {
                    List<string> weaponNames = new List<string>()
                    {
                    "nockgun", "blunderbuss", "musket", "handmortar",
                    "duckfoot", "standardPistol", "shortPistol", "matchlock" , "annelyRevolver",
                    "cutlass", "rapier", "axe", "dagger", "pike",
                    "tomohawk"
                    };

                    flag = alreadyDownloaded.Contains(players[i].weaponSkinName);
                    Texture2D weaponSkin;

                    for (int s = 0; s < weaponNames.Count; s++)
                    {
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "WeaponSkins/" + weaponNames[s] + "_" + players[i].weaponSkinName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + weaponNames[s] + "_" + players[i].weaponSkinName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog("Internal Weapon skins Download exception");
                                debugLog(e.Message);
                            }
                        }
                        try
                        {
                            if (players[i].weaponSkinName != "null")
                            {
                                weaponSkin = loadTexture(weaponNames[s] + "_" + players[i].weaponSkinName, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                weaponSkin.name = weaponNames[s] + "_" + players[i].weaponSkinName;
                                finalPlayer.weaponTextures.Add(weaponNames[s], weaponSkin);
                            }
                        }
                        catch (Exception e)
                        {
                            debugLog("------------------");
                            debugLog("Weapon Skin Download Error");
                            debugLog($"Weapon skin : -{weaponNames[s]}- -{players[i].weaponSkinName}-");
                            debugLog("filePath: " + texturesFilePath + "WeaponSkins/");
                            debugLog(e.Message);
                        }
                    }
                }

                if (players[i].mainSailName != "null")
                {
                    flag = alreadyDownloaded.Contains(players[i].mainSailName);
                    if (!flag)
                    {
                        www = new WWW(mainUrl + "MainSailSkins/" + players[i].mainSailName + ".png");
                        yield return www;

                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes(Application.dataPath + texturesFilePath + "MainSailSkins/" + players[i].mainSailName + ".png", bytes);
                        }
                        catch (Exception e)
                        {
                            debugLog(e.Message);
                        }
                    }
                    try
                    {
                        finalPlayer.mainSailTexture = loadTexture(players[i].mainSailName, texturesFilePath + "MainSailSkins/", 2048, 2048);
                        finalPlayer.mainSailTexture.name = players[i].mainSailName;
                    }
                    catch (Exception e)
                    {
                        debugLog("------------------");
                        debugLog("Main Sail Download Error");
                        debugLog(e.Message);
                    }
                }

                if (players[i].cannonSkinName != "null")
                {
                    flag = alreadyDownloaded.Contains(players[i].cannonSkinName);
                    if (!flag)
                    {
                        www = new WWW(mainUrl + "CannonSkins/" + players[i].cannonSkinName + ".png");
                        yield return www;

                        try
                        {
                            byte[] bytes = www.texture.EncodeToPNG();
                            File.WriteAllBytes(Application.dataPath + texturesFilePath + "CannonSkins/" + players[i].cannonSkinName + ".png", bytes);
                        }
                        catch (Exception e)
                        {
                            debugLog(e.Message);
                        }
                    }
                    try
                    {
                        finalPlayer.cannonSkinTexture = loadTexture(players[i].cannonSkinName, texturesFilePath + "CannonSkins/", 2048, 2048);
                        finalPlayer.cannonSkinTexture.name = players[i].cannonSkinName;
                    }
                    catch (Exception e)
                    {
                        debugLog("------------------");
                        debugLog("Cannon Skin Download Error");
                        debugLog(e.Message);
                    }
                }

                playerDictionary.Add(players[i].steamID, finalPlayer);
                float newPercentage = 20 + (60 * ((float)i / (float)players.Count));
                logLow(newPercentage.ToString());
                LoadingBar.updatePercentage(newPercentage, "Downloading Textures");
            }

            setupMainMenu();
        }

        void createDirectories()
        {
            //Create directories prior to downloading all asset files
            if (!File.Exists(Application.dataPath + texturesFilePath))
            {
                Directory.CreateDirectory(Application.dataPath + texturesFilePath);
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "Badges/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "WeaponSkins/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "SailSkins/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "MainSailSkins/");
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "CannonSkins/");
            }
            logLow("Finished directories");

            //Grab online JSON file
            StartCoroutine(loadJsonFile());
        }
        static void setupMainMenu()
        {
            LoadingBar.updatePercentage(90, "Preparing Main Menu");
            if (!ModGUI.useWeaponSkins && !ModGUI.useBadges)
            {
                LoadingBar.updatePercentage(100, "Finished!");
                return;
            }
            setMainmenuBadge();
        }
        static void setMainmenuBadge()
        {

            if (!ModGUI.useBadges)
            {
                LoadingBar.updatePercentage(95, "applying wpn skin");
                return;
            }

            //Only main menu that you will really see is the one intially started
            //This doesn't work if you return to the main menu from a server
            MainMenu mm = FindObjectOfType<MainMenu>();

            try
            {
                string steamID = Steamworks.SteamUser.GetSteamID().ToString();
                if (playerDictionary.TryGetValue(steamID, out playerObject player))
                {
                    if (mm.menuBadge.texture.name != "tournamentWake1Badge" ^ (!showTWBadges & mm.menuBadge.texture.name == "tournamentWake1Badge"))
                    {
                        if (player.badgeTexture)
                        {
                            mm.menuBadge.texture = player.badgeTexture;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                debugLog("Failed to assign custom badge to a player:");
                debugLog(e.Message);
            }

            LoadingBar.updatePercentage(95, "applying wpn skin");
            setMainMenuWeaponSkin();

        }
        static void setMainMenuWeaponSkin()
        {
            try
            {
                if (!ModGUI.useWeaponSkins)
                {
                    LoadingBar.updatePercentage(100, "Finished!");
                    return;
                }
                string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                logLow("Gotten steamID: " + steamID);
                if (playerDictionary.TryGetValue(steamID, out playerObject player))
                {
                    logLow("Gotten player: " + player.getSteamID());
                    if (player.weaponTextures.TryGetValue("musket", out Texture2D newTex))
                    {
                        logLow("Getting musket");
                        var musket = GameObject.Find("wpn_standardMusket_LOD1");
                        if (musket != null)
                        {
                            logLow("Got musket");
                            musket.GetComponent<Renderer>().material.mainTexture = newTex;
                            logLow("Set texture");
                        }
                        else
                        {
                            debugLog("Main menu musket not found.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                debugLog(e.Message);
            }

            LoadingBar.updatePercentage(100, "Finished!");
        }

        //Debugging purposes
        static void logLow(string message)
        {
            //Just easier to type than Log.logger.Log
            // Also lets me just set logLevel to 0 if I dont want to deal with the spam.
            if (logLevel > 0)
            {
                Log.logger.Log(message);
            }
        }
        
        //ALWAYS RUNS
        static void debugLog(string message)
        {
            //Just easier to type than Log.logger.Log
            //Will always log, so only use in try{} catch(Exception e) {} when absolutely needed
            Log.logger.Log(message);
        }

        public static Texture2D loadTexture(string texName, string filePath, int imgWidth, int imgHeight)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(Application.dataPath + filePath + texName + ".png");

                Texture2D tex = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24, false);
                tex.LoadImage(fileData);
                return tex;

            }
            catch (Exception e)
            {
                debugLog(string.Format("Error loading texture {0}", texName));
                debugLog(e.Message);
                // Return default white texture on failing to load
                return Texture2D.whiteTexture;
            }
        }

        static void resetAllShipsToDefault()
        {
            // Loop through all ships, and set all visuals to defaults in the following order:
            // Sails
            // Main Sails
            // Functioning cannons
            // Destroyed cannons
            foreach (KeyValuePair<string, cachedShip> individualShip in cachedGameObjects.ships)
            {
                foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.sailDict)
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

        //NEEDS FIXING
        static void assignNewTexturesToShips(string steamID, string index)
        {
            try
            {
                // Loop through all cached vessels and apply new textures in the following order:
                // Sails
                // Main Sails
                // Functional Cannons
                // Destroyed Cannons
                if (cachedGameObjects.ships.TryGetValue(index, out cachedShip mightyVessel))
                {
                    if (playerDictionary.TryGetValue(steamID, out playerObject player))
                    {

                        foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                        {
                            if (player.sailSkinTexture)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                            }
                        }

                        foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                        {
                            if (player.mainSailTexture)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                            }
                        }

                        foreach (KeyValuePair<string, CannonUse> indvidualCannon in mightyVessel.cannonOperationalDict)
                        {
                            if (player.cannonSkinTexture)
                            {
                                indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", player.cannonSkinTexture);
                            }
                        }

                        foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in mightyVessel.cannonDestroyDict)
                        {
                            if (player.cannonSkinTexture)
                            {
                                indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", player.cannonSkinTexture);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logLow(e.Message);
                //Ignore Exception
            }
        }

        static void assignWeaponToRenderer(WeaponRender __instance, Renderer renderer, playerObject player, string weapon, string type)
        {
            try
            {
                // If the player Dict contains a reference to the specific weapon, output the texture
                if (player.weaponTextures.TryGetValue(weapon, out Texture2D newTexture))
                {
                    logLow($"Assigning -{weapon}- skin to -{player.getSteamID()}-");
                    renderer.material.mainTexture = newTexture;
                }
                logLow("Applied to -" + renderer.material.name + "- in " + type + " mode.");
            }catch (Exception e)
            {
                debugLog(e.Message);
            }
        }

        static void weaponSkinHandler(WeaponRender __instance, playerObject player, string type)
        {

            Renderer renderer = __instance.GetComponent<Renderer>();
            logLow(renderer.material.mainTexture.name);

            switch (renderer.material.mainTexture.name)
            {
                case "wpn_standardMusket_stock_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "musket", type);
                    break;
                case "musket_diamond":
                    assignWeaponToRenderer(__instance, renderer, player, "musket", type);
                    break;
                case "wpn_standardCutlass_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "cutlass", type);
                    break;
                case "wpn_blunderbuss_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "blunderbuss", type);
                    break;
                case "wpn_nockGun_stock_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "nockgun", type);
                    break;
                case "wpn_handMortar_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "handmortar", type);
                    break;
                case "wpn_rapier_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "rapier", type);
                    break;
                case "wpn_dagger_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "dagger", type);
                    break;
                case "wpn_bottle_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "bottle", type);
                    break;
                case "wpn_standardPistol_stock_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "standardPistol", type);
                    break;
                case "wpn_shortpistol_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "shortPistol", type);
                    break;
                case "wpn_duckfoot_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "duckfoot", type);
                    break;
                case "wpn_annelyRevolver_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "annelyRevolver", type);
                    break;
                case "wpn_rumHealth_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "rumBottle", type);
                    break;
                case "wpn_teaCup_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "teaCup", type);
                    break;
                case "wpn_grenade_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "grenade", type);
                    break;
                case "prp_hammer_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "hammer", type);
                    break;
                case "prp_lighter_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "lighter", type);
                    break;
                case "wpn_booty_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "booty", type);
                    break;
                case "wpn_ramrod_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "ramrod", type);
                    break;
                case "wpn_tomohawk_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "tomohawk", type);
                    break;
                case "wpn_matchlockRevolver_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "matchlockRevolver", type);
                    break;
                case "wpn_twoHandAxe_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "axe", type);
                    break;
                case "wpn_boardingPike_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "pike", type);
                    break;
                case "wpn_spyglass_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "spyglass", type);
                    break;
                case "prp_atlas_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "atlass", type);
                    break;
                default:
                    // If not known, output here
                    logLow("Default name: -" + renderer.material.mainTexture.name + "-");
                    break;
            }
        }

        [HarmonyPatch(typeof(ScoreboardSlot), "ñòæëíîêïæîí", new Type[] { typeof(string), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        static class ScoreBoardSlotAdjuster
        {
            static void Postfix(ScoreboardSlot __instance, string ìåäòäóëäêèæ, int óîèòèðîðçîì, string ñíçæóñðæéòó, int çïîèïçïñêïæ, int äïóïåòòéðåç, int ìëäòìèçñçìí, int óíïòðíäóïçç, int íîóìóíèíñìå, bool ðèæòðìêóëïð, bool äåîéíèñèììñ, bool æíèòîîìðçóî, int ïîñíñóóåîîñ, int æìíñèéçñîíí, bool òêóçíïåæíîë, bool æåèòðéóçêçó, bool èëçòëæêäêîå, bool ëååííåïäæîè, bool ñîäèñæïîóçó)
            {
                try
                {
                    if (!ModGUI.useBadges)
                    {
                        return;
                    }
                    string steamID = GameMode.getPlayerInfo(ìåäòäóëäêèæ).steamID.ToString();
                    if (playerDictionary.TryGetValue(steamID, out playerObject player))
                    {
                        if (__instance.éòëèïòëóæèó.texture.name != "tournamentWake1Badge" ^ (!showTWBadges & __instance.éòëèïòëóæèó.texture.name == "tournamentWake1Badge"))
                        {
                            logLow("Found match for ID " + steamID.ToString());
                            __instance.éòëèïòëóæèó.texture = player.badgeTexture; // loadTexture(badgeName[i], 110, 47);
                            logLow("Set texture");
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
                        debugLog("Failed to assign custom badge to a player:");
                        debugLog(e.Message);
                    }
                }

            }
        }

        //applyGold()
        [HarmonyPatch(typeof(WeaponRender), "ìæóòèðêççæî")]
        static class goldApplyPatch
        {
            static void Postfix(WeaponRender __instance)
            {
                try
                {
                    if (!ModGUI.useWeaponSkins)
                    {
                        return;
                    }
                    PlayerInfo plyrInf = __instance.ìäóêäðçóììî.ìêïòëîåëìòñ.gameObject.transform.parent.parent.GetComponent<PlayerInfo>();
                    string steamID = plyrInf.steamID.ToString();

                    if (playerDictionary.TryGetValue(steamID, out playerObject player))
                    {
                        weaponSkinHandler(__instance, player, "3p");
                    }
                }
                catch (Exception e)
                {
                    debugLog("err: " + e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(WeaponRender), "Start")]
        static class weaponSkinpatch1stPerson
        {
            static void Postfix(WeaponRender __instance)
            {
                try
                {
                    if (!ModGUI.useWeaponSkins)
                    {
                        return;
                    }
                    if (!__instance.åïääìêêäéèç)
                    {
                        //Grab local steamID
                        string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                        if (playerDictionary.TryGetValue(steamID, out playerObject player))
                        {
                            weaponSkinHandler(__instance, player, "1p");
                        }
                    }
                }
                catch (Exception e)
                {
                    debugLog(e.Message);
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
                    if (!ModGUI.useMainSails && ModGUI.useSecondarySails)
                    {
                        return;
                    }
                    Transform shipTransf = __instance.transform.root;
                    if (shipTransf)
                    {
                        int teamNum = int.Parse(shipTransf.name.Split('m')[1]);

                        string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

                        if (cachedGameObjects.defaultSails == null)
                        {
                            cachedGameObjects.setDefaultSails((Texture2D)__instance.GetComponent<Renderer>().material.mainTexture);
                        }

                        if (!playerDictionary.TryGetValue(steamID, out playerObject player))
                        {
                            return;
                        }

                        string shipType = GameMode.Instance.shipTypes[teamNum - 1];
                        shipType = shipType.Remove(shipType.Length - 1);


                        switch (shipType)
                        {
                            case "cruiser":
                                if (__instance.name == "hmsSophie_sails08" && ModGUI.useMainSails)
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


                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "galleon_sails_01" && ModGUI.useMainSails)
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


                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "hmsSpeedy_sails04" && ModGUI.useMainSails)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "xebec_sail03" && ModGUI.useMainSails)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "bombVessel_sails07" && ModGUI.useMainSails)
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

                                    if (player.mainSailTexture && ModGUI.useSecondarySails)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "gunboat_sails02" && ModGUI.useMainSails)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "hmsAlert_sails02" && ModGUI.useMainSails)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "bombKetch_sails06" && ModGUI.useMainSails)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "carrack_sail03" && ModGUI.useMainSails)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if (__instance.name == "junk_sails_01" && ModGUI.useMainSails)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                                if ((__instance.name == "schooner_sails02" && ModGUI.useMainSails) || (__instance.name == "schooner_sails00" && ModGUI.useMainSails))
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture && ModGUI.useSecondarySails)
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = player.sailSkinTexture;
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
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
                    debugLog(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(CannonUse), "OnEnable")]
        static class cannonOperationalSkinPatch
        {
            static void Postfix(CannonUse __instance)
            {
                try
                {
                    if (!ModGUI.useCannonSkins)
                    {
                        return;
                    }
                    Transform child = __instance.transform.FindChild("cannon");
                    int.TryParse( child.transform.root.name.Split('m')[1] , out int index);
                    logLow($"Gotten index {index - 1} -operational");
                    if (cachedGameObjects.defaultCannons == null)
                    {
                        logLow("set default cannons -operational");
                        cachedGameObjects.setDefaultCannons((Texture2D)child.GetComponent<Renderer>().material.mainTexture);
                    }
                    logLow("Default cannons already exist -operational");
                    string steamID = GameMode.Instance.teamCaptains[index - 1].steamID.ToString();
                    logLow("Gotten steamID: " + steamID + " -operational");

                    if (playerDictionary.TryGetValue(steamID, out playerObject player))
                    {
                        logLow("Gotten player");
                        // If vessel is already cached, grab it and add, otherwise create new vessel
                        if (cachedGameObjects.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                        {
                            logLow($"Added to existing vessel -operational");
                            vessel.cannonOperationalDict.Add((vessel.cannonOperationalDict.Count + 1).ToString(), __instance);
                        }
                        else
                        {
                            logLow($"New vessel -operational");
                            cachedShip newVessel = new cachedShip();
                            newVessel.cannonOperationalDict.Add("1", __instance);
                            cachedGameObjects.ships.Add(index.ToString(), newVessel);
                        }

                        // If they have a custom texture, use it, else use default skin
                        if (player.cannonSkinTexture != null)
                        {
                            logLow("Setting cannon texture: -" + player.cannonSkinTexture.name + "- for -" + steamID + "-");
                            child.GetComponent<Renderer>().material.SetTexture("_MainTex", player.cannonSkinTexture);
                        }
                        else
                        {
                            logLow($"Setting default skin -operational");
                            child.GetComponent<Renderer>().material.SetTexture("_MainTex", cachedGameObjects.defaultCannons);
                        }
                    }
                    logLow($"Finished -operational");
                }
                catch (Exception e)
                {
                    debugLog("Cannon operational start");
                    debugLog(e.Message);
                    debugLog("Cannon operational end");
                }
            }
        }

        [HarmonyPatch(typeof(CannonDestroy), "Start")]
        static class cannonDestroySkinPatch
        {
            static void Postfix(CannonDestroy __instance)
            {
                try
                {
                    if (!ModGUI.useCannonSkins)
                    {
                        return;
                    }
                    int index = GameMode.getParentIndex(__instance.æïìçñðåììêç.transform.root);
                    logLow("Gotten index -destroy");
                    string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                    logLow("Gotten steamID -destroy");
                    if (playerDictionary.TryGetValue(steamID, out playerObject player))
                    {
                        // If vessel is cached, add cannon to it, else create new vessel
                        logLow("Gotten player -destroy");
                        if (cachedGameObjects.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                        {
                            vessel.cannonDestroyDict.Add((vessel.cannonDestroyDict.Count + 1).ToString(), __instance);
                            logLow("Added to vessel -destroy");
                        }
                        else
                        {
                            logLow("Created vessel -destroy");
                            cachedShip newVessel = new cachedShip();
                            newVessel.cannonDestroyDict.Add("1", __instance);
                            cachedGameObjects.ships.Add(index.ToString(), newVessel);
                        }

                        logLow("Passed vessel -destroy");

                        // If they have a cannon skin then apply
                        if (player.cannonSkinTexture != null)
                        {
                            logLow("Applied skin -destroy");
                            __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", player.cannonSkinTexture);
                        }

                        logLow("Finished -destroy");
                    }
                }catch (Exception e)
                {
                    debugLog("Cannon destroy start");
                    debugLog(e.Message);
                    debugLog("Cannon destroy end");
                }
            }
        }

        [HarmonyPatch(typeof(AccoladeItem), "ëîéæìêìëéæï")]
        static class accoladeSetInfoPatch
        {
            static void Postfix(AccoladeItem __instance, string óéíïñîèëëêð, int òææóïíéñåïñ, string çìîñìëðêëéò, CSteamID ìçíêääéïíòç)
            {
                // Sets win screen badges
                try
                {
                    if (!ModGUI.useBadges)
                    {
                        return;
                    }
                    string steamID = ìçíêääéïíòç.m_SteamID.ToString();
                    if (playerDictionary.TryGetValue(steamID, out playerObject player))
                    {
                        __instance.äæåéåîèòéîñ.texture = player.badgeTexture;
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
                // Reset all ship skins that are cached on newRound() to default textures
                resetAllShipsToDefault();
            }
        }

        [HarmonyPatch(typeof(PlayerOptions), "passCaptain")]
        static class passCaptainPatch
        {
            static void Prefix(PlayerOptions __instance)
            {
                // Untested
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
