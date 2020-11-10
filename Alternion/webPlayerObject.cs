using System;

namespace Alternion
{
    [Serializable]
    public class playerObject
    {
        public string steamID;
        public string maskSkinName;
        public string badgeName;
        public string sailSkinName;
        public string mainSailName;
        public string cannonSkinName;
        public weaponObject[] weaponSkins;
    }

    [Serializable]
    public class weaponObject
    {
        public string weaponName;
        public string weaponSkin;
    }
}
