using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{

    public class BatteryLog
    {
        //開機後讀取這個數值作為初始電量百分比，電量變動時後更改此數值
        public int InitialSoc { get; set; } = 70;
    }
}
