using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model.Configs
{

    public class AgvcConnectorConfig
    {
        //車輛編號
        public int ClientNum { get; set; }
        //車輛名稱
        public string ClientName { get; set; }
        //C端伺服器IP
        public string RemoteIp { get; set; }
        //C端伺服器Port
        public int RemotePort { get; set; }
        //M端socket IP(若C端有指定才有作用)
        public string LocalIp { get; set; }
        //M端socket Port(若C端有指定才有作用)
        public int LocalPort { get; set; }
        //接收訊息逾時時限
        public int RecvTimeoutMs { get; set; }
        //發送訊息逾時時限
        public int SendTimeoutMs { get; set; }
        //接收訊息最大讀取長度
        public int MaxReadSize { get; set; }
        //斷線重連間隔
        public int ReconnectionIntervalMs { get; set; }
        //最大斷線重連次數
        public int MaxReconnectionCount { get; set; }
        //SendRecv 發送後API內發現timeout時，API內自動重發次數。當這個次數用完才會回傳timeout給使用者。
        public int RetryCount { get; set; }
        //詢問通行權最長距離
        public int ReserveLengthMeter { get; set; }
        //詢問通行權間隔
        public int AskReserveIntervalMs { get; set; } = 2000;
        //這次收到L層座標與上次收到L層座標差距在這範圍內，則視為沒有移動，且不會用134通知C層。
        public int NeerlyNoMoveRangeMm { get; set; }
        //收發獨立執行緒間隔
        public int ScheduleIntervalMs { get; set; } = 50;
        //SendWait 訊息回應逾時後AgvcConnector重發次數
        public int SendWaitRetryTimes { get; set; } = 3;
        //Load/Unload arrival/complete 收到回應為 ReplyAction.wait時，下次重發前的間隔時間
        public int LulWaitIntervalMs { get; set; } = 1000;
    }
}
