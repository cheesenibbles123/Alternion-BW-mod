using System.Collections.Generic;
using UnityEngine;
using BWModLoader;
using Alternion.Structs;
using Alternion.UI;

namespace Alternion
{
    /// <summary>
    /// Stores all loaded assets.
    /// </summary>
    [Mod]
    public class TheGreatCacher : MonoBehaviour
    {
        public static TheGreatCacher Instance;

        public Texture defaultSwivel;
        public Texture defaultSwivelMet;
        public Texture defaultSwivelNrm;
        public Mesh defaultSwivelBarrelMesh;
        public Mesh defaultSwivelConnectorMesh;
        public Mesh defaultSwivelBaseMesh;

        public Texture defaultMortar;
        public Texture defaultMortarMet;
        public Texture defaultMortarNrm;
        public Mesh defaultMortarMesh;

        public Texture defaultSails;
        public Texture defaultSailsMet;

        public Texture defaultCannons;
        public Texture defaultCannonsMet;
        public Texture defaultCannonsNrm;
        public Mesh defaultCannonMesh;

        public Texture defaultMaskSkin;
        public Texture defaultMaskMet;
        public Texture defaultMaskNrm;
        public Mesh defaultMaskMesh;

        public Texture navyFlag;
        public Texture pirateFlag;

        public Dictionary<string, Mesh> defaultWeaponModels = new Dictionary<string, Mesh>();
        public Dictionary<string, Texture> defaultWeaponSkins = new Dictionary<string, Texture>();

        /// <summary>
        /// Stores all cached ships.
        /// </summary>
        public Dictionary<string, cachedShip> ships = new Dictionary<string, cachedShip>();
        /// <summary>
        /// Stores all cached weapon skins.
        /// </summary>
        public Dictionary<string, Texture> weaponSkins = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached custom weapon models.
        /// </summary>
        public Dictionary<string, Mesh> weaponModels = new Dictionary<string, Mesh>();
        /// <summary>
        /// Stores all cached badges.
        /// </summary>
        public Dictionary<string, Texture> badges = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached gold mask skins.
        /// </summary>
        public Dictionary<string, Texture> maskSkins = new Dictionary<string, Texture>();
        public Dictionary<string, Mesh> maskModels = new Dictionary<string, Mesh>();
        /// <summary>
        /// Stores all cached main sails.
        /// </summary>
        public Dictionary<string, Texture> mainSails = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached secondary sails.
        /// </summary>
        public Dictionary<string, Texture> secondarySails = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached cannon skins.
        /// </summary>
        public Dictionary<string, Texture> cannonSkins = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached cannon models.
        /// </summary>
        public Dictionary<string, Mesh> cannonModels = new Dictionary<string, Mesh>();
        /// <summary>
        /// Stores all cached mortar skins.
        /// </summary>
        public Dictionary<string, Texture> mortarSkins = new Dictionary<string, Texture>();
        public Dictionary<string, Mesh> mortarModels = new Dictionary<string, Mesh>();
        /// <summary>
        /// Stores all cached flags
        /// </summary>
        public Dictionary<string, Texture> flags = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached player loadouts.
        /// </summary>
        public Dictionary<string, playerObject> players = new Dictionary<string, playerObject>();
        /// <summary>
        /// Stores all swivel textures.
        /// </summary>
        public Dictionary<string, Texture> swivels = new Dictionary<string, Texture>();
        /// <summary>
        /// Dictionary containing the info about each skins attributes, indexed by skin name
        /// </summary>
        public Dictionary<string, weaponSkinAttributes> skinAttributes = new Dictionary<string, weaponSkinAttributes>();
        /// <summary>
        /// Array to store primary weapons default textures
        /// </summary>
        public static defaultPrimaryWeapon[] primaryWeaponsDefault = new defaultPrimaryWeapon[4];

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
            sortThroughAllTextures();
            sortThroughAllMeshs();
        }

        /// <summary>
        /// Forces an update of all users, only use if you know what you are doing
        /// </summary>
        public static void forceUpdate()
        {
            Instance.weaponSkins.Clear();
            Instance.weaponModels.Clear();
            Instance.badges.Clear();
            Instance.maskSkins.Clear();
            Instance.maskModels.Clear();
            Instance.mainSails.Clear();
            Instance.secondarySails.Clear();
            Instance.cannonSkins.Clear();
            Instance.mortarSkins.Clear();
            Instance.mortarModels.Clear();
            Instance.players.Clear();
            Instance.skinAttributes.Clear();
            Mainmod.Instance.createDirectories();
        }

        /// <summary>
        /// Fetch a cached ship.
        /// If none found creates new one.
        /// </summary>
        /// <param name="team">Team number</param>
        /// <returns>The ship for the given team</returns>
        public cachedShip getCachedShip(string team)
        {
            if (ships.TryGetValue(team, out cachedShip ship))
            {
                return ship;
            }
            else
            {
                cachedShip vessel = new cachedShip();
                ships.Add(team, vessel);
                return vessel;
            }
        }

        /// <summary>
        /// Used to pre-load all default textures, removing the need to run if-checks at runtime to set them
        /// </summary>
        private void sortThroughAllTextures()
        {
            Texture[] mainTex = Resources.FindObjectsOfTypeAll<Texture>();
            foreach (Texture texture in mainTex)
            {
                switch (texture.name)
                {
                    // GUI
                    case "oldmap1":
                        UI.SettingsMenu.setMainBoxBackground(texture);
                        break;
                    case "panel_medium":
                        UI.SettingsMenu.setMainButtonBackground(texture);
                        break;
                    case "Checkmark":
                        UI.SettingsMenu.setCheckmark(texture);
                        break;
                    case "UISprite":
                        UI.SettingsMenu.setCheckBox(texture);
                        break;

                    // CANNONS
                    case "prp_cannon_alb":
                        defaultCannons = texture;
                        break;
                    case "prp_cannon_met":
                        defaultCannonsMet = texture;
                        break;
                    case "prp_cannon_nrm":
                        defaultCannonsNrm = texture;
                        break;

                    // MORTAR
                    case "prp_mortar_alb":
                        defaultMortar = texture;
                        break;
                    case "prp_mortar_met":
                        defaultMortarMet = texture;
                        break;
                    case "prp_mortar_nrm":
                        defaultMortarNrm = texture;
                        break;

                    // Swivel
                    case "prp_swivel_alb":
                        defaultSwivel = texture;
                        break;
                    case "prp_swivel_met":
                        defaultSwivelMet = texture;
                        break;
                    case "prp_swivel_nrm":
                        defaultSwivelNrm = texture;
                        break;

                    case "ships_sails_alb":
                        defaultSails = texture;
                        break;

                    // FLAG
                    case "flag_british":
                        navyFlag = texture;
                        break;
                    case "flag_pirate":
                        pirateFlag = texture;
                        break;
                    // GOLD MASK
                    case "goldenSkullMask_alb":
                        defaultMaskSkin = texture;
                        break;
                    case "goldenSkullMask_met":
                        defaultMaskMet = texture;
                        break;
                    case "goldenSkullMask_nrm":
                        defaultMaskNrm = texture;
                        break;


                    // MAIN MENU WEAPONS
                    case "wpn_nockGun_stock_alb":
                        primaryWeaponsDefault[0].alb = texture;
                        break;
                    case "wpn_nockGun_stock_met":
                        primaryWeaponsDefault[0].met = texture;
                        break;
                    case "wpn_nockGun_stock_nrm":
                        primaryWeaponsDefault[0].nrm = texture;
                        break;
                    case "wpn_nockGun_stock_ao":
                        primaryWeaponsDefault[0].ao = texture;
                        break;

                    case "wpn_handMortar_alb":
                        primaryWeaponsDefault[1].alb = texture;
                        break;
                    case "wpn_handMortar_met":
                        primaryWeaponsDefault[1].met = texture;
                        break;
                    case "wpn_handMortar_nrm":
                        primaryWeaponsDefault[1].nrm = texture;
                        break;
                    case "wpn_handMortar_ao":
                        primaryWeaponsDefault[1].ao = texture;
                        break;

                    case "wpn_blunderbuss_alb":
                        primaryWeaponsDefault[2].alb = texture;
                        break;
                    case "wpn_blunderbuss_met":
                        primaryWeaponsDefault[2].met = texture;
                        break;
                    case "wpn_blunderbuss_nrm":
                        primaryWeaponsDefault[2].nrm = texture;
                        break;
                    case "wpn_blunderbuss_ao":
                        primaryWeaponsDefault[2].ao = texture;
                        break;

                    case "wpn_standardMusket_stock_alb":
                        primaryWeaponsDefault[3].alb = texture;
                        break;
                    case "wpn_standardMusket_stock_met":
                        primaryWeaponsDefault[3].met = texture;
                        break;
                    case "wpn_standardMusket_stock_nrm":
                        primaryWeaponsDefault[3].nrm = texture;
                        break;
                    case "wpn_standardMusket_stock_ao":
                        primaryWeaponsDefault[3].ao = texture;
                        break;
                                        
                    
                    default:
                        //log(texture.name);
                        break;
                }
            }
        }

        private void sortThroughAllMeshs()
        {
            Mesh[] allMeshes = Resources.FindObjectsOfTypeAll<Mesh>();
            foreach (Mesh mesh in allMeshes)
            {
                switch (mesh.name)
                {
                    // CANNON
                    case "cannon":
                        defaultCannonMesh = mesh;
                        break;

                    // SWIVEL
                    case "swivel_barrel":
                        defaultSwivelBarrelMesh = mesh;
                        break;
                    case "swivel_connector":
                        defaultSwivelConnectorMesh = mesh;
                        break;
                    case "swivel_base":
                        defaultSwivelBaseMesh = mesh;
                        break;

                    // MASK
                    case "goldenSkullMask":
                        defaultMaskMesh = mesh;
                        break;

                    // MORTAR
                    case "prp_mortar":
                        defaultMortarMesh = mesh;
                        break;
                    default:
                        //log(texture.name);
                        break;
                }
            }
        }
    }
}
