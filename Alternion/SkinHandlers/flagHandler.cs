using System;
using System.Collections;
using BWModLoader;
using Harmony;
using UnityEngine;
using Alternion.Structs;
using System.Reflection;

namespace Alternion.SkinHandlers
{
    /// <summary>
    /// Handles all flag interactions
    /// </summary>
    [Mod]
    public class flagHandler : MonoBehaviour
    {
        public static flagHandler Instance;
        private static Logger logger = new Logger("[FlagHandler]");
        const float assignDelay = 4f; // Game uses 3, so lower than this can be inconsistent
        int maxRuns = 20;

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

        private IEnumerator wasteTime(FlagSet __instance)
        {
            yield return new WaitForSeconds(.1f);
            int index = -1;
            int counter = 0;
            cachedShip vessel = null;
            while (counter < maxRuns)
            {
                index = GameMode.getParentIndex(__instance.transform.root);
                if (index != -1)
                {
                    if (vessel == null) vessel = TheGreatCacher.Instance.getCachedShip(index.ToString());
                    if (GameMode.Instance.teamCaptains[index])
                    {
                        Instance.StartCoroutine(Instance.setFlag(index, __instance.GetComponent<Renderer>(), null, 0));
                    }
                    break;
                }
                counter++;
                yield return new WaitForSeconds(.4f);
            }
        }
        public IEnumerator setFlag(int index, Renderer renderer, Texture newTex = null, float delay = assignDelay)
        {
            yield return new WaitForSeconds(delay);

            bool isPirates = GameMode.Instance.teamFactions[index] == "Pirates";
            string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
            cachedShip vessel = TheGreatCacher.Instance.getCachedShip(index.ToString());
            vessel.flags.Add(renderer);
            if (!vessel.isInitialized)
            {
                vessel.isNavy = !isPirates;
                vessel.isInitialized = true;
            }

            if (AlternionSettings.showFlags)
            {
                if (newTex)
                {
                    setFlagTextures(vessel, newTex);
                    vessel.hasChangedFlag = true;
                    yield break;
                }

                if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player) &&
                    TheGreatCacher.Instance.flags.TryGetValue(isPirates ? player.flagPirateSkinName : player.flagNavySkinName, out newTex))
                {
                    setFlagTextures(vessel, newTex);
                    vessel.hasChangedFlag = true;
                    yield break;
                }
            }
            resetFlag(vessel);
        }


        public static void resetFlag(cachedShip vessel)
        {
            if (vessel.hasChangedFlag)
            {
                setFlagTextures(vessel, vessel.isNavy ? TheGreatCacher.Instance.navyFlag : TheGreatCacher.Instance.pirateFlag, true);
            }
        }

        public static void setFlagTextures(cachedShip vessel, Texture newTexture, bool isChangingToDefault = false)
        {
            vessel.flags.ForEach((flag) => flag.material.mainTexture = newTexture);
            vessel.hasChangedFlag = !isChangingToDefault;
        }

        [HarmonyPatch(typeof(FlagSet), "OnEnable")]
        class FlatSetPatch
        {
            static bool Prefix(FlagSet __instance)
            {
                Instance.StartCoroutine(Instance.wasteTime(__instance));
                return false;
            }
        }
    }
}
