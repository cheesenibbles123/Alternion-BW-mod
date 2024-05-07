using System;
using System.Collections;
using UnityEngine;
using Harmony;
using BWModLoader;
using Alternion.Structs;

namespace Alternion.SkinHandlers
{
    [Mod]
    public class mortarHandler : MonoBehaviour
    {
        /// <summary>
        /// Mortar Handler instance
        /// </summary>
        public static mortarHandler Instance;
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
        /// Applies skin to mortar
        /// </summary>
        /// <param name="renderer">Mortar renderer</param>
        /// <param name="skinName">Player skin name</param>
        void applySkin(Renderer renderer, string skinName)
        {
            TheGreatCacher.Instance.skinAttributes.TryGetValue(skinName, out weaponSkinAttributes attrib);

            Texture img;
            if (attrib.hasAlb && TheGreatCacher.Instance.mortarSkins.TryGetValue(skinName, out img))
            {
                renderer.material.mainTexture = img;
            }else { renderer.material.mainTexture = TheGreatCacher.Instance.defaultMortar; }
            if (attrib.hasMet && TheGreatCacher.Instance.mortarSkins.TryGetValue(skinName + "_met", out img))
            {
                renderer.material.SetTexture("_Metallic", img);
            }
            else { renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMortarMet); }
            if (attrib.hasNrm && TheGreatCacher.Instance.mortarSkins.TryGetValue(skinName + "_nrm", out img))
            {
                renderer.material.SetTexture("_BumpMap", img);
            }
            else { renderer.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm); }

            if (attrib.hasMesh && TheGreatCacher.Instance.mortarModels.TryGetValue(skinName, out Mesh model))
            {
                // TODO : Figure out how to get the correct mesh here
            }else { }
        }

        /// <summary>
        /// Waits for captain to be set before applying skin
        /// </summary>
        /// <param name="renderer">Mortar renderer</param>
        /// <param name="index">Team index</param>
        /// <param name="vessel">Cached ship</param>
        /// <returns></returns>
        private IEnumerator wasteTime(Renderer renderer, int index)
        {
            yield return new WaitForSeconds(.1f);
            bool notFoundCaptain = true;
            while (notFoundCaptain)
            {
                if (GameMode.Instance.teamCaptains.Length >= index && GameMode.Instance.teamCaptains[index] != null)
                {
                    string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();

                    if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                    {
                        applySkin(renderer, player.mortarSkinName);
                    }
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        [HarmonyPatch(typeof(MortarUse), "Start")]
        class mortarUsePatch
        {
            static void Postfix(MortarUse __instance)
            {
                if (AlternionSettings.useMortarSkins)
                {
                    Renderer[] renderers = __instance.transform.parent.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        try
                        {
                            if (renderers[i] && renderers[i].name == "prp_mortar")
                            {
                                int index = GameMode.getParentIndex(__instance.transform.root);
                                cachedShip vessel = TheGreatCacher.Instance.getCachedShip(index.ToString());
                                vessel.mortars.Add(renderers[i]);
                                Instance.StartCoroutine(Instance.wasteTime(renderers[i], index));                                
                            }
                        }catch(Exception e)
                        {
                            // Cause for some reason a bunch of these renderers give the "object-reference-not-set-to-an-instance-of-an-object" error
                        }
                    }
                }
            }
        }
    }
}
