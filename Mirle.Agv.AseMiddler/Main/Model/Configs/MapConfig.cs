using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{

    public class MapConfig
    {
        //列有全部section內容的圖資檔案名稱
        public string SectionFileName { get; set; }
        //列有全部address內容的圖資檔案名稱
        public string AddressFileName { get; set; }
        //列有全部Port ID內容的圖資檔案名稱,通常只有AgvStation會有PortID,EQ沒有
        public string PortIdMapFileName { get; set; }
        //address座標為圓心這個範圍內視為在address上
        public double AddressAreaMm { get; set; } = 30;
    }
}
