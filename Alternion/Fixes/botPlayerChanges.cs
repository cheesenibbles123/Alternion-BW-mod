using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Harmony;
using System.Collections;

namespace Alternion.Fixes
{
    // CURRENTLY INACTIVE AND AWAITING UPDATE

    [HarmonyPatch(typeof(BotPlayer), "Unload")]
    class botPlayerUnloadPatch
    {
        static bool Prefix(BotPlayer __instance)
        {
            if (__instance.gameObject && __instance.gameObject.activeSelf)
            {
                return true;
            }
            return false;
        }
    }

#if EXTRAS
    [HarmonyPatch(typeof(BotPlayer), "die")]
    class botPlayerDiePatch
    {
        static bool Prefix(BotPlayer __instance, int æðæïçåíòéåð, Vector3 äåòéðññåîòì, int çññíïïíòóêê, string óêæóæìïóéñè)
        {
            if (!__instance) return true;
            if (!__instance.ìäóêäðçóììî) Logger.logger.DebugLog($"[Botplayer Die]: Undefined: ìäóêäðçóììî");
            if (!__instance.GetComponent<Collider>()) Logger.logger.DebugLog($"[Botplayer Die]: Undefined: Collider");
            if (!__instance.GetComponent<RagDoll>()) Logger.logger.DebugLog($"[Botplayer Die]: Undefined: RagDoll");
            // Seems to be an issue regarding the effect spawner
            return true;
        }
    }
#endif

#if EXTRAS
    public class botPlayerChanges : MonoBehaviour
    {
        public static botPlayerChanges Instance;
        private static Logger logger = new Logger("[BotPlayer]");

        int[] values = new int[]
        {
            5, // Suit
            5, // Hat
            2, // Beard
            3  // hair
        };
        int[] navyTeams = new int[] { 0, 1, 2 };
        int numberOfOutfitOptions = 27;

        Dictionary<int, OutfitItem[]> navyBotOutfits = new Dictionary<int, OutfitItem[]>();
        Dictionary<int, OutfitItem[]> pirateBotOutfits = new Dictionary<int, OutfitItem[]>();

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

        void Start()
        {
            setupBotOutfits();
        }

        void setupBotOutfits()
        {
            try
            {
                for (int i = 0; i < numberOfOutfitOptions; i++)
                {
                    navyBotOutfits.Add(i, getBotOutfitItems(true));
                }
                for (int i = 0; i < numberOfOutfitOptions; i++)
                {
                    pirateBotOutfits.Add(i, getBotOutfitItems(false));
                }
                logger.debugLog("Bot setup complete");
            }
            catch(Exception e)
            {
                logger.debugLog("Failed setting up bots");
                logger.debugLog(e.Message);
            }
        }

        OutfitItem[] pickRandomItem(bool navy)
        {
            logger.debugLog($"getting random outfit, navy={navy}");
            int num = UnityEngine.Random.Range(0, numberOfOutfitOptions);

            if (navy)
            {
                navyBotOutfits.TryGetValue(num, out OutfitItem[] navyOutfit);
                logger.debugLog("Got navy outfit");
                return navyOutfit;
            }
            else
            {
                pirateBotOutfits.TryGetValue(num, out OutfitItem[] pirateOutfit);
                logger.debugLog("Got pirate outfit");
                return pirateOutfit;
            }
        }

        void setupBots(BotHandler __instance)
        {
            int[] navyTeams = new int[] { 0, 1, 2 };
            for (int i = 0; i < __instance.êóæìíîìñäîí.Length; i++)
            {
                OutfitItem[] outfit;

                if (Array.IndexOf(navyTeams, __instance.êóæìíîìñäîí[i].transform.root) == -1)
                {
                    outfit = Instance.getBotOutfitItems(false); // Pirate bot
                    __instance.êóæìíîìñäîí[i].GetComponent<BotPlayer>().îòæîñìíïæêñ.èìëéçòíääåå = outfit[3]; // suit
                    __instance.êóæìíîìñäîí[i].GetComponent<BotPlayer>().îòæîñìíïæêñ.ìäóçðåòðåíè = outfit[2]; // hat
                    __instance.êóæìíîìñäîí[i].GetComponent<BotPlayer>().îòæîñìíïæêñ.äíñêòéóñäèæ = outfit[1]; // hair
                    __instance.êóæìíîìñäîí[i].GetComponent<BotPlayer>().îòæîñìíïæêñ.òìíåðëòíæåæ = outfit[0]; // beard
                }
                else
                {
                    outfit = Instance.getBotOutfitItems(true); // Navy bot
                    __instance.êóæìíîìñäîí[i].GetComponent<BotPlayer>().ìëòèìêðìíçñ.èìëéçòíääåå = outfit[3]; // suit
                    __instance.êóæìíîìñäîí[i].GetComponent<BotPlayer>().ìëòèìêðìíçñ.ìäóçðåòðåíè = outfit[2]; // hat
                    __instance.êóæìíîìñäîí[i].GetComponent<BotPlayer>().ìëòèìêðìíçñ.äíñêòéóñäèæ = outfit[1]; // hair
                    __instance.êóæìíîìñäîí[i].GetComponent<BotPlayer>().ìëòèìêðìíçñ.òìíåðëòíæåæ = outfit[0]; // beard
                }


            }
        }

        private IEnumerator wasteTime(BotHandler __instance)
        {
            yield return new WaitForSeconds(0.1f);
            while (__instance.êóæìíîìñäîí == null)
            {
                yield return new WaitForSeconds(.2f);
            }
            setupBots(__instance);
        }

        [HarmonyPatch(typeof(BotPlayer), "Start")]
        static class botPlayerPatch
        {
            static bool Prefix(BotPlayer __instance)
            {
                logger.debugLog("Entered patch");
                int index = GameMode.getParentIndex(__instance.transform.parent);
                logger.debugLog("Got transform");
                Instance.setupOutfit(__instance, Instance.navyTeams.Contains(index));
                logger.debugLog("Setup outfit");
                return true;
            }
        }

        void setupOutfit(BotPlayer instance, bool isNavy)
        {
            //OutfitItem[] outfit = new OutfitItem[3];
            if (isNavy)
            {
                OutfitItem[] outfit = pickRandomItem(true); // Navy bot
                logger.debugLog("Got outfit navy");
                instance.ìëòèìêðìíçñ.èìëéçòíääåå = outfit[3]; // suit
                instance.ìëòèìêðìíçñ.ìäóçðåòðåíè = outfit[2]; // hat
                instance.ìëòèìêðìíçñ.äíñêòéóñäèæ = outfit[1]; // hair
                instance.ìëòèìêðìíçñ.òìíåðëòíæåæ = outfit[0]; // beard
            }
            else
            {
                OutfitItem[] outfit = pickRandomItem(false); // Pirate bot
                logger.debugLog("Got outfit pir");
                instance.îòæîñìíïæêñ.èìëéçòíääåå = outfit[3]; // suit
                instance.îòæîñìíïæêñ.ìäóçðåòðåíè = outfit[2]; // hat
                instance.îòæîñìíïæêñ.äíñêòéóñäèæ = outfit[1]; // hair
                instance.îòæîñìíïæêñ.òìíåðëòíæåæ = outfit[0]; // beard
            }

            logger.debugLog("Complete.");
        }

        OutfitItem[] getBotOutfitItems(bool navy)
        {
            logger.debugLog("Entering outfit getter.");
            int suit = UnityEngine.Random.Range(0, values[0]);
            int hat = UnityEngine.Random.Range(0, values[1]);
            int beard = UnityEngine.Random.Range(0, values[2]);
            int hair = UnityEngine.Random.Range(0, values[3]);
            logger.debugLog("Generated values");
            if (navy)
            {
                logger.debugLog("generating navy");
                OutfitItem[] items = new OutfitItem[] {CharacterCustomizationUI.îêêæëçäëèñî.çðòðåäîìëòî[beard],
                        CharacterCustomizationUI.îêêæëçäëèñî.íòïæóçìîèèð[hair],
                        CharacterCustomizationUI.îêêæëçäëèñî.éèåòæéïìóíí[hat],
                        CharacterCustomizationUI.îêêæëçäëèñî.íìçææäðëåïè[suit]
                };
                logger.debugLog("Returning Navy");
                return items;
            }
            else {
                logger.debugLog("generating pirate");
                OutfitItem[] items = new OutfitItem[] {CharacterCustomizationUI.îêêæëçäëèñî.çðòðåäîìëòî[beard],
                        CharacterCustomizationUI.îêêæëçäëèñî.íòïæóçìîèèð[hair],
                        CharacterCustomizationUI.îêêæëçäëèñî.îåéìïíóìòèê[hat],
                        CharacterCustomizationUI.îêêæëçäëèñî.ìéìçæêîêêíæ[suit]
                };
                logger.debugLog("Returning pirate");
                return items;
            }
        }

        [HarmonyPatch(typeof(BotPlayer), "Unload")]
        class botPlayerUnloadPatch
        {
            static bool Prefix(BotPlayer __instance)
            {
                if (!__instance.gameObject ||
                    (__instance.gameObject && !__instance.gameObject.activeSelf))
                {
                    return false;
                }
                return true;
            }
        }
    }
#endif
}
