using System;
using System.Collections;
using UnityEngine;
using BWModLoader;
using Harmony;

namespace Alternion
{
    [Mod]
    public class CannonHandler : MonoBehaviour
    {

        /// <summary>
        /// Applies skin to cannon
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="steamID">Captain SteamID</param>
        /// <param name="renderer">Cannon renderer</param>
        void applySkins(cachedShip vessel, string steamID, Renderer renderer)
        {
            if (!theGreatCacher.Instance.setCannonDefaults)
            {
                theGreatCacher.Instance.setCannonDefaults = true;
                theGreatCacher.Instance.defaultCannons = renderer.material.mainTexture;
                theGreatCacher.Instance.defaultCannonsMet = renderer.material.GetTexture("_Metallic");
            }

            if (AlternionSettings.useCannonSkins && theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
            {
                if (player.cannonSkinName != "default")
                {
                    Texture newTex;
                    // ALB
                    if (theGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                    {
                        Logger.logLow("Applying skin: " + newTex.name);
                        renderer.material.mainTexture = newTex;
                        if (!vessel.hasChangedCannons)
                        {
                            vessel.hasChangedCannons = true;
                        }
                    }
                    else if (theGreatCacher.Instance.defaultCannons != null)
                    {
                        Logger.logLow("Applying default ALB to custom");
                        renderer.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                    }

                    // MET
                    if (theGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                    {
                        Logger.logLow("Applying Met: " + newTex.name);
                        renderer.material.SetTexture("_Metallic", newTex);
                        if (!vessel.hasChangedCannons)
                        {
                            vessel.hasChangedCannons = true;
                        }
                    }
                    else if (theGreatCacher.Instance.defaultCannonsMet != null)
                    {
                        Logger.logLow("Applying default MET to custom");
                        renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
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

        /// <summary>
        /// Cannon Skin Handler
        /// </summary>
        public static CannonHandler Instance;

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
            if (vessel.hasChangedCannons)
            {
                if (theGreatCacher.Instance.defaultCannons != null)
                {
                    Logger.logLow("Applying default as null");
                    renderer.material.mainTexture = theGreatCacher.Instance.defaultCannons;
                }
                if (theGreatCacher.Instance.defaultCannonsMet != null)
                {
                    Logger.logLow("Applying default as null");
                    renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultCannonsMet);
                }
            }
        }

        /// <summary>
        /// Checks if the ship is cacned
        /// </summary>
        /// <param name="__instance">Cannon</param>
        /// <param name="index">Team Num</param>
        /// <param name="steamID">Captain SteamID</param>
        void checkCached(CannonDestroy __instance, int index, string steamID)
        {
            if (theGreatCacher.Instance.ships.TryGetValue(index.ToString(), out cachedShip vessel))
            {
                vessel.cannonDestroyDict.Add((vessel.cannonDestroyDict.Count + 1).ToString(), __instance);
                Instance.applySkins(vessel, steamID, __instance.îæïíïíäìéêé.GetComponent<Renderer>());
                Transform trans = __instance.æïìçñðåììêç.transform.GetChild(2);
                Instance.applySkins(vessel, steamID, trans.GetComponent<Renderer>());
            }
            else
            {
                cachedShip newVessel = new cachedShip();
                newVessel.cannonDestroyDict.Add("0", __instance);
                theGreatCacher.Instance.ships.Add(index.ToString(), newVessel);
                Instance.applySkins(vessel, steamID, __instance.îæïíïíäìéêé.GetComponent<Renderer>());
                Transform trans = __instance.æïìçñðåììêç.transform.GetChild(2);
                Instance.applySkins(vessel, steamID, trans.GetComponent<Renderer>());
            }
        }

        /// <summary>
        /// Checks if the ship is cacned
        /// </summary>
        /// <param name="__instance">Cannon</param>
        /// <param name="index">Team Num</param>
        /// <param name="steamID">Captain SteamID</param>
        void checkCached(Renderer rend, int index, string steamID)
        {
            if (theGreatCacher.Instance.ships.TryGetValue(index.ToString(), out cachedShip vessel))
            {
                vessel.cannonLOD = rend;
                Instance.applySkins(vessel, steamID, rend);
            }
            else
            {
                cachedShip newVessel = new cachedShip();
                newVessel.cannonLOD = rend;
                theGreatCacher.Instance.ships.Add(index.ToString(), newVessel);
                Instance.applySkins(vessel, steamID, rend);
            }
        }

        /// <summary>
        /// Wastes time until the captain is found
        /// </summary>
        /// <param name="__instance">Cannon</param>
        private IEnumerator wasteTime(CannonDestroy __instance)
        {
            yield return new WaitForSeconds(0.1f);
            bool doesntExist = true;
            while (doesntExist)
            {
                int index = GameMode.getParentIndex(__instance.æïìçñðåììêç.transform.root);

                if (__instance.æïìçñðåììêç && GameMode.Instance.teamCaptains[index])
                {
                    checkCached(__instance, index, GameMode.Instance.teamCaptains[index].steamID.ToString());
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
                    checkCached(__instance, index, GameMode.Instance.teamCaptains[index].steamID.ToString());
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
        static class cannonDestroySkinPatch
        {
            static void Postfix(CannonDestroy __instance)
            {
                if (AlternionSettings.useCannonSkins)
                {
                    Instance.StartCoroutine(Instance.wasteTime(__instance));
                }
            }
        }

        /// <summary>
        /// Hook into cannonLOD start
        /// </summary>
        [HarmonyPatch(typeof(OnlyEnableOnMyShip), "Start")]
        static class OnlyEnableOnMyShipPatch
        {
            static void Postfix(OnlyEnableOnMyShip __instance)
            {
                if (AlternionSettings.useCannonSkins)
                {
                    if (__instance.name != "Only Enemy Ship") return;

                    Renderer[] components = __instance.gameObject.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer rend in components)
                    {
                        if (rend.name == "Cannonsfull")
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
