﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;

namespace Alternion
{
    class swivelHandler : MonoBehaviour
    {
        public static swivelHandler Instance;

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
                theGreatCacher.Instance.ships.Add(index.ToString(), newVessel);
                newVessel.Swivels.Add(rend);
                applySkin(rend, steamID);
            }
        }

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
                    renderer.material.mainTexture = newTex;
                }
            }
        }

        void updateSwivel(int index)
        {
            if (theGreatCacher.Instance.ships.TryGetValue(index.ToString(), out cachedShip vessel))
            {
                string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                {
                    Texture newTex;
                    foreach (Renderer ren in vessel.Swivels)
                    {
                        if (theGreatCacher.Instance.swivels.TryGetValue(steamID, out newTex))
                        {
                            ren.material.mainTexture = newTex;
                        }
                        if (theGreatCacher.Instance.swivels.TryGetValue(steamID, out newTex))
                        {
                            ren.material.SetTexture("_Metallic", newTex);
                        }
                    }
                }
                else
                {
                    resetSwivels(vessel);
                }
                
            }
        }

        void resetSwivels(cachedShip vessel)
        {
            if (vessel.hasChangedSwivels)
            {
                foreach (Renderer ren in vessel.Swivels)
                {
                    ren.material.mainTexture = theGreatCacher.Instance.defaultSwivel;
                    ren.material.SetTexture("_Metallic", theGreatCacher.Instance.defaultSwivelMet);
                }
            }
        }

        void setupDefaultImg(Renderer rend)
        {
            if (!theGreatCacher.Instance.setSwivelDefaults)
            {
                theGreatCacher.Instance.defaultSwivel = rend.material.mainTexture;
                theGreatCacher.Instance.defaultSwivelMet = rend.material.GetTexture("_Metallic");
            }
        }

        [HarmonyPatch(typeof(SwivelUse), "Start")]
        static class playerInfoPatch
        {
            static void Postfix(SwivelUse __instance)
            {
                return;
                Logger.debugLog("Ran swiveluse base");
                Renderer[] renderers = __instance.transform.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {

                    Logger.debugLog(rend.name);
                }

                Logger.debugLog("Ran swiveluse parent"); // swivel_connector
                Renderer[] renderers2 = __instance.transform.parent.parent.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers2)
                {
                    if (rend.name == "swiveltop" || rend.name == "swivel_connector" || rend.name == "swivel_base")
                    {
                        Instance.setupDefaultImg(rend);
                        Instance.setupShip(__instance, rend);
                    }
                    Logger.debugLog(rend.name);
                }
            }
        }
    }
}
