using System;
using Harmony;
using Steamworks;
using UnityEngine;
using BWModLoader;

namespace Alternion
{
    [Mod]
    public class WeaponSkinHandler : MonoBehaviour
    {

        /// <summary>
        /// Assigns the weapon skin to the weapon.
        /// </summary>
        /// <param name="renderer">Renderer to apply to</param>
        /// <param name="weaponSkin">Weapon skin to use</param>
        /// <param name="weapon">Name of the Weapon</param>
        static void assignWeaponToRenderer(Renderer renderer, string weaponSkin, string weapon)
        {
            try
            {
                // If the player Dict contains a reference to the specific weapon, output the texture
                if (weaponSkin != "default")
                {
                    Texture newTex;
                    if (theGreatCacher.Instance.weaponSkins.TryGetValue(weapon + "_" + weaponSkin, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                    }
                    if (theGreatCacher.Instance.weaponSkins.TryGetValue(weapon + "_" + weaponSkin + "_met", out newTex))
                    {
                        renderer.material.SetTexture("_Metallic", newTex);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.debugLog(e.Message);
            }
        }

        /// <summary>
        /// Handles the finding of which skin, to apply to the weapon, based off the player setup and weapon equipped.
        /// </summary>
        /// <param name="__instance">WeaponRender Instance</param>
        /// <param name="player">Player loadout</param>
        static void weaponSkinHandler(WeaponRender __instance, playerObject player)
        {

            Renderer renderer = __instance.GetComponent<Renderer>();
            //foreach (Transform transf in __instance.transform.parent)
            //{
            //logLow(transf.name);
            //}
            //logLow("__instance.transform.parent.parent:");
            //foreach (Transform transf in __instance.transform.parent.parent)
            //{
            //    logLow(transf.name);
            //}

            // Needs a rework as the following share the same texture, and so return the same texture name:
            // Axe + Rapier
            // Dagger + Cutlass
            switch (renderer.material.mainTexture.name)
            {
                case "wpn_standardMusket_stock_alb":
                    assignWeaponToRenderer(renderer, player.musketSkinName, "musket");
                    break;
                case "wpn_standardCutlass_alb":
                    if (renderer.name == "Cutlass" || renderer.name == "wpn_cutlass_LOD1")
                    {
                        assignWeaponToRenderer(renderer, player.cutlassSkinName, "cutlass");
                    }
                    else if (renderer.name == "wpn_dagger")
                    {
                        assignWeaponToRenderer(renderer, player.daggerSkinName, "dagger");
                    }
                    break;
                case "wpn_blunderbuss_alb":
                    assignWeaponToRenderer(renderer, player.blunderbussSkinName, "blunderbuss");
                    break;
                case "wpn_nockGun_stock_alb":
                    assignWeaponToRenderer(renderer, player.nockgunSkinName, "nockgun");
                    break;
                case "wpn_handMortar_alb":
                    assignWeaponToRenderer(renderer, player.handMortarSkinName, "handmortar");
                    break;
                case "wpn_dagger_alb":
                    assignWeaponToRenderer(renderer, player.daggerSkinName, "dagger");
                    break;
                case "wpn_bottle_alb":
                    assignWeaponToRenderer(renderer, player.bottleSkinName, "bottle");
                    break;
                case "wpn_rumHealth_alb":
                    assignWeaponToRenderer(renderer, player.healItemSkinName, "bottleHealth");
                    break;
                case "prp_hammer_alb":
                    assignWeaponToRenderer(renderer, player.hammerSkinName, "hammer");
                    break;
                case "wpn_standardPistol_stock_alb":
                    assignWeaponToRenderer(renderer, player.standardPistolSkinName, "standardPistol");
                    break;
                case "prp_atlas01_alb":
                    assignWeaponToRenderer(renderer, player.atlas01SkinName, "atlas01");
                    break;
                case "prp_bucket_alb":
                    assignWeaponToRenderer(renderer, player.bucketSkinName, "bucket");
                    break;
                case "wpn_shortpistol_alb":
                    assignWeaponToRenderer(renderer, player.shortPistolSkinName, "shortPistol");
                    break;
                case "wpn_duckfoot_alb":
                    assignWeaponToRenderer(renderer, player.duckfootSkinName, "duckfoot");
                    break;
                case "wpn_annelyRevolver_alb":
                    assignWeaponToRenderer(renderer, player.annelyRevolverSkinName, "annelyRevolver");
                    break;
                case "wpn_tomohawk_alb":
                    assignWeaponToRenderer(renderer, player.tomahawkSkinName, "tomahawk");
                    break;
                case "wpn_matchlockRevolver_alb":
                    assignWeaponToRenderer(renderer, player.matchlockRevolverSkinName, "matchlock");
                    break;
                case "wpn_twoHandAxe_alb":
                    if (renderer.name == "wpn_twoHandAxe")
                    {
                        assignWeaponToRenderer(renderer, player.axeSkinName, "axe");
                    }
                    else if (renderer.name == "wpn_rapier")
                    {
                        assignWeaponToRenderer(renderer, player.rapierSkinName, "rapier");
                    }
                    break;
                case "wpn_boardingPike_alb":
                    assignWeaponToRenderer(renderer, player.pikeSkinName, "pike");
                    break;
                case "wpn_spyglass_alb":
                    assignWeaponToRenderer(renderer, player.spyglassSkinName, "spyglass");
                    break;
                case "prp_teaCup_alb":
                    assignWeaponToRenderer(renderer, player.teaCupSkinName, "teaCup");
                    break;
                case "tea_alb":
                    assignWeaponToRenderer(renderer, player.teaWaterSkinName, "teaWater");
                    break;
                case "wpn_grenade_alb":
                    assignWeaponToRenderer(renderer, player.grenadeSkinName, "grenade");
                    break;
                default:
                    // If not known, output here
                    Logger.logLow("Type name: -" + renderer.name + "-");
                    Logger.logLow("Default name: -" + renderer.material.mainTexture.name + "-");
                    break;
            }
        }

        //applyGold()
        [HarmonyPatch(typeof(WeaponRender), "ìæóòèðêççæî")]
        static class goldApplyPatch
        {
            static void Postfix(WeaponRender __instance)
            {
                if (AlternionSettings.useWeaponSkins)
                {
                    try
                    {
                        //PlayerInfo plyrInf = __instance.ìäóêäðçóììî.ìêïòëîåëìòñ.gameObject.transform.parent.parent.GetComponent<PlayerInfo>();
                        string steamID = __instance.ìäóêäðçóììî.ìêïòëîåëìòñ.gameObject.transform.parent.parent.GetComponent<PlayerInfo>().steamID.ToString();
                        if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                        {
                            weaponSkinHandler(__instance, player);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog("err: " + e.Message);
                    }
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
                        if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                        {
                            if (theGreatCacher.Instance.maskSkins.TryGetValue(player.maskSkinName, out Texture newTex))
                            {
                                // Renderer renderer = __instance.éäéïéðïåææè.transform.GetComponent<Renderer>();
                                __instance.éäéïéðïåææè.transform.GetComponent<Renderer>().material.mainTexture = newTex;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.debugLog("err: " + e.Message);
                }
            }
        }

        [HarmonyPatch(typeof(WeaponRender), "Start")]
        static class weaponSkinpatch1stPerson
        {
            static void Postfix(WeaponRender __instance)
            {
                if (AlternionSettings.useWeaponSkins)
                {
                    try
                    {
                        if (!__instance.åïääìêêäéèç)
                        {
                            //Grab local steamID
                            string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                            if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                            {
                                weaponSkinHandler(__instance, player);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog(e.Message);
                    }
                }
            }
        }
    }
}
