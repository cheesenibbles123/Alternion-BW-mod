using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;

namespace Alternion.SkinHandlers
{
    public class mortarHandler : MonoBehaviour
    {
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

        void applySkin(Renderer renderer, string skinName)
        {
            Texture img;
            if (theGreatCacher.Instance.mortarSkins.TryGetValue(skinName, out img))
            {
                renderer.material.mainTexture = img;
            }
            if (theGreatCacher.Instance.mortarSkins.TryGetValue(skinName + "_met", out img))
            {
                renderer.material.SetTexture("_Metallic", img);
            }
        }

        void handleDefaults(Renderer renderer)
        {
            if (!theGreatCacher.Instance.setMortarDefaults)
            {
                theGreatCacher.Instance.defaultMortar = renderer.material.mainTexture;
                theGreatCacher.Instance.defaultMortarMet = renderer.material.GetTexture("_Metallic");
                theGreatCacher.Instance.setMortarDefaults = true;
            }
        }

        [HarmonyPatch(typeof(MortarUse), "Start")]
        static class mortarUsePatch
        {
            static void Postfix(MortarUse __instance)
            {
                if (AlternionSettings.useMortarSkins)
                {
                    Renderer[] renderers = __instance.transform.parent.GetComponentsInChildren<Renderer>(); //.transform.parent.parent.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        try
                        {
                            if (renderers[i].name == "prp_mortar")
                            {
                                int index = GameMode.getParentIndex(__instance.transform.root);
                                string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();

                                cachedShip vessel = theGreatCacher.getCachedShip(index.ToString());
                                vessel.mortars.Add(renderers[i]);
                                Instance.handleDefaults(renderers[i]);

                                if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                                {
                                    Instance.applySkin(renderers[i], player.mortarSkinName);
                                }
                                
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
