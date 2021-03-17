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

        /// <summary>
        /// Applies the skins to the sail
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="renderer">Sail Renderer</param>
        static void resetSail(cachedShip vessel, Renderer renderer)
        {
            if (vessel.hasChangedSails)
            {
                renderer.material.mainTexture = theGreatCacher.Instance.defaultSails;
                renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSailsMet);
            }
        }

        /// <summary>
        /// Applies the skins to the sail
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="steamID">Captain steamID</param>
        /// <param name="renderer">Sail Renderer</param>
        /// <param name="sailName">Name of the sail</param>
        static void setupShip(cachedShip vessel, string steamID, string sailName, Renderer renderer, SailHealth __instance)
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
                    setupShip(vessel, steamID, shipType, __instance.GetComponent<Renderer>(), __instance);
                }
                else
                {
                    cachedShip newVessel = new cachedShip();
                    theGreatCacher.Instance.ships.Add(teamNum.ToString(), newVessel);
                    setupShip(newVessel, steamID, shipType, __instance.GetComponent<Renderer>(), __instance);
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
