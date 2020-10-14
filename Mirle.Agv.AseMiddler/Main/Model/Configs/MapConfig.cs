using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{

    public class MapConfig
    {
        public string SectionFileName { get; set; }
        public string AddressFileName { get; set; }
        public string PortIdMapFileName { get; set; }
        public string AgvStationFileName { get; set; }
        public string SectionBeamDisablePathFileName { get; set; }
        public double AddressAreaMm { get; set; } = 30;
    }
}
