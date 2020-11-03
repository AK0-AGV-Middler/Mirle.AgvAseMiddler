using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    public class MapAgvStation
    {
        public string ID { get; set; } = "";
        public List<MapPort> Ports { get; set; } = new List<MapPort>();

        public bool IsInThisAgvStationForPortId(string portId)
        {
            return Ports.Any(x => x.ID.Trim() == portId.Trim());
        }
    }
}
