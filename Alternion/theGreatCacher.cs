using System.Collections.Generic;
using UnityEngine;
using BWModLoader;

namespace Alternion
{
    /// <summary>
    /// Stores all loaded assets.
    /// </summary>
    [Mod]
    public class theGreatCacher : MonoBehaviour
    {
        public static bool isActive = false;
        public static bool setCannonDefaults = false;
        public static bool setSailDefaults = false;
        public static bool setNavyFlag = false;
        public static bool setPirateFlag = false;
        /// <summary>
        /// Default sail texture.
        /// </summary>
        public static Texture defaultSails;
        /// <summary>
        /// Default sail met texture.
        /// </summary>
        public static Texture defaultSailsMet;
        /// <summary>
        /// Default cannon texture.
        /// </summary>
        public static Texture defaultCannons;
        /// <summary>
        /// Default cannon met texture.
        /// </summary>
        public static Texture defaultCannonsMet;
        public static Texture navyFlag;
        public static Texture pirateFlag;
        /// <summary>
        /// Stores all cached ships.
        /// </summary>
        public static Dictionary<string, cachedShip> ships = new Dictionary<string, cachedShip>();
        /// <summary>
        /// Stores all cached weapon skins.
        /// </summary>
        public static Dictionary<string, Texture> weaponSkins = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached badges.
        /// </summary>
        public static Dictionary<string, Texture> badges = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached gold mask skins.
        /// </summary>
        public static Dictionary<string, Texture> maskSkins = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached main sails.
        /// </summary>
        public static Dictionary<string, Texture> mainSails = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached secondary sails.
        /// </summary>
        public static Dictionary<string, Texture> secondarySails = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached cannon skins.
        /// </summary>
        public static Dictionary<string, Texture> cannonSkins = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached flags
        /// </summary>
        public static Dictionary<string, Texture> flags = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached player loadouts.
        /// </summary>
        public static Dictionary<string, playerObject> players = new Dictionary<string, playerObject>();

        void Start()
        {
            isActive = true;
        }

        /// <summary>
        /// Sets the default sail texture.
        /// </summary>
        /// <param name="newTexture">Default Sail Texture</param>
        public static void setDefaultSails(Texture newTexture)
        {
            defaultSails = newTexture;
        }

        /// <summary>
        /// Sets the default cannon texture.
        /// </summary>
        /// <param name="newTexture">Default Cannon Texture</param>
        public static void setDefaultCannons(Texture newTexture)
        {
            defaultCannons = newTexture;
        }

        /// <summary>
        /// Forces an update of all users
        /// </summary>
        public static void forceUpdate()
        {
            weaponSkins.Clear();
            badges.Clear();
            maskSkins.Clear();
            mainSails.Clear();
            secondarySails.Clear();
            cannonSkins.Clear();
            players.Clear();
            Mainmod.Instance.createDirectories();
        }
    }
}
