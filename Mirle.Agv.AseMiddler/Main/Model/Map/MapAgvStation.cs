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
        public List<string> AddressIds { get; set; } = new List<string>();
    }
}
