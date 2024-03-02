using System.Collections.Generic;
using UnityEngine;

namespace Alternion.Structs
{
    /// <summary>
    /// Single cached ship object, stores sails, flags and cannons.
    /// </summary>
    public class cachedShip
    {
        /// <summary>
        /// Check for if the cannons have been changed, or are default
        /// </summary>
        public bool hasChangedCannons = false;
        public bool hasChangedCannonModels = false;
        /// <summary>
        /// Check for if the sails have been changed, or are default
        /// </summary>
        public bool hasChangedSails = false;
        /// <summary>
        /// Check for if the flag has been changed, or is default
        /// </summary>
        public bool hasChangedFlag = false;
        /// <summary>
        /// Check for if the flag has been changed, or is default
        /// </summary>
        public bool hasChangedSwivels = false;
        /// <summary>
        /// Check for if the flag has been changed, or is default
        /// </summary>
        public bool hasChangedMortars = false;
        /// <summary>
        /// Check for if the ship is navy or pirate (navy = true, pirate = false)
        /// </summary>
        public bool isNavy;
        /// <summary>
        /// If initialized
        /// </summary>
        public bool isInitialized = false;
        /// <summary>
        /// Renderer for the ship's cannons LOD
        /// </summary>
        public Renderer cannonLOD;
        /// <summary>
        /// Ship flag renderer
        /// </summary>
        public List<Renderer> flags = new List<Renderer>();
        /// <summary>
        /// Ship swivel renderers
        /// </summary>
        public List<Renderer> Swivels = new List<Renderer>();
        /// <summary>
        /// Ship Closed sails renderer
        /// </summary>
        public List<Renderer> closedSails = new List<Renderer>();
        /// <summary>
        /// Mortar renderers
        /// </summary>
        public List<Renderer> mortars = new List<Renderer>();
        /// <summary>
        /// Stores all the cannons
        /// </summary>
        public List<Renderer> cannons = new List<Renderer>();
        public List<MeshFilter> cannonModels = new List<MeshFilter>();
        /// <summary>
        /// Stores all the secondary sails
        /// </summary>
        public Dictionary<string, SailHealth> sailDict = new Dictionary<string, SailHealth>();
        /// <summary>
        /// Stores all Main Sails.
        /// </summary>
        public Dictionary<string, SailHealth> mainSailDict = new Dictionary<string, SailHealth>();
    }
}
