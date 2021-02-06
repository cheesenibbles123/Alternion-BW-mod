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
                Instance.StartCoroutine(Instance.setFlag(team));
            }
        }

        private IEnumerator setFlag(int team)
        {
            yield return new WaitForSeconds(2f);
            Transform shipTransform = GameMode.Instance.teamParents[team];
            Renderer[] renderers = shipTransform.GetComponentsInChildren<Renderer>(true);

            if (theGreatCacher.flags.TryGetValue(GameMode.Instance.teamCaptains[team].steamID.ToString(), out Texture flag))
            {
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.name == "teamflag")
                    {
                        Logger.logLow($"Faction flag: -{renderer.material.mainTexture.name}-");
                        if (flag.name != "FAILED")
                        {
                            renderer.material.mainTexture = flag;
                        }
                    }
                }
            }
            
        }
    }
}
