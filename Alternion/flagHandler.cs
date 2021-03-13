using System.Collections;
using BWModLoader;
using Harmony;
using UnityEngine;

namespace Alternion
{
    [Mod]
    public class flagHandler : MonoBehaviour
    {
        /// <summary>
        /// flagHandler Instance
        /// </summary>
        public static flagHandler Instance;

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
            yield return new WaitForSeconds(3f);
            bool hasNotUpdated = true;
            Transform shipTransform = GameMode.Instance.teamParents[team];
            Renderer[] renderers = shipTransform.GetComponentsInChildren<Renderer>(true);

            if (theGreatCacher.Instance.ships.TryGetValue(team.ToString(), out cachedShip vessel))
            {
                if (theGreatCacher.Instance.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    if (theGreatCacher.Instance.flags.TryGetValue(player.flagSkinName, out Texture flag))
                    {
                        loopRenderers(renderers, vessel, flag, false);
                        hasNotUpdated = false;
                    }
                }
            }
            else
            {
                cachedShip newVessel = new cachedShip();
                theGreatCacher.Instance.ships.Add(team.ToString(), newVessel);
                if (theGreatCacher.Instance.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    if (theGreatCacher.Instance.flags.TryGetValue(player.flagSkinName, out Texture flag))
                    {
                        loopRenderers(renderers, newVessel, flag, true);
                        hasNotUpdated = false;
                    }
                }
            }

            if (hasNotUpdated)
            {
                resetFlag(vessel);
            }
        }

        void loopRenderers(Renderer[] renderers, cachedShip vessel, Texture flag, bool isNew)
        {
            if (isNew)
            {
                foreach (Renderer renderer in renderers)
                {
                    changeRenderer(renderer, vessel, flag, isNew);
                }
            }
            else
            {
                foreach (Renderer renderer in vessel.flags)
                {
                    changeRenderer(renderer, vessel, flag, isNew);
                }
            }
        }

        void changeRenderer(Renderer renderer, cachedShip vessel, Texture flag, bool isNew)
        {
            if (renderer.name == "teamflag")
            {
                defaultsHandler(vessel, renderer);

                if (flag.name != "FAILED")
                {
                    renderer.material.mainTexture = flag;
                    if (isNew)
                    {
                        vessel.flags.Add(renderer);
                    }
                    vessel.hasChangedFlag = true;
                }
            }
        }

        void defaultsHandler(cachedShip vessel, Renderer renderer)
        {
            if (!theGreatCacher.Instance.setNavyFlag && renderer.material.mainTexture.name == "flag_navy")
            {
                vessel.isNavy = true;
                theGreatCacher.setDefaultFlags(renderer.material.mainTexture, true);
            }
            else if (!theGreatCacher.Instance.setPirateFlag && renderer.material.mainTexture.name == "flag_pirate")
            {
                vessel.isNavy = false;
                theGreatCacher.setDefaultFlags(renderer.material.mainTexture, false);
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
            }
        }

        public void setFlagsToSkin(cachedShip vessel, Texture newFlagTex)
        {
            foreach (Renderer renderer in vessel.flags)
            {
                renderer.material.mainTexture = newFlagTex;
            }
        }
    }
}
