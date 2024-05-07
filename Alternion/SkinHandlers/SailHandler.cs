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
    public class sailHandler : MonoBehaviour
    {
        public static sailHandler Instance;

        /// <summary>
        /// List containing all the mainsail names
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
            if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
            {
                Texture newTex;
                string skinName = isMain ? player.mainSailName : player.sailSkinName;
                if (skinName != "default")
                {
                    if (isMain ? TheGreatCacher.Instance.mainSails.TryGetValue(skinName, out newTex) : TheGreatCacher.Instance.secondarySails.TryGetValue(skinName, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                        vessel.hasChangedSails = true;
                    }
                    else
                    {
                        resetSail(renderer);
                    }
                }
            }
            else
            {
                resetSail(renderer);
            }
        }

        /// <summary>
        /// Applies the skins to the sail
        /// </summary>
        /// <param name="renderer">Sail Renderer</param>
        static void resetSail(Renderer renderer)
        {
            renderer.material.mainTexture = TheGreatCacher.Instance.defaultSails;
            renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultSailsMet);
        }

        /// <summary>
        /// Determines if it should be a mainsail or not
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="steamID">Captain steamID</param>
        /// <param name="renderer">Sail Renderer</param>
        /// <param name="__instance">SailHealth</param>
        static void setupShip(cachedShip vessel, string steamID, Renderer renderer, Renderer closedRenderer, SailHealth __instance)
        {

            if (Instance.mainSailList.Contains(__instance.name) && AlternionSettings.useMainSails)
            {
                vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                applySkins(vessel, steamID, renderer, true);
                applySkins(vessel, steamID, closedRenderer, true);
            }
            else if (AlternionSettings.useSecondarySails)
            {
                vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                applySkins(vessel, steamID, renderer, false);
                applySkins(vessel, steamID, closedRenderer, false);
            }
            else
            {
                resetSail(renderer);
            }
        }

        /// <summary>
        /// Waits until the captain has been set
        /// </summary>
        /// <param name="teamNum">Team Number</param>
        /// <param name="__instance">SailHealth component</param>
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
            if (player != null &&
                (AlternionSettings.useSecondarySails || AlternionSettings.useMainSails))
            {
                Texture newTex;
                foreach (Renderer renderer in vessel.closedSails)
                {
                    if (mainSailList.Contains("text") && AlternionSettings.useMainSails)
                    {
                        // Handle Main Sail
                        if (TheGreatCacher.Instance.mainSails.TryGetValue(player.mainSailName, out newTex))
                        {
                            renderer.material.mainTexture = newTex;
                        }
                        else
                        {
                            renderer.material.mainTexture = TheGreatCacher.Instance.defaultSails;
                        }
                    }
                    else if (AlternionSettings.useSecondarySails)
                    {
                        // Handle secondary
                        if (TheGreatCacher.Instance.secondarySails.TryGetValue(player.mainSailName, out newTex))
                        {
                            renderer.material.mainTexture = newTex;
                        }
                        else
                        {
                            renderer.material.mainTexture = TheGreatCacher.Instance.defaultSails;
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
                renderer.material.mainTexture = TheGreatCacher.Instance.defaultSails;
                renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultSailsMet);
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
            cachedShip vessel = TheGreatCacher.Instance.getCachedShip(teamNum.ToString());
            setupShip(vessel, steamID, __instance.GetComponent<Renderer>(), __instance.êæïäîæïïíñå.GetComponent<Renderer>(), __instance);
        }

        /// <summary>
        /// Hooks into OnEnable
        /// </summary>
        [HarmonyPatch(typeof(SailHealth), "OnEnable")]
        class sailSkinPatch
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
