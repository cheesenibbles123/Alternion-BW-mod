﻿using System.Collections.Generic;
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
        public static theGreatCacher Instance;
        /// <summary>
        /// Check for if the default cannons have been set or not
        /// </summary>
        public bool setCannonDefaults = false;
        /// <summary>
        /// Check for if the default sails have been set or not
        /// </summary>
        public bool setSailDefaults = false;
        /// <summary>
        /// Check for if the default navy flag has been set or not
        /// </summary>
        public bool setNavyFlag = false;
        /// <summary>
        /// Check for if the default pirate flag has been set or not
        /// </summary>
        public bool setPirateFlag = false;
        /// <summary>
        /// Default sail texture.
        /// </summary>
        public Texture defaultSails;
        /// <summary>
        /// Default sail met texture.
        /// </summary>
        public Texture defaultSailsMet;
        /// <summary>
        /// Default cannon texture.
        /// </summary>
        public Texture defaultCannons;
        /// <summary>
        /// Default cannon met texture.
        /// </summary>
        public Texture defaultCannonsMet;
        /// <summary>
        /// Default navy flag texture
        /// </summary>
        public Texture navyFlag;
        /// <summary>
        /// Default pirate flag texture
        /// </summary>
        public Texture pirateFlag;
        /// <summary>
        /// Stores all cached ships.
        /// </summary>
        public Dictionary<string, cachedShip> ships = new Dictionary<string, cachedShip>();
        /// <summary>
        /// Stores all cached weapon skins.
        /// </summary>
        public Dictionary<string, Texture> weaponSkins = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached badges.
        /// </summary>
        public Dictionary<string, Texture> badges = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached gold mask skins.
        /// </summary>
        public Dictionary<string, Texture> maskSkins = new Dictionary<string, Texture>();
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
        /// Stores all cached flags
        /// </summary>
        public Dictionary<string, Texture> flags = new Dictionary<string, Texture>();
        /// <summary>
        /// Stores all cached player loadouts.
        /// </summary>
        public Dictionary<string, playerObject> players = new Dictionary<string, playerObject>();

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
        /// Sets the default sail texture.
        /// </summary>
        /// <param name="newTexture">Default Sail Texture</param>
        public static void setDefaultSails(Texture newTexture)
        {
            Instance.defaultSails = newTexture;
        }

        /// <summary>
        /// Sets the default cannon texture.
        /// </summary>
        /// <param name="newTexture">Default Cannon Texture</param>
        public static void setDefaultCannons(Texture newTexture)
        {
            Instance.defaultCannons = newTexture;
        }

        /// <summary>
        /// Sets the default flag texture(s).
        /// </summary>
        /// <param name="newTexture">Default Flag Texture</param>
        public static void setDefaultFlags(Texture newTexture, bool isNavy)
        {
            if (isNavy && !Instance.setNavyFlag)
            {
                Instance.navyFlag = newTexture;
                Instance.setNavyFlag = true;
                Logger.logLow("Set navy default flag");
            }
            else if (!Instance.setPirateFlag)
            {
                Instance.pirateFlag = newTexture;
                Instance.setPirateFlag = true;
                Logger.logLow("Set pirate default flag");
            }
        }

        /// <summary>
        /// Forces an update of all users
        /// </summary>
        public static void forceUpdate()
        {
            Instance.weaponSkins.Clear();
            Instance.badges.Clear();
            Instance.maskSkins.Clear();
            Instance.mainSails.Clear();
            Instance.secondarySails.Clear();
            Instance.cannonSkins.Clear();
            Instance.players.Clear();
            Mainmod.Instance.createDirectories();
        }
    }
}
