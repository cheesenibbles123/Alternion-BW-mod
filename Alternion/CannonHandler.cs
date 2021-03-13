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
        static void applySkins(cachedShip vessel, string steamID, Renderer renderer)
        {
            if (!theGreatCacher.Instance.setCannonDefaults)
            {
                theGreatCacher.Instance.setCannonDefaults = true;
                theGreatCacher.Instance.defaultCannons = renderer.material.mainTexture;
                theGreatCacher.Instance.defaultCannonsMet = renderer.material.GetTexture("_Metallic");
            }

            if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
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
                    resetCannon(vessel, renderer);
                }
            }
            else
            {
                resetCannon(vessel, renderer);
            }
        }

        // static CannonHandler Instance;

        /// <summary>
        /// Resets the cannon skin to default
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="renderer">Cannon renderer</param>
        static void resetCannon(cachedShip vessel, Renderer renderer)
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
                vessel.hasChangedCannons = false;
            }
        }

        /// <summary>
        /// Harmony patch to "OnEnable" for CannonUse
        /// </summary>
        [HarmonyPatch(typeof(CannonUse), "OnEnable")]
        static class cannonOperationalSkinPatch
        {
            static void Postfix(CannonUse __instance)
            {
                if (AlternionSettings.useCannonSkins)
                {
                    int index = 1000;
                    try
                    {
                        Transform child = __instance.transform.FindChild("cannon");
                        int.TryParse(__instance.transform.root.name.Split('m')[1], out index);
                        Logger.logLow($"Got Operational index: -{index}-");
                        string steamID = "0";
                        if (GameMode.Instance.teamCaptains[index - 1])
                        {
                            Logger.logLow("Team has captain");
                            steamID = GameMode.Instance.teamCaptains[index - 1].steamID.ToString();
                        }
                        else
                        {
                            Logger.logLow($"Team has not got a captain at index: -{index}- (position: {index - 1})");
                        }

                        if (theGreatCacher.Instance.ships.TryGetValue((index - 1).ToString(), out cachedShip vessel))
                        {
                            Logger.logLow($"Adding to ship at index: -{index}- (position: {index - 1})");
                            vessel.cannonOperationalDict.Add((vessel.cannonOperationalDict.Count + 1).ToString(), __instance);
                            Logger.logLow($"Added to ship at index: -{vessel.cannonOperationalDict.Count + 1}- (position: {index - 1})");
                            applySkins(vessel, steamID, child.GetComponent<Renderer>());
                        }
                        else
                        {
                            cachedShip newVessel = new cachedShip();
                            Logger.logLow("Generated new ship");
                            newVessel.cannonOperationalDict.Add("0", __instance);
                            Logger.logLow($"Added 1st cannon to ship at index: -{index}- (position: {index - 1})");
                            theGreatCacher.Instance.ships.Add(index.ToString(), newVessel);
                            Logger.logLow($"Added vessel to ship cache: -{index}-");
                            applySkins(vessel, steamID, child.GetComponent<Renderer>());
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
                            Logger.debugLog("Cannon operational error start");
                            Logger.debugLog(e.Message);
                            Logger.debugLog($"Issue at index: -{index}- (position {index - 1})");
                            Logger.debugLog($"Team: -{__instance.transform.root.name}-");
                            Logger.debugLog($"Num: -{__instance.transform.root.name.Split('m')[1]}-");
                            for (int i = 0; i < GameMode.Instance.teamCaptains.Length; i++)
                            {
                                if (GameMode.Instance.teamCaptains[i])
                                {
                                    Logger.debugLog($"Got captain at index -{i}- with steamID -{GameMode.Instance.teamCaptains[i].steamID}-");
                                }
                                else
                                {
                                    Logger.debugLog($"No captain at index -{i}-");
                                }
                            }
                            Logger.debugLog("Cannon operational error end");
                        }
                    }
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
                    try
                    {
                        int index = GameMode.getParentIndex(__instance.æïìçñðåììêç.transform.root);
                        string steamID = "0";
                        if (GameMode.Instance.teamCaptains[index])
                        {
                            steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                        }

                        if (theGreatCacher.Instance.ships.TryGetValue(index.ToString(), out cachedShip vessel))
                        {
                            vessel.cannonDestroyDict.Add((vessel.cannonDestroyDict.Count + 1).ToString(), __instance);
                            applySkins(vessel, steamID, __instance.îæïíïíäìéêé.GetComponent<Renderer>());
                        }
                        else
                        {
                            cachedShip newVessel = new cachedShip();
                            newVessel.cannonDestroyDict.Add("0", __instance);
                            theGreatCacher.Instance.ships.Add(index.ToString(), newVessel);
                            applySkins(vessel, steamID, __instance.îæïíïíäìéêé.GetComponent<Renderer>());
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog("Cannon destroy start");
                        Logger.debugLog(e.Message);
                        Logger.debugLog("Cannon destroy end");
                    }
                }
            }
        }

    }
}
