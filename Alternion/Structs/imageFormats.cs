using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alternion.Structs
{
#if EXTRAS
    public class imageFormats
    {
        static string badgeLocation = "Badges/";
        static string maskLocation = "MaskSkins/";
        static string sailLocation = "SailSkins/";
        static string mainSailLocation = "MainSailSkins/";
        static string cannonLocation = "CannonSkins/";
        static string swivelLocation = "SwivelSkins/";
        static string mortarLocation = "MortarSkins/";
        static string flagLocation = "Flags/";

        static Vector2 badgeResolution = new Vector2(100, 40);
        static Vector2 twoKRes = new Vector2(2048, 2048);

        public static Vector2 getResByType(textureType type)
        {
            switch (type)
            {
                case textureType.BADGE:
                    return badgeResolution;
                case textureType.FLAG_NAVY:
                case textureType.FLAG_PIRATE:
                    return badgeResolution;
                default:
                    return twoKRes;
            }
        }

        public static string getLocationByType(textureType type)
        {
            switch (type)
            {
                case textureType.BADGE:
                    return badgeLocation;
                case textureType.MASK:
                    return maskLocation;
                case textureType.SECONDARY_SAIL:
                    return sailLocation;
                case textureType.MAIN_SAIL:
                    return mainSailLocation;
                case textureType.CANNON:
                    return cannonLocation;
                case textureType.SWIVEL:
                    return swivelLocation;
                case textureType.MORTAR:
                    return mortarLocation;
                case textureType.FLAG_NAVY:
                case textureType.FLAG_PIRATE:
                    return flagLocation;
                default:
                    return "";
            }
        }
    }
#endif
}
