using System.Collections.Generic;
using UnityEngine;

namespace Alternion
{
    /// <summary>
    /// Single cached ship object, stores sails and cannons.
    /// </summary>
    public class cachedShip
    {
        /// <summary>
        /// Check for if the cannons have been changed, or are default
        /// </summary>
        public bool hasChangedCannons;
        /// <summary>
        /// Check for if the sails have been changed, or are default
        /// </summary>
        public bool hasChangedSails;
        /// <summary>
        /// Check for if the flag has been changed, or is default
        /// </summary>
        public bool hasChangedFlag;
        /// <summary>
        /// Check for if the ship is navy or pirate (navy = true, pirate = false)
        /// </summary>
        public bool isNavy;
        public bool isInitialized = false;
        /// <summary>
        /// Ship flag renderer
        /// </summary>
        public List<Renderer> flags = new List<Renderer>();
        /// <summary>
        /// Ship Closed sails renderer
        /// </summary>
        public List<Renderer> closedSails = new List<Renderer>();
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
