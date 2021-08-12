using System;
using System.Collections;
using BWModLoader;
using Harmony;
using UnityEngine;
using Alternion.Structs;

namespace Alternion.SkinHandlers
{
    /// <summary>
    /// Handles all flag interactions
    /// </summary>
    [Mod]
    public class flagHandler : MonoBehaviour
    {
        /// <summary>
        /// flagHandler Instance
        /// </summary>
        public static flagHandler Instance;

        float assignDelay = 4f;

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
        /// Harmony patch "allBuildShip" for ShipConstruction
        /// </summary>
        [HarmonyPatch(typeof(ShipConstruction), "allBuildShip")]
        class buildShipPatch
        {
            static void Postfix(ShipConstruction __instance, string shipType, int team, ïçîìäîóäìïæ.åéðñðçîîïêç info)
            {
                if (AlternionSettings.showFlags)
                {
                    Instance.StartCoroutine(Instance.setFlag(team));
                }
                else if (TheGreatCacher.Instance.ships.TryGetValue(team.ToString(), out cachedShip vessel))
                {
                    Instance.resetFlag(vessel);
                }
            }
        }

        /// <summary>
        /// Applies skin to Flag
        /// </summary>
        /// <param name="team">Ship team</param>
        private IEnumerator setFlag(int team)
        {
            yield return new WaitForSeconds(assignDelay);
            Transform shipTransform = GameMode.Instance.teamParents[team];
            Renderer[] renderers = shipTransform.GetComponentsInChildren<Renderer>(true); // Get all renderers

            if (TheGreatCacher.Instance.ships.TryGetValue(team.ToString(), out cachedShip vessel)) // Fetch existing ship
            {
                loopRenderers(renderers, vessel, false, team);
            }
            else
            {
                cachedShip newVessel = new cachedShip(); // Create new ship
                TheGreatCacher.Instance.ships.Add(team.ToString(), newVessel); // Add ship to cache

                loopRenderers(renderers, newVessel, true, team);

            }
        }

        /// <summary>
        /// Loops over the renderers
        /// </summary>
        /// <param name="renderers">Renderer Array</param>
        /// <param name="vessel">Cached Ship</param>
        /// <param name="isNew">Is a newly created ship or not</param>
        /// <param name="team">Team number</param>
        void loopRenderers(Renderer[] renderers, cachedShip vessel, bool isNew, int team)
        {
            foreach (Renderer renderer in renderers)
            {
                try
                {
                    changeRenderer(renderer, vessel, isNew, team);
                }
                catch (Exception e)
                {
                    Logger.logLow(e.Message);
                }
            }
        }

        /// <summary>
        /// Sets the flags
        /// </summary>
        /// <param name="renderer">Renderer</param>
        /// <param name="vessel">Cached Ship</param>
        /// <param name="isNew">Is a newly created ship or not</param>
        void changeRenderer(Renderer renderer, cachedShip vessel, bool isNew, int team)
        {
            if (renderer.name == "teamflag" || renderer.name.ToLower().StartsWith("squadflag"))
            {
                defaultsHandler(renderer);

                if (!vessel.isInitialized)
                {
                    vessel.isNavy = (renderer.material.mainTexture.name == "flag_navy" || renderer.material.mainTexture.name == "flag_british");
                    vessel.isInitialized = true;
                }

                if (TheGreatCacher.Instance.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    string flagSkin = vessel.isNavy ? player.flagNavySkinName : player.flagPirateSkinName;
                    if (TheGreatCacher.Instance.flags.TryGetValue(flagSkin, out Texture flag))
                    {
                        if (flag.name != "FAILED")
                        {
                            renderer.material.mainTexture = flag;
                            vessel.hasChangedFlag = true;
                        }
                        else
                        {
                            resetFlag(vessel);
                        }
                    }
                    else
                    {
                        resetFlag(vessel);
                    }
                }
                else
                {
                    resetFlag(vessel);
                }

                if (isNew)
                {
                    vessel.flags.Add(renderer);
                }


            }
        }

        /// <summary>
        /// Sets up the defaults
        /// </summary>
        /// <param name="renderer">Renderer</param>
        void defaultsHandler(Renderer renderer)
        {
            if (!TheGreatCacher.Instance.setNavyFlag && (renderer.material.mainTexture.name == "flag_navy" || renderer.material.mainTexture.name == "flag_british"))
            {
                TheGreatCacher.setDefaultFlags(renderer.material.mainTexture, true);
            }
            else if (!TheGreatCacher.Instance.setPirateFlag && renderer.material.mainTexture.name == "flag_pirate")
            {
                TheGreatCacher.setDefaultFlags(renderer.material.mainTexture, false);
            }
        }

        /// <summary>
        /// Applies skin to Flag
        /// </summary>
        /// <param name="vessel">Ship</param>
        public void resetFlag(cachedShip vessel)
        {
            if (vessel.hasChangedFlag)
            {
                if (vessel.isNavy)
                {
                    setFlagsToSkin(vessel, TheGreatCacher.Instance.navyFlag);
                }
                else
                {
                    setFlagsToSkin(vessel, TheGreatCacher.Instance.pirateFlag);
                }
                vessel.hasChangedFlag = false;
            }
        }

        /// <summary>
        /// Update all flags in ship to flag
        /// </summary>
        /// <param name="vessel">Cached Ship</param>
        /// <param name="newFlagTex">Flag to use</param>
        public void setFlagsToSkin(cachedShip vessel, Texture newFlagTex)
        {
            foreach (Renderer renderer in vessel.flags)
            {
                renderer.material.mainTexture = newFlagTex;
            }
        }
    }
}
