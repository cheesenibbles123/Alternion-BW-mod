using System.Collections.Generic;

namespace Alternion
{
    public class cachedShip
    {
        //Format will be OBJECTNAME / OBJECT
        public Dictionary<string, SailHealth> sailDict = new Dictionary<string, SailHealth>();
        public Dictionary<string, SailHealth> mainSailDict = new Dictionary<string, SailHealth>();
        public Dictionary<string, CannonUse> cannonOperationalDict = new Dictionary<string, CannonUse>();
        public Dictionary<string, CannonDestroy> cannonDestroyDict = new Dictionary<string, CannonDestroy>();
    }
}
