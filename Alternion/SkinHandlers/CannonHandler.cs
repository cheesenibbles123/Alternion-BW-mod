using System.Collections;
using UnityEngine;
using BWModLoader;
using Harmony;
using Alternion.Structs;
using System;

namespace Alternion.SkinHandlers
{
    [Mod]
    public class cannonHandler : MonoBehaviour
    {

        /// <summary>
        /// Cannon Skin Handler
        /// </summary>
        public static cannonHandler Instance;

        /// <summary>
        /// Applies skin to cannon
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="steamID">Captain SteamID</param>
        /// <param name="renderer">Cannon renderer</param>
        void applySkins(cachedShip vessel, string steamID, Renderer renderer)
        {
            if (!TheGreatCacher.Instance.setCannonDefaults)
            {
                TheGreatCacher.Instance.defaultCannons = renderer.material.mainTexture;
                TheGreatCacher.Instance.defaultCannonsMet = renderer.material.GetTexture("_Metallic");
                TheGreatCacher.Instance.setCannonDefaults = true;
            }

            if (AlternionSettings.useCannonSkins && TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
            {
                if (player.cannonSkinName != "default")
                {
                    Texture newTex;
                    // ALB
                    if (TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                        if (!vessel.hasChangedCannons)
                        {
                            vessel.hasChangedCannons = true;
                        }
                    }
                    else if (TheGreatCacher.Instance.setCannonDefaults)
                    {
                        renderer.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                    }

                    // MET
                    if (TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                    {
                        renderer.material.SetTexture("_Metallic", newTex);
                        if (!vessel.hasChangedCannons)
                        {
                            vessel.hasChangedCannons = true;
                        }
                    }
                    else if (TheGreatCacher.Instance.setCannonDefaults)
                    {
                        renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                    }
                }
                else
                {
                    Instance.resetCannon(vessel, renderer);
                }
            }
            else
            {
                Instance.resetCannon(vessel, renderer);
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
        /// Resets the cannon skin to default
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="renderer">Cannon renderer</param>
        void resetCannon(cachedShip vessel, Renderer renderer)
        {
            if (vessel.hasChangedCannons && renderer != null)
            {
                if (TheGreatCacher.Instance.setCannonDefaults)
                {
                    renderer.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                    renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                }
            }
        }

        /// <summary>
        /// Fetches and sets up the cannon LOD
        /// </summary>
        /// <param name="rend">Cannon LOD renderer</param>
        /// <param name="index">Team Num</param>
        /// <param name="steamID">Captain SteamID</param>
        void setupLod(Renderer rend, int index, string steamID)
        {
            cachedShip vessel = TheGreatCacher.getCachedShip(index.ToString());
            vessel.cannonLOD = rend;
            Instance.applySkins(vessel, steamID, rend);
        }

        /// <summary>
        /// Wastes time until the captain is found for the external LOD
        /// </summary>
        /// <param name="__instance">Cannon Renderer</param>
        private IEnumerator wasteTime(Renderer __instance)
        {
            yield return new WaitForSeconds(0.1f);
            bool doesntExist = true;
            while (doesntExist)
            {
                int index = GameMode.getParentIndex(__instance.gameObject.transform.root);

                if (GameMode.Instance.teamCaptains[index])
                {
                    setupLod(__instance, index, GameMode.Instance.teamCaptains[index].steamID.ToString());
                    doesntExist = false;
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        /// <summary>
        /// Wastes time until the captain is found
        /// </summary>
        /// <param name="cannon">Cannon Renderer</param>
        /// <param name="index">Team num</param>
        private IEnumerator wasteTime(CannonDestroy __instance, int index)
        {
            yield return new WaitForSeconds(0.1f);
            bool doesntExist = true;
            while (doesntExist)
            {
                if (GameMode.Instance.teamCaptains[index])
                {
                    cachedShip vessel = TheGreatCacher.getCachedShip(index.ToString());
                    try {
                        Renderer destroyedCannon = __instance.îæïíïíäìéêé.GetComponent<Renderer>();
                        Renderer functionalCannon = __instance.æïìçñðåììêç.GetComponent<CannonUse>().transform.FindChild("cannon").GetComponent<Renderer>();
                        vessel.cannons.Add(destroyedCannon);
                        vessel.cannons.Add(functionalCannon);
                        string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                        Instance.applySkins(vessel, steamID, destroyedCannon);
                        Instance.applySkins(vessel, steamID, functionalCannon);
                    } catch (Exception e) {
                        Logger.debugLog("Error applying cannon skin(s).");
                        Logger.debugLog(e.Message);
                    }
                    doesntExist = false;
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        /// <summary>
        /// Harmony patch to "Start" for CannonDestroy
        /// </summary>
        [HarmonyPatch(typeof(CannonDestroy), "Start")]
        class cannonDestroySkinPatch
        {
            static void Postfix(CannonDestroy __instance)
            {
                if (AlternionSettings.useCannonSkins)
                {
                    Instance.StartCoroutine(Instance.wasteTime(__instance, GameMode.getParentIndex(__instance.æïìçñðåììêç.transform.root)));
                }
            }
        }

        /// <summary>
        /// Hook into cannonLOD start
        /// </summary>
        [HarmonyPatch(typeof(OnlyEnableOnMyShip), "Start")]
        class OnlyEnableOnMyShipPatch
        {
            static void Postfix(OnlyEnableOnMyShip __instance)
            {
                if (AlternionSettings.useCannonSkins)
                {
                    if (__instance.name != "Only Enemy Ship") return;

                    // If only there was an easier way that i was smart enough to figure out
                    Renderer[] components = __instance.gameObject.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer rend in components)
                    {
                        if (rend.name == "Cannonsfull") // Single mesh + material
                        {
                            Instance.StartCoroutine(Instance.wasteTime(rend));
                            break;
                        }
                    }
                }
            }
        }

    }
}
