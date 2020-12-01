using System.Collections.Generic;
using UnityEngine;
using BWModLoader;

namespace Alternion
{
    [Mod]
    public class theGreatCacher : MonoBehaviour
    {
        //Format will be TEAMNUMBER / SHIP
        public static bool isActive = false;
        public static Texture defaultSails;
        public static Texture defaultCannons;
        public static Dictionary<string, cachedShip> ships = new Dictionary<string, cachedShip>();
        public static Dictionary<string, Texture> weaponSkins = new Dictionary<string, Texture>();
        public static Dictionary<string, Texture> badges = new Dictionary<string, Texture>();
        public static Dictionary<string, Texture> maskSkins = new Dictionary<string, Texture>();
        public static Dictionary<string, Texture> mainSails = new Dictionary<string, Texture>();
        public static Dictionary<string, Texture> secondarySails = new Dictionary<string, Texture>();
        public static Dictionary<string, Texture> cannonSkins = new Dictionary<string, Texture>();
        public static Dictionary<string, playerObject> players = new Dictionary<string, playerObject>();
        void Start()
        {
            isActive = true;
        }
        public static void setDefaultSails(Texture newTexture)
        {
            defaultSails = newTexture;
        }
        public static void setDefaultCannons(Texture newTexture)
        {
            defaultCannons = newTexture;
        }
    }
}
