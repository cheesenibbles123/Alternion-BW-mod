using System;
using System.Collections;
using BWModLoader;
using UnityEngine;
using Harmony;
using Alternion.Structs;

namespace Alternion.SkinHandlers
{
    [Mod]
    public class swivelHandler : MonoBehaviour
    {
        public static swivelHandler Instance;
        private static Logger logger = new Logger("[SwivelHandler]");
        private int maxRuns = 40;

        void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(Instance);
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        /// <summary>
        /// Sets up caching of vessel
        /// </summary>
        /// <param name="__instance">Cached ship</param>
        /// <param name="rend">Swivel Renderer</param>
        private IEnumerator setupShip(Renderer rend, int index)
        {
            yield return new WaitForSeconds(0.1f);
            bool doesntExist = true;
            int counter = 0;
            while (doesntExist && counter < maxRuns)
            {
                if (GameMode.Instance.teamCaptains[index]) {
                    string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                    cachedShip vessel = TheGreatCacher.Instance.getCachedShip(index.ToString());

                    vessel.Swivels.Add(rend);
                    applySkin(rend, steamID);

                    doesntExist = false;
                }
                else
                {
                    counter++;
                    yield return new WaitForSeconds(.4f);
                }
            }
        }

        /// <summary>
        /// Apply skin to swivel
        /// </summary>
        /// <param name="renderer">Swivel Renderer</param>
        /// <param name="steamID">Captain steamID</param>
        void applySkin(Renderer renderer, string steamID)
        {
            if (!renderer) return;

            if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player) && TheGreatCacher.Instance.skinAttributes.TryGetValue("swivel_" + player.swivelSkinName, out weaponSkinAttributes attrib)) {
                Texture newTex;
                if (attrib.hasAlb && TheGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName, out newTex))
                {
                    renderer.material.mainTexture = newTex;
                }
                else { renderer.material.mainTexture = TheGreatCacher.Instance.defaultSwivel; }
                if (attrib.hasAlb && TheGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName + "_met", out newTex))
                {
                    renderer.material.SetTexture("_Metallic", newTex);
                }
                else { renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultSwivelMet); }
                if (attrib.hasNrm && TheGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName + "_nrm", out newTex))
                {
                    renderer.material.SetTexture("_BumpMap", newTex);
                }
                else { renderer.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultSwivelNrm); }
            }
            else
            {
                renderer.material.mainTexture = TheGreatCacher.Instance.defaultSwivel;
                renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultSwivelMet);
                renderer.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultSwivelNrm);
            }
        }

        /// <summary>
        /// Update swivel textures
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="player">Captain</param>
        public void updateSwivels(cachedShip vessel, playerObject player)
        {
            if (vessel == null) return;
            if (player != null && TheGreatCacher.Instance.skinAttributes.TryGetValue(player.swivelSkinName, out weaponSkinAttributes attrib))
            {
                Texture newTex;
                Texture mainTex;
                if (attrib.hasAlb && TheGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName, out newTex))
                {
                    mainTex = newTex;
                }
                else {  mainTex = TheGreatCacher.Instance.defaultSwivel; }
                Texture metTex;
                if (attrib.hasMet && TheGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName + "_met", out newTex))
                {
                    metTex = newTex;
                }
                else { metTex = TheGreatCacher.Instance.defaultSwivelMet; }
                Texture nrmTex;
                if (attrib.hasNrm && TheGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName + "_met", out newTex))
                {
                    nrmTex = newTex;
                }
                else { nrmTex = TheGreatCacher.Instance.defaultSwivelMet; }

                foreach (Renderer renderer in vessel.Swivels)
                {
                    renderer.material.mainTexture = mainTex;
                    renderer.material.SetTexture("_Metallic", metTex);
                    renderer.material.SetTexture("_BumpMap", nrmTex);
                }
            }
            else
            {
                resetSwivels(vessel);
            }
        }

        /// <summary>
        /// Resets the swivels to default textures
        /// </summary>
        /// /// <param name="vessel">Cached Ship</param>
        public void resetSwivels(cachedShip vessel)
        {
            if (vessel != null && vessel.hasChangedSwivels)
            {
                foreach (Renderer ren in vessel.Swivels)
                {
                    ren.material.mainTexture = TheGreatCacher.Instance.defaultSwivel;
                    ren.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultSwivelMet);
                    ren.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultSwivelNrm);
                }
                vessel.hasChangedSwivels = false;
            }
        }

        /// <summary>
        /// Patches into SwivelUse.Start()
        /// </summary>
        [HarmonyPatch(typeof(SwivelUse), "Start")]
        class swivelPatch
        {
            static bool Prefix(SwivelUse __instance)
            {
                if (AlternionSettings.useSwivelSkins)
                {
                    int index = GameMode.getParentIndex(__instance.transform.root); // If we do this any later we get a -1 for the team index, might be something to do with how the transform is changed in Start()?
                    Renderer[] renderers2 = __instance.transform.parent.parent.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers2.Length; i++)
                    {
                        if (renderers2[i].name == "swiveltop" || renderers2[i].name == "swivel_connector" || renderers2[i].name == "swivel_base")
                        {
                            try
                            {
                                Instance.StartCoroutine(Instance.setupShip(renderers2[i], index));
                            }
                            catch (Exception e)
                            {
                                logger.debugLog(e.Message);
                            }
                        }
                    }
                }
                return true;
            }
        }
    }
}
