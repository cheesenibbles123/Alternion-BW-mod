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
                    setupShipFlags(team);
                }
            }
        }

        public static void setupShipFlags(int team)
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
            Transform shipTransform = GameMode.Instance.teamParents[team];
            Renderer[] renderers = shipTransform.GetComponentsInChildren<Renderer>(true);

            if (theGreatCacher.ships.TryGetValue(team.ToString(), out cachedShip vessel))
            {
                if (theGreatCacher.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    if (theGreatCacher.flags.TryGetValue(player.flagSkinName, out Texture flag))
                    {
                        foreach (Renderer renderer in renderers)
                        {
                            if (renderer.name == "teamflag")
                            {
                                if (renderer.material.mainTexture.name == "flag_navy")
                                {
                                    vessel.isNavy = true;
                                    if (!theGreatCacher.setNavyFlag)
                                    {
                                        theGreatCacher.navyFlag = renderer.material.mainTexture;
                                        theGreatCacher.setNavyFlag = true;
                                    }
                                }
                                else if (renderer.material.mainTexture.name == "flag_pirate")
                                {
                                    vessel.isNavy = false;
                                    if (!theGreatCacher.setPirateFlag)
                                    {
                                        theGreatCacher.pirateFlag = renderer.material.mainTexture;
                                        theGreatCacher.setPirateFlag = true;
                                    }
                                }
                                if (flag.name != "FAILED")
                                {
                                    renderer.material.mainTexture = flag;
                                    vessel.hasChangedFlag = true;
                                }
                            }
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
            }
            else // BROKEN
            {
                cachedShip newVessel = new cachedShip();
                theGreatCacher.ships.Add(team.ToString(), newVessel);
                if (theGreatCacher.players.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out playerObject player))
                {
                    if (theGreatCacher.flags.TryGetValue(player.flagSkinName, out Texture flag))
                    {
                        foreach (Renderer renderer in renderers)
                        {
                            if (renderer.name == "teamflag")
                            {
                                if (renderer.material.mainTexture.name == "flag_navy")
                                {
                                    vessel.isNavy = true;
                                    if (!theGreatCacher.setNavyFlag)
                                    {
                                        theGreatCacher.navyFlag = renderer.material.mainTexture;
                                        theGreatCacher.setNavyFlag = true;
                                    }
                                }
                                else if (renderer.material.mainTexture.name == "flag_pirate")
                                {
                                    vessel.isNavy = false;
                                    if (!theGreatCacher.setPirateFlag)
                                    {
                                        theGreatCacher.pirateFlag = renderer.material.mainTexture;
                                        theGreatCacher.setPirateFlag = true;
                                    }
                                }
                                if (flag.name != "FAILED")
                                {
                                    renderer.material.mainTexture = flag;
                                    newVessel.flag = renderer;
                                    vessel.hasChangedFlag = true;
                                }
                            }
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
            }
        }

        /// <summary>
        /// Applies skin to Flag
        /// </summary>
        /// <param name="vessel">Ship</param>
        static void resetFlag(cachedShip vessel)
        {
            if (vessel.hasChangedFlag)
            {
                if (vessel.isNavy)
                {
                    vessel.flag.material.mainTexture = theGreatCacher.navyFlag;
                }
                else
                {
                    vessel.flag.material.mainTexture = theGreatCacher.pirateFlag;
                }
                vessel.hasChangedFlag = false;
            }
        }
    }
}
