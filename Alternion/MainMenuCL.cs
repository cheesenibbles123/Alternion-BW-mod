﻿using System;
using Harmony;
using UnityEngine;
using BWModLoader;
using Steamworks;
using System.Collections;

namespace Alternion
{
    [Mod]
    public class MainMenuCL : MonoBehaviour
    {
        /// <summary>
        /// Main menu character transform.
        /// </summary>
        static Transform menuCharacter;

        /// <summary>
        /// MainMenuCL Instance.
        /// </summary>
        public static MainMenuCL Instance;

        /// <summary>
        /// Sets the main menu weapon skin.
        /// </summary>
        static void setMainMenuWeaponSkin()
        {
            if (AlternionSettings.useWeaponSkins)
            {
                try
                {
                    string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                    if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                    {
                        var musket = GameObject.Find("wpn_standardMusket_LOD1");
                        if (musket != null)
                        {
                            Texture newTex;
                            if (theGreatCacher.weaponSkins.TryGetValue("musket_" + player.musketSkinName, out newTex))
                            {
                                musket.GetComponent<Renderer>().material.mainTexture = newTex;
                            }
                            if (theGreatCacher.weaponSkins.TryGetValue("musket_" + player.musketSkinName + "_met", out newTex))
                            {
                                musket.GetComponent<Renderer>().material.SetTexture("_Metallic",newTex);
                            }
                        }
                        else
                        {
                            Logger.debugLog("Main menu musket not found.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.debugLog(e.Message);
                }
            }

            LoadingBar.updatePercentage(100, "Finished!");
        }

        /// <summary>
        /// Sets the main menu badge.
        /// </summary>
        public static void setMainMenuBadge()
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
                    Logger.debugLog($"Got player {player.steamID} => {player.badgeName}");
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
                Logger.debugLog("Failed to assign custom badge to a player:");
                Logger.debugLog(e.Message);
            }

            LoadingBar.updatePercentage(95, "Applying weapon skin");
            setMainMenuWeaponSkin();
            setMenuFlag();

        }

        /// <summary>
        /// Sets the main menu flag.
        /// </summary>
        static void setMenuFlag()
        {
            string steamID = Steamworks.SteamUser.GetSteamID().ToString();
            if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
            {
                GameObject flag = GameObject.Find("teamflag");
                if (theGreatCacher.flags.TryGetValue(player.flagSkinName, out Texture newTex))
                {
                    flag.GetComponent<Renderer>().material.mainTexture = newTex;
                }
            }
        }

        /// <summary>
        /// Sets the main menu character transform.
        /// </summary>
        static void setMenuCharacter()
        {
            // Find the musket object
            var musket = GameObject.Find("wpn_standardMusket_LOD1");
            if (musket != null)
            {
                // If it exists, then go to root and find the character model in the heirachy
                Transform rootTransf = musket.transform.root;
                foreach (Transform transform in rootTransf)
                {
                    if (transform.name == "default_character_rig")
                    {
                        // Save it for the rotating in Update()
                        menuCharacter = transform;
                        break;
                    }
                }
            }
        }

        void Start()
        {
            //Rotate Character
            setMenuCharacter();
        }
        void Update()
        {
            if (!óèïòòåææäêï.åìçæçìíäåóë.activeSelf && global::Input.GetMouseButton(1) && menuCharacter)
            {
                // If it has been found
                // Rotation code copied from CharacterCustomizationUI
                menuCharacter.Rotate(Vector3.up, 1000f * Time.deltaTime * -global::Input.GetAxisRaw("Mouse X"));

            }

            if (Input.GetKeyUp("`"))
            {
                Logger.debugLog("v7.0");
            }
        }

        [HarmonyPatch(typeof(MainMenu), "leaveCustomization")]
        static class flagPatch
        {
            static void postfix(MainMenu __instance)
            {
                setMenuFlag();
            }
        }

        [HarmonyPatch(typeof(MainMenu), "toggleKSBadge")]
        static class toggleKSPatch
        {
            static void Postfix(MainMenu __instance, bool on)
            {
                if (AlternionSettings.useBadges)
                {
                    if (!on)
                    {
                        setMainMenuBadge();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MainMenu), "Start")]
        static class mainMenuStuffPatch
        {
            static void Postfix(MainMenu __instance)
            {
                // Call these so that they set correctly again on returning to the main menu
                setMainMenuBadge();
                setMenuCharacter();
            }
        }
    }
}
