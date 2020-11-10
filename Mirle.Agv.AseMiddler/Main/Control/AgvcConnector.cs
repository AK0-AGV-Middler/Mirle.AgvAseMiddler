using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using com.mirle.iibg3k0.ttc.Common.TCPIP.DecodRawData;
using Google.Protobuf.Collections;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using Mirle.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class AgvcConnector
    {
        #region Events
        public event EventHandler<string> OnMessageShowOnMainFormEvent;
        public event EventHandler<AgvcTransferCommand> OnInstallTransferCommandEvent;
        public event EventHandler<AseMovingGuide> OnAvoideRequestEvent;
        public event EventHandler<string> OnCmdReceiveEvent;
        public event EventHandler<string> OnCmdSendEvent;
        public event EventHandler<string> OnPassReserveSectionEvent;
        public event EventHandler<AseCarrierSlotStatus> OnRenameCassetteIdEvent;
        public event EventHandler OnSendRecvTimeoutEvent;
        public event EventHandler<EnumSlotNumber> OnCstRenameEvent;

        #endregion

        private Vehicle Vehicle { get; set; } = Vehicle.Instance;
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private MainFlowHandler mainFlowHandler;

        private ConcurrentQueue<MapSection> quePartMoveSections = new ConcurrentQueue<MapSection>();
        private ConcurrentQueue<MapSection> queNeedReserveSections = new ConcurrentQueue<MapSection>();
        public ConcurrentQueue<MapSection> queReserveOkSections = new ConcurrentQueue<MapSection>();
        public bool IsAskReservePause { get; private set; } = false;
        private MapPosition lastReportPosition { get; set; } = new MapPosition();

        private Thread thdSchedule;
        public bool IsSchedulePause { get; set; } = false;
        public ConcurrentQueue<ScheduleWrapper> PrimarySendQueue { get; set; } = new ConcurrentQueue<ScheduleWrapper>();
        public ConcurrentQueue<ScheduleWrapper> SecondarySendQueue { get; set; } = new ConcurrentQueue<ScheduleWrapper>();
        public ConcurrentQueue<TcpIpEventArgs> PrimaryReceiveQueue { get; set; } = new ConcurrentQueue<TcpIpEventArgs>();
        public ConcurrentQueue<TcpIpEventArgs> SecondaryReceiveQueue { get; set; } = new ConcurrentQueue<TcpIpEventArgs>();
        private Thread thdSendWaitSchedule;
        public bool IsSendWaitSchedulePause { get; set; } = false;
        public ConcurrentQueue<ScheduleWrapper> PrimarySendWaitQueue { get; set; } = new ConcurrentQueue<ScheduleWrapper>();
        public bool IsCheckingReserveOkSections { get; set; } = false;
        public bool IsSleepByAskReserveFail { get; set; } = false;
        public TcpIpAgent ClientAgent { get; private set; }
        public string AgvcConnectorAbnormalMsg { get; set; } = "";
        public bool IsAgvcReplyBcrRead { get; set; } = false;

        public AgvcConnector(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            Vehicle.AgvcConnectorConfig = Vehicle.AgvcConnectorConfig;

            CreatTcpIpClientAgent();
            if (!Vehicle.MainFlowConfig.IsSimulation)
            {
                Connect();
            }
            InitialThreads();
        }

        #region Initial
        public void InitialThreads()
        {
            thdSchedule = new Thread(Schedule);
            thdSchedule.IsBackground = true;
            thdSchedule.Start();

            thdSendWaitSchedule = new Thread(SendWaitSchedule);
            thdSendWaitSchedule.IsBackground = true;
            thdSendWaitSchedule.Start();
        }
        public void CreatTcpIpClientAgent()
        {
            IDecodReceiveRawData RawDataDecoder = new DecodeRawData_Google(unPackWrapperMsg);

            int clientNum = Vehicle.AgvcConnectorConfig.ClientNum;
            string clientName = Vehicle.AgvcConnectorConfig.ClientName;
            string sRemoteIP = Vehicle.AgvcConnectorConfig.RemoteIp;
            int iRemotePort = Vehicle.AgvcConnectorConfig.RemotePort;
            string sLocalIP = Vehicle.AgvcConnectorConfig.LocalIp;
            int iLocalPort = Vehicle.AgvcConnectorConfig.LocalPort;

            int recv_timeout_ms = Vehicle.AgvcConnectorConfig.RecvTimeoutMs; //等待sendRecv Reply的Time out時間(milliseconds)
            int send_timeout_ms = Vehicle.AgvcConnectorConfig.SendTimeoutMs; //暫時無用
            int max_readSize = Vehicle.AgvcConnectorConfig.MaxReadSize; //暫時無用
            int reconnection_interval_ms = Vehicle.AgvcConnectorConfig.ReconnectionIntervalMs; // Dis-Connect 多久之後再進行一次嘗試恢復連線的動作
            int max_reconnection_count = Vehicle.AgvcConnectorConfig.MaxReconnectionCount; // Dis-Connect 後最多嘗試幾次重新恢復連線 (若設定為0則不進行自動重新連線)
            int retry_count = Vehicle.AgvcConnectorConfig.RetryCount; //SendRecv Time out後要再重複發送的次數

            try
            {
                ClientAgent = new TcpIpAgent(clientNum, clientName, sLocalIP, iLocalPort, sRemoteIP, iRemotePort, TcpIpAgent.TCPIP_AGENT_COMM_MODE.CLINET_MODE, recv_timeout_ms, send_timeout_ms, max_readSize, reconnection_interval_ms, max_reconnection_count, retry_count, AppConstants.FrameBuilderType.PC_TYPE_MIRLE);
                ClientAgent.injectDecoder(RawDataDecoder);

                EventInitial();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private static Google.Protobuf.IMessage unPackWrapperMsg(byte[] raw_data)
        {
            WrapperMessage WarpperMsg = ToObject<WrapperMessage>(raw_data);
            return WarpperMsg;
        }
        private static T ToObject<T>(byte[] buf) where T : Google.Protobuf.IMessage<T>, new()
        {
            if (buf == null)
                return default(T);

            Google.Protobuf.MessageParser<T> parser = new Google.Protobuf.MessageParser<T>(() => new T());
            return parser.ParseFrom(buf);
        }
        public bool IsClientAgentNull() => ClientAgent == null;
        public void ReConnect()
        {
            try
            {
                DisConnect();

                Connect();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void DisConnect()
        {
            try
            {
                if (ClientAgent != null)
                {
                    string msg = $"AgvcConnector : Disconnect Stop, [IsNull={IsClientAgentNull()}][IsConnect={IsConnected()}]";
                    LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

                    //ClientAgent.stop();//dabid- 200822
                    //ClientAgent = null;
                }
                else
                {
                    string msg = $"ClientAgent is null cannot disconnect";
                    LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Connect()
        {
            if (ClientAgent != null)
            {
                if (!ClientAgent.IsConnection)
                {
                    //Task.Run(() => ClientAgent.clientConnection());
                    Task.Run(() => ClientAgent.start());
                }
                else
                {
                    string msg = $"Already connect cannot connect again.";
                    LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                }
            }
            else
            {
                CreatTcpIpClientAgent();
                Connect();
                string msg = $"ClientAgent is null cannot connect.";
                LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }

        }
        public void StopClientAgent()
        {
            if (ClientAgent != null)
            {
                if (ClientAgent.IsConnection)
                {
                    //Task.Run(() => ClientAgent.stop());//dabid- 200822
                }
            }
        }
        protected void ClientAgent_OnConnectionChangeEvent(object sender, TcpIpEventArgs e)
        {
            TcpIpAgent agent = sender as TcpIpAgent;
            Vehicle.IsAgvcConnect = agent.IsConnection;
            if (!Vehicle.IsAgvcConnect)
            {
                mainFlowHandler.AgvcDisconnected();
                //mainFlowHandler.SetAlarmFromAgvm(56);
            }
            var isConnect = agent.IsConnection ? " Connect " : " Dis-Connect ";
            mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"AgvcConnector : {agent.Name},AgvcConnection = {isConnect}");
        }
        private void EventInitial()
        {
            foreach (var item in Enum.GetValues(typeof(EnumCmdNum)))
            {
                ClientAgent.addTcpIpReceivedHandler((int)item, LogRecvMsg);
                ClientAgent.addTcpIpReceivedHandler((int)item, RecieveCommandMediator);
            }

            ClientAgent.addTcpIpConnectedHandler(ClientAgent_OnConnectionChangeEvent); //連線時的通知
            ClientAgent.addTcpIpDisconnectedHandler(ClientAgent_OnConnectionChangeEvent); // Dis-Connect 時的通知
        }
        private void LogRecvMsg(object sender, TcpIpEventArgs e)
        {
            string msg = $"[RECV] [SeqNum = {e.iSeqNum}][{e.iPacketID}][{(EnumCmdNum)int.Parse(e.iPacketID)}][ObjPacket = {e.objPacket}]";
            OnCmdReceiveEvent?.Invoke(this, msg);
            LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
        }
        private void RecieveCommandMediator(object sender, TcpIpEventArgs e)
        {
            EnumCmdNum cmdNum = (EnumCmdNum)int.Parse(e.iPacketID);

            if (Vehicle.AutoState != EnumAutoState.Auto && !IsApplyOnly(cmdNum))
            {
                var msg = $"AgvcConnector : Manual mode, Ignore Agvc Command";
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                return;
            }

            switch (cmdNum)
            {
                case EnumCmdNum.Cmd000_EmptyCommand:
                    break;
                case EnumCmdNum.Cmd11_CouplerInfoReport:
                    PrimaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd31_TransferRequest:
                    PrimaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd38_GuideInfoResponse:
                    SecondaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                    PrimaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd37_TransferCancelRequest:
                    PrimaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd39_PauseRequest:
                    PrimaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd43_StatusRequest:
                    PrimaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd51_AvoidRequest:
                    PrimaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                    SecondaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd91_AlarmResetRequest:
                    PrimaryReceiveQueue.Enqueue(e);
                    break;
                case EnumCmdNum.Cmd94_AlarmResponse:
                    SecondaryReceiveQueue.Enqueue(e);
                    break;
                default:
                    break;
            }
        }
        private bool IsApplyOnly(EnumCmdNum cmdNum)
        {
            switch (cmdNum)
            {
                case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                    return true;
                case EnumCmdNum.Cmd43_StatusRequest:
                    return true;
                case EnumCmdNum.Cmd91_AlarmResetRequest:
                    return true;
                default:
                    break;
            }

            return false;
        }
        private void LogSendMsg(WrapperMessage wrapper)
        {
            try
            {
                string msg = $"[SEND] [SeqNum = {wrapper.SeqNum}][{wrapper.ID}][{(EnumCmdNum)wrapper.ID}] {wrapper}";
                OnCmdSendEvent?.Invoke(this, msg);
                LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Thd Schedule

        private void Schedule()
        {
            while (true)
            {
                try
                {
                    //if (IsSchedulePause)
                    //{
                    //    SpinWait.SpinUntil(() => !IsSchedulePause, Vehicle.AgvcConnectorConfig.ScheduleIntervalMs);

                    //    continue;
                    //}

                    if (Vehicle.IsAgvcConnect)
                    {
                        if (PrimarySendQueue.Any())
                        {
                            PrimarySendQueue.TryDequeue(out ScheduleWrapper scheduleWrapper);
                            PrimarySend(ref scheduleWrapper);
                        }

                        if (SecondarySendQueue.Any())
                        {
                            SecondarySendQueue.TryDequeue(out ScheduleWrapper scheduleWrapper);
                            SecondarySend(ref scheduleWrapper);
                        }

                        if (PrimaryReceiveQueue.Any())
                        {
                            PrimaryReceiveQueue.TryDequeue(out TcpIpEventArgs tcpIpEventArgs);
                            DealPrimaryReceived(tcpIpEventArgs);
                        }

                        if (SecondaryReceiveQueue.Any())
                        {
                            SecondaryReceiveQueue.TryDequeue(out TcpIpEventArgs tcpIpEventArgs);
                            DealSecondaryReceived(tcpIpEventArgs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                SpinWait.SpinUntil(()=>false,Vehicle.AgvcConnectorConfig.ScheduleIntervalMs);
            }
        }
        private void PrimarySend(ref ScheduleWrapper scheduleWrapper)
        {
            try
            {
                LogSendMsg(scheduleWrapper.Wrapper);

                ClientAgent.TrxTcpIp.SendGoogleMsg(scheduleWrapper.Wrapper, false);
            }
            catch (System.Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void SecondarySend(ref ScheduleWrapper scheduleWrapper)
        {
            try
            {
                LogSendMsg(scheduleWrapper.Wrapper);

                ClientAgent.TrxTcpIp.SendGoogleMsg(scheduleWrapper.Wrapper, true);
            }
            catch (System.Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void DealPrimaryReceived(TcpIpEventArgs tcpIpEventArgs)
        {
            try
            {
                EnumCmdNum cmdNum = (EnumCmdNum)int.Parse(tcpIpEventArgs.iPacketID);

                switch (cmdNum)
                {
                    case EnumCmdNum.Cmd11_CouplerInfoReport:
                        Receive_Cmd11_CouplerInfoReport(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd31_TransferRequest:
                        Receive_Cmd31_TransferRequest(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                        Receive_Cmd35_CarrierIdRenameRequest(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd37_TransferCancelRequest:
                        Receive_Cmd37_TransferCancelRequest(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd39_PauseRequest:
                        Receive_Cmd39_PauseRequest(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd43_StatusRequest:
                        Receive_Cmd43_StatusRequest(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd51_AvoidRequest:
                        Receive_Cmd51_AvoidRequest(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd91_AlarmResetRequest:
                        Receive_Cmd91_AlarmResetRequest(this, tcpIpEventArgs);
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void DealSecondaryReceived(TcpIpEventArgs tcpIpEventArgs)
        {
            try
            {
                EnumCmdNum cmdNum = (EnumCmdNum)int.Parse(tcpIpEventArgs.iPacketID);

                switch (cmdNum)
                {
                    case EnumCmdNum.Cmd38_GuideInfoResponse:
                        Receive_Cmd38_GuideInfoResponse(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                        Receive_Cmd52_AvoidCompleteResponse(this, tcpIpEventArgs);
                        break;
                    case EnumCmdNum.Cmd94_AlarmResponse:
                        Receive_Cmd94_AlarmResponse(this, tcpIpEventArgs);
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void SendWrapperToSchedule(WrapperMessage wrapper, bool isReply, bool isSendWait)
        {
            ScheduleWrapper scheduleWrapper = new ScheduleWrapper(wrapper);

            if (isSendWait)
            {
                scheduleWrapper.IsSendWait = true;
                scheduleWrapper.RetrySendWaitCounter = Vehicle.AgvcConnectorConfig.SendWaitRetryTimes;
            }

            if (isReply)
            {
                SecondarySendQueue.Enqueue(scheduleWrapper);
            }
            else
            {
                if (isSendWait)
                {
                    PrimarySendWaitQueue.Enqueue(scheduleWrapper);
                }
                else
                {
                    PrimarySendQueue.Enqueue(scheduleWrapper);
                }
            }
        }

        private void SendWaitSchedule()
        {
            while (true)
            {
                if (Vehicle.IsAgvcConnect)
                {
                    try
                    {
                        if (PrimarySendWaitQueue.Any())
                        {
                            PrimarySendWaitQueue.TryDequeue(out ScheduleWrapper scheduleWrapper);
                            PrimarySendWait(scheduleWrapper);
                        }
                    }
                    catch (Exception ex)//200828 dabid for Watch Not AskAllSectionsReserveInOnce
                    {
                        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                    }

                    try
                    {
                        if (queNeedReserveSections.Any())
                        {
                            if (!IsAskReservePause && mainFlowHandler.IsMoveStep() && mainFlowHandler.CanVehMove() && !Vehicle.AseMoveStatus.IsMoveEnd && !IsSleepByAskReserveFail)
                            {
                                AskAllSectionsReserveInOnce();
                            }
                            else
                            {
                                mainFlowHandler.LogDebug(GetType().Name, $"IsAskReservePause = {IsAskReservePause} ,IsMoveStep = {mainFlowHandler.IsMoveStep()}, CanVehMove = {mainFlowHandler.CanVehMove()},IsMoveEnd = {Vehicle.AseMoveStatus.IsMoveEnd},IsSleepByAskReserveFail = {IsSleepByAskReserveFail}");
                            }
                        }
                    }
                    catch (Exception ex)//200828 dabid for Watch Not AskAllSectionsReserveInOnce
                    {
                        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                        Vehicle.TMP_e = ex.Message;
                    }
                }

                SpinWait.SpinUntil(()=>false,Vehicle.AgvcConnectorConfig.ScheduleIntervalMs);
            }
        }

        private void PrimarySendWait(ScheduleWrapper scheduleWrapper)
        {
            try
            {
                LogSendMsg(scheduleWrapper.Wrapper);
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"PrimarySendWait : [{scheduleWrapper.Wrapper.ID}][RetrySendWaitCounter={scheduleWrapper.RetrySendWaitCounter}]");

                switch (scheduleWrapper.Wrapper.ID)
                {
                    case 136:
                        {

                            TrxTcpIp.ReturnCode returnCode = TrxTcpIp.ReturnCode.Timeout;
                            returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(scheduleWrapper.Wrapper, out ID_36_TRANS_EVENT_RESPONSE response, out string rtnMsg);

                            if (returnCode == TrxTcpIp.ReturnCode.Normal)
                            {
                                if (!Vehicle.mapTransferCommands.ContainsKey(response.CmdID.Trim())) break;
                                ReceiveSent_Cmd36_TransferEventResponse(response, scheduleWrapper.Wrapper.ImpTransEventRep);
                            }
                            else
                            {
                                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"PrimarySendWait : [{scheduleWrapper.Wrapper.ID}][RetrySendWaitCounter={scheduleWrapper.RetrySendWaitCounter}]");

                                if (scheduleWrapper.RetrySendWaitCounter <= 0)
                                {
                                    OnSendRecvTimeoutEvent?.Invoke(this, default(EventArgs));
                                }
                                else
                                {
                                    scheduleWrapper.RetrySendWaitCounter--;
                                    PrimarySendWaitQueue.Enqueue(scheduleWrapper);
                                    mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"PrimarySendWait : [{scheduleWrapper.Wrapper.ID}][RetrySendWaitCounter={scheduleWrapper.RetrySendWaitCounter}][Back To SendWaitQueue]");

                                }
                            }
                        }
                        break;
                    case 132:
                        {
                            TrxTcpIp.ReturnCode returnCode = TrxTcpIp.ReturnCode.Timeout;
                            returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(scheduleWrapper.Wrapper, out ID_32_TRANS_COMPLETE_RESPONSE response, out string rtnMsg);
                            if (returnCode == TrxTcpIp.ReturnCode.Normal)
                            {
                                ReceiveSent_Cmd32_TransCompleteResponse(response);
                            }
                            else
                            {
                                if (scheduleWrapper.RetrySendWaitCounter <= 0)
                                {
                                    OnSendRecvTimeoutEvent?.Invoke(this, default(EventArgs));
                                }
                                else
                                {
                                    scheduleWrapper.RetrySendWaitCounter--;
                                    PrimarySendWaitQueue.Enqueue(scheduleWrapper);
                                }
                            }
                        }
                        break;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ClearAllReserve()
        {
            ClearNeedReserveSections();
            ClearGotReserveOkSections();
            mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[清除.所有路權]  ClearAllReserve.");
        }
        public void AskGuideAddressesAndSections(string endAddressId)
        {
            Send_Cmd138_GuideInfoRequest(Vehicle.AseMoveStatus.LastAddress.Id, endAddressId);
        }
        public void AskAllSectionsReserveInOnce()
        {
            try
            {
                IsSleepByAskReserveFail = true;
                var needReserveSections = queNeedReserveSections.ToList();
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $@"Ask All Sections Reserve In Once [{needReserveSections.Count}]");
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);

                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = EventType.ReserveReq;
                FitReserveInfos(report.ReserveInfos, needReserveSections);
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrapper.ImpTransEventRep = report;

                #region Ask reserve and wait reply

                LogSendMsg(wrapper);

                var returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(wrapper, out ID_36_TRANS_EVENT_RESPONSE response, out string rtnMsg);

                if (returnCode == TrxTcpIp.ReturnCode.Normal)
                {
                    if (response.IsReserveSuccess == ReserveResult.Success)
                    {
                        IsCheckingReserveOkSections = true;

                        List<MapSection> reserveOkSections = new List<MapSection>();
                        var reserveInfos = response.ReserveInfos.ToList();
                        for (int i = 0; i < reserveInfos.Count; i++)
                        {
                            var reserveInfo = reserveInfos[i];
                            var needReserveSection = needReserveSections[i];
                            if (reserveInfo.ReserveSectionID.Trim() != needReserveSection.Id)
                            {
                                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Ask All Sections Reserve In Once NG. Response Section ID = [{reserveInfo.ReserveSectionID.Trim()}] unmatched Ask Section ID = [{needReserveSection.Id}].");
                                break;
                            }
                            if (reserveInfo.DriveDirction != DriveDirctionParse(needReserveSection.CmdDirection))
                            {
                                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Ask All Sections Reserve In Once NG. Response Section ID = [{needReserveSection.Id}], Direction = [{reserveInfo.DriveDirction}] unmatched Ask Section Direction = [{needReserveSection.CmdDirection}].");
                                break;
                            }

                            // ReserveOkAskNext = true;
                            IsSleepByAskReserveFail = false;
                            queNeedReserveSections.TryDequeue(out MapSection aReserveOkSection);
                            queReserveOkSections.Enqueue(needReserveSection);
                            quePartMoveSections.Enqueue(needReserveSection);

                            if (queNeedReserveSections.IsEmpty)
                            {
                                break;
                            }
                        }

                        RefreshPartMoveSections();

                        if (Vehicle.AseMovingGuide.ReserveStop == VhStopSingle.On)
                        {
                            Vehicle.AseMovingGuide.ReserveStop = VhStopSingle.Off;
                            StatusChangeReport();
                        }

                        IsCheckingReserveOkSections = false;
                    }
                    else
                    {
                        if (Vehicle.AseMovingGuide.ReserveStop == VhStopSingle.Off)
                        {
                            Vehicle.AseMovingGuide.ReserveStop = VhStopSingle.On;
                            StatusChangeReport();
                        }
                        mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Ask All Sections Reserve In Once Reply. Unsuccess.");
                    }
                }
                else
                {
                    mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[逾時] AskAllSectionsReserveInOnce send wait timeout[{Vehicle.TransferCommand.CommandId}]");
                    OnSendRecvTimeoutEvent?.Invoke(this, default(EventArgs));
                }

                if (IsSleepByAskReserveFail)
                {
                    Task.Run(() =>
                    {
                        SpinWait.SpinUntil(()=>false,Vehicle.AgvcConnectorConfig.AskReserveIntervalMs);
                        IsSleepByAskReserveFail = false;
                    });
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void FitReserveInfos(RepeatedField<ReserveInfo> reserveInfos, List<MapSection> needReserveSections)
        {
            reserveInfos.Clear();
            foreach (var needReserveSection in needReserveSections)
            {
                ReserveInfo reserveInfo = new ReserveInfo();
                reserveInfo.ReserveSectionID = needReserveSection.Id;
                if (needReserveSection.CmdDirection == EnumCommandDirection.Backward)
                {
                    reserveInfo.DriveDirction = DriveDirction.DriveDirReverse;
                }
                else if (needReserveSection.CmdDirection == EnumCommandDirection.None)
                {
                    reserveInfo.DriveDirction = DriveDirction.DriveDirNone;
                }
                else
                {
                    reserveInfo.DriveDirction = DriveDirction.DriveDirForward;
                }
                reserveInfos.Add(reserveInfo);
            }
        }

        // Thd Ask Reserve     
        public void PauseAskReserve()
        {
            IsAskReservePause = true;
            mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"AgvcConnector : PauseAskReserve");
        }
        public void ResumeAskReserve()
        {
            IsAskReservePause = false;
            mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "AgvcConnector : ResumeAskReserve");
        }
        public bool IsGotReserveOkSectionsFull()
        {
            if (Vehicle.AgvcConnectorConfig.ReserveLengthMeter < 0) return false;
            int reserveOkSectionsTotalLength = GetReserveOkSectionsTotalLength();
            return reserveOkSectionsTotalLength >= Vehicle.AgvcConnectorConfig.ReserveLengthMeter * 1000;
        }
        private string QueMapSectionsToString(ConcurrentQueue<MapSection> aQue)
        {
            try
            {
                var sectionIds = aQue.ToList().Select(x => string.IsNullOrEmpty(x.Id) ? "" : x.Id).ToList();
                return string.Concat("[", string.Join(", ", sectionIds), "]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }
        private int GetReserveOkSectionsTotalLength()
        {
            double result = 0;
            List<MapSection> reserveOkSections = new List<MapSection>(queReserveOkSections);
            foreach (var item in reserveOkSections)
            {
                result += item.HeadToTailDistance;
            }
            return (int)result;
        }
        public void ClearNeedReserveSections()
        {
            queNeedReserveSections = new ConcurrentQueue<MapSection>();
        }
        public void ClearGotReserveOkSections()
        {
            queReserveOkSections = new ConcurrentQueue<MapSection>();
        }
        public void SetupNeedReserveSections()
        {
            try
            {
                queNeedReserveSections = new ConcurrentQueue<MapSection>(Vehicle.AseMovingGuide.MovingSections);
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[設定 所需路權] SetupNeedReserveSections[{QueMapSectionsToString(queNeedReserveSections)}][Count={queNeedReserveSections.Count}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public List<MapSection> GetNeedReserveSections()
        {
            return queNeedReserveSections.ToList();
        }
        public List<MapSection> GetReserveOkSections()
        {
            return new List<MapSection>(queReserveOkSections);
        }
        public void DequeueGotReserveOkSections()
        {
            if (queReserveOkSections.IsEmpty)
            {
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"AgvcConnector :queReserveOkSections.Count=[{queReserveOkSections.Count}], Dequeue Got Reserve Ok Sections Fail.");
                return;
            }
            else
            {
                queReserveOkSections.TryDequeue(out MapSection passSection);
                string passSectionId = passSection.Id;
                OnPassReserveSectionEvent?.Invoke(this, passSectionId);
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"AgvcConnector : passSectionId = [{passSectionId}].");
            }
        }
        private void RefreshPartMoveSections()
        {
            try
            {
                if (quePartMoveSections.Any())
                {
                    List<MapSection> partMoveSections = quePartMoveSections.ToList();
                    for (int i = 0; i < partMoveSections.Count; i++)
                    {
                        EnumIsExecute enumKeepOrGo = EnumIsExecute.Keep;
                        // End of PartMoveSections => Go
                        if (i + 1 == partMoveSections.Count)
                        {
                            enumKeepOrGo = EnumIsExecute.Go;

                        } //partMoveSections[i] and partMoveSections[i+1] exist
                        else if (HeadDirectionChange(partMoveSections[i + 1]))
                        {
                            enumKeepOrGo = EnumIsExecute.Go;
                        }
                        else if (partMoveSections[i].Type != partMoveSections[i + 1].Type) //Section Type Change => Go
                        {
                            enumKeepOrGo = EnumIsExecute.Go;
                        }

                        mainFlowHandler.AgvcConnector_GetReserveOkUpdateMoveControlNextPartMovePosition(partMoveSections[i], enumKeepOrGo);
                        SpinWait.SpinUntil(() => false, 50);
                    }
                    quePartMoveSections = new ConcurrentQueue<MapSection>();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private bool HeadDirectionChange(MapSection mapSection)
        {
            return mapSection.HeadAddress.VehicleHeadAngle != mapSection.TailAddress.VehicleHeadAngle;
        }


        #endregion

        public void SendAgvcConnectorFormCommands(int cmdNum, Dictionary<string, string> pairs)
        {
            try
            {
                WrapperMessage wrapper = new WrapperMessage();

                var cmdType = (EnumCmdNum)cmdNum;
                switch (cmdType)
                {
                    case EnumCmdNum.Cmd31_TransferRequest:
                        {
                            ID_31_TRANS_REQUEST aCmd = new ID_31_TRANS_REQUEST();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.DestinationAdr = pairs["DestinationAdr"];
                            aCmd.LoadAdr = pairs["LoadAdr"];
                            wrapper.ID = WrapperMessage.TransReqFieldNumber;
                            wrapper.TransReq = aCmd;
                            break;
                        }
                    case EnumCmdNum.Cmd32_TransferCompleteResponse:
                        {
                            ID_32_TRANS_COMPLETE_RESPONSE aCmd = new ID_32_TRANS_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.TranCmpRespFieldNumber;
                            wrapper.TranCmpResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                        {
                            ID_35_CST_ID_RENAME_REQUEST aCmd = new ID_35_CST_ID_RENAME_REQUEST();
                            aCmd.NEWCSTID = pairs["NEWCSTID"];
                            aCmd.OLDCSTID = pairs["OLDCSTID"];

                            wrapper.ID = WrapperMessage.CSTIDRenameReqFieldNumber;
                            wrapper.CSTIDRenameReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd36_TransferEventResponse:
                        {
                            ID_36_TRANS_EVENT_RESPONSE aCmd = new ID_36_TRANS_EVENT_RESPONSE();
                            aCmd.IsBlockPass = PassTypeParse(pairs["IsBlockPass"]);
                            aCmd.IsReserveSuccess = ReserveResultParse(pairs["IsReserveSuccess"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.ImpTransEventRespFieldNumber;
                            wrapper.ImpTransEventResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd37_TransferCancelRequest:
                        {
                            ID_37_TRANS_CANCEL_REQUEST aCmd = new ID_37_TRANS_CANCEL_REQUEST();
                            aCmd.CmdID = pairs["CmdID"];

                            wrapper.ID = WrapperMessage.TransCancelReqFieldNumber;
                            wrapper.TransCancelReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd39_PauseRequest:
                        {
                            ID_39_PAUSE_REQUEST aCmd = new ID_39_PAUSE_REQUEST();
                            aCmd.EventType = PauseEventParse(pairs["EventType"]);
                            aCmd.PauseType = PauseTypeParse(pairs["PauseType"]);

                            wrapper.ID = WrapperMessage.PauseReqFieldNumber;
                            wrapper.PauseReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd41_ModeChange:
                        {
                            ID_41_MODE_CHANGE_REQ aCmd = new ID_41_MODE_CHANGE_REQ();
                            aCmd.OperatingVHMode = OperatingVHModeParse(pairs["OperatingVHMode"]);

                            wrapper.ID = WrapperMessage.ModeChangeReqFieldNumber;
                            wrapper.ModeChangeReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd43_StatusRequest:
                        {
                            ID_43_STATUS_REQUEST aCmd = new ID_43_STATUS_REQUEST();

                            wrapper.ID = WrapperMessage.StatusReqFieldNumber;
                            wrapper.StatusReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd44_StatusRequest:
                        {
                            ID_44_STATUS_CHANGE_RESPONSE aCmd = new ID_44_STATUS_CHANGE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.StatusChangeRespFieldNumber;
                            wrapper.StatusChangeResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd45_PowerOnoffRequest:
                        {
                            ID_45_POWER_OPE_REQ aCmd = new ID_45_POWER_OPE_REQ();
                            aCmd.OperatingPowerMode = OperatingPowerModeParse(pairs["OperatingPowerMode"]);

                            wrapper.ID = WrapperMessage.PowerOpeReqFieldNumber;
                            wrapper.PowerOpeReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd51_AvoidRequest:
                        {
                            ID_51_AVOID_REQUEST aCmd = new ID_51_AVOID_REQUEST();
                            aCmd.GuideAddresses.AddRange(StringSpilter(pairs["GuideAddresses"]));
                            aCmd.GuideSections.AddRange(StringSpilter(pairs["GuideSections"]));

                            wrapper.ID = WrapperMessage.AvoidReqFieldNumber;
                            wrapper.AvoidReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                        {
                            ID_52_AVOID_COMPLETE_RESPONSE aCmd = new ID_52_AVOID_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.AvoidCompleteRespFieldNumber;
                            wrapper.AvoidCompleteResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd71_RangeTeachRequest:
                        {
                            ID_71_RANGE_TEACHING_REQUEST aCmd = new ID_71_RANGE_TEACHING_REQUEST();
                            aCmd.FromAdr = pairs["FromAdr"];
                            aCmd.ToAdr = pairs["ToAdr"];

                            wrapper.ID = WrapperMessage.RangeTeachingReqFieldNumber;
                            wrapper.RangeTeachingReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd72_RangeTeachCompleteResponse:
                        {
                            ID_72_RANGE_TEACHING_COMPLETE_RESPONSE aCmd = new ID_72_RANGE_TEACHING_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.RangeTeachingCmpRespFieldNumber;
                            wrapper.RangeTeachingCmpResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd74_AddressTeachResponse:
                        {
                            ID_74_ADDRESS_TEACH_RESPONSE aCmd = new ID_74_ADDRESS_TEACH_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.AddressTeachRespFieldNumber;
                            wrapper.AddressTeachResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd91_AlarmResetRequest:
                        {
                            ID_91_ALARM_RESET_REQUEST aCmd = new ID_91_ALARM_RESET_REQUEST();

                            wrapper.ID = WrapperMessage.AlarmResetReqFieldNumber;
                            wrapper.AlarmResetReq = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd94_AlarmResponse:
                        {
                            ID_94_ALARM_RESPONSE aCmd = new ID_94_ALARM_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            break;
                        }
                    case EnumCmdNum.Cmd131_TransferResponse:
                        {
                            ID_131_TRANS_RESPONSE aCmd = new ID_131_TRANS_RESPONSE();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.NgReason = pairs["NgReason"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.TransRespFieldNumber;
                            wrapper.TransResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd132_TransferCompleteReport:
                        {
                            ID_132_TRANS_COMPLETE_REPORT aCmd = new ID_132_TRANS_COMPLETE_REPORT();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.CmdDistance = int.Parse(pairs["CmdDistance"]);
                            aCmd.CmdPowerConsume = uint.Parse(pairs["CmdPowerConsume"]);
                            aCmd.CmpStatus = CompleteStatusParse(pairs["CmpStatus"]);
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];

                            wrapper.ID = WrapperMessage.TranCmpRepFieldNumber;
                            wrapper.TranCmpRep = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd134_TransferEventReport:
                        {
                            ID_134_TRANS_EVENT_REP aCmd = new ID_134_TRANS_EVENT_REP();
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeParse(pairs["EventType"]);
                            aCmd.DrivingDirection = (DriveDirction)Enum.Parse(typeof(DriveDirction), pairs["DrivingDirection"].Trim());

                            wrapper.ID = WrapperMessage.TransEventRepFieldNumber;
                            wrapper.TransEventRep = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd135_CarrierIdRenameResponse:
                        {
                            ID_135_CST_ID_RENAME_RESPONSE aCmd = new ID_135_CST_ID_RENAME_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.CSTIDRenameRespFieldNumber;
                            wrapper.CSTIDRenameResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd136_TransferEventReport:
                        {
                            ID_136_TRANS_EVENT_REP aCmd = new ID_136_TRANS_EVENT_REP();
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeParse(pairs["EventType"]);

                            wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                            wrapper.ImpTransEventRep = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd137_TransferCancelResponse:
                        {
                            ID_137_TRANS_CANCEL_RESPONSE aCmd = new ID_137_TRANS_CANCEL_RESPONSE();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.TransCancelRespFieldNumber;
                            wrapper.TransCancelResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd139_PauseResponse:
                        {
                            ID_139_PAUSE_RESPONSE aCmd = new ID_139_PAUSE_RESPONSE();
                            aCmd.EventType = PauseEventParse(pairs["EventType"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.PauseRespFieldNumber;
                            wrapper.PauseResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd141_ModeChangeResponse:
                        {
                            ID_141_MODE_CHANGE_RESPONSE aCmd = new ID_141_MODE_CHANGE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.ModeChangeRespFieldNumber;
                            wrapper.ModeChangeResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd143_StatusResponse:
                        {
                            //TODO: 補完屬性
                            ID_143_STATUS_RESPONSE aCmd = new ID_143_STATUS_RESPONSE();
                            aCmd.ActionStatus = VHActionStatusParse(pairs["ActionStatus"]);
                            aCmd.BatteryCapacity = uint.Parse(pairs["BatteryCapacity"]);
                            aCmd.BatteryTemperature = int.Parse(pairs["BatteryTemperature"]);
                            aCmd.BlockingStatus = VhStopSingleParse(pairs["BlockingStatus"]);
                            aCmd.ChargeStatus = VhChargeStatusParse(pairs["ChargeStatus"]);
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];

                            wrapper.ID = WrapperMessage.StatusReqRespFieldNumber;
                            wrapper.StatusReqResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd144_StatusReport:
                        {
                            //TODO: 補完屬性
                            ID_144_STATUS_CHANGE_REP aCmd = new ID_144_STATUS_CHANGE_REP();
                            aCmd.ActionStatus = VHActionStatusParse(pairs["ActionStatus"]);
                            aCmd.BatteryCapacity = uint.Parse(pairs["BatteryCapacity"]);
                            aCmd.BatteryTemperature = int.Parse(pairs["BatteryTemperature"]);
                            aCmd.BlockingStatus = VhStopSingleParse(pairs["BlockingStatus"]);
                            aCmd.ChargeStatus = VhChargeStatusParse(pairs["ChargeStatus"]);

                            wrapper.ID = WrapperMessage.StatueChangeRepFieldNumber;
                            wrapper.StatueChangeRep = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd151_AvoidResponse:
                        {
                            ID_151_AVOID_RESPONSE aCmd = new ID_151_AVOID_RESPONSE();
                            aCmd.NgReason = pairs["NgReason"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.AvoidRespFieldNumber;
                            wrapper.AvoidResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd152_AvoidCompleteReport:
                        {
                            ID_152_AVOID_COMPLETE_REPORT aCmd = new ID_152_AVOID_COMPLETE_REPORT();
                            aCmd.CmpStatus = int.Parse(pairs["CmpStatus"]);

                            wrapper.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                            wrapper.AvoidCompleteRep = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd171_RangeTeachResponse:
                        {
                            ID_171_RANGE_TEACHING_RESPONSE aCmd = new ID_171_RANGE_TEACHING_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.RangeTeachingRespFieldNumber;
                            wrapper.RangeTeachingResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd172_RangeTeachCompleteReport:
                        {
                            ID_172_RANGE_TEACHING_COMPLETE_REPORT aCmd = new ID_172_RANGE_TEACHING_COMPLETE_REPORT();
                            aCmd.CompleteCode = int.Parse(pairs["CompleteCode"]);
                            aCmd.FromAdr = pairs["FromAdr"];
                            aCmd.SecDistance = uint.Parse(pairs["SecDistance"]);
                            aCmd.ToAdr = pairs["ToAdr"];

                            wrapper.ID = WrapperMessage.RangeTeachingCmpRepFieldNumber;
                            wrapper.RangeTeachingCmpRep = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd174_AddressTeachReport:
                        {
                            ID_174_ADDRESS_TEACH_REPORT aCmd = new ID_174_ADDRESS_TEACH_REPORT();
                            aCmd.Addr = pairs["Addr"];
                            aCmd.Position = int.Parse(pairs["Position"]);

                            wrapper.ID = WrapperMessage.AddressTeachRepFieldNumber;
                            wrapper.AddressTeachRep = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd191_AlarmResetResponse:
                        {
                            ID_191_ALARM_RESET_RESPONSE aCmd = new ID_191_ALARM_RESET_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrapper.ID = WrapperMessage.AlarmResetRespFieldNumber;
                            wrapper.AlarmResetResp = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd194_AlarmReport:
                        {
                            ID_194_ALARM_REPORT aCmd = new ID_194_ALARM_REPORT();
                            aCmd.ErrCode = pairs["ErrCode"];
                            aCmd.ErrDescription = pairs["ErrDescription"];
                            aCmd.ErrStatus = ErrorStatusParse(pairs["ErrStatus"]);

                            wrapper.ID = WrapperMessage.AlarmRepFieldNumber;
                            wrapper.AlarmRep = aCmd;

                            break;
                        }
                    case EnumCmdNum.Cmd000_EmptyCommand:
                    default:
                        {
                            ID_1_HOST_BASIC_INFO_VERSION_REP aCmd = new ID_1_HOST_BASIC_INFO_VERSION_REP();

                            wrapper.ID = WrapperMessage.HostBasicInfoRepFieldNumber;
                            wrapper.HostBasicInfoRep = aCmd;

                            break;
                        }
                }

                SendWrapperToSchedule(wrapper, false, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string[] StringSpilter(string v)
        {
            v = v.Trim(new char[] { ' ', '[', ']' });
            if (string.IsNullOrEmpty(v))
            {
                return new string[1] { " " };
            }
            return v.Split(',');
        }

        public void AseBatteryControl_OnBatteryPercentageChangeEvent(object sender, double batteryPercentage)
        {
            BatteryPercentageChangeReport(MethodBase.GetCurrentMethod().Name, (ushort)batteryPercentage);
        }

        private void BatteryPercentageChangeReport(string sender, ushort batteryPercentage)
        {
            Send_Cmd144_StatusChangeReport(sender, batteryPercentage);
        }

        public void SetlAlarmToAgvc(int errorCode, bool isAlarm)
        {
            Send_Cmd194_AlarmReport(errorCode.ToString(), ErrorStatus.ErrSet);
            if (Vehicle.ErrorStatus == VhStopSingle.Off && isAlarm)
            {
                Vehicle.ErrorStatus = VhStopSingle.On;
                StatusChangeReport();
            }
        }
        public void ResetAllAlarmsToAgvc()
        {
            if (Vehicle.ErrorStatus == VhStopSingle.On)
            {
                Vehicle.ErrorStatus = VhStopSingle.Off;
                StatusChangeReport();
            }
            Send_Cmd194_AlarmReport("0", ErrorStatus.ErrReset);
        }

        #region Public Functions

        public void ReportSectionPass()
        {
            Send_Cmd134_TransferEventReport(EventType.AdrPass);
        }
        public void ReportAddressPass()
        {
            if (!IsNeerlyNoMove())
            {
                Send_Cmd134_TransferEventReport(EventType.AdrPass);
            }
        }
        private bool IsNeerlyNoMove()
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
            var lastPosition = aseMoveStatus.LastMapPosition;
            if (Math.Abs(lastPosition.X - lastReportPosition.X) <= Vehicle.AgvcConnectorConfig.NeerlyNoMoveRangeMm && Math.Abs(lastPosition.Y - lastReportPosition.Y) <= Vehicle.AgvcConnectorConfig.NeerlyNoMoveRangeMm)
            {
                return true;
            }
            else
            {
                lastReportPosition = lastPosition;
                return false;
            }
        }
        public void ReportLoadArrival()
        {
            SendRecv_Cmd136_TransferEventReport(EventType.LoadArrivals, Vehicle.TransferCommand.LoadPortId);
        }
        public void Loading()
        {
            Send_Cmd136_TransferEventReport(EventType.Vhloading, Vehicle.TransferCommand.CommandId, Vehicle.TransferCommand.SlotNumber, Vehicle.TransferCommand.LoadPortId);
        }
        public void TransferComplete(AgvcTransferCommand transferCommand)
        {
            SendRecv_Cmd132_TransferCompleteReport(transferCommand, (int)EnumAgvcReplyCode.Accept);
        }
        public void LoadComplete()
        {
            StatusChangeReport();
            SendRecv_Cmd136_TransferEventReport(EventType.LoadComplete, Vehicle.TransferCommand.LoadPortId);
        }
        public void ReportUnloadArrival()
        {
            SendRecv_Cmd136_TransferEventReport(EventType.UnloadArrivals, Vehicle.TransferCommand.UnloadPortId);
        }
        public void Unloading()
        {
            Send_Cmd136_TransferEventReport(EventType.Vhunloading, Vehicle.TransferCommand.CommandId, Vehicle.TransferCommand.SlotNumber, Vehicle.TransferCommand.UnloadPortId);
        }
        public void UnloadComplete()
        {
            StatusChangeReport();
            SendRecv_Cmd136_TransferEventReport(EventType.UnloadComplete, Vehicle.TransferCommand.UnloadPortId);
        }
        public void MoveArrival()
        {
            Send_Cmd134_TransferEventReport(EventType.AdrOrMoveArrivals);
        }
        public void AvoidComplete()
        {
            Send_Cmd152_AvoidCompleteReport(0);
        }
        public void AvoidFail()
        {
            Send_Cmd152_AvoidCompleteReport(1);
        }
        public void NoCommand()
        {
            Vehicle.ActionStatus = VHActionStatus.NoCommand;
            StatusChangeReport();
        }
        public void Commanding()
        {
            Vehicle.ActionStatus = VHActionStatus.Commanding;
            StatusChangeReport();
        }
        public void ReplyTransferCommand(string cmdId, CommandActionType type, ushort seqNum, int replyCode, string reason)
        {
            Send_Cmd131_TransferResponse(cmdId, type, seqNum, replyCode, reason);
        }
        public void ReplyAvoidCommand(ushort seqNum, int replyCode, string reason)
        {
            Send_Cmd151_AvoidResponse(seqNum, replyCode, reason);
        }
        public void ChargHandshaking()
        {
            Vehicle.ChargeStatus = VhChargeStatus.ChargeStatusHandshaking;
            StatusChangeReport();
        }
        public void Charging()
        {
            Vehicle.ChargeStatus = VhChargeStatus.ChargeStatusCharging;
            StatusChangeReport();
        }
        public void ChargeOff()
        {
            Vehicle.ChargeStatus = VhChargeStatus.ChargeStatusNone;
            StatusChangeReport();
        }
        public void PauseReply(ushort seqNum, int replyCode, PauseEvent type)
        {
            Send_Cmd139_PauseResponse(seqNum, replyCode, type);
        }
        public void StatusChangeReport()
        {
            Send_Cmd144_StatusChangeReport();
        }
        public void CSTStatusReport()
        {
            if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty)
            {
                Send_Cmd136_CstRemove(EnumSlotNumber.L);
            }
            else
            {
                Send_Cmd136_CstIdReadReport(EnumSlotNumber.L); //200625 dabid+
            }
            SpinWait.SpinUntil(()=>false,50);
            if (Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty)
            {
                Send_Cmd136_CstRemove(EnumSlotNumber.R);
            }
            else
            {
                Send_Cmd136_CstIdReadReport(EnumSlotNumber.R); //200625 dabid+
            }
        }
        private void ShowTransferCmdToForm(AgvcTransferCommand transferCommand)
        {
            mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"\r\n Get {transferCommand.AgvcTransCommandType},\r\n Load Adr={transferCommand.LoadAddressId}, Load Port Id={transferCommand.LoadPortId},\r\n Unload Adr={transferCommand.UnloadAddressId}, Unload Port Id={transferCommand.UnloadPortId}.");
        }
        public bool IsConnected() => ClientAgent == null ? false : ClientAgent.IsConnection;

        #endregion

        #region Send_Or_Receive_CmdNum
        public void Receive_Cmd94_AlarmResponse(object sender, TcpIpEventArgs e)
        {
            ID_94_ALARM_RESPONSE receive = (ID_94_ALARM_RESPONSE)e.objPacket;
        }
        public void Send_Cmd194_AlarmReport(string alarmCode, ErrorStatus status)
        {
            try
            {
                ID_194_ALARM_REPORT report = new ID_194_ALARM_REPORT();
                report.ErrCode = alarmCode;
                report.ErrStatus = status;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.AlarmRepFieldNumber;
                wrapper.AlarmRep = report;

                SendWrapperToSchedule(wrapper, false, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void Receive_Cmd91_AlarmResetRequest(object sender, TcpIpEventArgs e)
        {
            ID_91_ALARM_RESET_REQUEST receive = (ID_91_ALARM_RESET_REQUEST)e.objPacket;

            mainFlowHandler.ResetAllAlarmsFromAgvc();

            int replyCode = 0;
            Send_Cmd191_AlarmResetResponse(e.iSeqNum, replyCode);

            Vehicle.ErrorStatus = VhStopSingle.Off;
            StatusChangeReport();
        }

        public void Send_Cmd191_AlarmResetResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_191_ALARM_RESET_RESPONSE response = new ID_191_ALARM_RESET_RESPONSE();
                response.ReplyCode = replyCode;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.AlarmResetRespFieldNumber;
                wrapper.SeqNum = seqNum;
                wrapper.AlarmResetResp = response;

                SendWrapperToSchedule(wrapper, true, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void Receive_Cmd52_AvoidCompleteResponse(object sender, TcpIpEventArgs e)
        {
            ID_52_AVOID_COMPLETE_RESPONSE receive = (ID_52_AVOID_COMPLETE_RESPONSE)e.objPacket;
            if (receive.ReplyCode != 0)
            {
                //Alarm and Log
            }
        }
        public void Send_Cmd152_AvoidCompleteReport(int completeStatus)
        {
            try
            {
                ID_152_AVOID_COMPLETE_REPORT report = new ID_152_AVOID_COMPLETE_REPORT();
                report.CmpStatus = completeStatus;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                wrapper.AvoidCompleteRep = report;

                SendWrapperToSchedule(wrapper, false, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void Receive_Cmd51_AvoidRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                if (IsCheckingReserveOkSections)
                {
                    Send_Cmd151_AvoidResponse(e.iSeqNum, 1, "Vehicle is checking reserve-Ok sections, can not do avoid request.");
                    return;
                }
                ID_51_AVOID_REQUEST receive = (ID_51_AVOID_REQUEST)e.objPacket;
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $" Get Avoid Command");
                AseMovingGuide aseMovingGuide = new AseMovingGuide(receive, e.iSeqNum);
                ShowAvoidRequestToForm(aseMovingGuide);
                OnAvoideRequestEvent?.Invoke(this, aseMovingGuide);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void ShowAvoidRequestToForm(AseMovingGuide aseMovingGuide)
        {
            try
            {
                var msg = $" Get Avoid Command ,Avoid End Adr={aseMovingGuide.ToAddressId}.";

                msg += Environment.NewLine + "Avoid Section ID:";
                foreach (var secId in aseMovingGuide.GuideSectionIds)
                {
                    msg += $"({secId})";
                }
                msg += Environment.NewLine + "Avoid Address ID:";
                foreach (var adrId in aseMovingGuide.GuideAddressIds)
                {
                    msg += $"({adrId})";
                }
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd151_AvoidResponse(ushort seqNum, int replyCode, string reason)
        {
            try
            {
                ID_151_AVOID_RESPONSE iD_151_AVOID_RESPONSE = new ID_151_AVOID_RESPONSE();
                iD_151_AVOID_RESPONSE.ReplyCode = replyCode;
                iD_151_AVOID_RESPONSE.NgReason = reason;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.AvoidRespFieldNumber;
                wrapper.SeqNum = seqNum;
                wrapper.AvoidResp = iD_151_AVOID_RESPONSE;

                SendWrapperToSchedule(wrapper, true, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void Send_Cmd144_StatusChangeReport()
        {
            try
            {
                ID_144_STATUS_CHANGE_REP report = GetCmd144ReportBody();

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrapper.StatueChangeRep = report;

                SendWrapperToSchedule(wrapper, false, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }
        public void Send_Cmd144_StatusChangeReport(string sender, ushort batteryPercentage)
        {
            try
            {
                ID_144_STATUS_CHANGE_REP report = GetCmd144ReportBody();
                report.BatteryCapacity = batteryPercentage;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrapper.StatueChangeRep = report;

                SendWrapperToSchedule(wrapper, false, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }
        private ID_144_STATUS_CHANGE_REP GetCmd144ReportBody()
        {
            ID_144_STATUS_CHANGE_REP report = new ID_144_STATUS_CHANGE_REP();
            report.ModeStatus = VHModeStatusParse(Vehicle.AutoState);
            report.PowerStatus = Vehicle.PowerStatus;
            report.ObstacleStatus = Vehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Block ? VhStopSingle.On : VhStopSingle.Off;
            report.ErrorStatus = Vehicle.ErrorStatus;
            report.DrivingDirection = Vehicle.DrivingDirection;
            report.BatteryCapacity = (uint)Vehicle.AseBatteryStatus.Percentage;
            report.BatteryTemperature = (int)Vehicle.AseBatteryStatus.Temperature;
            report.ChargeStatus = VhChargeStatusParse(Vehicle.IsCharging);
            report.XAxis = Vehicle.AseMoveStatus.LastMapPosition.X;
            report.YAxis = Vehicle.AseMoveStatus.LastMapPosition.Y;
            report.Speed = Vehicle.AseMoveStatus.Speed;

            report.OpPauseStatus = Vehicle.OpPauseStatus;
            report.PauseStatus = Vehicle.PauseFlags[PauseType.Normal] ? VhStopSingle.On : VhStopSingle.Off; // Vehicle.AseMovingGuide.PauseStatus;
            report.SafetyPauseStatus = Vehicle.PauseFlags[PauseType.Safety] ? VhStopSingle.On : VhStopSingle.Off;
            report.EarthquakePauseTatus = Vehicle.PauseFlags[PauseType.EarthQuake] ? VhStopSingle.On : VhStopSingle.Off;
            report.BlockingStatus = Vehicle.BlockingStatus;


            switch (Vehicle.MainFlowConfig.SlotDisable)
            {
                case EnumSlotSelect.None:
                    {
                        report.ShelfStatusL = ShelfStatus.Enable;
                        report.ShelfStatusR = ShelfStatus.Enable;
                    }
                    break;
                case EnumSlotSelect.Left:
                    {
                        report.ShelfStatusL = ShelfStatus.Disable;
                        report.ShelfStatusR = ShelfStatus.Enable;
                    }
                    break;
                case EnumSlotSelect.Right:
                    {
                        report.ShelfStatusL = ShelfStatus.Enable;
                        report.ShelfStatusR = ShelfStatus.Disable;
                    }
                    break;
                case EnumSlotSelect.Both:
                    {
                        report.ShelfStatusL = ShelfStatus.Disable;
                        report.ShelfStatusR = ShelfStatus.Disable;
                    }
                    break;
            }

            AseMovingGuide aseMovingGuide = new AseMovingGuide(Vehicle.AseMovingGuide);
            report.WillPassGuideSection.Clear();
            report.WillPassGuideSection.AddRange(aseMovingGuide.GuideSectionIds);
            report.ReserveStatus = aseMovingGuide.ReserveStop;

            AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
            report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
            report.CurrentSecID = aseMoveStatus.LastSection.Id;
            report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
            report.DirectionAngle = aseMoveStatus.MovingDirection;
            report.VehicleAngle = aseMoveStatus.HeadDirection;

            List<AgvcTransferCommand> transferCommands = Vehicle.mapTransferCommands.Values.ToList();
            report.CmdId1 = transferCommands.Count > 0 ? transferCommands[0].CommandId : "";
            report.CmsState1 = transferCommands.Count > 0 ? transferCommands[0].EnrouteState : CommandState.None;
            report.CmdId2 = transferCommands.Count > 1 ? transferCommands[1].CommandId : "";
            report.CmsState2 = transferCommands.Count > 1 ? transferCommands[1].EnrouteState : CommandState.None;
            report.CmdId3 = transferCommands.Count > 2 ? transferCommands[2].CommandId : "";
            report.CmsState3 = transferCommands.Count > 2 ? transferCommands[2].EnrouteState : CommandState.None;
            report.CmdId4 = transferCommands.Count > 3 ? transferCommands[3].CommandId : "";
            report.CmsState4 = transferCommands.Count > 3 ? transferCommands[3].EnrouteState : CommandState.None;
            report.CurrentExcuteCmdId = Vehicle.TransferCommand.CommandId;
            report.ActionStatus = Vehicle.ActionStatus;

            report.HasCstL = Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
            report.CstIdL = Vehicle.AseCarrierSlotL.CarrierId;
            report.HasCstR = Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
            report.CstIdR = Vehicle.AseCarrierSlotR.CarrierId;

            return report;
        }

        private void Receive_Cmd43_StatusRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                Send_Cmd143_StatusResponse(e.iSeqNum);

                ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket;
                SetSystemTime(receive.SystemTime);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd143_StatusResponse(ushort seqNum)
        {
            try
            {
                ID_143_STATUS_RESPONSE response = new ID_143_STATUS_RESPONSE();
                response.ModeStatus = VHModeStatusParse(Vehicle.AutoState);

                response.PowerStatus = Vehicle.PowerStatus;
                response.ObstacleStatus = Vehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Block ? VhStopSingle.On : VhStopSingle.Off;
                response.ReserveStatus = Vehicle.AseMovingGuide.ReserveStop;
                response.ErrorStatus = Vehicle.ErrorStatus;
                response.ObstDistance = Vehicle.ObstDistance;
                response.ObstVehicleID = Vehicle.ObstVehicleID;
                response.HasCstL = Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
                response.CstIdL = Vehicle.AseCarrierSlotL.CarrierId;
                response.HasCstR = Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
                response.CstIdR = Vehicle.AseCarrierSlotR.CarrierId;
                response.ChargeStatus = VhChargeStatusParse(Vehicle.IsCharging);
                response.BatteryCapacity = (uint)Vehicle.AseBatteryStatus.Percentage;
                response.BatteryTemperature = (int)Vehicle.AseBatteryStatus.Temperature;
                response.XAxis = Vehicle.AseMoveStatus.LastMapPosition.X;
                response.YAxis = Vehicle.AseMoveStatus.LastMapPosition.Y;
                response.DirectionAngle = Vehicle.AseMoveStatus.MovingDirection;
                response.VehicleAngle = Vehicle.AseMoveStatus.HeadDirection;
                response.Speed = Vehicle.AseMoveStatus.Speed;
                response.StoppedBlockID = Vehicle.StoppedBlockID;

                response.OpPauseStatus = Vehicle.OpPauseStatus;
                response.PauseStatus = Vehicle.PauseFlags[PauseType.Normal] ? VhStopSingle.On : VhStopSingle.Off; // Vehicle.AseMovingGuide.PauseStatus;
                response.SafetyPauseStatus = Vehicle.PauseFlags[PauseType.Safety] ? VhStopSingle.On : VhStopSingle.Off;
                response.EarthquakePauseTatus = Vehicle.PauseFlags[PauseType.EarthQuake] ? VhStopSingle.On : VhStopSingle.Off;
                response.BlockingStatus = Vehicle.BlockingStatus;

                switch (Vehicle.MainFlowConfig.SlotDisable)
                {
                    case EnumSlotSelect.None:
                        {
                            response.ShelfStatusL = ShelfStatus.Enable;
                            response.ShelfStatusR = ShelfStatus.Enable;
                        }
                        break;
                    case EnumSlotSelect.Left:
                        {
                            response.ShelfStatusL = ShelfStatus.Disable;
                            response.ShelfStatusR = ShelfStatus.Enable;
                        }
                        break;
                    case EnumSlotSelect.Right:
                        {
                            response.ShelfStatusL = ShelfStatus.Enable;
                            response.ShelfStatusR = ShelfStatus.Disable;
                        }
                        break;
                    case EnumSlotSelect.Both:
                        {
                            response.ShelfStatusL = ShelfStatus.Disable;
                            response.ShelfStatusR = ShelfStatus.Disable;
                        }
                        break;
                }

                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                response.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                response.CurrentSecID = aseMoveStatus.LastSection.Id;
                response.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                response.DrivingDirection = DriveDirctionParse(aseMoveStatus.LastSection.CmdDirection);

                List<AgvcTransferCommand> transferCommands = Vehicle.mapTransferCommands.Values.ToList();
                response.CmdId1 = transferCommands.Count > 0 ? transferCommands[0].CommandId : "";
                response.CmsState1 = transferCommands.Count > 0 ? transferCommands[0].EnrouteState : CommandState.None;
                response.CmdId2 = transferCommands.Count > 1 ? transferCommands[1].CommandId : "";
                response.CmsState2 = transferCommands.Count > 1 ? transferCommands[1].EnrouteState : CommandState.None;
                response.CmdId3 = transferCommands.Count > 2 ? transferCommands[2].CommandId : "";
                response.CmsState3 = transferCommands.Count > 2 ? transferCommands[2].EnrouteState : CommandState.None;
                response.CmdId4 = transferCommands.Count > 3 ? transferCommands[3].CommandId : "";
                response.CmsState4 = transferCommands.Count > 3 ? transferCommands[3].EnrouteState : CommandState.None;
                response.ActionStatus = Vehicle.ActionStatus;
                response.CurrentExcuteCmdId = Vehicle.TransferCommand.CommandId;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.StatusReqRespFieldNumber;
                wrapper.SeqNum = seqNum;
                wrapper.StatusReqResp = response;

                SendWrapperToSchedule(wrapper, true, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void Receive_Cmd39_PauseRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_39_PAUSE_REQUEST receive = (ID_39_PAUSE_REQUEST)e.objPacket;

                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[收到.暫停] [{receive.EventType}][{receive.PauseType}].");

                switch (receive.EventType)
                {
                    case PauseEvent.Pause:
                        mainFlowHandler.AgvcConnector_OnCmdPauseEvent(e.iSeqNum, receive.PauseType);
                        break;
                    case PauseEvent.Continue:
                        mainFlowHandler.AgvcConnector_OnCmdResumeEvent(e.iSeqNum, receive.PauseType);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd139_PauseResponse(ushort seqNum, int replyCode, PauseEvent eventType)
        {
            try
            {
                ID_139_PAUSE_RESPONSE response = new ID_139_PAUSE_RESPONSE();
                response.EventType = eventType;
                response.ReplyCode = replyCode;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.PauseRespFieldNumber;
                wrapper.SeqNum = seqNum;
                wrapper.PauseResp = response;

                SendWrapperToSchedule(wrapper, true, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void Receive_Cmd38_GuideInfoResponse(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_38_GUIDE_INFO_RESPONSE response = (ID_38_GUIDE_INFO_RESPONSE)e.objPacket;
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取得.路線] ID_38_GUIDE_INFO_RESPONSE.");

                ShowGuideInfoResponse(response);

                Vehicle.AseMovingGuide = new AseMovingGuide(response);
                //ClearAllReserve();
                Vehicle.AseMoveStatus.IsMoveEnd = false;
                mainFlowHandler.SetupAseMovingGuideMovingSections();
                SetupNeedReserveSections();
                StatusChangeReport();
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"IsMoveEnd Need False And Cur IsMoveEnd = {Vehicle.AseMoveStatus.IsMoveEnd.ToString()}");

                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, Vehicle.AseMovingGuide.GetInfo());
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void ShowGuideInfoResponse(ID_38_GUIDE_INFO_RESPONSE response)
        {
            try
            {
                var info = response.GuideInfoList[0];
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $" Get Guide Address[{info.FromTo.From}]->[{info.FromTo.To}],Guide=[{info.GuideSections.Count}] Sections 和 [{info.GuideAddresses.Count}] Addresses.");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd138_GuideInfoRequest(string fromAddress, string toAddress)
        {
            try
            {
                //ClearAllReserve();//200827 dabid+
                ID_138_GUIDE_INFO_REQUEST request = new ID_138_GUIDE_INFO_REQUEST();
                FitGuideInfos(request.FromToAdrList, fromAddress, toAddress);

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.GuideInfoReqFieldNumber;
                wrapper.GuideInfoReq = request;

                SendWrapperToSchedule(wrapper, false, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void FitGuideInfos(RepeatedField<FromToAdr> fromToAdrList, string fromAddress, string toAddress)
        {
            fromToAdrList.Clear();
            FromToAdr fromToAdr = new FromToAdr();
            fromToAdr.From = fromAddress;
            fromToAdr.To = toAddress;
            fromToAdrList.Add(fromToAdr);
        }

        public void Receive_Cmd37_TransferCancelRequest(object sender, TcpIpEventArgs e)
        {
            ID_37_TRANS_CANCEL_REQUEST receive = (ID_37_TRANS_CANCEL_REQUEST)e.objPacket;

            try
            {
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"AgvcConnector : Get [{receive.CancelAction}] command");

                if (receive.CancelAction == CancelActionType.CmdEms)
                {
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, (int)EnumAgvcReplyCode.Accept, receive);
                    mainFlowHandler.SetAlarmFromAgvm(000037);
                    mainFlowHandler.StopClearAndReset();
                    return;
                }

                throw new Exception($"AgvcConnector : AGVL can not Cancel or Abort now.");

                if (Vehicle.mapTransferCommands.Count == 0)
                {
                    throw new Exception($"AgvcConnector : Vehicle Idle, reject [{receive.CancelAction}].");
                }

                var cmdId = receive.CmdID.Trim();

                if (!Vehicle.mapTransferCommands.ContainsKey(cmdId))
                {
                    throw new Exception($"AgvcConnector : No [{cmdId}] to cancel, reject [{receive.CancelAction}].");
                }

                switch (receive.CancelAction)
                {
                    case CancelActionType.CmdCancel:
                    case CancelActionType.CmdAbort:
                        Send_Cmd137_TransferCancelResponse(e.iSeqNum, (int)EnumAgvcReplyCode.Accept, receive);
                        mainFlowHandler.AgvcConnector_OnCmdCancelAbortEvent(e.iSeqNum, receive);
                        break;
                    case CancelActionType.CmdCancelIdMismatch:
                    case CancelActionType.CmdCancelIdReadFailed:
                    case CancelActionType.CmdNone:
                    default:
                        throw new Exception($"AgvcConnector : Reject Unkonw CancelAction [{receive.CancelAction}].");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                Send_Cmd137_TransferCancelResponse(e.iSeqNum, (int)EnumAgvcReplyCode.Reject, receive);
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd137_TransferCancelResponse(ushort seqNum, int replyCode, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            try
            {
                ID_137_TRANS_CANCEL_RESPONSE response = new ID_137_TRANS_CANCEL_RESPONSE();
                response.CmdID = receive.CmdID;
                response.CancelAction = receive.CancelAction;
                response.ReplyCode = replyCode;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.TransCancelRespFieldNumber;
                wrapper.SeqNum = seqNum;
                wrapper.TransCancelResp = response;

                SendWrapperToSchedule(wrapper, true, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void Send_Cmd136_TransferEventReport(EventType eventType, string cmdId, EnumSlotNumber slotNumber, string portId)
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = eventType;
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.Location = slotNumber == EnumSlotNumber.L ? AGVLocation.Left : AGVLocation.Right;
                report.CmdID = cmdId;
                report.CurrentPortID = portId;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrapper.ImpTransEventRep = report;

                SendWrapperToSchedule(wrapper, false, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void ReceiveSent_Cmd36_TransferEventResponse(ID_36_TRANS_EVENT_RESPONSE response, ID_136_TRANS_EVENT_REP report)
        {
            try
            {
                switch (response.EventType)
                {
                    case EventType.LoadArrivals:
                        Vehicle.TransferCommand.IsLoadArrivalReply = true;
                        break;
                    case EventType.LoadComplete:
                        Vehicle.TransferCommand.IsLoadCompleteReply = true;
                        break;
                    case EventType.UnloadArrivals:
                        if (Vehicle.TransferCommand.TransferStep == EnumTransferStep.WaitMoveArrivalVitualPortReply)
                        {
                            if (response.PortInfos.Count == 0)
                            {
                                mainFlowHandler.StopClearAndReset();
                            }
                            else
                            {
                                Vehicle.PortInfos = response.PortInfos.ToList();
                                Vehicle.TransferCommand.IsVitualPortUnloadArrivalReply = true;
                            }
                        }
                        else if (Vehicle.TransferCommand.TransferStep == EnumTransferStep.WaitUnloadArrivalReply)
                        {
                            Vehicle.TransferCommand.IsUnloadArrivalReply = true;
                        }
                        break;
                    case EventType.UnloadComplete:
                        Vehicle.TransferCommand.IsUnloadCompleteReply = true;
                        break;
                    case EventType.ReserveReq:
                        break;
                    case EventType.Bcrread:
                        {
                            if (string.IsNullOrEmpty(response.CmdID.Trim()))
                            {
                                OnMessageShowOnMainFormEvent?.Invoke(this, $"[上報 儲位狀態 成功] Report Cst ID Read Reply Ok.");

                                if (!string.IsNullOrEmpty(response.RenameCarrierID))
                                {
                                    switch (report.Location)
                                    {
                                        case AGVLocation.Right:
                                            Vehicle.AseCarrierSlotR.CarrierId = response.RenameCarrierID;
                                            OnCstRenameEvent?.Invoke(this, EnumSlotNumber.R);
                                            break;
                                        case AGVLocation.Left:
                                            Vehicle.AseCarrierSlotL.CarrierId = response.RenameCarrierID;
                                            OnCstRenameEvent?.Invoke(this, EnumSlotNumber.L);
                                            break;
                                        case AGVLocation.None:
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                if (response.ReplyAction != ReplyActionType.Continue)
                                {
                                    mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨失敗 放棄命令] Load fail, [ReplyAction = {response.ReplyAction}][RenameCarrierID = {response.RenameCarrierID}]");

                                    mainFlowHandler.ResetAllAlarmsFromAgvm();
                                    if (Vehicle.mapTransferCommands.ContainsKey(report.CmdID))
                                    {
                                        var cmd = Vehicle.mapTransferCommands[report.CmdID];
                                        if (!string.IsNullOrEmpty(response.RenameCarrierID))
                                        {
                                            cmd.CassetteId = response.RenameCarrierID;
                                            switch (cmd.SlotNumber)
                                            {
                                                case EnumSlotNumber.L:
                                                    Vehicle.AseCarrierSlotL.CarrierId = response.RenameCarrierID;
                                                    break;
                                                case EnumSlotNumber.R:
                                                    Vehicle.AseCarrierSlotR.CarrierId = response.RenameCarrierID;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            OnCstRenameEvent?.Invoke(this, cmd.SlotNumber);
                                        }
                                        //Vehicle.mapTransferCommands[report.CmdID].CompleteStatus = GetCancelCompleteStatus(response.ReplyAction, Vehicle.mapTransferCommands[report.CmdID].CompleteStatus);
                                        //mainFlowHandler.TransferComplete(report.CmdID);
                                        Vehicle.TransferCommand.CompleteStatus = GetCancelCompleteStatus(response.ReplyAction, Vehicle.TransferCommand.CompleteStatus);
                                        Vehicle.TransferCommand.IsStopAndClear = true;
                                    }
                                }
                                else
                                {
                                    mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨階段 CST ID 上報 完成] Load Complete and BcrReadReplyOk");
                                    if (Vehicle.mapTransferCommands.ContainsKey(report.CmdID))
                                    {
                                        var cmd = Vehicle.mapTransferCommands[report.CmdID];
                                        if (!string.IsNullOrEmpty(response.RenameCarrierID))
                                        {
                                            cmd.CassetteId = response.RenameCarrierID;
                                            switch (cmd.SlotNumber)
                                            {
                                                case EnumSlotNumber.L:
                                                    Vehicle.AseCarrierSlotL.CarrierId = response.RenameCarrierID;
                                                    break;
                                                case EnumSlotNumber.R:
                                                    Vehicle.AseCarrierSlotR.CarrierId = response.RenameCarrierID;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            OnCstRenameEvent?.Invoke(this, cmd.SlotNumber);
                                        }
                                    }
                                    Vehicle.TransferCommand.IsCstIdReadReply = true;
                                }
                            }
                        }
                        break;
                    case EventType.Cstremove:
                        OnMessageShowOnMainFormEvent?.Invoke(this, $"SendRecv_Cmd136_CstRemove, AGVC reply OK.");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void SendRecv_Cmd136_TransferEventReport(EventType eventType, string portId)
        {
            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = eventType;
                report.CurrentAdrID = Vehicle.AseMoveStatus.LastAddress.Id;
                report.CurrentSecID = Vehicle.AseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)Vehicle.AseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.CmdID = Vehicle.TransferCommand.CommandId;
                report.Location = Vehicle.TransferCommand.SlotNumber == EnumSlotNumber.L ? AGVLocation.Left : AGVLocation.Right;
                report.CurrentPortID = portId;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrapper.ImpTransEventRep = report;

                SendWrapperToSchedule(wrapper, false, true);

                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"SendRecv transfer event report. [{eventType}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void SendRecv_Cmd136_CstIdReadReport()
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
            AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.TransferCommand.SlotNumber == EnumSlotNumber.L ? Vehicle.AseCarrierSlotL : Vehicle.AseCarrierSlotR;

            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = EventType.Bcrread;
                report.CSTID = aseCarrierSlotStatus.CarrierId;
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.BCRReadResult = Vehicle.TransferCommand.SlotNumber == EnumSlotNumber.L ? Vehicle.LeftReadResult : Vehicle.RightReadResult;
                report.CmdID = Vehicle.TransferCommand.CommandId; //200525 dabid#

                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨階段 CST ID 上報] BCRReadResult : {report.BCRReadResult}"); //200525 dabid+

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrapper.ImpTransEventRep = report;

                SendWrapperToSchedule(wrapper, false, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd136_CstIdReadReport(EnumSlotNumber SlotNumber) //200625 dabid+
        {

            AseMoveStatus moveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
            AseCarrierSlotStatus slotStatus = Vehicle.GetAseCarrierSlotStatus(SlotNumber);

            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();

                report.EventType = EventType.Bcrread;
                report.CSTID = slotStatus.CarrierId;
                report.CurrentAdrID = moveStatus.LastAddress.Id;
                report.CurrentSecID = moveStatus.LastSection.Id;
                report.SecDistance = (uint)moveStatus.LastSection.VehicleDistanceSinceHead;
                report.BCRReadResult = SlotNumber == EnumSlotNumber.L ? Vehicle.LeftReadResult : Vehicle.RightReadResult;
                report.Location = SlotNumber == EnumSlotNumber.L ? AGVLocation.Left : AGVLocation.Right;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrapper.ImpTransEventRep = report;

                SendWrapperToSchedule(wrapper, false, false);

                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[讀取儲位資訊 上報] SendRecv_Cmd136_CstIdReadReport send success.");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd136_CstRemove(EnumSlotNumber SlotNumber) //200625 dabid+
        {

            AseMoveStatus moveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
            AseCarrierSlotStatus slotStatus = Vehicle.GetAseCarrierSlotStatus(SlotNumber);

            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();

                report.EventType = EventType.Cstremove;
                report.CurrentAdrID = moveStatus.LastAddress.Id;
                report.CurrentSecID = moveStatus.LastSection.Id;
                report.SecDistance = (uint)moveStatus.LastSection.VehicleDistanceSinceHead;
                report.Location = SlotNumber == EnumSlotNumber.L ? AGVLocation.Left : AGVLocation.Right;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrapper.ImpTransEventRep = report;

                SendWrapperToSchedule(wrapper, false, false);

                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移除儲位資訊 上報] SendRecv_Cmd136_CstRemove send success.");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private CompleteStatus GetCancelCompleteStatus(ReplyActionType replyAction, CompleteStatus completeStatus)
        {
            switch (replyAction)
            {
                case ReplyActionType.Continue:
                    break;
                case ReplyActionType.Wait:
                    break;
                case ReplyActionType.Retry:
                    break;
                case ReplyActionType.Cancel:
                    return CompleteStatus.Cancel;
                case ReplyActionType.Abort:
                    return CompleteStatus.Abort;
                case ReplyActionType.CancelIdMisnatch:
                    return CompleteStatus.IdmisMatch;
                case ReplyActionType.CancelIdReadFailed:
                    return CompleteStatus.IdreadFailed;
                case ReplyActionType.CancelPidFailed:
                    break;
                default:
                    break;
            }

            return completeStatus;
        }

        public void Receive_Cmd35_CarrierIdRenameRequest(object sender, TcpIpEventArgs e)
        {
            ID_35_CST_ID_RENAME_REQUEST receive = (ID_35_CST_ID_RENAME_REQUEST)e.objPacket;
            var result = false;

            if (Vehicle.AseCarrierSlotL.CarrierId == receive.OLDCSTID.Trim())
            {
                AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.AseCarrierSlotL;
                aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
                aseCarrierSlotStatus.CarrierId = receive.NEWCSTID;
                OnRenameCassetteIdEvent?.Invoke(this, aseCarrierSlotStatus);
                result = true;
            }
            else if (Vehicle.AseCarrierSlotR.CarrierId == receive.OLDCSTID.Trim())
            {
                AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.AseCarrierSlotR;
                aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
                aseCarrierSlotStatus.CarrierId = receive.NEWCSTID;
                OnRenameCassetteIdEvent?.Invoke(this, aseCarrierSlotStatus);
                result = true;
            }

            int replyCode = result ? 0 : 1;
            Send_Cmd135_CarrierIdRenameResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd135_CarrierIdRenameResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_135_CST_ID_RENAME_RESPONSE response = new ID_135_CST_ID_RENAME_RESPONSE();
                response.ReplyCode = replyCode;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.CSTIDRenameRespFieldNumber;
                wrapper.SeqNum = seqNum;
                wrapper.CSTIDRenameResp = response;

                SendWrapperToSchedule(wrapper, true, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void Send_Cmd134_TransferEventReport(EventType type)
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);

            try
            {
                ID_134_TRANS_EVENT_REP report = new ID_134_TRANS_EVENT_REP();
                report.EventType = type;
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.DrivingDirection = DriveDirctionParse(aseMoveStatus.LastSection.CmdDirection);
                report.XAxis = Vehicle.AseMoveStatus.LastMapPosition.X;
                report.YAxis = Vehicle.AseMoveStatus.LastMapPosition.Y;
                report.Speed = Vehicle.AseMoveStatus.Speed;
                report.DirectionAngle = Vehicle.AseMoveStatus.MovingDirection;
                report.VehicleAngle = Vehicle.AseMoveStatus.HeadDirection;

                mirleLogger.Log(new LogFormat("Info", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Angle=[{aseMoveStatus.MovingDirection}]"));

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.TransEventRepFieldNumber;
                wrapper.TransEventRep = report;

                SendWrapperToSchedule(wrapper, false, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReceiveSent_Cmd32_TransCompleteResponse(ID_32_TRANS_COMPLETE_RESPONSE response)
        {
            try
            {
                int waitTime = response.WaitTime;
                SpinWait.SpinUntil(() => false, waitTime);
                StatusChangeReport();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void SendRecv_Cmd132_TransferCompleteReport(AgvcTransferCommand transferCommand, int delay = 0)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);

                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Transfer Complete, Complete Status={transferCommand.CompleteStatus}, Command ID={transferCommand.CommandId}");

                ID_132_TRANS_COMPLETE_REPORT report = new ID_132_TRANS_COMPLETE_REPORT();
                report.CmdID = transferCommand.CommandId;
                report.CSTID = transferCommand.CassetteId;
                report.CmpStatus = transferCommand.CompleteStatus;
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.CmdPowerConsume = Vehicle.CmdPowerConsume;
                report.CmdDistance = Vehicle.CmdDistance;
                report.XAxis = Vehicle.AseMoveStatus.LastMapPosition.X;
                report.YAxis = Vehicle.AseMoveStatus.LastMapPosition.Y;
                report.DirectionAngle = Vehicle.AseMoveStatus.MovingDirection;
                report.VehicleAngle = Vehicle.AseMoveStatus.HeadDirection;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.TranCmpRepFieldNumber;
                wrapper.TranCmpRep = report;

                SendWrapperToSchedule(wrapper, false, true);
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Send transfer complete report success. [{report.CmpStatus}]");
                LogCommandEnd(report);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void Receive_Cmd31_TransferRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_31_TRANS_REQUEST transRequest = (ID_31_TRANS_REQUEST)e.objPacket;
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[收到搬送命令] Get Transfer Command: {transRequest.CommandAction}");
                LogCommandStart(transRequest);
                if (Vehicle.mapTransferCommands.ContainsKey(transRequest.CmdID.Trim()))
                {
                    mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[拒絕搬送命令] Reject Transfer Command: {transRequest.CommandAction}. Same command id is working.");
                    Send_Cmd131_TransferResponse(transRequest.CmdID, transRequest.CommandAction, e.iSeqNum, (int)EnumAgvcReplyCode.Unknow, "Unknow command.");
                    return;
                }

                AgvcTransferCommand transferCommand = new AgvcTransferCommand(transRequest, e.iSeqNum);
                ShowTransferCmdToForm(transferCommand);
                OnInstallTransferCommandEvent?.Invoke(this, transferCommand);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Send_Cmd131_TransferResponse(string cmdId, CommandActionType commandAction, ushort seqNum, int replyCode, string reason)
        {
            try
            {
                ID_131_TRANS_RESPONSE response = new ID_131_TRANS_RESPONSE();
                response.CmdID = cmdId;
                response.CommandAction = commandAction;
                response.ReplyCode = replyCode;
                response.NgReason = reason;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.TransRespFieldNumber;
                wrapper.SeqNum = seqNum;
                wrapper.TransResp = response;

                SendWrapperToSchedule(wrapper, true, false);

                if (replyCode == 0)
                {
                    Commanding();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void Receive_Cmd11_CouplerInfoReport(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_11_COUPLER_INFO_REP report = (ID_11_COUPLER_INFO_REP)e.objPacket;

                if (Vehicle.MainFlowConfig.AgreeAgvcSetCoupler)
                {
                    ModifyAddressIsCharger(report.CouplerInfos.ToList());

                    SendRecv_Cmd111_CouplerInfoResponse(e.iSeqNum, (int)EnumAgvcReplyCode.Accept);
                }
                else
                {
                    SendRecv_Cmd111_CouplerInfoResponse(e.iSeqNum, (int)EnumAgvcReplyCode.Reject);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void ModifyAddressIsCharger(List<CouplerInfo> couplerInfos)
        {
            try
            {
                List<string> enableChargeAddressIds = new List<string>();
                foreach (var item in couplerInfos)
                {
                    if (item.CouplerStatus == CouplerStatus.Enable)
                    {
                        mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"{item.AddressID} is charger.");
                        enableChargeAddressIds.Add(item.AddressID.Trim());
                    }
                    else
                    {
                        mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"{item.AddressID} is not charger.");
                    }
                }

                foreach (var addressId in Vehicle.Mapinfo.addressMap.Keys.ToArray())
                {
                    if (enableChargeAddressIds.Contains(addressId))
                    {
                        Vehicle.Mapinfo.addressMap[addressId].ChargeDirection = EnumAddressDirection.Right;
                    }
                    else
                    {
                        Vehicle.Mapinfo.addressMap[addressId].ChargeDirection = EnumAddressDirection.None;
                    }
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void SendRecv_Cmd111_CouplerInfoResponse(ushort seqNum, int replyCode)
        {
            try
            {
                mainFlowHandler.LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Coupler Info Response, [ReplyCode = {replyCode.ToString()}]");

                ID_111_COUPLER_INFO_RESPONSE response = new ID_111_COUPLER_INFO_RESPONSE();
                response.ReplyCode = replyCode;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.CouplerInfoRespFieldNumber;
                wrapper.CouplerInfoResp = response;
                wrapper.SeqNum = seqNum;

                //SendCommandWrapper(wrapper, true);
                SendWrapperToSchedule(wrapper, true, false);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region EnumParse
        private VhChargeStatus VhChargeStatusParse(string v)
        {
            try
            {
                v = v.Trim();

                return (VhChargeStatus)Enum.Parse(typeof(VhChargeStatus), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return VhChargeStatus.ChargeStatusCharging;
            }
        }
        private VhChargeStatus VhChargeStatusParse(bool charging)
        {
            return charging ? VhChargeStatus.ChargeStatusCharging : VhChargeStatus.ChargeStatusNone;
        }
        private VhStopSingle VhStopSingleParse(string v)
        {
            try
            {
                v = v.Trim();

                return (VhStopSingle)Enum.Parse(typeof(VhStopSingle), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return VhStopSingle.Off;
            }
        }
        private VHActionStatus VHActionStatusParse(string v)
        {
            try
            {
                v = v.Trim();

                return (VHActionStatus)Enum.Parse(typeof(VHActionStatus), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return VHActionStatus.Commanding;
            }
        }
        private EventType EventTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (EventType)Enum.Parse(typeof(EventType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return EventType.AdrOrMoveArrivals;
            }
        }
        private CompleteStatus CompleteStatusParse(string v)
        {
            try
            {
                v = v.Trim();

                return (CompleteStatus)Enum.Parse(typeof(CompleteStatus), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return CompleteStatus.Abort;
            }
        }
        private OperatingPowerMode OperatingPowerModeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (OperatingPowerMode)Enum.Parse(typeof(OperatingPowerMode), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return OperatingPowerMode.OperatingPowerOff;
            }
        }
        private OperatingVHMode OperatingVHModeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (OperatingVHMode)Enum.Parse(typeof(OperatingVHMode), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return OperatingVHMode.OperatingAuto;
            }
        }
        private PauseType PauseTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (PauseType)Enum.Parse(typeof(PauseType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return PauseType.None;
            }
        }
        private PauseEvent PauseEventParse(string v)
        {
            try
            {
                v = v.Trim();

                return (PauseEvent)Enum.Parse(typeof(PauseEvent), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return PauseEvent.Pause;
            }
        }
        private ReserveResult ReserveResultParse(string v)
        {
            try
            {
                v = v.Trim();

                return (ReserveResult)Enum.Parse(typeof(ReserveResult), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return ReserveResult.Success;
            }
        }
        private PassType PassTypeParse(string v)
        {
            try
            {
                v = v.Trim();

                return (PassType)Enum.Parse(typeof(PassType), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return PassType.Pass;
            }
        }
        private ErrorStatus ErrorStatusParse(string v)
        {
            try
            {
                v = v.Trim();

                return (ErrorStatus)Enum.Parse(typeof(ErrorStatus), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return ErrorStatus.ErrReset;
            }
        }
        private VHModeStatus VHModeStatusParse(EnumAutoState autoState)
        {
            switch (autoState)
            {
                case EnumAutoState.Auto:
                    return VHModeStatus.AutoRemote;
                case EnumAutoState.Manual:
                    return VHModeStatus.Manual;
                case EnumAutoState.None:
                    return VHModeStatus.Manual;
                default:
                    return VHModeStatus.None;
            }
        }
        private DriveDirction DriveDirctionParse(EnumCommandDirection cmdDirection)
        {
            try
            {
                switch (cmdDirection)
                {
                    case EnumCommandDirection.None:
                        return DriveDirction.DriveDirNone;
                    case EnumCommandDirection.Forward:
                        return DriveDirction.DriveDirForward;
                    case EnumCommandDirection.Backward:
                        return DriveDirction.DriveDirReverse;
                    default:
                        return DriveDirction.DriveDirNone;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return DriveDirction.DriveDirNone;
            }
        }

        #endregion

        #region Get/Set System Date Time

        // 用於設置系統時間
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        // 用於設置系統時間
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetLocalTime(ref SYSTEMTIME st);

        // 用於獲得系統時間
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetSystemTime(ref SYSTEMTIME st);

        private void SetSystemTime(string timeStamp)
        {
            try
            {
                SYSTEMTIME st = GetSYSTEMTIME(timeStamp);
                SetLocalTime(ref st);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private SYSTEMTIME GetSYSTEMTIME(string timeStamp)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            st.wYear = short.Parse(timeStamp.Substring(0, 4));
            st.wMonth = short.Parse(timeStamp.Substring(4, 2));
            st.wDay = short.Parse(timeStamp.Substring(6, 2));

            int hour = (int.Parse(timeStamp.Substring(8, 2))) % 24;
            st.wHour = (short)hour;
            st.wMinute = short.Parse(timeStamp.Substring(10, 2));
            st.wSecond = short.Parse(timeStamp.Substring(12, 2));
            int ms = int.Parse(timeStamp.Substring(14, 2)) * 10;
            st.wMilliseconds = (short)ms;

            return st;
        }

        #endregion

        #region Log

        private void LogException(string classMethodName, string exMsg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Error", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", exMsg));
            }
            catch (Exception) { }
        }

        private void LogDebug(string classMethodName, string msg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", msg));
            }
            catch (Exception) { }
        }

        private void LogComm(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Comm", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", msg));
        }

        public void LogCommandList(string msg)
        {
            mirleLogger.LogString("CommandList", msg);
        }

        public void LogCommandStart(ID_31_TRANS_REQUEST request)
        {
            LogCommandList($@"[Start][Type = {request.CommandAction.ToString()}, CmdID = {request.CmdID}, CstID = {request.CSTID}, load = {request.LoadAdr}, loadPort = {request.LoadPortID}, loadGate = {request.IsLoadPortHasGate.ToString()}, unload = {request.DestinationAdr}, unloadPort = {request.UnloadPortID}, unloadGate = {request.IsUnloadPortHasGate}]");
        }

        public void LogCommandEnd(ID_132_TRANS_COMPLETE_REPORT report)
        {
            LogCommandList($@"[End][Type = {report.CmpStatus.ToString()}, CmdID = {report.CmdID}, CstID = {report.CSTID}, section = {report.CurrentSecID}, address = {report.CurrentAdrID}, X = {report.XAxis.ToString()}, Y = {report.YAxis.ToString()}]");
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }

    public class ScheduleWrapper
    {
        public int RetrySendWaitCounter { get; set; } = 0;
        public WrapperMessage Wrapper { get; set; } = new WrapperMessage();
        public bool IsSendWait { get; set; } = false;
        public ScheduleWrapper(WrapperMessage wrapper)
        {
            this.Wrapper = wrapper;
        }
    }
}