using UnityEngine;

namespace Alternion.Structs
{
    /// <summary>
    /// Stores information about the textures used for a primary weapon
    /// </summary>
    public struct defaultPrimaryWeapon
    {
        /// <summary>
        /// Albedo texture
        /// </summary>
        public Texture alb;
        /// <summary>
        /// Metallic texture
        /// </summary>
        public Texture met;
        /// <summary>
        /// Normal texture
        /// </summary>
        public Texture nrm;
        /// <summary>
        /// Ambient Occlusion texture
        /// </summary>
        public Texture ao;
    }
}
