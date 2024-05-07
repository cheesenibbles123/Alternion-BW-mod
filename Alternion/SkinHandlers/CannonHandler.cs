using System.Collections;
using UnityEngine;
using BWModLoader;
using Harmony;
using Alternion.Structs;

namespace Alternion.SkinHandlers
{
    [Mod]
    public class cannonHandler : MonoBehaviour
    {
        public static cannonHandler Instance;

        /// <summary>
        /// Max times it will loop for a captain for a given vessel before timing out
        /// </summary>
        private int maxRuns = 40;

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
        /// Applies skin to cannon
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="steamID">Captain SteamID</param>
        /// <param name="renderer">Cannon renderer</param>
        void applySkins(cachedShip vessel, string steamID, Renderer renderer)
        {
            if (AlternionSettings.useCannonSkins && TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
            {
                if (player.cannonSkinName != "default")
                {
                    TheGreatCacher.Instance.skinAttributes.TryGetValue("cannon_" + player.cannonSkinName, out weaponSkinAttributes attrib);
                    Texture newTex;
                    if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                    {
                        renderer.material.mainTexture = newTex;
                        if (!vessel.hasChangedCannons)
                        {
                            vessel.hasChangedCannons = true;
                        }
                    }
                    else { renderer.material.mainTexture = TheGreatCacher.Instance.defaultCannons; }

                    if (attrib.hasMet && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                    {
                        renderer.material.SetTexture("_Metallic", newTex);
                        if (!vessel.hasChangedCannons)
                        {
                            vessel.hasChangedCannons = true;
                        }
                    }
                    else { renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet); }

                    if (attrib.hasNrm && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_nrm", out newTex))
                    {
                        renderer.material.SetTexture("_BumpMap", newTex);
                        if (!vessel.hasChangedCannons)
                        {
                            vessel.hasChangedCannons = true;
                        }
                    }
                    else { renderer.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultCannonsNrm); }
                }
                else
                {
                    Instance.resetCannon(vessel, renderer);
                }
            }
            else
            {
                Instance.resetCannon(vessel, renderer);
            }
        }

        void applyMesh(cachedShip vessel, string steamID, MeshFilter meshFilter)
        {
            if (AlternionSettings.useCannonSkins && TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
            {
                if (player.cannonSkinName != "default" && TheGreatCacher.Instance.cannonModels.TryGetValue(player.cannonSkinName, out Mesh cannonMesh))
                {
                    vessel.hasChangedCannonModels = true;
                    meshFilter.mesh = cannonMesh;
                    return;
                } else { Instance.resetCannon(vessel, meshFilter); }
            } else { Instance.resetCannon(vessel, meshFilter); }
        }

        /// <summary>
        /// Resets the cannon skin to default
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="renderer">Cannon renderer</param>
        void resetCannon(cachedShip vessel, Renderer renderer)
        {
            if (vessel.hasChangedCannons && renderer != null)
            {
                renderer.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                renderer.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
            }
        }

        /// <summary>
        /// Resets the cannon skin to default
        /// </summary>
        /// <param name="vessel">Ship</param>
        /// <param name="meshFilter">Cannon Mesh Filter</param>
        void resetCannon(cachedShip vessel, MeshFilter meshFilter)
        {
            if (vessel.hasChangedCannonModels && meshFilter != null)
            {
                meshFilter.mesh = TheGreatCacher.Instance.defaultCannonMesh;
            }
        }

        /// <summary>
        /// Wastes time until the captain is found for the external LOD
        /// </summary>
        /// <param name="__instance">Cannon Renderer</param>
        private IEnumerator wasteTimeLOD(Renderer __instance)
        {
            yield return new WaitForSeconds(0.1f);
            bool doesntExist = true;
            int counter = 0;
            int index = GameMode.getParentIndex(__instance.gameObject.transform.root);
            while (doesntExist && counter < maxRuns)
            {
                if (GameMode.Instance.teamCaptains.Length > index && GameMode.Instance.teamCaptains[index])
                {
                    cachedShip vessel = TheGreatCacher.Instance.getCachedShip(index.ToString());
                    vessel.cannonLOD = __instance;
                    Instance.applySkins(vessel, GameMode.Instance.teamCaptains[index].steamID.ToString(), __instance);
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
        /// Wastes time until the captain is found
        /// </summary>
        /// <param name="__instance">Destroyed cannon component</param>
        /// <param name="index">Team num</param>
        private IEnumerator wasteTimeCannonDestroy(CannonDestroy __instance, int index)
        {
            yield return new WaitForSeconds(0.1f);
            bool doesntExist = true;
            int counter = 0;
            while (doesntExist && counter < maxRuns)
            {
                if (GameMode.Instance.teamCaptains.Length > index && GameMode.Instance.teamCaptains[index])
                {
                    cachedShip vessel = TheGreatCacher.Instance.getCachedShip(index.ToString());
                    Renderer destroyedCannon = __instance.îæïíïíäìéêé.GetComponent<Renderer>();
                    CannonUse cannonUse = __instance.æïìçñðåììêç.GetComponent<CannonUse>();

                    if (cannonUse && destroyedCannon)
                    {
                        Transform childCannon = cannonUse.transform.FindChild("cannon");
                        if (childCannon)
                        {
                            Renderer functionalCannon = childCannon.GetComponent<Renderer>();
                            MeshFilter functionCannonMeshFilter = childCannon.GetComponent<MeshFilter>();
                            if (functionalCannon && functionCannonMeshFilter)
                            {
                                vessel.cannons.Add(destroyedCannon);
                                vessel.cannons.Add(functionalCannon);
                                vessel.cannonModels.Add(functionCannonMeshFilter);

                                string steamID = GameMode.Instance.teamCaptains[index].steamID.ToString();
                                Instance.applySkins(vessel, steamID, destroyedCannon);
                                Instance.applySkins(vessel, steamID, functionalCannon);
                                Instance.applyMesh(vessel, steamID, functionCannonMeshFilter);
                                doesntExist = false;
                            }
                        }
                    }

                    if (doesntExist)
                    {
                        counter++;
                        yield return new WaitForSeconds(.4f);
                    }
                
                }
                else
                {
                    yield return new WaitForSeconds(.4f);
                }
            }
        }

        /// <summary>
        /// Harmony patch to "Start" for CannonDestroy
        /// </summary>
        [HarmonyPatch(typeof(CannonDestroy), "Start")]
        class cannonDestroySkinPatch
        {
            static void Postfix(CannonDestroy __instance)
            {
                if (AlternionSettings.useCannonSkins)
                {
                    Instance.StartCoroutine(Instance.wasteTimeCannonDestroy(__instance, GameMode.getParentIndex(__instance.æïìçñðåììêç.transform.root)));
                }
            }
        }

        /// <summary>
        /// Hook into cannonLOD start
        /// </summary>
        [HarmonyPatch(typeof(OnlyEnableOnMyShip), "Start")]
        class OnlyEnableOnMyShipPatch
        {
            static void Postfix(OnlyEnableOnMyShip __instance)
            {
                if (AlternionSettings.useCannonSkins)
                {
                    if (__instance.name != "Only Enemy Ship") return;

                    // If only there was an easier way that i was smart enough to figure out
                    Renderer[] components = __instance.gameObject.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer rend in components)
                    {
                        if (rend.name == "Cannonsfull") // Single mesh + material
                        {
                            Instance.StartCoroutine(Instance.wasteTimeLOD(rend));
                            break;
                        }
                    }
                }
            }
        }

    }
}
