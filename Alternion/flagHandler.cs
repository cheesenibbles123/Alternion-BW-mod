using System;
using System.Collections;
using BWModLoader;
using Harmony;
using UnityEngine;

namespace Alternion
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
        static class buildShipPatch
        {
            static void Postfix(ShipConstruction __instance, string shipType, int team, ïçîìäîóäìïæ.åéðñðçîîïêç info)
            {
                if (AlternionSettings.showFlags)
                {
                    Instance.setupShipFlags(team);
                }
                else if (theGreatCacher.Instance.ships.TryGetValue(team.ToString(), out cachedShip vessel))
                {
                    Instance.resetFlag(vessel);
                }
            }
        }

        /// <summary>
        /// Starts Coroutine
        /// </summary>
        /// <param name="team">Ship team Number</param>
        void setupShipFlags(int team)
        {
            Instance.StartCoroutine(Instance.setFlag(team));
        }

        /// <summary>
        /// Applies skin to Flag
        /// </summary>
        /// <param name="team">Ship team</param>
        private IEnumerator setFlag(int team)
        {
            yield return new WaitForSeconds(assignDelay);
            bool hasNotUpdated = true;
            Transform shipTransform = GameMode.Instance.teamParents[team];
            Renderer[] renderers = shipTransform.GetComponentsInChildren<Renderer>(true);

            if (theGreatCacher.Instance.ships.TryGetValue(team.ToString(), out cachedShip vessel))
            {
                if (theGreatCacher.Instance.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    string flagSkin = vessel.isNavy ? player.flagNavyName : player.flagPirateName;
                    if (flagSkin != "default" && theGreatCacher.Instance.flags.TryGetValue(flagSkin, out Texture flag))
                    {
                        Logger.logLow("Setup existing ship");
                        loopRenderers(renderers, vessel, flag, false, team);
                        hasNotUpdated = false;
                    }
                    else
                    {
                        Instance.resetFlag(vessel);
                    }
                }
            }
            else
            {
                cachedShip newVessel = new cachedShip();
                theGreatCacher.Instance.ships.Add(team.ToString(), newVessel);
                if (theGreatCacher.Instance.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    string flagSkin = vessel.isNavy ? player.flagNavyName : player.flagPirateName;
                    if (flagSkin != "default" && theGreatCacher.Instance.flags.TryGetValue(flagSkin, out Texture flag))
                    {
                        Logger.logLow("Setup new ship");
                        loopRenderers(renderers, newVessel, flag, true, team);
                        hasNotUpdated = false;
                    }
                    else
                    {
                        Instance.resetFlag(newVessel);
                    }
                }
            }

            if (hasNotUpdated)
            {
                Logger.logLow("Resetting flag");
                resetFlag(vessel);
            }
        }

        /// <summary>
        /// Loops over the renderers
        /// </summary>
        /// <param name="renderers">Renderer Array</param>
        /// <param name="vessel">Cached Ship</param>
        /// <param name="flag">Flag to apply</param>
        /// <param name="isNew">Is a newly created ship or not</param>
        /// <param name="team">Team number</param>
        void loopRenderers(Renderer[] renderers, cachedShip vessel, Texture flag, bool isNew, int team)
        {
            foreach (Renderer renderer in renderers)
            {
                try
                {
                    changeRenderer(renderer, vessel, flag, isNew);
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
        /// <param name="flag">Flag to apply</param>
        /// <param name="isNew">Is a newly created ship or not</param>
        void changeRenderer(Renderer renderer, cachedShip vessel, Texture flag, bool isNew)
        {
            if (renderer.name == "teamflag" || renderer.name.ToLower().StartsWith("squadflag"))
            {
                Logger.logLow("found teamflag renderer");
                defaultsHandler(renderer);

                if (!vessel.isInitialized)
                {
                    vessel.isNavy = (renderer.material.mainTexture.name == "flag_navy");
                    vessel.isInitialized = true;
                }
                if (flag.name != "FAILED")
                {
                    renderer.material.mainTexture = flag;
                    Logger.logLow("Set new flag " + flag.name);
                    if (isNew)
                    {
                        vessel.flags.Add(renderer);
                        Logger.logLow("Added new flag");
                    }
                    vessel.hasChangedFlag = true;
                    Logger.logLow("Updated bool");
                }
            }
        }

        /// <summary>
        /// Sets up the defaults
        /// </summary>
        /// <param name="renderer">Renderer</param>
        void defaultsHandler(Renderer renderer)
        {
            if (!theGreatCacher.Instance.setNavyFlag && renderer.material.mainTexture.name == "flag_navy")
            {
                theGreatCacher.setDefaultFlags(renderer.material.mainTexture, true);
                Logger.logLow("Setup default navy flag");
            }
            else if (!theGreatCacher.Instance.setPirateFlag && renderer.material.mainTexture.name == "flag_pirate")
            {
                theGreatCacher.setDefaultFlags(renderer.material.mainTexture, false);
                Logger.logLow("Setup default pirate flag");
            }
            else
            {
                Logger.logLow("found flag " + renderer.material.mainTexture.name);
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
                    setFlagsToSkin(vessel, theGreatCacher.Instance.navyFlag);
                }
                else
                {
                    setFlagsToSkin(vessel, theGreatCacher.Instance.pirateFlag);
                }
                vessel.hasChangedFlag = false;
                Logger.logLow("reset hasChangedFlag bool");
            }
            else
            {
                Logger.logLow("Is default");
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
