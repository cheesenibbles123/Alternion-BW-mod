using Harmony;
using System.Collections;
using UnityEngine;
using System.Text;

namespace Alternion.Fixes
{
    // CURRENTLY INACTIVE AND AWAITING UPDATE
#if EXTRAS

    public class birbHandler : MonoBehaviour
    {
        public static birbHandler Instance;

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
        /// Handles disabling/enabling bird on winners
        /// </summary>
        /// <param name="__instance">Current Player's PlayerInfo</param> 
        private IEnumerator handleBird(PlayerInfo __instance)
        {
            yield return new WaitForSeconds(1f);
            while (__instance.character == null)
            {
                yield return new WaitForSeconds(.5f);
            }
            logger.logLow($"{!__instance.character.æïðèñìæêêñç}");
            __instance.character.óððêäóäîçñè.SetActive(!__instance.character.æïðèñìæêêñç); // Only enable if not owner
            logger.logLow("IsBird");
            if (__instance.character.æïðèñìæêêñç)
            {
                __instance.character.óððêäóäîçñè.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly; // Sets to only use shadows, so it still shows in FP
                Logger.logLow("Removed from FP");
            }
            if (__instance.lobbyPlayer != null)
            {
                __instance.lobbyPlayer.ñéçåäçëñåæê = true;
            }
        }

        /// <summary>
        /// Harmony patch to disable bird in first person
        /// </summary>
        [HarmonyPatch(typeof(PlayerInfo), "setWinner")]
        class isWinnerPatch
        {
            static void Postfx(PlayerInfo __instance, ïçîìäîóäìïæ.åéðñðçîîïêç info)
            {
                if (info.åéñëîíèðòçé)
                {
                    Instance.StartCoroutine(Instance.handleBird(__instance));
                }
            }
        }
    }
#endif
}
