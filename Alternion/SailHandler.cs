using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BWModLoader;
using UnityEngine;

namespace Alternion
{
    [Mod]
    public class SailHandler : MonoBehaviour
    {
        static void applySkins(cachedShip vessel, string steamID, Renderer renderer, bool isMain)
        {
            if (theGreatCacher.players.TryGetValue(steamID, out playerObject player))
            {
                Texture newTex;
                if (isMain)
                {
                    if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                    }
                }
                else
                {
                    if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                    }
                }
            }
            else
            {
                resetSail(vessel, renderer);
            }
        }

        static void resetSail(cachedShip vessel, Renderer renderer)
        {
            if (vessel.hasChangedSails)
            {
                renderer.material.mainTexture = theGreatCacher.defaultSails;
                renderer.material.SetTexture("_Metallic", theGreatCacher.defaultSailsMet);
            }
        }

        static void setupShip(cachedShip vessel, string steamID, string shipType, string sailName, Renderer renderer, SailHealth __instance)
        {
            switch (shipType)
            {
                case "cruiser":
                    if (sailName == "hmsSophie_sails08" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "galleon":
                    if (__instance.name == "galleon_sails_01" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "brig":
                    if (__instance.name == "hmsSpeedy_sails04" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "xebec":
                    if (__instance.name == "xebec_sail03" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "bombvessel":
                    if (__instance.name == "bombVessel_sails07" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "gunboat":
                    if (__instance.name == "gunboat_sails02" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "cutter":
                    if (__instance.name == "hmsAlert_sails02" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "bombketch":
                    if (__instance.name == "bombKetch_sails06" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "carrack":
                    if (__instance.name == "carrack_sail03" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "junk":
                    if (__instance.name == "junk_sails_01" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                case "schooner":
                    if ((__instance.name == "schooner_sails02" && AlternionSettings.useMainSails) || (__instance.name == "schooner_sails00" && AlternionSettings.useMainSails))
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, false);
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                    break;
                default:
                    break;
            }
        }

        [HarmonyPatch(typeof(SailHealth), "OnEnable")]
        static class sailSkinPatch
        {
            static void Postfix(SailHealth __instance)
            {
                if (AlternionSettings.useMainSails || AlternionSettings.useSecondarySails)
                {
                    try
                    {
                        Transform shipTransf = __instance.transform.root;
                        if (shipTransf)
                        {
                            int teamNum = int.Parse(shipTransf.name.Split('m')[1]);

                            string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

                            string shipType = GameMode.Instance.shipTypes[teamNum - 1];
                            shipType = shipType.Remove(shipType.Length - 1);

                            if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                            {
                                vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                setupShip(vessel, steamID, shipType, __instance.name, __instance.GetComponent<Renderer>(), __instance);
                            }
                            else
                            {
                                cachedShip newVessel = new cachedShip();
                                newVessel.mainSailDict.Add("0", __instance);
                                theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                setupShip(vessel, steamID, shipType, __instance.name, __instance.GetComponent<Renderer>(), __instance);
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
