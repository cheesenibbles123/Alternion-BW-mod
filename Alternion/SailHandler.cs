using System;
using System.Collections;
using System.Collections.Generic;
using Harmony;
using BWModLoader;
using UnityEngine;


namespace Alternion
{
    [Mod]
    public class SailHandler : MonoBehaviour
    {

        /// <summary>
        /// SailHandler Instance
        /// </summary>
        public static SailHandler Instance;

        List<string> mainSailList = new List<string>()
        {
            "hmsSophie_sails08",
            "galleon_sails_01",
            "hmsSpeedy_sails04",
            "xebec_sail03",
            "bombVessel_sails07",
            "gunboat_sails02",
            "hmsAlert_sails02",
            "bombKetch_sails06",
            "carrack_sail03",
            "junk_sails_01",
            "schooner_sails00",
            "hoy_sails_00"
        };

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

        static void applySkins(cachedShip vessel, string steamID, Renderer renderer, bool isMain)
        {
            if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
            {
                Texture newTex;
                if (isMain)
                {
                    if (theGreatCacher.Instance.mainSails.TryGetValue(player.mainSailName, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                    }
                }
                else
                {
                    if (theGreatCacher.Instance.secondarySails.TryGetValue(player.sailSkinName, out newTex))
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
                renderer.material.mainTexture = theGreatCacher.Instance.defaultSails;
                renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSailsMet);
            }
        }

        static void setupShip(cachedShip vessel, string steamID, string shipType, string sailName, Renderer renderer, SailHealth __instance)
        {

            if (Instance.mainSailList.Contains(sailName) && AlternionSettings.useMainSails)
            {
                vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                applySkins(vessel, steamID, renderer, true);
                applySkins(vessel, steamID, __instance.êæïäîæïïíñå.GetComponent<Renderer>(), true);
            }
            else if (AlternionSettings.useSecondarySails)
            {
                vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                applySkins(vessel, steamID, renderer, false);
                applySkins(vessel, steamID, __instance.êæïäîæïïíñå.GetComponent<Renderer>(), false);
            }
            else
            {
                resetSail(vessel, renderer);
            }
            return;
            switch (shipType)
            {
                case "cruiser":
                    if (sailName == "hmsSophie_sails08" && AlternionSettings.useMainSails)
                    {
                        vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                        applySkins(vessel, steamID, renderer, true);
                        applySkins(vessel, steamID, __instance.êæïäîæïïíñå.GetComponent<Renderer>(), true);
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
                    if ((__instance.name == "schooner_sails00" && AlternionSettings.useMainSails))
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
                    break; // (__instance.name == "schooner_sails02" && AlternionSettings.useMainSails)
                case "hoy":
                    if (__instance.name == "hoy_sails_00" && AlternionSettings.useMainSails)
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
                    Logger.logLow("New ship type found");
                    Logger.logLow(shipType);
                    break;
            }
        }

        private IEnumerator wasteTime(int teamNum, SailHealth __instance)
        {
            yield return new WaitForSeconds(0.1f);
            bool captainAwol = true;
            while (captainAwol)
            {
                if (GameMode.Instance.teamCaptains[teamNum - 1])
                {
                    captainAwol = false;
                    handleShipSetup(teamNum, __instance);
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        void handleShipSetup(int teamNum, SailHealth __instance)
        {
            string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

            string shipType = GameMode.Instance.shipTypes[teamNum - 1];
            shipType = shipType.Remove(shipType.Length - 1);
            try
            {
                if (theGreatCacher.Instance.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                {
                    setupShip(vessel, steamID, shipType, __instance.name, __instance.GetComponent<Renderer>(), __instance);
                }
                else
                {
                    cachedShip newVessel = new cachedShip();
                    theGreatCacher.Instance.ships.Add(teamNum.ToString(), newVessel);
                    setupShip(newVessel, steamID, shipType, __instance.name, __instance.GetComponent<Renderer>(), __instance);
                }
            }
            catch (Exception e)
            {
                Logger.debugLog("### Issue setting up sails ###");
                Logger.debugLog(e.Message);
                Logger.debugLog("##############################");
            }
        }

        [HarmonyPatch(typeof(SailHealth), "OnEnable")]
        static class sailSkinPatch
        {
            static void Postfix(SailHealth __instance)
            {
                if (AlternionSettings.useMainSails || AlternionSettings.useSecondarySails)
                {
                    Logger.logLow("Spawned instance for " + __instance.name);
                    Transform shipTransf = __instance.transform.root;
                    if (shipTransf)
                    {
                        Logger.logLow("Found transform for " + __instance.name);
                        int teamNum = int.Parse(shipTransf.name.Split('m')[1]);
                        Logger.logLow("Found closed sail:" + __instance.êæïäîæïïíñå.name);
                        Instance.StartCoroutine(Instance.wasteTime(teamNum, __instance));
                    }
                }
            }
        }

        public void handleClosedSails(cachedShip vessel, Renderer closedSail, int team)
        {
            return;
            if (!vessel.closedSails.Contains(closedSail))
            {
                vessel.closedSails.Add(closedSail);
            }
            if (Instance.mainSailList.Contains(closedSail.name))
            {
                if (theGreatCacher.Instance.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    if (theGreatCacher.Instance.mainSails.TryGetValue(player.mainSailName, out Texture newMainSail))
                    {
                        closedSail.material.mainTexture = newMainSail;
                    }
                }
            }
            else
            {
                if (theGreatCacher.Instance.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    if (theGreatCacher.Instance.secondarySails.TryGetValue(player.sailSkinName, out Texture newSecondarySail))
                    {
                        closedSail.material.mainTexture = newSecondarySail;
                    }
                }
            }
        }

    }
}
