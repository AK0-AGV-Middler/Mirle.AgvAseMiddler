using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{

    public class MainFlowConfig
    {
        //階段監控執行緒執行間隔
        public int VisitTransferStepsSleepTimeMs { get; set; } = 50;
        //位置監控執行緒執行間隔
        public int TrackPositionSleepTimeMs { get; set; } = 5000;
        //低電量監控執行緒執行間隔
        public int WatchLowPowerSleepTimeMs { get; set; } = 5000;
        //對L層發出充電命令後，等待L層回應開始充電的時限
        public int StartChargeWaitingTimeoutMs { get; set; } = 30 * 1000;
        //對L層發出中斷充電命令後，等待L層回應開始結束充電的時限
        public int StopChargeWaitingTimeoutMs { get; set; } = 30 * 1000;  
        //手臂動作中提早中斷充電機制，從最後一次取/放開始多久後中斷充電。
        public int ChargeIntervalInRobotingSec { get; set; } = -1;
        //是否為模擬模式
        public bool IsSimulation { get; set; }
        //是否兩筆以上命令交錯執行，若否則一筆命令做到完才換另一筆
        public bool DualCommandOptimize { get; set; } = false;
        //是否忽略BcrReader讀取結果，開啟此功能時若載荷有則視為讀取成功，若載荷無則視為讀取失敗。
        public bool BcrByPass { get; set; } = false;
        //充電門檻高水位
        public int HighPowerPercentage { get; set; } = 90;
        //充電門檻低水位
        public int LowPowerPercentage { get; set; } = 70;
        //是否暫停左/右/全部儲位使用
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public EnumSlotSelect SlotDisable { get; set; } = EnumSlotSelect.None;
        //在收到L層通知切換成Auto的檢查中，若當前座標離圖資內最近Address座標超過此距離則視為切換Auto失敗，並發出警告。
        public int InitialPositionRangeMm { get; set; } = 500;
        //若上次回報座標與這次收到更新的座標差距在此距離內視為沒有移動過
        public int IdleReportRangeMm { get; set; } = 100;
        //若車輛停在原地達到此時間以上則發出原地發呆警報
        public int IdleReportIntervalMs { get; set; } = 2 * 60 * 1000;  
        //是否接受C層修改各站點可否充電設定
        public bool AgreeAgvcSetCoupler { get; set; } = true;
        //是否開啟單交握內一取加一放功能
        public bool IsE84Continue { get; set; } = false;
        //是否根據Port類別影響下一筆命令的優先順序，即若這筆命令是在EQ執行則下一筆優先考慮有在EQ執行的命令，反之若本筆在AgvStation則下一筆優先考慮不離開Station的命令
        public bool IsEqPortOrderOptimize { get; set; } = true;
        //各類別即時資訊保留最大長度
        public int StringBuilderMax { get; set; } = 1500;
    }
}
