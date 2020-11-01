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
        static Dictionary<string, int> uniquePlayerRenderers = new Dictionary<string, int>();

        static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";

        static int logLevel = 1;

        static bool showTWBadges = false;
        static bool run1pWeaponSkins = false;

        static int timesApplied = 0;
        static int timesCalled = 0;

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

        void Update()
        {
            if (Input.GetKeyDown("]"))
            {
                //logLow("applied :" + timesApplied.ToString());
                //logLow("called :" + timesCalled.ToString());
                run1pWeaponSkins = true;
            }
        }

        private IEnumerator loadJsonFile()
        {
            //webPlayerObject player = new webPlayerObject();
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
                Log.logger.Log("------------------");
                Log.logger.Log("Loading from JSON error");
                Log.logger.Log(e.Message);
                Log.logger.Log("------------------");
            }
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
                    logLow("Error downloading watermark:");
                    logLow(e.Message);
                }

            }

            watermarkTex = loadTexture("pfp", texturesFilePath, 258, 208);
        }
        private IEnumerator DownloadTextures(List<webPlayerObject> players)
        {
            List<string> alreadyDownloaded = new List<string>();
            WWW www;
            bool flag;

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
                            logLow(e.Message);
                        }
                    }
                    try
                    {

                        finalPlayer.badgeTexture = loadTexture(players[i].badgeName, texturesFilePath + "Badges/", 100, 40);
                    }
                    catch (Exception e)
                    {
                        Log.logger.Log("------------------");
                        Log.logger.Log("Badge Download Error");
                        Log.logger.Log(e.Message);
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
                            Log.logger.Log("------------------");
                            Log.logger.Log("Sail Skin Download Error");
                            logLow(e.Message);
                        }
                    }
                    try
                    {
                        finalPlayer.sailSkinTexture = loadTexture(players[i].sailSkinName, texturesFilePath + "SailSkins/", 2048, 2048);
                    }
                    catch (Exception e)
                    {
                        Log.logger.Log("------------------");
                        Log.logger.Log(e.Message);
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
                                logLow("Internal Weapon skins Download exception");
                                logLow(e.Message);
                            }
                        }
                        try
                        {
                            if (players[i].weaponSkinName != "null")
                            {
                                weaponSkin = loadTexture(weaponNames[s] + "_" + players[i].weaponSkinName, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                weaponSkin.name = players[i].weaponSkinName;
                                finalPlayer.weaponTextures.Add(weaponNames[s], weaponSkin);
                            }
                        }
                        catch (Exception e)
                        {
                            logLow("------------------");
                            logLow("Weapon Skin Download Error");
                            logLow($"Weapon skin : -{weaponNames[s]}- -{players[i].weaponSkinName}-");
                            logLow("filePath: " + texturesFilePath + "WeaponSkins/");
                            logLow(e.Message);
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
                            logLow(e.Message);
                        }
                    }
                    try
                    {
                        finalPlayer.mainSailTexture = loadTexture(players[i].mainSailName, texturesFilePath + "MainSailSkins/", 2048, 2048);
                    }
                    catch (Exception e)
                    {
                        Log.logger.Log("------------------");
                        Log.logger.Log("Main Sail Download Error");
                        Log.logger.Log(e.Message);
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
                            logLow(e.Message);
                        }
                    }
                    try
                    {
                        finalPlayer.cannonSkinTexture = loadTexture(players[i].cannonSkinName, texturesFilePath + "CannonSkins/", 2048, 2048);
                    }
                    catch (Exception e)
                    {
                        Log.logger.Log("------------------");
                        Log.logger.Log("Cannon Skin Download Error");
                        Log.logger.Log(e.Message);
                    }
                }

                playerDictionary.Add(players[i].steamID, finalPlayer);
            }
            setMainmenuBadge();
        }

        private void outputPlayerDict()
        {

            // For logging purposes ATM
            try
            {
                foreach (KeyValuePair<string, playerObject> individual in playerDictionary)
                {
                    logLow("------------ PlayerDictObjStart ------------");
                    try
                    {
                        logLow(individual.Key);
                        if (individual.Value.weaponTextures.Count >= 1)
                        {
                            foreach (KeyValuePair<string, Texture2D> weaponSkin in individual.Value.weaponTextures)
                            {
                                logLow("Weapon: " + weaponSkin.Key);
                                logLow("Skin: " + weaponSkin.Value.name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logLow(e.Message);
                    }
                    logLow("------------ PlayerDictObjEnd------------");
                }
            }
            catch (Exception e)
            {
                logLow(e.Message);
            }
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
            StartCoroutine(loadJsonFile());
        }
        void setMainmenuBadge()
        {
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
                logLow("Failed to assign custom badge to a player:");
                logLow(e.Message);
            }

        }

        static void logLow(string message)
        {
            //Just easier to type than Log.logger.Log
            // Also lets me just set logLevel to 0 if I dont want to deal with the spam.
            if (logLevel > 0)
            {
                Log.logger.Log(message);
            }
        }

        static Texture2D loadTexture(string texName, string filePath, int imgWidth, int imgHeight)
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
                logLow(string.Format("Error loading texture {0}", texName));
                logLow(e.Message);
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

        // NEEDS FIXING
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

        [HarmonyPatch(typeof(ScoreboardSlot), "ñòæëíîêïæîí", new Type[] { typeof(string), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        static class ScoreBoardSlotAdjuster
        {
            static void Postfix(ScoreboardSlot __instance, string ìåäòäóëäêèæ, int óîèòèðîðçîì, string ñíçæóñðæéòó, int çïîèïçïñêïæ, int äïóïåòòéðåç, int ìëäòìèçñçìí, int óíïòðíäóïçç, int íîóìóíèíñìå, bool ðèæòðìêóëïð, bool äåîéíèñèììñ, bool æíèòîîìðçóî, int ïîñíñóóåîîñ, int æìíñèéçñîíí, bool òêóçíïåæíîë, bool æåèòðéóçêçó, bool èëçòëæêäêîå, bool ëååííåïäæîè, bool ñîäèñæïîóçó)
            {
                try
                {
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
                        logLow("Failed to assign custom badge to a player:");
                        logLow(e.Message);
                    }
                }

            }
        }

        static void assignWeaponToRenderer(Renderer renderer, playerObject player, string weapon, string type)
        {
            if (type == "3p")
            {
                // If the player Dict contains a reference to the specific weapon, output the texture
                if (player.weaponTextures.TryGetValue(weapon, out Texture2D newTexture))
                {
                    logLow($"Assigning -{weapon}- skin to -{player.getSteamID()}- 3p");
                    renderer.material.mainTexture = newTexture;
                }
            }
            else
            {
                if (player.weaponTextures.TryGetValue(weapon, out Texture2D newTexture))
                {
                    logLow($"Assigning -{weapon}- skin to -{player.getSteamID()}- 1p");
                    renderer.material.mainTexture = newTexture;
                }
            }
            logLow("Applied to -" + renderer.material.name + "-");
        }

        [HarmonyPatch(typeof(Character), "íëðäêñïçêêñ", new Type[] { typeof(string) })]
        static class weaponSkinPatch3rdPerson
        {
            // íëðäêñïçêêñ = setCurrentWeapon(string weapon)
            // îëðíîïïêñîî = new weapon
            static void Postfix(Character __instance, string îëðíîïïêñîî)
            {
                // ìñíððåñéåèæ = weaponHand
                try
                {
                    if (__instance.ìñíððåñéåèæ == null)
                    {
                        return;
                    }
                    PlayerInfo plyrInfo = __instance.transform.parent.GetComponent<PlayerInfo>();
                    string steamID = plyrInfo.steamID.ToString();
                    logLow("Gotten: -" + steamID + "-");
                    logLow(plyrInfo.name);

                    if (playerDictionary.TryGetValue(steamID, out playerObject player))
                    {
                        logLow("Gotten player");
                        string playerObjSteamID = player.getSteamID();
                        if (playerObjSteamID == steamID)
                        {
                            if (player.weaponTextures.Count >= 1)
                            {
                                for (int i = 0; i < __instance.ìñíððåñéåèæ.childCount; i++)
                                {
                                    if (îëðíîïïêñîî == __instance.ìñíððåñéåèæ.GetChild(i).name)
                                    {
                                        WeaponRender component = __instance.ìñíððåñéåèæ.GetChild(i).GetComponent<WeaponRender>();
                                        plyrInfo.name = "EDITED";
                                        if (component != null)
                                        {
                                            Renderer renderer = component.GetComponent<Renderer>();
                                            switch (îëðíîïïêñîî)
                                            {
                                                case "wpn_standardMusket_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "musket", "3p");
                                                    break;
                                                case "wpn_cutlass_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "cutlass", "3p");
                                                    break;
                                                case "wpn_standardBlunder_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "blunderbuss", "3p");
                                                    break;
                                                case "wpn_standardNockGun_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "nockgun", "3p");
                                                    break;
                                                case "wpn_standardHandMortar_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "handmortar", "3p");
                                                    break;
                                                case "wpn_rapier":
                                                    assignWeaponToRenderer(renderer, player, "rapier", "3p");
                                                    break;
                                                case "wpn_dagger":
                                                    assignWeaponToRenderer(renderer, player, "dagger", "3p");
                                                    break;
                                                case "wpn_bottle":
                                                    assignWeaponToRenderer(renderer, player, "bottle", "3p");
                                                    break;
                                                case "wpn_standardSD_Pistol_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "standardPistol", "3p");
                                                    break;
                                                case "wpn_standardShort_Pistol_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "shortPistol", "3p");
                                                    break;
                                                case "wpn_standardDuckfoot_Pistol_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "duckfoot", "3p");
                                                    break;
                                                case "item_crate_a":
                                                    assignWeaponToRenderer(renderer, player, "crate", "3p");
                                                    break;
                                                case "wpn_annleyRevolver_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "annelyRevolver", "3p");
                                                    break;
                                                case "RumBottle":
                                                    assignWeaponToRenderer(renderer, player, "rumBottle", "3p");
                                                    break;
                                                case "Tea":
                                                    assignWeaponToRenderer(renderer, player, "teaCup", "3p");
                                                    break;
                                                case "item_grenade_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "grenade", "3p");
                                                    break;
                                                case "hammer":
                                                    assignWeaponToRenderer(renderer, player, "hammer", "3p");
                                                    break;
                                                case "lighter":
                                                    assignWeaponToRenderer(renderer, player, "lighter", "3p");
                                                    break;
                                                case "booty":
                                                    assignWeaponToRenderer(renderer, player, "booty", "3p");
                                                    break;
                                                case "ramrod":
                                                    assignWeaponToRenderer(renderer, player, "ramrod", "3p");
                                                    break;
                                                case "tomohawk":
                                                    assignWeaponToRenderer(renderer, player, "tomohawk", "3p");
                                                    break;
                                                case "wpn_matchlockRevolver_LOD1":
                                                    assignWeaponToRenderer(renderer, player, "matchlockRevolver", "3p");
                                                    break;
                                                case "wpn_twoHandAxe":
                                                    assignWeaponToRenderer(renderer, player, "axe", "3p");
                                                    break;
                                                case "wpn_boardingPike":
                                                    assignWeaponToRenderer(renderer, player, "pike", "3p");
                                                    break;
                                                case "wpn_spyglass":
                                                    assignWeaponToRenderer(renderer, player, "spyglass", "3p");
                                                    break;
                                                default:
                                                    logLow("mat name: " + renderer.material.name);
                                                    break;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }else
                        {
                            logLow($"No match for {playerObjSteamID} and {steamID}");
                        }
                    }
                }catch (Exception e)
                {
                    logLow(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(WeaponRender), "Start")]
        static class weaponSkinpatch1stPerson
        {
            static void Postfix(WeaponRender __instance)
            {
                bool isLocal = __instance.ìäóêäðçóììî.æïðèñìæêêñç;
                if (isLocal)
                {
                    //Grab local steamID
                    string steamID = SteamUser.GetSteamID().m_SteamID.ToString();

                    //Fetch the player's object by steamID
                    if (playerDictionary.TryGetValue(steamID, out playerObject player))
                    {
                        // Check If they have atleast one weapon skin
                        if (player.weaponTextures.Count >= 1)
                        {
                            //Get the WeaponRenderer's Renderer
                            Renderer renderer = __instance.GetComponent<Renderer>();
                            timesCalled += 1;
                            // Switch on texture name
                            switch (renderer.material.mainTexture.name)
                            {
                                case "wpn_standardMusket_stock_alb":
                                    assignWeaponToRenderer(renderer, player, "musket", "1p");
                                    break;
                                case "wpn_standardCutlass_alb":
                                    assignWeaponToRenderer(renderer, player, "cutlass", "1p");
                                    break;
                                case "wpn_blunderbuss_alb":
                                    assignWeaponToRenderer(renderer, player, "blunderbuss", "1p");
                                    break;
                                case "wpn_nockGun_stock_alb":
                                    assignWeaponToRenderer(renderer, player, "nockgun", "1p");
                                    break;
                                case "wpn_handMortar_alb":
                                    assignWeaponToRenderer(renderer, player, "handmortar", "1p");
                                    break;
                                case "wpn_rapier_alb":
                                    assignWeaponToRenderer(renderer, player, "rapier", "1p");
                                    break;
                                case "wpn_dagger_alb":
                                    assignWeaponToRenderer(renderer, player, "dagger", "1p");
                                    break;
                                case "wpn_bottle_alb":
                                    assignWeaponToRenderer(renderer, player, "bottle", "1p");
                                    break;
                                case "wpn_standardPistol_stock_alb":
                                    assignWeaponToRenderer(renderer, player, "standardPistol", "1p");
                                    break;
                                case "wpn_shortpistol_alb":
                                    assignWeaponToRenderer(renderer, player, "shortPistol", "1p");
                                    break;
                                case "wpn_duckfoot_alb":
                                    assignWeaponToRenderer(renderer, player, "duckfoot", "1p");
                                    break;
                                case "wpn_annelyRevolver_alb":
                                    assignWeaponToRenderer(renderer, player, "annelyRevolver", "1p");
                                    break;
                                case "wpn_rumHealth_alb":
                                    assignWeaponToRenderer(renderer, player, "rumBottle", "1p");
                                    break;
                                case "wpn_teaCup_alb":
                                    assignWeaponToRenderer(renderer, player, "teaCup", "1p");
                                    break;
                                case "wpn_grenade_alb":
                                    assignWeaponToRenderer(renderer, player, "grenade", "1p");
                                    break;
                                case "prp_hammer_alb":
                                    assignWeaponToRenderer(renderer, player, "hammer", "1p");
                                    break;
                                case "prp_lighter_alb":
                                    assignWeaponToRenderer(renderer, player, "lighter", "1p");
                                    break;
                                case "wpn_booty_alb":
                                    assignWeaponToRenderer(renderer, player, "booty", "1p");
                                    break;
                                case "wpn_ramrod_alb":
                                    assignWeaponToRenderer(renderer, player, "ramrod", "1p");
                                    break;
                                case "wpn_tomohawk_alb":
                                    assignWeaponToRenderer(renderer, player, "tomohawk", "1p");
                                    break;
                                case "wpn_matchlockRevolver_alb":
                                    assignWeaponToRenderer(renderer, player, "matchlockRevolver", "1p");
                                    break;
                                case "wpn_twoHandAxe_alb":
                                    assignWeaponToRenderer(renderer, player, "axe", "1p");
                                    break;
                                case "wpn_boardingPike_alb":
                                    assignWeaponToRenderer(renderer, player, "pike", "1p");
                                    break;
                                case "wpn_spyglass_alb":
                                    assignWeaponToRenderer(renderer, player, "spyglass", "1p");
                                    break;
                                case "prp_atlas_alb":
                                    assignWeaponToRenderer(renderer, player, "atlass", "1p");
                                    break;
                                default:
                                    // If not known, output here
                                    logLow("Default 1p name: -" + renderer.material.mainTexture.name + "-");
                                    break;
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


                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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


                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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

                                    if (player.mainSailTexture)
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = player.mainSailTexture;
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = cachedGameObjects.defaultSails;
                                    }
                                }
                                else if (player.sailSkinTexture)
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
                    logLow(e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(CannonUse), "OnEnable")]
        static class cannonOperationalSkinPatch
        {
            static void Postfix(CannonUse __instance)
            {
                Transform child = __instance.transform.FindChild("cannon");
                int index = GameMode.getParentIndex(child.transform.root);

                string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                if (playerDictionary.TryGetValue(steamID, out playerObject player))
                {
                    if (cachedGameObjects.defaultCannons == null)
                    {
                        cachedGameObjects.setDefaultCannons((Texture2D)child.GetComponent<Renderer>().material.mainTexture);
                    }

                    // If vessel is already cached, grab it and add, otherwise create new vessel
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

                    // If they have a custom texture, use it, else use default skin
                    if (player.cannonSkinTexture)
                    {
                        child.GetComponent<Renderer>().material.SetTexture("_MainTex", player.cannonSkinTexture);
                    }
                    else
                    {
                        child.GetComponent<Renderer>().material.SetTexture("_MainTex", cachedGameObjects.defaultCannons);
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
                if (playerDictionary.TryGetValue(steamID, out playerObject player))
                {
                    // If vessel is cached, add cannon to it, else create new vessel
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

                    // If they have a cannon skin then apply
                    if (player.cannonSkinTexture)
                    {
                        __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", player.cannonSkinTexture);
                    }
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
