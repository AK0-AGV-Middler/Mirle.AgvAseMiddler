using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{

    public class InitialConfig
    {
        //初始畫面收到All Ok後跳轉到主畫面前的間隔時間
        public int StartOkShowMs { get; set; } = 1000;
        //初始畫面收到初始失敗後關閉程式前的間隔時間
        public int StartNgCloseSec { get; set; } = 30;
    }
}
