using System.Collections.Generic;

namespace Alternion
{
    /// <summary>
    /// Single cached ship object, stores sails and cannons.
    /// </summary>
    public class cachedShip
    {
        /// <summary>
        /// Stores all Secondary Sails.
        /// </summary>
        public Dictionary<string, SailHealth> sailDict = new Dictionary<string, SailHealth>();
        /// <summary>
        /// Stores all Main Sails.
        /// </summary>
        public Dictionary<string, SailHealth> mainSailDict = new Dictionary<string, SailHealth>();
        /// <summary>
        /// Stores Operational Cannons.
        /// </summary>
        public Dictionary<string, CannonUse> cannonOperationalDict = new Dictionary<string, CannonUse>();
        /// <summary>
        /// Stores Destroyed cannons.
        /// </summary>
        public Dictionary<string, CannonDestroy> cannonDestroyDict = new Dictionary<string, CannonDestroy>();
    }
}
