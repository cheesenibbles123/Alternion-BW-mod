using Alternion.Structs;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alternion.SkinHandlers
{
    class GoldMaskHandler
    {
        private static Logger logger = new Logger("[GoldMask]");

        /// <summary>
        /// Mask skin patch
        /// </summary>
        [HarmonyPatch(typeof(Character), "setGoldMask")]
        class goldMaskPatch
        {
            static void Postfix(Character __instance)
            {
                try
                {
                    if (AlternionSettings.useMaskSkins)
                    {
                        string steamID = __instance.transform.parent.GetComponent<PlayerInfo>().steamID.ToString();
                        if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                        {
                            if (player.maskSkinName == "default") return;
                            string skinName = "mask_" + player.maskSkinName;
                            TheGreatCacher.Instance.skinAttributes.TryGetValue(skinName, out weaponSkinAttributes attrib);

                            Texture newTex;
                            Renderer maskRend = __instance.éäéïéðïåææè.transform.GetComponent<Renderer>();
                            if (attrib.hasAlb && TheGreatCacher.Instance.maskSkins.TryGetValue(skinName, out newTex))
                            {
                                maskRend.material.mainTexture = newTex;
                            }else { maskRend.material.mainTexture = TheGreatCacher.Instance.defaultMaskSkin; }

                            if (attrib.hasMet && TheGreatCacher.Instance.maskSkins.TryGetValue(skinName + "_met", out newTex))
                            {
                                maskRend.material.SetTexture("_Metallic", newTex);
                            }
                            else { maskRend.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMaskMet); }

                            if (attrib.hasAlb && TheGreatCacher.Instance.maskSkins.TryGetValue(skinName + "_nrm", out newTex))
                            {
                                maskRend.material.SetTexture("_BumpMap", newTex);
                            }
                            else { maskRend.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMaskNrm); }

                            MeshFilter maskFilter = __instance.éäéïéðïåææè.transform.GetComponent<MeshFilter>();
                            if (attrib.hasMesh && TheGreatCacher.Instance.maskModels.TryGetValue(skinName, out Mesh mesh))
                            {
                                maskFilter.mesh = mesh;
                            }
                            else { maskFilter.mesh = TheGreatCacher.Instance.defaultMaskMesh; }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.debugLog(e.Message);
                }
            }
        }
    }
}
