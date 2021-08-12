using System.Collections;
using System.Collections.Generic;
using Harmony;
using BWModLoader;
using UnityEngine;
using Alternion.Structs;

namespace Alternion.SkinHandlers
{
    /// <summary>
    /// Handles all sail interactions
    /// </summary>
    [Mod]
    public class SailHandler : MonoBehaviour
    {

        /// <summary>
        /// SailHandler Instance
        /// </summary>
        public static SailHandler Instance;

        /// <summary>
        /// List containing all the mainsails
        /// </summary>
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

        /// <summary>
        /// Applies the skins to the sail
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="steamID">Captain steamID</param>
        /// <param name="renderer">Sail Renderer</param>
        /// <param name="isMain">Is mainsail or not</param>
        static void applySkins(cachedShip vessel, string steamID, Renderer renderer, bool isMain)
        {
            if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
            {
                Texture newTex;
                if (isMain)
                {
                    if (player.mainSailName != "default" && theGreatCacher.Instance.mainSails.TryGetValue(player.mainSailName, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                        vessel.hasChangedSails = true;
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                }
                else
                {
                    if (player.mainSailName != "default" && theGreatCacher.Instance.secondarySails.TryGetValue(player.sailSkinName, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                        vessel.hasChangedSails = true;
                    }
                    else
                    {
                        resetSail(vessel, renderer);
                    }
                }
            }
            else
            {
                resetSail(vessel, renderer);
            }
        }

        /// <summary>
        /// Applies the skins to the sail
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="renderer">Sail Renderer</param>
        static void resetSail(cachedShip vessel, Renderer renderer)
        {
            //if (vessel.hasChangedSails)
            //{
                renderer.material.mainTexture = theGreatCacher.Instance.defaultSails;
                renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSailsMet);
            //}
        }

        /// <summary>
        /// Determines if it should be a mainsail or not
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="steamID">Captain steamID</param>
        /// <param name="renderer">Sail Renderer</param>
        /// <param name="__instance">SailHealth</param>
        static void setupShip(cachedShip vessel, string steamID, Renderer renderer, SailHealth __instance)
        {

            if (Instance.mainSailList.Contains(__instance.name) && AlternionSettings.useMainSails)
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
        }

        /// <summary>
        /// Waits until the captain has been set
        /// </summary>
        /// <param name="teamNum">Team Number</param>
        /// <param name="__instance">SailHealth</param>
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

        public void handleClosedSails(cachedShip vessel, playerObject player)
        {
            if (player != null)
            {
                Texture newTex;
                foreach (Renderer renderer in vessel.closedSails)
                {
                    Logger.debugLog("#############");
                    Logger.debugLog(renderer.name);
                    Logger.debugLog(renderer.transform.parent.name);
                    Logger.debugLog("#############");
                    if (mainSailList.Contains("text") && AlternionSettings.useMainSails)
                    {
                        // Handle Main Sail
                        if (theGreatCacher.Instance.mainSails.TryGetValue(player.mainSailName, out newTex))
                        {
                            renderer.material.mainTexture = newTex;
                        }
                        else
                        {
                            renderer.material.mainTexture = theGreatCacher.Instance.defaultSails;
                        }
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        // Handle secondary
                        if (theGreatCacher.Instance.secondarySails.TryGetValue(player.mainSailName, out newTex))
                        {
                            renderer.material.mainTexture = newTex;
                        }
                        else
                        {
                            renderer.material.mainTexture = theGreatCacher.Instance.defaultSails;
                        }
                    }
                    else
                    {
                        resetClosedSails(vessel);
                    }
                }
            }
            else
            {
                resetClosedSails(vessel);
            }
        }

        public void resetClosedSails(cachedShip vessel)
        {
            foreach (Renderer renderer in vessel.closedSails)
            {
                renderer.material.mainTexture = theGreatCacher.Instance.defaultSails;
                renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSailsMet);
            }
        }

        /// <summary>
        /// Creates/pulls cached ship
        /// </summary>
        /// <param name="teamNum">Team Number</param>
        /// <param name="__instance">SailHealth</param>
        void handleShipSetup(int teamNum, SailHealth __instance)
        {
            string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

            if (!theGreatCacher.Instance.setSailDefaults)
            {
                theGreatCacher.Instance.defaultSails = __instance.GetComponent<Renderer>().material.mainTexture;
                theGreatCacher.Instance.setSailDefaults = true;
            }
            if (theGreatCacher.Instance.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
            {
                setupShip(vessel, steamID, __instance.GetComponent<Renderer>(), __instance);
            }
            else
            {
                cachedShip newVessel = new cachedShip();
                theGreatCacher.Instance.ships.Add(teamNum.ToString(), newVessel);
                setupShip(newVessel, steamID, __instance.GetComponent<Renderer>(), __instance);
            }
        }

        /// <summary>
        /// Hooks into OnEnable
        /// </summary>
        [HarmonyPatch(typeof(SailHealth), "OnEnable")]
        static class sailSkinPatch
        {
            static void Postfix(SailHealth __instance)
            {
                if (AlternionSettings.useMainSails || AlternionSettings.useSecondarySails)
                {
                    Transform shipTransf = __instance.transform.root;
                    if (shipTransf)
                    {
                        int teamNum = int.Parse(shipTransf.name.Split('m')[1]);
                        Instance.StartCoroutine(Instance.wasteTime(teamNum, __instance));
                    }
                }
            }
        }
    }
}
