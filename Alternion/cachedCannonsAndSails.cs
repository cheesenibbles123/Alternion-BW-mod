using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Alternion
{
    class cachedCannonsAndSails
    {
        //Format will be TEAMNUMBER / SHIP
        public Texture2D defaultSails = null;
        public Texture defaultCannons = null;
        public Texture defaultDestroyCannons = null;
        public Dictionary<string, cachedShip> ships = new Dictionary<string, cachedShip>();

        public void setDefaultSails(Texture2D newTexture)
        {
            defaultSails = newTexture;
        }
        public void setDefaultCannons(Texture2D newTexture)
        {
            defaultCannons = newTexture;
        }
    }
}
