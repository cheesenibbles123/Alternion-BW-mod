using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWModLoader;
using Harmony;
using UnityEngine;

namespace Alternion
{
    [Mod]
    public class flagHandler : MonoBehaviour
    {

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

        [HarmonyPatch(typeof(ShipConstruction), "allBuildShip")]
        static class buildShipPatch
        {
            static void Postfix(ShipConstruction __instance, string shipType, int team, ïçîìäîóäìïæ.åéðñðçîîïêç info)
            {
                if (AlternionSettings.showFlags)
                {
                    Instance.StartCoroutine(Instance.setFlag(team));
                }
            }
        }

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
            else
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
