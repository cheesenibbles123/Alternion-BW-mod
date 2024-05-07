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
        public static flagHandler Instance;
        private static Logger logger = new Logger("[FlagHandler]");
        const float assignDelay = 4f; // Game uses 3, so lower than this can be inconsistent

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
            yield return new WaitForSeconds(assignDelay);
            Instance.StartCoroutine(Instance.setFlag(GameMode.getParentIndex(__instance.transform.root), __instance.GetComponent<Renderer>(), null, 0));
        }
        public IEnumerator setFlag(int index, Renderer renderer, Texture newTex = null, float delay = assignDelay)
        {
            yield return new WaitForSeconds(delay);
            bool isPirates = GameMode.Instance.teamFactions[index] == "Pirates";
            string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();

            cachedShip vessel = TheGreatCacher.Instance.getCachedShip(index.ToString());
            if (!vessel.isInitialized)
            {
                vessel.isNavy = !isPirates;
                vessel.flag = renderer;
                vessel.isInitialized = true;
            }

            if (AlternionSettings.showFlags)
            {
                if (newTex)
                {
                    setFlagTexture(vessel, newTex);
                    vessel.hasChangedFlag = true;
                    yield break;
                }

                if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player) &&
                    TheGreatCacher.Instance.flags.TryGetValue(isPirates ? player.flagPirateSkinName : player.flagNavySkinName, out newTex))
                {
                    setFlagTexture(vessel, newTex);
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
                setFlagTexture(vessel, vessel.isNavy ? TheGreatCacher.Instance.navyFlag : TheGreatCacher.Instance.pirateFlag, true);
            }
        }

        public static void setFlagTexture(cachedShip vessel, Texture newTexture, bool isChangingToDefault = false)
        {
            vessel.flag.material.mainTexture = newTexture;
            vessel.hasChangedFlag = !isChangingToDefault;
        }

        [HarmonyPatch(typeof(FlagSet), "OnEnable")]
        class FlatSetPatch
        {
            static void Postfix(FlagSet __instance)
            {
                Instance.StartCoroutine(Instance.wasteTime(__instance));
            }
        }
    }
}
