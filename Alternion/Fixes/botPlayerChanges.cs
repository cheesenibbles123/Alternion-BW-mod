using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using System.Collections;

namespace Alternion.Fixes
{
#if EXTRAS
    public class botPlayerChanges : MonoBehaviour
    {
        public static botPlayerChanges Instance;

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
                Logger.debugLog("Bot setup complete");
            }
            catch(Exception e)
            {
                Logger.debugLog("Failed setting up bots");
                Logger.debugLog(e.Message);
            }
        }

        OutfitItem[] pickRandomItem(bool navy)
        {
            Logger.debugLog($"getting random outfit, navy={navy}");
            int num = UnityEngine.Random.Range(0, numberOfOutfitOptions);

            if (navy)
            {
                navyBotOutfits.TryGetValue(num, out OutfitItem[] navyOutfit);
                Logger.debugLog("Got navy outfit");
                return navyOutfit;
            }
            else
            {
                pirateBotOutfits.TryGetValue(num, out OutfitItem[] pirateOutfit);
                Logger.debugLog("Got pirate outfit");
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
                Logger.debugLog("Entered patch");
                int index = GameMode.getParentIndex(__instance.transform.parent);
                Logger.debugLog("Got transform");
                Instance.setupOutfit(__instance, Instance.navyTeams.Contains(index));
                Logger.debugLog("Setup outfit");
                return true;
            }
        }

        void setupOutfit(BotPlayer instance, bool isNavy)
        {
            //OutfitItem[] outfit = new OutfitItem[3];
            if (isNavy)
            {
                OutfitItem[] outfit = pickRandomItem(true); // Navy bot
                Logger.debugLog("Got outfit navy");
                instance.ìëòèìêðìíçñ.èìëéçòíääåå = outfit[3]; // suit
                instance.ìëòèìêðìíçñ.ìäóçðåòðåíè = outfit[2]; // hat
                instance.ìëòèìêðìíçñ.äíñêòéóñäèæ = outfit[1]; // hair
                instance.ìëòèìêðìíçñ.òìíåðëòíæåæ = outfit[0]; // beard
            }
            else
            {
                OutfitItem[] outfit = pickRandomItem(false); // Pirate bot
                Logger.debugLog("Got outfit pir");
                instance.îòæîñìíïæêñ.èìëéçòíääåå = outfit[3]; // suit
                instance.îòæîñìíïæêñ.ìäóçðåòðåíè = outfit[2]; // hat
                instance.îòæîñìíïæêñ.äíñêòéóñäèæ = outfit[1]; // hair
                instance.îòæîñìíïæêñ.òìíåðëòíæåæ = outfit[0]; // beard
            }

            Logger.debugLog("Complete.");
        }

        OutfitItem[] getBotOutfitItems(bool navy)
        {
            Logger.debugLog("Entering outfit getter.");
            int suit = UnityEngine.Random.Range(0, values[0]);
            int hat = UnityEngine.Random.Range(0, values[1]);
            int beard = UnityEngine.Random.Range(0, values[2]);
            int hair = UnityEngine.Random.Range(0, values[3]);
            Logger.debugLog("Generated values");
            if (navy)
            {
                Logger.debugLog("generating navy");
                OutfitItem[] items = new OutfitItem[] {CharacterCustomizationUI.îêêæëçäëèñî.çðòðåäîìëòî[beard],
                        CharacterCustomizationUI.îêêæëçäëèñî.íòïæóçìîèèð[hair],
                        CharacterCustomizationUI.îêêæëçäëèñî.éèåòæéïìóíí[hat],
                        CharacterCustomizationUI.îêêæëçäëèñî.íìçææäðëåïè[suit]
                };
                Logger.debugLog("Returning Navy");
                return items;
            }
            else {
                Logger.debugLog("generating pirate");
                OutfitItem[] items = new OutfitItem[] {CharacterCustomizationUI.îêêæëçäëèñî.çðòðåäîìëòî[beard],
                        CharacterCustomizationUI.îêêæëçäëèñî.íòïæóçìîèèð[hair],
                        CharacterCustomizationUI.îêêæëçäëèñî.îåéìïíóìòèê[hat],
                        CharacterCustomizationUI.îêêæëçäëèñî.ìéìçæêîêêíæ[suit]
                };
                Logger.debugLog("Returning pirate");
                return items;
            }
        }

        [HarmonyPatch(typeof(BotPlayer), "Unload")]
        class botPlayerUnloadPatch
        {
            static bool Prefix(BotPlayer __instance)
            {
                if (!__instance.gameObject.activeSelf)
                {
                    return false;
                }

                if (__instance.transform.parent)
                {
                    __instance.transform.parent = null;
                }
                __instance.gameObject.SetActive(false);

                return false;
            }
        }
    }
#endif
}
