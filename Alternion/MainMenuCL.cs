using System;
using Harmony;
using UnityEngine;
using BWModLoader;
using Steamworks;
using Alternion.Structs;
using System.Collections.Generic;

namespace Alternion
{
    [Mod]
    public class MainMenuCL : MonoBehaviour
    {
        /// <summary>
        /// Main menu character transform.
        /// </summary>
        static Transform menuCharacter;
        static Animation menuCharacterAnimation;
        static GameObject weapon;

        static List<menuAnimation> animationClips = new List<menuAnimation>();
        static int currentClip = 0;

        /// <summary>
        /// MainMenuCL Instance.
        /// </summary>
        public static MainMenuCL Instance;

        /// <summary>
        /// Sets the main menu weapon skin.
        /// </summary>
        static void setMainMenuWeaponSkin()
        {
            if (AlternionSettings.useWeaponSkins)
            {
                try
                {
                    string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
                    if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                    {
                        var musket = GameObject.Find("wpn_standardMusket_LOD1");
                        if (musket != null)
                        {
                            Texture newTex;
                            if (TheGreatCacher.Instance.weaponSkins.TryGetValue("musket_" + player.musketSkinName, out newTex))
                            {
                                musket.GetComponent<Renderer>().material.mainTexture = newTex;
                            }
                            if (TheGreatCacher.Instance.weaponSkins.TryGetValue("musket_" + player.musketSkinName + "_met", out newTex))
                            {
                                musket.GetComponent<Renderer>().material.SetTexture("_Metallic",newTex);
                            }
                        }
                        else
                        {
                            Logger.debugLog("Main menu musket not found.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.debugLog(e.Message);
                }
            }

            LoadingBar.updatePercentage(100, "Finished!");
        }

        /// <summary>
        /// Sets the main menu badge.
        /// </summary>
        public static void setMainMenuBadge()
        {

            if (!AlternionSettings.useBadges)
            {
                LoadingBar.updatePercentage(95, "applying weapon skin");
                setMainMenuWeaponSkin();
                return;
            }

            //Only main menu that you will really see is the one intially started
            //This doesn't work if you return to the main menu from a server
            MainMenu mm = FindObjectOfType<MainMenu>();

            try
            {
                string steamID = Steamworks.SteamUser.GetSteamID().ToString();
                if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                {
                    if (mm.menuBadge.texture.name != "tournamentWake1Badge" ^ (!AlternionSettings.showTWBadges & mm.menuBadge.texture.name == "tournamentWake1Badge"))
                    {
                        if (TheGreatCacher.Instance.badges.TryGetValue(player.badgeName, out Texture newTex))
                        {
                            mm.menuBadge.texture = newTex;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Logger.debugLog("Failed to assign custom badge to a player:");
                Logger.debugLog(e.Message);
            }

            LoadingBar.updatePercentage(95, "Applying weapon skin");
            setMainMenuWeaponSkin();
            Instance.setMenuFlag();

        }
        
        /// <summary>
        /// Sets the main menu flag.
        /// </summary>
        public void setMenuFlag()
        {
            string steamID = SteamUser.GetSteamID().ToString();
            if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
            {
                SkinnedMeshRenderer menuFlag = CharacterCustomizationUI.îêêæëçäëèñî.çóîóëðåïåóñ;
                string flagName = CharacterCustomizationUI.îêêæëçäëèñî.òïîîóðçèèæì.enabled ? player.flagNavySkinName : player.flagPirateSkinName;
                if (TheGreatCacher.Instance.flags.TryGetValue(flagName, out Texture newTex))
                {
                    menuFlag.material.mainTexture = newTex;
                }
            }
        }

        /// <summary>
        /// Sets the main menu character transform.
        /// </summary>
        void setMenuCharacter()
        {
            // Find the musket object
            weapon = GameObject.Find("wpn_standardMusket_LOD1");
            if (weapon != null)
            {
                // Get Animator and vendor clips
                int vendorNum = 0;
                foreach (Animation anim in weapon.transform.root.GetComponentsInChildren<Animation>())
                {
                    if (anim.name == "default_character_rig")
                    {
                        menuCharacterAnimation = anim;
                        menuCharacter = anim.transform;
                        animationClips.Add(new menuAnimation(
                                anim.clip,
                                true,
                                anim.name
                            ));
                    }
                    else
                    {
                        animationClips.Add(new menuAnimation(
                                anim.clip,
                                false,
                                $"{anim.name}-{vendorNum}"
                            ));
                        vendorNum++;
                    }
                }
                setupMenuCharacterAnimations();
            }
        }

        void setupMenuCharacterAnimations()
        {
            foreach (menuAnimation animation in animationClips)
            {
                menuCharacterAnimation.AddClip(animation.animation, animation.clipName);
            }
        }

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

            AnimationClip[] animations = Resources.FindObjectsOfTypeAll<AnimationClip>();
            List<string> clips = new List<string>()
                {
                    "Sprint",
                    "Crouch_Idle",
                };
            for (int i = 0; i < animations.Length; i++)
            {
                if (clips.Contains(animations[i].name))
                {
                    animationClips.Add(new menuAnimation(
                        animations[i],
                        true,
                        animations[i].name
                        ));
                }
            }
        }

        void Start()
        {
            //Rotate Character
            setMenuCharacter();
        }

        void Update()
        {
            if (menuCharacter)
            {
                if (óèïòòåææäêï.åìçæçìíäåóë != null && !óèïòòåææäêï.åìçæçìíäåóë.activeSelf && global::Input.GetMouseButton(1))
                {
                    /*
                     * If it has been found
                     * Rotation code copied from CharacterCustomizationUI
                     */
                    menuCharacter.Rotate(Vector3.up, 1000f * Time.deltaTime * -global::Input.GetAxisRaw("Mouse X"));
                }
                if (menuCharacterAnimation)
                {
                    handleMenuAnimationAndPoses();
                }
                if (weapon)
                {
                    handleWeaponChange();
                }
            }
        }

        void handleMenuAnimationAndPoses()
        {
            if (getKeyPress(AlternionSettings.mainMenuAnimationStepKey))
            {
                menuCharacterAnimation.CrossFade(animationClips[currentClip].clipName);
                if (animationClips[currentClip].weaponVisible && !weapon.activeSelf)
                {
                    weapon.SetActive(true);
                }else if (!animationClips[currentClip].weaponVisible && weapon.activeSelf)
                {
                    weapon.SetActive(false);
                }
                currentClip++;
                if (currentClip >= animationClips.Count)
                {
                    currentClip = 0;
                }
            }
        }

        /// <summary>
        /// Does the same as Input.GetKey("key"). Just looks nicer.
        /// </summary>
        /// <param name="key">Key to check if released</param>
        /// <returns>True/False</returns>
        bool getKeyPress(string key)
        {
            return Input.GetKeyUp(key);
        }

        void handleWeaponChange()
        {
            
        }

        /// <summary>
        /// Sets flag when leaving customization menu to prevent something being overwritten
        /// </summary>
        [HarmonyPatch(typeof(MainMenu), "leaveCustomization")]
        class flagPatch
        {
            static void postfix(MainMenu __instance)
            {
                Instance.setMenuFlag();
            }
        }

        /// <summary>
        /// Toggles custom/KS on KS toggle
        /// </summary>
        [HarmonyPatch(typeof(MainMenu), "toggleKSBadge")]
        class toggleKSPatch
        {
            static void Postfix(MainMenu __instance, bool on)
            {
                if (AlternionSettings.useBadges)
                {
                    if (!on)
                    {
                        setMainMenuBadge();
                    }
                }
            }
        }

        /// <summary>
        /// Sets up initial calls for returning to the mainmenu
        /// </summary>
        [HarmonyPatch(typeof(MainMenu), "Start")]
        class mainMenuStuffPatch
        {
            static void Postfix(MainMenu __instance)
            {
                // Call these so that they set correctly again on returning to the main menu
                setMainMenuBadge();
                Instance.setMenuCharacter();
                Instance.setMenuFlag();
            }
        }

        /// <summary>
        /// Updates flag in main menu based off current viewing faction
        /// </summary>
        [HarmonyPatch(typeof(CharacterCustomizationUI), "setFaction")]
        class characterCustomizationPatch
        {
            static void Postfix(CharacterCustomizationUI __instance, int íïïìîóðíçëæ)
            {
                Instance.setMenuFlag();
            }
        }
    }
}
