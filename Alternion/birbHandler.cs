using Harmony;
using System.Collections;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace Alternion
{
#if DEBUG
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
            __instance.isTournyWinner = true;
            Logger.logLow($"{!__instance.character.æïðèñìæêêñç}");
            __instance.character.óððêäóäîçñè.SetActive(!__instance.character.æïðèñìæêêñç); // Only enable if not owner
            Logger.logLow("IsBird");
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
        static class isWinnerPatch
        {
            static bool Prefix(PlayerInfo __instance, ïçîìäîóäìïæ.åéðñðçîîïêç info)
            {
                if (info.åéñëîíèðòçé)
                {
                    Instance.StartCoroutine(Instance.handleBird(__instance));
                }

                return false;
            }
        }
    }
#endif
}
