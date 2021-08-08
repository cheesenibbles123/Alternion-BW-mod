using System;
using BWModLoader;
using UnityEngine;
using Harmony;

namespace Alternion
{
    [Mod]
    public class swivelHandler : MonoBehaviour
    {
        /// <summary>
        /// Swivel Handler
        /// </summary>
        public static swivelHandler Instance;

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
        void setupShip(SwivelUse __instance, Renderer rend)
        {
            int index = GameMode.getParentIndex(__instance.transform.root);

            string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();

            if (theGreatCacher.Instance.ships.TryGetValue(index.ToString(), out cachedShip vessel))
            {
                vessel.Swivels.Add(rend);
                applySkin(rend, steamID);
            }
            else
            {
                cachedShip newVessel = new cachedShip();
                newVessel.Swivels.Add(rend);
                theGreatCacher.Instance.ships.Add(index.ToString(), newVessel);
                applySkin(rend, steamID);
            }
        }

        /// <summary>
        /// Apply skin to swivel
        /// </summary>
        /// <param name="renderer">Swivel Renderer</param>
        /// <param name="steamID">Captain steamID</param>
        void applySkin(Renderer renderer, string steamID)
        {
            if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player)) {
                Texture newTex;
                if (theGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName, out newTex))
                {
                    renderer.material.mainTexture = newTex;
                }
                if (theGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName + "_met", out newTex))
                {
                    renderer.material.SetTexture("_Metallic", newTex);
                }
            }
        }

        /// <summary>
        /// Update swivel textures
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="player">Captain</param>
        public void updateSwivels(cachedShip vessel, playerObject player)
        {
            if (player != null)
            {
                Texture newTex;
                foreach (Renderer renderer in vessel.Swivels)
                {
                    if (theGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName,out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                    }
                    else
                    {
                        renderer.material.mainTexture = theGreatCacher.Instance.defaultSwivel;
                    }

                    if (theGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName + "_met", out newTex))
                    {
                        renderer.material.SetTexture("_Metallic", newTex);
                    }
                    else
                    {
                        renderer.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSwivelMet);
                    }
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
            if (vessel.hasChangedSwivels)
            {
                foreach (Renderer ren in vessel.Swivels)
                {
                    ren.material.mainTexture = theGreatCacher.Instance.defaultSwivel;
                    ren.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSwivelMet);
                }
                vessel.hasChangedSwivels = false;
            }
        }

        /// <summary>
        /// Setups up caching of default alb and met textures
        /// </summary>
        /// /// <param name="rend">Swivel Renderer</param>
        void setupDefaultImg(Renderer rend)
        {
            if (!theGreatCacher.Instance.setSwivelDefaults)
            {
                theGreatCacher.Instance.defaultSwivel = rend.material.mainTexture;
                theGreatCacher.Instance.defaultSwivelMet = rend.material.GetTexture("_Metallic");
                theGreatCacher.Instance.setSwivelDefaults = true;
            }
        }

        /// <summary>
        /// Patches into SwivelUse.Start()
        /// </summary>
        [HarmonyPatch(typeof(SwivelUse), "Start")]
        static class swivelPatch
        {
            static void Postfix(SwivelUse __instance)
            {
                if (AlternionSettings.useSwivelSkins)
                {
                    Renderer[] renderers2 = __instance.transform.parent.parent.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers2.Length; i++)
                    {
                        Renderer rend = renderers2[i];
                        if (rend.name == "swiveltop" || rend.name == "swivel_connector" || rend.name == "swivel_base")
                        {
                            try
                            {
                                Instance.setupDefaultImg(rend);
                                Instance.setupShip(__instance, rend);
                            }
                            catch (Exception e)
                            {
                                Logger.debugLog(e.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}
