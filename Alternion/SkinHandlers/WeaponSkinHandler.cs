using System;
using Harmony;
using Steamworks;
using UnityEngine;
using BWModLoader;

using Alternion.Structs;

namespace Alternion.SkinHandlers
{
    [Mod]
    public class weaponSkinHandler : MonoBehaviour
    {
        /// <summary>
        /// WeaponSkinHandler instance
        /// </summary>
        public static weaponSkinHandler Instance;

        /// <summary>
        /// Assigns the weapon skin to the weapon.
        /// </summary>
        /// <param name="renderer">Renderer to apply to</param>
        /// <param name="weaponSkin">Weapon skin to use</param>
        /// <param name="weapon">Name of the Weapon</param>
        /// <param name="meshFilter">The weapons current meshfilter</param>
        static void assignWeaponToRenderer(Renderer renderer, string weaponSkin, string weapon, MeshFilter meshFilter)
        {
            try
            {
                // If the player Dict contains a reference to the specific weapon, output the texture
                if (!TheGreatCacher.Instance.defaultWeaponModels.ContainsKey(weapon))
                {
                    TheGreatCacher.Instance.defaultWeaponModels.Add(weapon, meshFilter.mesh);
                }

                if (weaponSkin != "default")
                {
                    Texture newTex;
                    if (TheGreatCacher.Instance.weaponSkins.TryGetValue(weapon + "_" + weaponSkin, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                    }
                    if (TheGreatCacher.Instance.weaponSkins.TryGetValue(weapon + "_" + weaponSkin + "_met", out newTex))
                    {
                        renderer.material.SetTexture("_Metallic", newTex);
                    }

                    Mesh mesh;
                    if (TheGreatCacher.Instance.weaponModels.TryGetValue(weapon + "_" + weaponSkin, out mesh))
                    {
                        meshFilter.mesh = mesh;
                    }else if (TheGreatCacher.Instance.defaultWeaponModels.TryGetValue(weapon, out mesh))
                    {
                        meshFilter.mesh = mesh;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.debugLog("[WeaponSkinHandler] " + e.Message);
            }
        }

        /// <summary>
        /// Handles the finding of which skin, to apply to the weapon, based off the player setup and weapon equipped.
        /// </summary>
        /// <param name="__instance">WeaponRender Instance</param>
        /// <param name="player">Player loadout</param>
        static void mainSkinHandler(WeaponRender __instance, playerObject player)
        {

            Renderer renderer = __instance.GetComponent<Renderer>();
            MeshFilter meshFilter = __instance.GetComponent<MeshFilter>();

            switch (renderer.material.mainTexture.name)
            {
                case "wpn_standardMusket_stock_alb":
                    assignWeaponToRenderer(renderer, player.musketSkinName, "musket", meshFilter);
                    break;
                case "wpn_standardCutlass_alb":
                    if (renderer.name == "Cutlass" || renderer.name == "wpn_cutlass_LOD1")
                    {
                        assignWeaponToRenderer(renderer, player.cutlassSkinName, "cutlass", meshFilter);
                    }
                    else if (renderer.name == "wpn_dagger")
                    {
                        assignWeaponToRenderer(renderer, player.daggerSkinName, "dagger", meshFilter);
                    }
                    break;
                case "wpn_blunderbuss_alb":
                    assignWeaponToRenderer(renderer, player.blunderbussSkinName, "blunderbuss", meshFilter);
                    break;
                case "wpn_nockGun_stock_alb":
                    assignWeaponToRenderer(renderer, player.nockgunSkinName, "nockgun", meshFilter);
                    break;
                case "wpn_handMortar_alb":
                    assignWeaponToRenderer(renderer, player.handMortarSkinName, "handmortar", meshFilter);
                    break;
                case "wpn_dagger_alb":
                    assignWeaponToRenderer(renderer, player.daggerSkinName, "dagger", meshFilter);
                    break;
                case "wpn_bottle_alb":
                    assignWeaponToRenderer(renderer, player.bottleSkinName, "bottle", meshFilter);
                    break;
                case "wpn_rumHealth_alb":
                    assignWeaponToRenderer(renderer, player.healItemSkinName, "bottleHealth", meshFilter);
                    break;
                case "prp_hammer_alb":
                    assignWeaponToRenderer(renderer, player.hammerSkinName, "hammer", meshFilter);
                    break;
                case "wpn_standardPistol_stock_alb":
                    assignWeaponToRenderer(renderer, player.standardPistolSkinName, "standardPistol", meshFilter);
                    break;
                case "prp_atlas01_alb":
                    assignWeaponToRenderer(renderer, player.atlas01SkinName, "atlas01", meshFilter);
                    break;
                case "prp_bucket_alb":
                    assignWeaponToRenderer(renderer, player.bucketSkinName, "bucket", meshFilter);
                    break;
                case "wpn_shortpistol_alb":
                    assignWeaponToRenderer(renderer, player.shortPistolSkinName, "shortPistol", meshFilter);
                    break;
                case "wpn_duckfoot_alb":
                    assignWeaponToRenderer(renderer, player.duckfootSkinName, "duckfoot", meshFilter);
                    break;
                case "wpn_annelyRevolver_alb":
                    assignWeaponToRenderer(renderer, player.annelyRevolverSkinName, "annelyRevolver", meshFilter);
                    break;
                case "wpn_tomohawk_alb":
                    assignWeaponToRenderer(renderer, player.tomahawkSkinName, "tomahawk", meshFilter);
                    break;
                case "wpn_matchlockRevolver_alb":
                    assignWeaponToRenderer(renderer, player.matchlockRevolverSkinName, "matchlock", meshFilter);
                    break;
                case "wpn_twoHandAxe_alb":
                    if (renderer.name == "wpn_twoHandAxe")
                    {
                        assignWeaponToRenderer(renderer, player.axeSkinName, "axe", meshFilter);
                    }
                    else if (renderer.name == "wpn_rapier")
                    {
                        assignWeaponToRenderer(renderer, player.rapierSkinName, "rapier", meshFilter);
                    }
                    break;
                case "wpn_boardingPike_alb":
                    assignWeaponToRenderer(renderer, player.pikeSkinName, "pike", meshFilter);
                    break;
                case "wpn_spyglass_alb":
                    assignWeaponToRenderer(renderer, player.spyglassSkinName, "spyglass", meshFilter);
                    break;
                case "prp_teaCup_alb":
                    assignWeaponToRenderer(renderer, player.teaCupSkinName, "teaCup", meshFilter);
                    break;
                case "tea_alb":
                    assignWeaponToRenderer(renderer, player.teaWaterSkinName, "teaWater", meshFilter);
                    break;
                case "wpn_grenade_alb":
                    assignWeaponToRenderer(renderer, player.grenadeSkinName, "grenade", meshFilter);
                    break;
                default:
#if DEBUG
                    // If not known, output here
                    //Logger.logLow("Type name: -" + renderer.name + "-");
                    //Logger.logLow("Default name: -" + renderer.material.mainTexture.name + "-");
#endif
                    break;
            }
        }

        void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        /// <summary>
        /// Weapon skin patch (Third person)
        /// </summary>
        [HarmonyPatch(typeof(WeaponRender), "ìæóòèðêççæî")]
        class goldApplyPatch
        {
            static void Postfix(WeaponRender __instance)
            {
                if (AlternionSettings.useWeaponSkins)
                {
                    try
                    {
                        string steamID = __instance.ìäóêäðçóììî.ìêïòëîåëìòñ.gameObject.transform.parent.parent.GetComponent<PlayerInfo>().steamID.ToString();
                        if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                        {
                            mainSkinHandler(__instance, player);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog("[WeaponSkinHandler] " + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Mask skin patch
        /// </summary>
        [HarmonyPatch(typeof(Character), "setGoldMask")]
        class goldMaskPatch
        {
            static void Postfix(Character __instance)
            {
                try
                {
                    if (AlternionSettings.useMaskSkins)
                    {
                        string steamID = __instance.transform.parent.GetComponent<PlayerInfo>().steamID.ToString();
                        if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                        {
                            if (TheGreatCacher.Instance.maskSkins.TryGetValue(player.maskSkinName, out Texture newTex))
                            {
                                // Renderer renderer = __instance.éäéïéðïåææè.transform.GetComponent<Renderer>();
                                __instance.éäéïéðïåææè.transform.GetComponent<Renderer>().material.mainTexture = newTex;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.debugLog("[WeaponSkinHandler] " + e.Message);
                }
            }
        }

        /// <summary>
        /// Weapon skin patch (First person)
        /// </summary>
        [HarmonyPatch(typeof(WeaponRender), "Start")]
        class weaponSkinpatch1stPerson
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
                            if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                            {
                                mainSkinHandler(__instance, player);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog("[WeaponSkinHandler] " + e.Message);
                    }
                }
            }
        }
    }
}
