using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Alternion
{
    public class playerObject
    {
        private string steamID;
        public Texture2D badgeTexture;
        public Texture2D sailSkinTexture;
        public Texture2D mainSailTexture;
        public Texture2D cannonSkinTexture;
        public Dictionary<string, Texture2D> weaponTextures;
        public void Init(string newSteamID)
        {
            steamID = newSteamID;
            weaponTextures = new Dictionary<string, Texture2D>();
        }
        public string getSteamID()
        {
            return steamID;
        }
    }
}
