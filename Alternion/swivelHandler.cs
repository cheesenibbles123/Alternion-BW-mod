using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BWModLoader;
using UnityEngine;
using Harmony;

namespace Alternion
{
    [Mod]
    public class swivelHandler : MonoBehaviour
    {
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

        void setupShip(SwivelUse __instance, Renderer rend)
        {
            Logger.debugLog("Entered ship setup");
            int index = GameMode.getParentIndex(__instance.transform.root);

            string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();

            if (theGreatCacher.Instance.ships.TryGetValue(index.ToString(), out cachedShip vessel))
            {
                Logger.debugLog("Old");
                vessel.Swivels.Add(rend);
                applySkin(rend, steamID);
            }
            else
            {
                Logger.debugLog("New");
                cachedShip newVessel = new cachedShip();
                newVessel.Swivels.Add(rend);
                theGreatCacher.Instance.ships.Add(index.ToString(), newVessel);
                applySkin(rend, steamID);
            }
        }

        void applySkin(Renderer renderer, string steamID)
        {
            Logger.debugLog("Trying to apply skin");
            if (theGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player)) {
                Texture newTex;
                if (theGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName, out newTex))
                {
                    Logger.debugLog("Setting skin: " + player.swivelSkinName);
                    renderer.material.mainTexture = newTex;
                }
                if (theGreatCacher.Instance.swivels.TryGetValue(player.swivelSkinName + "_met", out newTex))
                {
                    renderer.material.SetTexture("_Metallic", newTex);
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
                Logger.debugLog("Setup defaults");
                theGreatCacher.Instance.defaultSwivel = rend.material.mainTexture;
                Logger.debugLog("Setup alb");
                theGreatCacher.Instance.defaultSwivelMet = rend.material.GetTexture("_Metallic");
                Logger.debugLog("Setup met");
                theGreatCacher.Instance.setSwivelDefaults = true;
            }
        }

        [HarmonyPatch(typeof(SwivelUse), "Start")]
        static class swivelPatch
        {
            static void Postfix(SwivelUse __instance)
            {
                //return;

                /*
                Logger.debugLog("Ran swiveluse base");
                Renderer[] renderers = __instance.transform.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {

                    Logger.debugLog(rend.name);
                }
                */

                Logger.debugLog("Ran swiveluse parent"); // swivel_connector
                Renderer[] renderers2 = __instance.transform.parent.parent.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers2.Length; i++)
                {
                    Renderer rend = renderers2[i];
                    Logger.debugLog("Looping over: " + rend.name);
                    if (rend.name == "swiveltop" || rend.name == "swivel_connector" || rend.name == "swivel_base")
                    {
                        try
                        {
                            Logger.debugLog("Found swivel part");
                            Logger.debugLog(Instance.isActiveAndEnabled.ToString());
                            Instance.setupDefaultImg(rend);
                            Logger.debugLog("Managed defaults");
                            Instance.setupShip(__instance, rend);
                        }
                        catch (Exception e)
                        {
                            Logger.debugLog(e.Message);
                        }
                    }
                    Logger.debugLog(rend.name);
                }
            }
        }
    }
}
