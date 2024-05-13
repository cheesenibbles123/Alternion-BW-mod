using System;
using Harmony;
using UnityEngine;
using BWModLoader;
using Steamworks;
using Alternion.Structs;
using System.Collections.Generic;
using static Triangulation;

namespace Alternion
{
    /// <summary>
    /// Handles everything that happens within the main menu scene
    /// </summary>
    [Mod]
    public class MainMenuCL : MonoBehaviour
    {
        /// <summary>
        /// Main menu character transform.
        /// </summary>
        static Transform menuCharacter;
        /// <summary>
        /// Animation reference for the main menu character
        /// </summary>
        static Animation menuCharacterAnimation;
        /// <summary>
        /// Weapon gameobject
        /// </summary>
        static GameObject weapon;
        /// <summary>
        /// List of all the animations to add to the menu character
        /// </summary>
        static List<menuAnimation> animationClips = new List<menuAnimation>();
        /// <summary>
        /// Position of current animation within the animationClips list
        /// </summary>
        static int currentClip = 0;
        /// <summary>
        /// Position of the next weapon to be equipped
        /// </summary>
        static int currentEquippedWeapon = 0;
        /// <summary>
        /// List of all the primary weapon meshes
        /// </summary>
        static List<Mesh> wpnl = new List<Mesh>();
        /// <summary>
        /// MeshFilter of the current main menu weapon
        /// </summary>
        static MeshFilter currentWeapon;
        /// <summary>
        /// Renderer of the current main menu weapon
        /// </summary>
        static Renderer currentWeaponRend;

        static MeshFilter currentMask;
        static Renderer currentMaskRend;
        /// <summary>
        /// MainMenuCL Instance.
        /// </summary>
        public static MainMenuCL Instance;
        private static Logger logger = new Logger("[MainMenuCL]");

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
                            logger.debugLog("Main menu musket not found.");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.debugLog(e.Message);
                }
            }

            LoadingBar.updatePercentage(100, "Finished!");
        }

        /// <summary>
        /// Sets the main menu badge.
        /// </summary>
        public static void setMainMenuBadge()
        {

            if (AlternionSettings.useBadges)
            {
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
                    logger.debugLog("Failed to assign custom badge to a player:");
                    logger.debugLog(e.Message);
                }
            }
            LoadingBar.updatePercentage(95, "Applying weapon skin");
            setMainMenuWeaponSkin();
            setMenuFlag();
            setGoldMask();
        }
        
        /// <summary>
        /// Sets the main menu flag.
        /// </summary>
        public static void setMenuFlag()
        {
            if (AlternionSettings.showFlags)
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
        }

        public static void setGoldMask()
        {
            currentMask = CharacterCustomizationUI.îêêæëçäëèñî.îìèòñéðèòæñ.GetComponent<MeshFilter>();
            currentMaskRend = CharacterCustomizationUI.îêêæëçäëèñî.îìèòñéðèòæñ.GetComponent<Renderer>();

            string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
            TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player);
            string skinName = "mask_" + player.maskSkinName;

            if (AlternionSettings.useMaskSkins)
            {
                TheGreatCacher.Instance.skinAttributes.TryGetValue(skinName, out weaponSkinAttributes attrib);
                if (attrib.hasAlb)
                {
                    TheGreatCacher.Instance.maskSkins.TryGetValue(skinName, out Texture skin);
                    currentMaskRend.material.mainTexture = skin;
                }
                else { currentMaskRend.material.mainTexture = TheGreatCacher.Instance.defaultMaskSkin; }
                if (attrib.hasMet)
                {
                    TheGreatCacher.Instance.maskSkins.TryGetValue(skinName + "_met", out Texture skin);
                    currentMaskRend.material.SetTexture("_Metallic", skin);
                }
                else { currentMaskRend.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMaskMet); }
                if (attrib.hasNrm)
                {
                    TheGreatCacher.Instance.maskSkins.TryGetValue(skinName + "_nrm", out Texture skin);
                    currentMaskRend.material.SetTexture("_BumpMap", skin);
                }
                else { currentMaskRend.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMaskNrm); }
                if (attrib.hasMesh)
                {
                    TheGreatCacher.Instance.maskModels.TryGetValue(skinName, out Mesh model);
                    currentMask.mesh = model;
                }
                else { currentMask.mesh = TheGreatCacher.Instance.defaultMaskMesh; }
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
                        // If default IDLE animation
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
                        // If vendor pose
                        animationClips.Add(new menuAnimation(
                                anim.clip,
                                false,
                                $"{anim.name}-{vendorNum}"
                            ));
                        vendorNum++;
                    }
                }

                currentWeapon = weapon.GetComponent<MeshFilter>();
                currentWeaponRend = weapon.GetComponent<Renderer>();
                setupMenuCharacterAnimations();
            }
        }

        /// <summary>
        /// Adds all the loaded animations to the character
        /// </summary>
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

            /*
             * Load all animations, then load the relevant ones (This is just to load the sprint and crouch animations)
             */
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
            // Loop over all meshes and load the LOD1 of each primary
            Mesh[] meshes = Resources.FindObjectsOfTypeAll<Mesh>();
            List<string> wpns = new List<string>()
                {
                    "wpn_standardMusket_LOD1",
                    "wpn_standardBlunder_LOD1",
                    "wpn_standardNockGun_LOD1",
                    "wpn_standardHandMotar_LOD1"
                };
            for (int i = 0; i < meshes.Length; i++)
            {
                if (wpns.Contains(meshes[i].name))
                {
                    wpnl.Add(meshes[i]);
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
            // Only check if menuCharacter isn't null
            if (menuCharacter)
            {
                if (óèïòòåææäêï.åìçæçìíäåóë != null && !óèïòòåææäêï.åìçæçìíäåóë.activeSelf && global::Input.GetMouseButton(1))
                {
                    /*
                     * If the character is found in the scene
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

        /// <summary>
        /// Loops over the poses/clips for the main menu.
        /// Toggles weapon based off animation preset.
        /// </summary>
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

        /// <summary>
        /// Sets the weapon skin for the weapon
        /// </summary>
        /// <param name="wpn">ID of the weapon used</param>
        void setMenuSkin(int wpn)
        {
            string steamID = SteamUser.GetSteamID().m_SteamID.ToString();
            TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player);

            string skinName = "";
            switch (wpn)
            {
                case 0:
                    skinName = $"nockgun_{player.nockgunSkinName}";
                    break;
                case 1:
                    skinName = $"handmortar_{player.handMortarSkinName}";
                    break;
                case 2:
                    skinName = $"blunderbuss_{player.blunderbussSkinName}";
                    break;
                case 3:
                    skinName = $"musket_{player.musketSkinName}";
                    break;
            }
            Texture skin;
            if (AlternionSettings.useWeaponSkins && TheGreatCacher.Instance.weaponSkins.TryGetValue(skinName, out skin))
            {
                currentWeaponRend.material.mainTexture = skin;
            }
            else
            {
                currentWeaponRend.material.mainTexture = TheGreatCacher.primaryWeaponsDefault[wpn].alb;
            }

            if (AlternionSettings.useWeaponSkins && TheGreatCacher.Instance.weaponSkins.TryGetValue(skinName + "_met", out skin))
            {
                currentWeaponRend.material.SetTexture("_Metallic", skin);
            }
            else
            {
                currentWeaponRend.material.SetTexture("_Metallic", TheGreatCacher.primaryWeaponsDefault[wpn].met);
            }

            if (AlternionSettings.useWeaponSkins && TheGreatCacher.Instance.weaponSkins.TryGetValue(skinName + "_nrm", out skin))
            {
                currentWeaponRend.material.SetTexture("_BumpMap", skin);
            }
            else
            {
                currentWeaponRend.material.SetTexture("_BumpMap", TheGreatCacher.primaryWeaponsDefault[wpn].nrm);
            }

            // If AO support ever gets added, needs to be setup for it
            currentWeaponRend.material.SetTexture("_Occlusion", TheGreatCacher.primaryWeaponsDefault[wpn].ao);


        }

        /// <summary>
        /// Switches the weapon based on keypress, and then calls the function to setup the weapon skin
        /// </summary>
        void handleWeaponChange()
        {
            if (getKeyPress(AlternionSettings.mainMenuWeaponStepKey))
            {
                currentWeapon.mesh = wpnl[currentEquippedWeapon];
                setMenuSkin(currentEquippedWeapon);
                currentEquippedWeapon++;
                if (currentEquippedWeapon >= 4)
                {
                    currentEquippedWeapon = 0;
                }
            }
        }

        /// <summary>
        /// Sets flag when leaving customization menu to prevent something being overwritten
        /// </summary>
        [HarmonyPatch(typeof(MainMenu), "leaveCustomization")]
        class flagPatch
        {
            static void postfix(MainMenu __instance)
            {
                setMenuFlag();
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
                setMenuFlag();
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
                setMenuFlag();
            }
        }
    }
}
