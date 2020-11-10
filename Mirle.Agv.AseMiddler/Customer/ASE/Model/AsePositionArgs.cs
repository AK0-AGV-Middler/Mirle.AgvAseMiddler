using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    public class AsePositionArgs : EventArgs
    {
        public EnumAseArrival Arrival { get; set; } = EnumAseArrival.Arrival;
        public MapPosition MapPosition { get; set; } = new MapPosition();
        public int HeadAngle { get; set; } = 0;
        public int MovingDirection { get; set; } = 0;
        public int Speed { get; set; } = 0;
    }
}
