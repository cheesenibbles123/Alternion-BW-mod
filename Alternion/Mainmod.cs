using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;

using UnityEngine;
using BWModLoader;
using Harmony;
using Steamworks;


namespace Alternion
{
    

    [Mod]
    public class Mainmod : MonoBehaviour
    {
        Texture2D watermarkTex;

        public static string texturesFilePath = "/Managed/Mods/Assets/Archie/Textures/";
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

                //Rotate Character
                //InvokeRepeating("rotateMainMenuCharacter", 1, 0.1f);
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

            WWW www = new WWW(mainUrl + "playerObjectList2.json");
            yield return www;
            logLow("WWW START");
            logLow(www.text);
            logLow("WWW END");

            try
            {
                string[] json = www.text.Split('&');
                for (int i = 0; i < json.Length; i++)
                {
                    logLow("----------");
                    logLow(json[i]);
                    logLow("----------");
                    playerObject player = JsonUtility.FromJson<playerObject>(json[i]);
                    theGreatCacher.players.Add(player.steamID, player);
                    logLow(theGreatCacher.players.Count.ToString());
                    LoadingBar.updatePercentage(0 + (20 * ((float)i / (float)json.Length)), "Downloading players");
                }
            }
            catch (Exception e)
            {
                debugLog("------------------");
                debugLog("Loading from JSON error");
                debugLog(e.Message);
                debugLog("------------------");
            }
            LoadingBar.updatePercentage(20, "Finished getting players");
            StartCoroutine(DownloadTextures());
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
        private IEnumerator DownloadTextures()
        {
            List<string> alreadyDownloaded = new List<string>();
            WWW www;
            bool flag;
            Texture newTex;
            LoadingBar.updatePercentage(20, "Preparing to download");
            //Grab UI textures
            logLow("Player Count:" + theGreatCacher.players.Count.ToString());
            //Grab Player textures
            for (int i = 0; i < theGreatCacher.players.Count; i++)
            {
                foreach (KeyValuePair<string, playerObject> player in theGreatCacher.players)
                {
                    if (player.Value.badgeName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.badgeName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "Badges/" + player.Value.badgeName + ".png");
                            yield return www;
                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "Badges/" + player.Value.badgeName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }

                            try
                            {
                                newTex = loadTexture(player.Value.badgeName, texturesFilePath + "Badges/", 100, 40);
                                newTex.name = player.Value.badgeName;
                                theGreatCacher.badges.Add(player.Value.badgeName, newTex);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Badge Download Error");
                                debugLog(e.Message);
                                debugLog("------------------");
                            }
                        }
                    }

                    if (player.Value.maskSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.maskSkinName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "MaskSkins/" + player.Value.maskSkinName + ".png");
                            yield return www;
                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "MaskSkins/" + player.Value.maskSkinName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }
                            try
                            {

                                newTex = loadTexture(player.Value.maskSkinName, texturesFilePath + "MaskSkins/", 100, 40);
                                newTex.name = player.Value.maskSkinName;
                                theGreatCacher.maskSkins.Add(player.Value.maskSkinName, newTex);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Mask Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.sailSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.sailSkinName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "SailSkins/" + player.Value.sailSkinName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "SailSkins/" + player.Value.sailSkinName + ".png", bytes);
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
                            newTex = loadTexture(player.Value.sailSkinName, texturesFilePath + "SailSkins/", 2048, 2048);
                            newTex.name = player.Value.sailSkinName;
                            theGreatCacher.secondarySails.Add(player.Value.sailSkinName, newTex);
                        }
                        catch (Exception e)
                        {
                            debugLog("------------------");
                            debugLog(e.Message);
                            debugLog("------------------");
                        }
                    }

                    if (player.Value.weaponSkins[0].weaponName != "null")
                    {
                        foreach (weaponObject weapon in player.Value.weaponSkins)
                        {
                            flag = alreadyDownloaded.Contains(weapon.weaponName + "_" + weapon.weaponSkin);
                            Texture2D weaponSkin;

                            if (!flag)
                            {
                                www = new WWW(mainUrl + "WeaponSkins/" + weapon.weaponName + "_" + weapon.weaponSkin + ".png");
                                yield return www;

                                try
                                {
                                    byte[] bytes = www.texture.EncodeToPNG();
                                    File.WriteAllBytes(Application.dataPath + texturesFilePath + "WeaponSkins/" + weapon.weaponName + "_" + weapon.weaponSkin + ".png", bytes);
                                }
                                catch (Exception e)
                                {
                                    debugLog("Internal Weapon skins Download exception");
                                    debugLog(e.Message);
                                }
                                try
                                {
                                    weaponSkin = loadTexture(weapon.weaponName + "_" + weapon.weaponSkin, texturesFilePath + "WeaponSkins/", 2048, 2048);
                                    weaponSkin.name = weapon.weaponName + "_" + weapon.weaponSkin;
                                    theGreatCacher.weaponSkins.Add(weapon.weaponName + "_" + weapon.weaponSkin, weaponSkin);
                                }
                                catch (Exception e)
                                {
                                    debugLog("------------------");
                                    debugLog("Weapon Skin Download Error");
                                    debugLog($"Weapon skin : -{weapon.weaponName}- -{weapon.weaponSkin}-");
                                    debugLog("filePath: " + texturesFilePath + "WeaponSkins/");
                                    debugLog(e.Message);
                                }
                            }
                        }
                    }

                    if (player.Value.mainSailName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.mainSailName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "MainSailSkins/" + player.Value.mainSailName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "MainSailSkins/" + player.Value.mainSailName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }
                            try
                            {
                                newTex = loadTexture(player.Value.mainSailName, texturesFilePath + "MainSailSkins/", 2048, 2048);
                                newTex.name = player.Value.mainSailName;
                                theGreatCacher.mainSails.Add(player.Value.mainSailName, newTex);
                            }
                            catch (Exception e)
                            {
                                debugLog("------------------");
                                debugLog("Main Sail Download Error");
                                debugLog(e.Message);
                            }
                        }
                    }

                    if (player.Value.cannonSkinName != "null")
                    {
                        flag = alreadyDownloaded.Contains(player.Value.cannonSkinName);
                        if (!flag)
                        {
                            www = new WWW(mainUrl + "CannonSkins/" + player.Value.cannonSkinName + ".png");
                            yield return www;

                            try
                            {
                                byte[] bytes = www.texture.EncodeToPNG();
                                File.WriteAllBytes(Application.dataPath + texturesFilePath + "CannonSkins/" + player.Value.cannonSkinName + ".png", bytes);
                            }
                            catch (Exception e)
                            {
                                debugLog(e.Message);
                            }
                        }
                        try
                        {
                            newTex = loadTexture(player.Value.cannonSkinName, texturesFilePath + "CannonSkins/", 2048, 2048);
                            newTex.name = player.Value.cannonSkinName;
                            theGreatCacher.cannonSkins.Add(player.Value.cannonSkinName, newTex);
                        }
                        catch (Exception e)
                        {
                            debugLog("------------------");
                            debugLog("Cannon Skin Download Error");
                            debugLog(e.Message);
                        }
                    }
                }

                float newPercentage = 20 + (60 * ((float)i / (float)theGreatCacher.players.Count));
                LoadingBar.updatePercentage(newPercentage, "Downloading Textures");
            }
            // outputPlayerDict();
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
                Directory.CreateDirectory(Application.dataPath + texturesFilePath + "MaskSkins/");
            }

            //Grab online JSON file
            StartCoroutine(loadJsonFile());
        }
        static void setupMainMenu()
        {
            LoadingBar.updatePercentage(90, "Preparing Main Menu");
            if (!AlternionSettings.useWeaponSkins && !AlternionSettings.useBadges)
            {
                LoadingBar.updatePercentage(100, "Finished!");
                return;
            }
            setMainmenuBadge();
        }
        static void setMainmenuBadge()
        {

            if (!AlternionSettings.useBadges)
            {
                LoadingBar.updatePercentage(95, "applying weapon skin");
                setMainMenuWeaponSkin();
                return;
            }

            //Only main menu that you will really see is the one intially started
            //This doesn't work if you return to the main menu from a server
            MainMenu mm = FindObjectOfType<MainMenu>();

            try
            {
                string steamID = Steamworks.SteamUser.GetSteamID().ToString();
                if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                {
                    if (mm.menuBadge.texture.name != "tournamentWake1Badge" ^ (!AlternionSettings.showTWBadges & mm.menuBadge.texture.name == "tournamentWake1Badge"))
                    {
                        if (theGreatCacher.badges.TryGetValue(player.badgeName, out Texture newTex))
                        {
                            mm.menuBadge.texture = newTex;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                debugLog("Failed to assign custom badge to a player:");
                debugLog(e.Message);
            }

            LoadingBar.updatePercentage(95, "Applying weapon skin");
            setMainMenuWeaponSkin();

        }
        static void setMainMenuWeaponSkin()
        {
            try
            {
                if (!AlternionSettings.useWeaponSkins)
                {
                    LoadingBar.updatePercentage(100, "Finished!");
                    return;
                }
                string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                {
                    for (int i = 0; i < player.weaponSkins.Length; i++)
                    {
                        if (player.weaponSkins[i].weaponName == "musket")
                        {
                            var musket = GameObject.Find("wpn_standardMusket_LOD1");
                            if (musket != null)
                            {
                                if (theGreatCacher.weaponSkins.TryGetValue("musket_" + player.weaponSkins[i].weaponSkin, out Texture newTex))
                                {
                                    musket.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                            }
                            else
                            {
                                debugLog("Main menu musket not found.");
                            }
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
            if (AlternionSettings.loggingLevel > 0)
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
            foreach (KeyValuePair<string, cachedShip> individualShip in theGreatCacher.ships)
            {
                foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.sailDict)
                {
                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                }

                foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.mainSailDict)
                {
                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                }

                foreach (KeyValuePair<string, CannonUse> indvidualCannon in individualShip.Value.cannonOperationalDict)
                {
                    indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
                }

                foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in individualShip.Value.cannonDestroyDict)
                {
                    indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
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
                Texture newTex;
                if (theGreatCacher.ships.TryGetValue(index, out cachedShip mightyVessel))
                {
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        if (AlternionSettings.useSecondarySails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                            }
                        }

                        if (AlternionSettings.useMainSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                            }
                        }

                        if (AlternionSettings.useCannonSkins)
                        {
                            foreach (KeyValuePair<string, CannonUse> indvidualCannon in mightyVessel.cannonOperationalDict)
                            {
                                if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                                {
                                    indvidualCannon.Value.transform.FindChild("cannon").GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
                                }
                            }
                        }

                        if (AlternionSettings.useCannonSkins)
                        {
                            foreach (KeyValuePair<string, CannonDestroy> indvidualCannon in mightyVessel.cannonDestroyDict)
                            {
                                if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                                {
                                    indvidualCannon.Value.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                debugLog(e.Message);
                //Ignore Exception
            }
        }

        static void assignWeaponToRenderer(WeaponRender __instance, Renderer renderer, playerObject player, string weapon)
        {
            try
            {
                // If the player Dict contains a reference to the specific weapon, output the texture
                for (int i = 0; i < player.weaponSkins.Length; i++)
                {
                    if (theGreatCacher.weaponSkins.TryGetValue(weapon + "_" + player.weaponSkins[i].weaponSkin, out Texture newTexture))
                    {
                        renderer.material.mainTexture = newTexture;
                        break;
                    }
                }
            }catch (Exception e)
            {
                debugLog(e.Message);
            }
        }

        static void weaponSkinHandler(WeaponRender __instance, playerObject player, string type)
        {

            Renderer renderer = __instance.GetComponent<Renderer>();

            switch (renderer.material.mainTexture.name)
            {
                case "wpn_standardMusket_stock_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "musket");
                    break;
                case "wpn_standardCutlass_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "cutlass");
                    break;
                case "wpn_blunderbuss_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "blunderbuss");
                    break;
                case "wpn_nockGun_stock_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "nockgun");
                    break;
                case "wpn_handMortar_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "handmortar");
                    break;
                case "wpn_rapier_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "rapier");
                    break;
                case "wpn_dagger_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "dagger");
                    break;
                case "wpn_bottle_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "bottle");
                    break;
                case "wpn_rumHealth_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "rum");
                    break;
                case "prp_hammer_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "hammer");
                    break;
                case "wpn_standardPistol_stock_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "standardPistol");
                    break;
                case "prp_atlas01_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "atlas01");
                    break;
                case "prp_bucket_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "bucket");
                    break;
                case "wpn_shortpistol_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "shortPistol");
                    break;
                case "wpn_duckfoot_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "duckfoot");
                    break;
                case "wpn_annelyRevolver_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "annelyRevolver");
                    break;
                case "wpn_tomohawk_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "tomohawk");
                    break;
                case "wpn_matchlockRevolver_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "matchlockRevolver");
                    break;
                case "wpn_twoHandAxe_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "axe");
                    break;
                case "wpn_boardingPike_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "pike");
                    break;
                case "wpn_spyglass_alb":
                    assignWeaponToRenderer(__instance, renderer, player, "spyglass");
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
                    if (!AlternionSettings.useBadges)
                    {
                        return;
                    }
                    string steamID = GameMode.getPlayerInfo(ìåäòäóëäêèæ).steamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        if (__instance.éòëèïòëóæèó.texture.name != "tournamentWake1Badge" ^ (!AlternionSettings.showTWBadges & __instance.éòëèïòëóæèó.texture.name == "tournamentWake1Badge"))
                        {
                            if (theGreatCacher.badges.TryGetValue(steamID, out Texture newTexture))
                            {
                                __instance.éòëèïòëóæèó.texture = newTexture; // loadTexture(badgeName[i], 110, 47);
                            }
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
                    if (!AlternionSettings.useWeaponSkins)
                    {
                        return;
                    }
                    PlayerInfo plyrInf = __instance.ìäóêäðçóììî.ìêïòëîåëìòñ.gameObject.transform.parent.parent.GetComponent<PlayerInfo>();
                    string steamID = plyrInf.steamID.ToString();

                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
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

        [HarmonyPatch(typeof(Character), "setGoldMask")]
        static class goldMaskPatch
        {
            static void Postfix(Character __instance)
            {
                try
                {
                    if (AlternionSettings.useMaskSkins)
                    {
                        string steamID = __instance.transform.parent.GetComponent<PlayerInfo>().steamID.ToString();
                        if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                        {  
                            if (theGreatCacher.maskSkins.TryGetValue(player.maskSkinName, out Texture newTex))
                            {
                                Renderer renderer = __instance.éäéïéðïåææè.transform.GetComponent<Renderer>();
                                renderer.material.mainTexture = newTex;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    debugLog("err: " + e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenu), "Start")]
        static class mainMenuStuffPatch
        {
            static void Postfix(MainMenu __instance)
            {
                setMainmenuBadge();
                setMainMenuWeaponSkin();
            }
        }

        [HarmonyPatch(typeof(MainMenu), "toggleKSBadge")]
        static class toggleKSPatch
        {
            static void Postfix(MainMenu __instance, bool on)
            {
                if (!AlternionSettings.useBadges)
                {
                    if (!on)
                    {
                        setMainmenuBadge();
                    }
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
                    if (!AlternionSettings.useWeaponSkins)
                    {
                        return;
                    }
                    if (!__instance.åïääìêêäéèç)
                    {
                        //Grab local steamID
                        string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                        if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
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
                    if (!AlternionSettings.useMainSails && !AlternionSettings.useSecondarySails)
                    {
                        return;
                    }
                    Transform shipTransf = __instance.transform.root;
                    if (shipTransf)
                    {
                        int teamNum = int.Parse(shipTransf.name.Split('m')[1]);

                        string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

                        if (!theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                        {
                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                            return;
                        }

                        string shipType = GameMode.Instance.shipTypes[teamNum - 1];
                        shipType = shipType.Remove(shipType.Length - 1);


                        switch (shipType)
                        {
                            case "cruiser":
                                if (__instance.name == "hmsSophie_sails08" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }


                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }


                                if (__instance.name != "hmsSophie_sails08")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "galleon":
                                if (__instance.name == "galleon_sails_01" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }


                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "galleon_sails_01")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "brig":
                                if (__instance.name == "hmsSpeedy_sails04" && AlternionSettings.useMainSails)
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "hmsSpeedy_sails04")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "xebec":
                                if (__instance.name == "xebec_sail03" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "xebec_sail03")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "bombvessel":
                                if (__instance.name == "bombVessel_sails07" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "bombVessel_sails07")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "gunboat":
                                if (__instance.name == "gunboat_sails02" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "gunboat_sails02")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "cutter":
                                if (__instance.name == "hmsAlert_sails02" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "hmsAlert_sails02")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "bombketch":
                                if (__instance.name == "bombKetch_sails06" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "bombKetch_sails06")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "carrack":
                                if (__instance.name == "carrack_sail03" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "carrack_sail03")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "junk":
                                if (__instance.name == "junk_sails_01" && AlternionSettings.useMainSails)
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "junk_sails_01")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }
                                }

                                break;
                            case "schooner":
                                if ((__instance.name == "schooner_sails02" && AlternionSettings.useMainSails) || (__instance.name == "schooner_sails00" && AlternionSettings.useMainSails))
                                {

                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.mainSailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                    }

                                    if (player.mainSailName != "null")
                                    {
                                        if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }
                                }
                                else if (player.sailSkinName != "null" && AlternionSettings.useSecondarySails)
                                {
                                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                    }
                                }
                                else
                                {
                                    __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                }

                                if (__instance.name != "schooner_sails02" && __instance.name != "schooner_sails00")
                                {
                                    if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                    {
                                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                    }
                                    else
                                    {
                                        cachedShip newVessel = new cachedShip();
                                        newVessel.sailDict.Add("1", __instance);
                                        theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
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
                    if (!AlternionSettings.useCannonSkins)
                    {
                        return;
                    }
                    Transform child = __instance.transform.FindChild("cannon");
                    int.TryParse( child.transform.root.name.Split('m')[1] , out int index);
                    string steamID = GameMode.Instance.teamCaptains[index - 1].steamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        // If vessel is already cached, grab it and add, otherwise create new vessel
                        if (theGreatCacher.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                        {
                            vessel.cannonOperationalDict.Add((vessel.cannonOperationalDict.Count + 1).ToString(), __instance);
                        }
                        else
                        {
                            cachedShip newVessel = new cachedShip();
                            newVessel.cannonOperationalDict.Add("1", __instance);
                            theGreatCacher.ships.Add(index.ToString(), newVessel);
                        }

                        // If they have a custom texture, use it, else use default skin
                        if (player.cannonSkinName != "null")
                        {
                            if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out Texture newTex))
                            {
                                child.GetComponent<Renderer>().material.mainTexture = newTex;
                            }
                        }
                        else
                        {
                            if (theGreatCacher.defaultCannons != null)
                            {
                                child.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultCannons;
                            }
                        }
                    }
                    else
                    {
                        child.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultCannons;
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
                        debugLog("Cannon operational error start");
                        debugLog(e.Message);
                        debugLog("Cannon operational error end");
                    }
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
                    if (!AlternionSettings.useCannonSkins)
                    {
                        return;
                    }

                    int.TryParse(__instance.æïìçñðåììêç.transform.root.name.Split('m')[1], out int index);
                    string steamID = GameMode.Instance.teamCaptains[index - 1].steamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        // If vessel is cached, add cannon to it, else create new vessel
                        if (theGreatCacher.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                        {
                            vessel.cannonDestroyDict.Add((vessel.cannonDestroyDict.Count + 1).ToString(), __instance);
                        }
                        else
                        {
                            cachedShip newVessel = new cachedShip();
                            newVessel.cannonDestroyDict.Add("1", __instance);
                            theGreatCacher.ships.Add(index.ToString(), newVessel);
                        }


                        // If they have a cannon skin then apply
                        if (player.cannonSkinName != "null")
                        {
                            if (theGreatCacher.cannonSkins.TryGetValue(player.cannonSkinName, out Texture newTex))
                            {
                                __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
                            }
                        }
                    }else
                    {
                        __instance.îæïíïíäìéêé.GetComponent<Renderer>().material.SetTexture("_MainTex", theGreatCacher.defaultCannons);
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
                    if (!AlternionSettings.useBadges)
                    {
                        return;
                    }
                    string steamID = ìçíêääéïíòç.m_SteamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        if (theGreatCacher.badges.TryGetValue(player.badgeName, out Texture newTex))
                        {
                            __instance.äæåéåîèòéîñ.texture = newTex;
                        }
                    }
                }
                catch (Exception e)
                {
                    debugLog(e.Message);
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
