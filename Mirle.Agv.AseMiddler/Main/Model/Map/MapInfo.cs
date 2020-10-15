using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Mirle.Agv.AseMiddler.Model
{

    public class MapInfo
    {
        public ConcurrentDictionary<string, MapAddress> addressMap = new ConcurrentDictionary<string, MapAddress>();
        public Dictionary<string, MapSection> sectionMap = new Dictionary<string, MapSection>();
        public List<MapAddress> chargerAddressMap = new List<MapAddress>();
        public Dictionary<string, string> gateTypeMap = new Dictionary<string, string>();
        public Dictionary<string, MapAgvStation> agvStationMap = new Dictionary<string, MapAgvStation>();
        public Dictionary<string, MapPort> portMap = new Dictionary<string, MapPort>();
        public MapInfo()
        {
        }
    }
}
