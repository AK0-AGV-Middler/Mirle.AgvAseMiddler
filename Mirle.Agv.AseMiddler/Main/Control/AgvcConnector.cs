﻿using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using com.mirle.iibg3k0.ttc.Common.TCPIP.DecodRawData;
using Google.Protobuf.Collections;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;
using Mirle.Tools;
using System.Runtime.InteropServices;

namespace Mirle.Agv.AseMiddler.Controller
{
    [Serializable]
    public class AgvcConnector
    {
        public class SendWaitWrapper
        {
            public int RetrySendWaitCounter { get; set; } = 10;
            public WrapperMessage Wrapper { get; set; } = new WrapperMessage();

            public SendWaitWrapper(WrapperMessage wrapper)
            {
                this.Wrapper = wrapper;
            }
        }

        #region Events
        public event EventHandler<string> OnMessageShowOnMainFormEvent;
        public event EventHandler<AgvcTransCmd> OnInstallTransferCommandEvent;
        public event EventHandler<AgvcOverrideCmd> OnOverrideCommandEvent;
        public event EventHandler<AseMovingGuide> OnAvoideRequestEvent;
        public event EventHandler<string> OnCmdReceiveEvent;
        public event EventHandler<string> OnCmdSendEvent;
        public event EventHandler<string> OnReserveOkEvent;
        public event EventHandler<string> OnPassReserveSectionEvent;
        public event EventHandler<LogFormat> OnLogMsgEvent;
        public event EventHandler<AseCarrierSlotStatus> OnRenameCassetteIdEvent;
        public event EventHandler OnStopClearAndResetEvent;
        public event EventHandler OnAgvcAcceptMoveArrivalEvent;
        public event EventHandler OnAgvcAcceptLoadArrivalEvent;
        public event EventHandler OnAgvcAcceptUnloadArrivalEvent;
        public event EventHandler OnAgvcAcceptLoadCompleteEvent;
        public event EventHandler OnAgvcAcceptBcrReadReply;
        public event EventHandler OnAgvcAcceptUnloadCompleteEvent;
        public event EventHandler OnSendRecvTimeoutEvent;

        #endregion

        private Vehicle theVehicle = Vehicle.Instance;
        private AgvcConnectorConfig agvcConnectorConfig;
        private AlarmHandler alarmHandler;
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private MainFlowHandler mainFlowHandler;

        private Thread thdAskReserve;
        private ConcurrentQueue<MapSection> quePartMoveSections = new ConcurrentQueue<MapSection>();
        private ConcurrentQueue<MapSection> queNeedReserveSections = new ConcurrentQueue<MapSection>();
        private ConcurrentQueue<MapSection> queReserveOkSections = new ConcurrentQueue<MapSection>();
        private bool ReserveOkAskNext { get; set; } = false;
        private ConcurrentBag<MapSection> CbagNeedReserveSections { get; set; } = new ConcurrentBag<MapSection>();
        public bool IsAskReservePause { get; private set; }
        private bool IsWaitReserveReply { get; set; }
        private MapPosition lastReportPosition { get; set; } = new MapPosition();
        public bool IsOptimizingSteps { get; set; } = false;

        public ConcurrentQueue<SendWaitWrapper> queSendWaitWrappers = new ConcurrentQueue<SendWaitWrapper>();


        public TcpIpAgent ClientAgent { get; private set; }
        public string AgvcConnectorAbnormalMsg { get; set; } = "";
        public bool IsAgvcReplyBcrRead { get; set; } = false;
        public TrxTcpIp.ReturnCode ReturnCode { get; set; } = TrxTcpIp.ReturnCode.Timeout;
        public EnumCstIdReadResult ReadResult { get; set; } = EnumCstIdReadResult.Fail;


        public AgvcConnector(MainFlowHandler mainFlowHandler)
        {
            this.mainFlowHandler = mainFlowHandler;
            agvcConnectorConfig = mainFlowHandler.GetAgvcConnectorConfig();
            alarmHandler = mainFlowHandler.GetAlarmHandler();
            mirleLogger = MirleLogger.Instance;

            CreatTcpIpClientAgent();
            if (!theVehicle.IsSimulation)
            {
                Connect();
            }
            StartAskReserve();
        }

        #region Initial

        public void CreatTcpIpClientAgent()
        {

            IDecodReceiveRawData RawDataDecoder = new DecodeRawData_Google(unPackWrapperMsg);

            int clientNum = agvcConnectorConfig.ClientNum;
            string clientName = agvcConnectorConfig.ClientName;
            string sRemoteIP = agvcConnectorConfig.RemoteIp;
            int iRemotePort = agvcConnectorConfig.RemotePort;
            string sLocalIP = agvcConnectorConfig.LocalIp;
            int iLocalPort = agvcConnectorConfig.LocalPort;

            int recv_timeout_ms = agvcConnectorConfig.RecvTimeoutMs;                         //等待sendRecv Reply的Time out時間(milliseconds)
            int send_timeout_ms = agvcConnectorConfig.SendTimeoutMs;                         //暫時無用
            int max_readSize = agvcConnectorConfig.MaxReadSize;                              //暫時無用
            int reconnection_interval_ms = agvcConnectorConfig.ReconnectionIntervalMs;       // Dis-Connect 多久之後再進行一次嘗試恢復連線的動作
            int max_reconnection_count = agvcConnectorConfig.MaxReconnectionCount;           // Dis-Connect 後最多嘗試幾次重新恢復連線 (若設定為0則不進行自動重新連線)
            int retry_count = agvcConnectorConfig.RetryCount;                                //SendRecv Time out後要再重複發送的次數

            try
            {
                ClientAgent = new TcpIpAgent(clientNum, clientName, sLocalIP, iLocalPort, sRemoteIP, iRemotePort, TcpIpAgent.TCPIP_AGENT_COMM_MODE.CLINET_MODE, recv_timeout_ms, send_timeout_ms, max_readSize, reconnection_interval_ms, max_reconnection_count, retry_count, AppConstants.FrameBuilderType.PC_TYPE_MIRLE);
                ClientAgent.injectDecoder(RawDataDecoder);

                EventInitial();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
        public AgvcConnectorConfig GetAgvcConnectorConfig()
        {
            return agvcConnectorConfig;
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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

                    ClientAgent.stop();
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                    Task.Run(() => ClientAgent.stop());
                }
            }
        }
        protected void ClientAgent_OnConnectionChangeEvent(object sender, TcpIpEventArgs e)
        {
            TcpIpAgent agent = sender as TcpIpAgent;
            bool isConnected = agent.IsConnection;
            if (theVehicle.IsAgvcConnect != isConnected)
            {
                theVehicle.IsAgvcConnect = isConnected;
                OnMessageShowOnMainFormEvent?.Invoke(this, $"AGVC connection changed.{agent.IsConnection}");
            }
        }
        private void EventInitial()
        {
            foreach (var item in Enum.GetValues(typeof(EnumCmdNum)))
            {
                ClientAgent.addTcpIpReceivedHandler((int)item, LogRecvMsg);
                ClientAgent.addTcpIpReceivedHandler((int)item, RecieveCommandMediator);
            }

            ClientAgent.addTcpIpConnectedHandler(ClientAgent_OnConnectionChangeEvent);       //連線時的通知
            ClientAgent.addTcpIpDisconnectedHandler(ClientAgent_OnConnectionChangeEvent);    // Dis-Connect 時的通知
        }
        private void SendCommandWrapper(WrapperMessage wrapper, bool isReply = false, int delay = 0)
        {
            if (!IsConnected())
            {
                var msg = $"AgvcConnector : AGVC  Dis-Connect , Can not send [{wrapper.SeqNum}][id {wrapper.ID}{(EnumCmdNum)wrapper.ID}]";
                OnCmdSendEvent?.Invoke(this, msg);
                msg += wrapper.ToString();
                LogComm(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                return;
            }
            else
            {
                LogSendMsg(wrapper);

                if (delay != 0)
                {
                    Task.Run(() =>
                    {
                        SpinWait.SpinUntil(() => false, delay);
                        ClientAgent.TrxTcpIp.SendGoogleMsg(wrapper, isReply);
                    });
                }
                else
                {
                    Task.Run(() => ClientAgent.TrxTcpIp.SendGoogleMsg(wrapper, isReply));
                }
            }
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

            if (theVehicle.AutoState != EnumAutoState.Auto && !IsApplyOnly(cmdNum))
            {
                var msg = $"AgvcConnector : Manual mode, Ignore Agvc Command";
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                return;
            }

            switch (cmdNum)
            {
                case EnumCmdNum.Cmd000_EmptyCommand:
                    break;
                case EnumCmdNum.Cmd31_TransferRequest:
                    Receive_Cmd31_TransferRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd32_TransferCompleteResponse:
                    Receive_Cmd32_TransferCompleteResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd38_GuideInfoResponse:
                    Receive_Cmd38_GuideInfoResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                    Receive_Cmd35_CarrierIdRenameRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd36_TransferEventResponse:
                    Receive_Cmd36_TransferEventResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd37_TransferCancelRequest:
                    Receive_Cmd37_TransferCancelRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd39_PauseRequest:
                    Receive_Cmd39_PauseRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd41_ModeChange:
                    Receive_Cmd41_ModeChange(sender, e);
                    break;
                case EnumCmdNum.Cmd43_StatusRequest:
                    Receive_Cmd43_StatusRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd44_StatusRequest:
                    Receive_Cmd44_StatusRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd51_AvoidRequest:
                    Receive_Cmd51_AvoidRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                    Receive_Cmd52_AvoidCompleteResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd74_AddressTeachResponse:
                    Receive_Cmd74_AddressTeachResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd91_AlarmResetRequest:
                    Receive_Cmd91_AlarmResetRequest(sender, e);
                    break;
                case EnumCmdNum.Cmd94_AlarmResponse:
                    Receive_Cmd94_AlarmResponse(sender, e);
                    break;
                case EnumCmdNum.Cmd131_TransferResponse:
                    break;
                case EnumCmdNum.Cmd132_TransferCompleteReport:
                    break;
                case EnumCmdNum.Cmd133_ControlZoneCancelResponse:
                    break;
                case EnumCmdNum.Cmd134_TransferEventReport:
                    break;
                case EnumCmdNum.Cmd135_CarrierIdRenameResponse:
                    break;
                case EnumCmdNum.Cmd136_TransferEventReport:
                    break;
                case EnumCmdNum.Cmd137_TransferCancelResponse:
                    break;
                case EnumCmdNum.Cmd139_PauseResponse:
                    break;
                case EnumCmdNum.Cmd141_ModeChangeResponse:
                    break;
                case EnumCmdNum.Cmd143_StatusResponse:
                    break;
                case EnumCmdNum.Cmd144_StatusReport:
                    break;
                case EnumCmdNum.Cmd145_PowerOnoffResponse:
                    break;
                case EnumCmdNum.Cmd151_AvoidResponse:
                    break;
                case EnumCmdNum.Cmd152_AvoidCompleteReport:
                    break;
                case EnumCmdNum.Cmd171_RangeTeachResponse:
                    break;
                case EnumCmdNum.Cmd172_RangeTeachCompleteReport:
                    break;
                case EnumCmdNum.Cmd174_AddressTeachReport:
                    break;
                case EnumCmdNum.Cmd191_AlarmResetResponse:
                    break;
                case EnumCmdNum.Cmd194_AlarmReport:
                    break;
                default:
                    break;
            }
        }
        object sendRecv_LockObj = new object();
        public TrxTcpIp.ReturnCode SendRecv<TSource2>(WrapperMessage wrapper, out TSource2 stRecv, out string rtnMsg)
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(sendRecv_LockObj, 30000, ref lockTaken);
                if (!lockTaken)
                    throw new TimeoutException("snedRecv time out lock happen");
                LogSendMsg(wrapper);
                return ClientAgent.TrxTcpIp.sendRecv_Google(wrapper, out stRecv, out rtnMsg);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(sendRecv_LockObj);
            }
        }

        private bool IsApplyOnly(EnumCmdNum cmdNum)
        {
            switch (cmdNum)
            {
                case EnumCmdNum.Cmd000_EmptyCommand:
                    break;
                case EnumCmdNum.Cmd31_TransferRequest:
                    break;
                case EnumCmdNum.Cmd32_TransferCompleteResponse:
                    break;
                case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                    return true;
                case EnumCmdNum.Cmd36_TransferEventResponse:
                    break;
                case EnumCmdNum.Cmd37_TransferCancelRequest:
                    break;
                case EnumCmdNum.Cmd39_PauseRequest:
                    break;
                case EnumCmdNum.Cmd41_ModeChange:
                    break;
                case EnumCmdNum.Cmd43_StatusRequest:
                    return true;
                case EnumCmdNum.Cmd44_StatusRequest:
                    break;
                case EnumCmdNum.Cmd51_AvoidRequest:
                    break;
                case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                    break;
                case EnumCmdNum.Cmd71_RangeTeachRequest:
                    break;
                case EnumCmdNum.Cmd72_RangeTeachCompleteResponse:
                    break;
                case EnumCmdNum.Cmd74_AddressTeachResponse:
                    break;
                case EnumCmdNum.Cmd91_AlarmResetRequest:
                    return true;
                case EnumCmdNum.Cmd94_AlarmResponse:
                    break;
                case EnumCmdNum.Cmd131_TransferResponse:
                    break;
                case EnumCmdNum.Cmd132_TransferCompleteReport:
                    break;
                case EnumCmdNum.Cmd133_ControlZoneCancelResponse:
                    break;
                case EnumCmdNum.Cmd134_TransferEventReport:
                    break;
                case EnumCmdNum.Cmd135_CarrierIdRenameResponse:
                    break;
                case EnumCmdNum.Cmd136_TransferEventReport:
                    break;
                case EnumCmdNum.Cmd137_TransferCancelResponse:
                    break;
                case EnumCmdNum.Cmd139_PauseResponse:
                    break;
                case EnumCmdNum.Cmd141_ModeChangeResponse:
                    break;
                case EnumCmdNum.Cmd143_StatusResponse:
                    break;
                case EnumCmdNum.Cmd144_StatusReport:
                    break;
                case EnumCmdNum.Cmd145_PowerOnoffResponse:
                    break;
                case EnumCmdNum.Cmd151_AvoidResponse:
                    break;
                case EnumCmdNum.Cmd152_AvoidCompleteReport:
                    break;
                case EnumCmdNum.Cmd171_RangeTeachResponse:
                    break;
                case EnumCmdNum.Cmd172_RangeTeachCompleteReport:
                    break;
                case EnumCmdNum.Cmd174_AddressTeachReport:
                    break;
                case EnumCmdNum.Cmd191_AlarmResetResponse:
                    break;
                case EnumCmdNum.Cmd194_AlarmReport:
                    break;
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        #endregion

        #region Thd Ask Reserve
        private void AskReserve()
        {
            while (true)
            {
                try
                {
                    if (IsAskReservePause || theVehicle.AseMoveStatus.IsMoveEnd)
                    {
                        SpinWait.SpinUntil(() => false, 50);
                    }

                    #region Pause And Stop Check
                    if (IsAskReservePause) continue;
                    if (!ClientAgent.IsConnection)
                    {
                        SpinWait.SpinUntil(() => false, 100);
                        continue;
                    }
                    #endregion

                    if (queNeedReserveSections.Any())
                    {
                        if (CanAskReserve())
                        {
                            ReserveOkAskNext = false;
                            queNeedReserveSections.TryPeek(out MapSection askReserveSection);

                            Send_Cmd136_AskReserve(askReserveSection);
                            SpinWait.SpinUntil(() => ReserveOkAskNext, agvcConnectorConfig.AskReserveIntervalMs);
                            ReserveOkAskNext = false;
                            SpinWait.SpinUntil(() => false, 5);
                        }
                    }

                    if (queSendWaitWrappers.Any())
                    {
                        queSendWaitWrappers.TryPeek(out SendWaitWrapper sendWaitWrapper);
                        string rtnMsg = "";
                        switch (sendWaitWrapper.Wrapper.ID)
                        {
                            case 132:
                                {
                                    ID_32_TRANS_COMPLETE_RESPONSE response = new ID_32_TRANS_COMPLETE_RESPONSE();
                                    OnMessageShowOnMainFormEvent?.Invoke(this, $"Send transfer complete report. [{sendWaitWrapper.Wrapper.TranCmpRep.CmpStatus}][queNeedReserveSections.Count={queNeedReserveSections.Count}]");
                                    TrxTcpIp.ReturnCode returnCode = SendRecv(sendWaitWrapper.Wrapper, out response, out rtnMsg);
                                    if (returnCode == TrxTcpIp.ReturnCode.Normal)
                                    {
                                        queSendWaitWrappers.TryDequeue(out SendWaitWrapper replyedWrapper);
                                        int waitTime = response.WaitTime;
                                        //SpinWait.SpinUntil(() => false, waitTime);
                                        mainFlowHandler.IsAgvcReplySendWaitMessage = true;
                                    }
                                    else
                                    {
                                        sendWaitWrapper.RetrySendWaitCounter--;
                                        if (sendWaitWrapper.RetrySendWaitCounter <= 0)
                                        {
                                            queSendWaitWrappers.TryDequeue(out SendWaitWrapper timeoutWrapper);
                                            string msg = $"TransferComplete send wait timeout[{timeoutWrapper.Wrapper.TranCmpRep.CmdID}][queNeedReserveSections.Count={queNeedReserveSections.Count}]";
                                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                                            OnSendRecvTimeoutEvent?.Invoke(this, default(EventArgs));
                                            mainFlowHandler.IsAgvcReplySendWaitMessage = true;
                                            mainFlowHandler.ResumeVisitTransferSteps();
                                        }
                                    }
                                }
                                break;
                            case 136:
                                {
                                    ID_36_TRANS_EVENT_RESPONSE response = new ID_36_TRANS_EVENT_RESPONSE();
                                    OnMessageShowOnMainFormEvent?.Invoke(this, $"Send transfer event report. [{sendWaitWrapper.Wrapper.ImpTransEventRep.EventType}][queNeedReserveSections.Count={queNeedReserveSections.Count}]");
                                    TrxTcpIp.ReturnCode returnCode = SendRecv(sendWaitWrapper.Wrapper, out response, out rtnMsg);
                                    if (returnCode == TrxTcpIp.ReturnCode.Normal)
                                    {
                                        queSendWaitWrappers.TryDequeue(out SendWaitWrapper replyedWrapper);
                                        switch (replyedWrapper.Wrapper.ImpTransEventRep.EventType)
                                        {
                                            case EventType.AdrOrMoveArrivals:
                                                OnAgvcAcceptMoveArrivalEvent?.Invoke(this, default(EventArgs));
                                                break;
                                            case EventType.LoadArrivals:
                                                OnAgvcAcceptLoadArrivalEvent?.Invoke(this, default(EventArgs));
                                                break;
                                            case EventType.UnloadArrivals:
                                                OnAgvcAcceptUnloadArrivalEvent?.Invoke(this, default(EventArgs));
                                                break;
                                            case EventType.LoadComplete:
                                                //OnAgvcAcceptLoadCompleteEvent?.Invoke(this, default(EventArgs));
                                                //SendRecv_Cmd136_CstIdReadReport();
                                                mainFlowHandler.IsAgvcReplySendWaitMessage = true;
                                                break;
                                            case EventType.UnloadComplete:
                                                OnAgvcAcceptUnloadCompleteEvent?.Invoke(this, default(EventArgs));
                                                break;
                                            case EventType.Bcrread:
                                                OnAgvcReplyCstIdReadEvent(response);
                                                mainFlowHandler.IsAgvcReplySendWaitMessage = true;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        sendWaitWrapper.RetrySendWaitCounter--;
                                        if (sendWaitWrapper.RetrySendWaitCounter <= 0)
                                        {
                                            queSendWaitWrappers.TryDequeue(out SendWaitWrapper timeoutWrapper);
                                            if (timeoutWrapper.Wrapper.ImpTransEventRep.EventType == EventType.Bcrread)
                                            {
                                                var cmdId = mainFlowHandler.GetCurTransferStep().CmdId;
                                                theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = GetCancelCompleteStatus(timeoutWrapper.Wrapper.ImpTransEventRep.BCRReadResult, theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus);
                                            }
                                            string msg = $"TransferEvent[{timeoutWrapper.Wrapper.ImpTransEventRep.EventType}] send wait timeout[{timeoutWrapper.Wrapper.TranCmpRep.CmdID}]";
                                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                                            OnSendRecvTimeoutEvent?.Invoke(this, default(EventArgs));
                                            mainFlowHandler.IsAgvcReplySendWaitMessage = true;
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                        SpinWait.SpinUntil(() => false, 500);
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
            }
        }
        public void StartAskReserve()
        {
            IsAskReservePause = false;
            thdAskReserve = new Thread(new ThreadStart(AskReserve));
            thdAskReserve.IsBackground = true;
            thdAskReserve.Start();
            OnMessageShowOnMainFormEvent?.Invoke(this, $"AgvcConnector : StartAskReserve");
        }
        public void PauseAskReserve()
        {
            IsAskReservePause = true;
            OnMessageShowOnMainFormEvent?.Invoke(this, $"AgvcConnector : PauseAskReserve");
        }
        public void ResumeAskReserve()
        {
            IsAskReservePause = false;
            OnMessageShowOnMainFormEvent?.Invoke(this, "AgvcConnector : ResumeAskReserve");
        }
        public bool CanDoReserveWork()
        {
            return !IsAskReservePause && !theVehicle.AseMoveStatus.IsMoveEnd;
        }
        private bool CanAskReserve()
        {
            return mainFlowHandler.IsMoveStep() && mainFlowHandler.CanVehMove() && !IsGotReserveOkSectionsFull() && !theVehicle.AseMoveStatus.IsMoveEnd;
        }
        public bool IsGotReserveOkSectionsFull()
        {
            if (agvcConnectorConfig.ReserveLengthMeter < 0) return false;
            int reserveOkSectionsTotalLength = GetReserveOkSectionsTotalLength();
            return reserveOkSectionsTotalLength >= agvcConnectorConfig.ReserveLengthMeter * 1000;
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
        public MapSection GetAskingReserveSection()
        {
            var getSectionOk = queNeedReserveSections.TryPeek(out MapSection mapSection);
            return getSectionOk ? mapSection : new MapSection();
        }
        public void SetupNeedReserveSections()
        {
            try
            {
                queNeedReserveSections = new ConcurrentQueue<MapSection>(theVehicle.AseMovingGuide.MovingSections);
                var msg = $"AgvcConnector : SetupNeedReserveSections[{QueMapSectionsToString(queNeedReserveSections)}][Count={queNeedReserveSections.Count}]";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public List<MapSection> GetNeedReserveSections()
        {
            return new List<MapSection>(queNeedReserveSections);
        }
        public List<MapSection> GetReserveOkSections()
        {
            return new List<MapSection>(queReserveOkSections);
        }
        public void DequeueGotReserveOkSections()
        {
            if (queReserveOkSections.IsEmpty)
            {
                var msg = $"AgvcConnector :queReserveOkSections.Count=[{queReserveOkSections.Count}], Dequeue Got Reserve Ok Sections Fail.";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                return;
            }
            else
            {
                queReserveOkSections.TryDequeue(out MapSection passSection);
                string passSectionId = passSection.Id;
                OnPassReserveSectionEvent?.Invoke(this, passSectionId);
                var msg = $"AgvcConnector : passSectionId = [{passSectionId}].";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
        }
        public void OnGetReserveOk(string sectionId)
        {
            if (queNeedReserveSections.IsEmpty)
            {
                var msg = $"AgvcConnector :  Reserve-got SectionId = [{sectionId}] fail, queNeedReserveSections.IsEmpty";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                return;
            }

            if (!theVehicle.AseMoveStatus.IsMoveEnd)
            {
                queNeedReserveSections.TryPeek(out MapSection needReserveSection);

                if (needReserveSection.Id == sectionId || "XXX" == sectionId)
                {
                    queNeedReserveSections.TryDequeue(out MapSection aReserveOkSection);
                    queReserveOkSections.Enqueue(aReserveOkSection);
                    OnReserveOkEvent?.Invoke(this, aReserveOkSection.Id);
                    //mainFlowHandler.AgvcConnector_GetReserveOkUpdateMoveControlNextPartMovePosition(needReserveSection);
                    quePartMoveSections.Enqueue(aReserveOkSection);
                    if (queNeedReserveSections.IsEmpty)
                    {
                        RefreshPartMoveSections();
                    }
                    ReserveOkAskNext = true;
                }
                else
                {
                    var msg = $"AgvcConnector : Reserve-got SectionId = [{sectionId}] fail, needReserveSection.Id=[{needReserveSection.Id}]";
                    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                }
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
                        EnumKeepOrGo enumKeepOrGo = EnumKeepOrGo.Keep;
                        // End of PartMoveSections => Go
                        if (i + 1 == partMoveSections.Count)
                        {
                            enumKeepOrGo = EnumKeepOrGo.Go;

                        }//partMoveSections[i] and partMoveSections[i+1] exist
                        else if (HeadDirectionChange(partMoveSections[i + 1]))
                        {
                            enumKeepOrGo = EnumKeepOrGo.Go;
                        }
                        else if (partMoveSections[i].Type != partMoveSections[i + 1].Type)  //Section Type Change => Go
                        {
                            enumKeepOrGo = EnumKeepOrGo.Go;
                        }

                        mainFlowHandler.AgvcConnector_GetReserveOkUpdateMoveControlNextPartMovePosition(partMoveSections[i], enumKeepOrGo);
                        SpinWait.SpinUntil(() => false, 200);
                    }
                    quePartMoveSections = new ConcurrentQueue<MapSection>();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private bool HeadDirectionChange(MapSection mapSection)
        {
            return mapSection.HeadAddress.VehicleHeadAngle != mapSection.TailAddress.VehicleHeadAngle;
        }

        public void ClearAllReserve()
        {
            IsAskReservePause = true;
            ClearGotReserveOkSections();
            ClearNeedReserveSections();
            IsAskReservePause = false;
            var msg = $"AgvcConnector : ClearAllReserve.";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
        }
        public void AskGuideAddressesAndSections(MoveCmdInfo moveCmdInfo)
        {
            Send_Cmd138_GuideInfoRequest(theVehicle.AseMoveStatus.LastAddress.Id, moveCmdInfo.EndAddress.Id);
        }
        #endregion

        public void SendAgvcConnectorFormCommands(int cmdNum, Dictionary<string, string> pairs)
        {
            try
            {
                WrapperMessage wrappers = new WrapperMessage();

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
                            wrappers.ID = WrapperMessage.TransReqFieldNumber;
                            wrappers.TransReq = aCmd;
                            break;
                        }
                    case EnumCmdNum.Cmd32_TransferCompleteResponse:
                        {
                            ID_32_TRANS_COMPLETE_RESPONSE aCmd = new ID_32_TRANS_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.TranCmpRespFieldNumber;
                            wrappers.TranCmpResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd35_CarrierIdRenameRequest:
                        {
                            ID_35_CST_ID_RENAME_REQUEST aCmd = new ID_35_CST_ID_RENAME_REQUEST();
                            aCmd.NEWCSTID = pairs["NEWCSTID"];
                            aCmd.OLDCSTID = pairs["OLDCSTID"];

                            wrappers.ID = WrapperMessage.CSTIDRenameReqFieldNumber;
                            wrappers.CSTIDRenameReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd36_TransferEventResponse:
                        {
                            ID_36_TRANS_EVENT_RESPONSE aCmd = new ID_36_TRANS_EVENT_RESPONSE();
                            aCmd.IsBlockPass = PassTypeParse(pairs["IsBlockPass"]);
                            aCmd.IsReserveSuccess = ReserveResultParse(pairs["IsReserveSuccess"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.ImpTransEventRespFieldNumber;
                            wrappers.ImpTransEventResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd37_TransferCancelRequest:
                        {
                            ID_37_TRANS_CANCEL_REQUEST aCmd = new ID_37_TRANS_CANCEL_REQUEST();
                            aCmd.CmdID = pairs["CmdID"];

                            wrappers.ID = WrapperMessage.TransCancelReqFieldNumber;
                            wrappers.TransCancelReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd39_PauseRequest:
                        {
                            ID_39_PAUSE_REQUEST aCmd = new ID_39_PAUSE_REQUEST();
                            aCmd.EventType = PauseEventParse(pairs["EventType"]);
                            aCmd.PauseType = PauseTypeParse(pairs["PauseType"]);

                            wrappers.ID = WrapperMessage.PauseReqFieldNumber;
                            wrappers.PauseReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd41_ModeChange:
                        {
                            ID_41_MODE_CHANGE_REQ aCmd = new ID_41_MODE_CHANGE_REQ();
                            aCmd.OperatingVHMode = OperatingVHModeParse(pairs["OperatingVHMode"]);

                            wrappers.ID = WrapperMessage.ModeChangeReqFieldNumber;
                            wrappers.ModeChangeReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd43_StatusRequest:
                        {
                            ID_43_STATUS_REQUEST aCmd = new ID_43_STATUS_REQUEST();

                            wrappers.ID = WrapperMessage.StatusReqFieldNumber;
                            wrappers.StatusReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd44_StatusRequest:
                        {
                            ID_44_STATUS_CHANGE_RESPONSE aCmd = new ID_44_STATUS_CHANGE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.StatusChangeRespFieldNumber;
                            wrappers.StatusChangeResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd45_PowerOnoffRequest:
                        {
                            ID_45_POWER_OPE_REQ aCmd = new ID_45_POWER_OPE_REQ();
                            aCmd.OperatingPowerMode = OperatingPowerModeParse(pairs["OperatingPowerMode"]);

                            wrappers.ID = WrapperMessage.PowerOpeReqFieldNumber;
                            wrappers.PowerOpeReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd51_AvoidRequest:
                        {
                            ID_51_AVOID_REQUEST aCmd = new ID_51_AVOID_REQUEST();
                            aCmd.GuideAddresses.AddRange(StringSpilter(pairs["GuideAddresses"]));
                            aCmd.GuideSections.AddRange(StringSpilter(pairs["GuideSections"]));

                            wrappers.ID = WrapperMessage.AvoidReqFieldNumber;
                            wrappers.AvoidReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd52_AvoidCompleteResponse:
                        {
                            ID_52_AVOID_COMPLETE_RESPONSE aCmd = new ID_52_AVOID_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AvoidCompleteRespFieldNumber;
                            wrappers.AvoidCompleteResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd71_RangeTeachRequest:
                        {
                            ID_71_RANGE_TEACHING_REQUEST aCmd = new ID_71_RANGE_TEACHING_REQUEST();
                            aCmd.FromAdr = pairs["FromAdr"];
                            aCmd.ToAdr = pairs["ToAdr"];

                            wrappers.ID = WrapperMessage.RangeTeachingReqFieldNumber;
                            wrappers.RangeTeachingReq = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd72_RangeTeachCompleteResponse:
                        {
                            ID_72_RANGE_TEACHING_COMPLETE_RESPONSE aCmd = new ID_72_RANGE_TEACHING_COMPLETE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.RangeTeachingCmpRespFieldNumber;
                            wrappers.RangeTeachingCmpResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd74_AddressTeachResponse:
                        {
                            ID_74_ADDRESS_TEACH_RESPONSE aCmd = new ID_74_ADDRESS_TEACH_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AddressTeachRespFieldNumber;
                            wrappers.AddressTeachResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd91_AlarmResetRequest:
                        {
                            ID_91_ALARM_RESET_REQUEST aCmd = new ID_91_ALARM_RESET_REQUEST();

                            wrappers.ID = WrapperMessage.AlarmResetReqFieldNumber;
                            wrappers.AlarmResetReq = aCmd;


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

                            wrappers.ID = WrapperMessage.TransRespFieldNumber;
                            wrappers.TransResp = aCmd;


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

                            wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;
                            wrappers.TranCmpRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd134_TransferEventReport:
                        {
                            ID_134_TRANS_EVENT_REP aCmd = new ID_134_TRANS_EVENT_REP();
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeParse(pairs["EventType"]);
                            aCmd.DrivingDirection = (DriveDirction)Enum.Parse(typeof(DriveDirction), pairs["DrivingDirection"].Trim());

                            wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                            wrappers.TransEventRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd135_CarrierIdRenameResponse:
                        {
                            ID_135_CST_ID_RENAME_RESPONSE aCmd = new ID_135_CST_ID_RENAME_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.CSTIDRenameRespFieldNumber;
                            wrappers.CSTIDRenameResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd136_TransferEventReport:
                        {
                            ID_136_TRANS_EVENT_REP aCmd = new ID_136_TRANS_EVENT_REP();
                            aCmd.CSTID = pairs["CSTID"];
                            aCmd.CurrentAdrID = pairs["CurrentAdrID"];
                            aCmd.CurrentSecID = pairs["CurrentSecID"];
                            aCmd.EventType = EventTypeParse(pairs["EventType"]);

                            wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                            wrappers.ImpTransEventRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd137_TransferCancelResponse:
                        {
                            ID_137_TRANS_CANCEL_RESPONSE aCmd = new ID_137_TRANS_CANCEL_RESPONSE();
                            aCmd.CmdID = pairs["CmdID"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.TransCancelRespFieldNumber;
                            wrappers.TransCancelResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd139_PauseResponse:
                        {
                            ID_139_PAUSE_RESPONSE aCmd = new ID_139_PAUSE_RESPONSE();
                            aCmd.EventType = PauseEventParse(pairs["EventType"]);
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.PauseRespFieldNumber;
                            wrappers.PauseResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd141_ModeChangeResponse:
                        {
                            ID_141_MODE_CHANGE_RESPONSE aCmd = new ID_141_MODE_CHANGE_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.ModeChangeRespFieldNumber;
                            wrappers.ModeChangeResp = aCmd;


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

                            wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                            wrappers.StatusReqResp = aCmd;


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

                            wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                            wrappers.StatueChangeRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd151_AvoidResponse:
                        {
                            ID_151_AVOID_RESPONSE aCmd = new ID_151_AVOID_RESPONSE();
                            aCmd.NgReason = pairs["NgReason"];
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AvoidRespFieldNumber;
                            wrappers.AvoidResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd152_AvoidCompleteReport:
                        {
                            ID_152_AVOID_COMPLETE_REPORT aCmd = new ID_152_AVOID_COMPLETE_REPORT();
                            aCmd.CmpStatus = int.Parse(pairs["CmpStatus"]);

                            wrappers.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                            wrappers.AvoidCompleteRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd171_RangeTeachResponse:
                        {
                            ID_171_RANGE_TEACHING_RESPONSE aCmd = new ID_171_RANGE_TEACHING_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.RangeTeachingRespFieldNumber;
                            wrappers.RangeTeachingResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd172_RangeTeachCompleteReport:
                        {
                            ID_172_RANGE_TEACHING_COMPLETE_REPORT aCmd = new ID_172_RANGE_TEACHING_COMPLETE_REPORT();
                            aCmd.CompleteCode = int.Parse(pairs["CompleteCode"]);
                            aCmd.FromAdr = pairs["FromAdr"];
                            aCmd.SecDistance = uint.Parse(pairs["SecDistance"]);
                            aCmd.ToAdr = pairs["ToAdr"];

                            wrappers.ID = WrapperMessage.RangeTeachingCmpRepFieldNumber;
                            wrappers.RangeTeachingCmpRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd174_AddressTeachReport:
                        {
                            ID_174_ADDRESS_TEACH_REPORT aCmd = new ID_174_ADDRESS_TEACH_REPORT();
                            aCmd.Addr = pairs["Addr"];
                            aCmd.Position = int.Parse(pairs["Position"]);

                            wrappers.ID = WrapperMessage.AddressTeachRepFieldNumber;
                            wrappers.AddressTeachRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd191_AlarmResetResponse:
                        {
                            ID_191_ALARM_RESET_RESPONSE aCmd = new ID_191_ALARM_RESET_RESPONSE();
                            aCmd.ReplyCode = int.Parse(pairs["ReplyCode"]);

                            wrappers.ID = WrapperMessage.AlarmResetRespFieldNumber;
                            wrappers.AlarmResetResp = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd194_AlarmReport:
                        {
                            ID_194_ALARM_REPORT aCmd = new ID_194_ALARM_REPORT();
                            aCmd.ErrCode = pairs["ErrCode"];
                            aCmd.ErrDescription = pairs["ErrDescription"];
                            aCmd.ErrStatus = ErrorStatusParse(pairs["ErrStatus"]);

                            wrappers.ID = WrapperMessage.AlarmRepFieldNumber;
                            wrappers.AlarmRep = aCmd;


                            break;
                        }
                    case EnumCmdNum.Cmd000_EmptyCommand:
                    default:
                        {
                            ID_1_HOST_BASIC_INFO_VERSION_REP aCmd = new ID_1_HOST_BASIC_INFO_VERSION_REP();

                            wrappers.ID = WrapperMessage.HostBasicInfoRepFieldNumber;
                            wrappers.HostBasicInfoRep = aCmd;


                            break;
                        }
                }

                SendCommandWrapper(wrappers);  //似乎是SendFunction底層會咬住等待回應所以開THD去發  
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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

        public void SetlAlarmToAgvc(object sender, Alarm alarm)
        {
            if (theVehicle.ErrorStatus == VhStopSingle.Off && alarmHandler.HasAlarm)
            {
                theVehicle.ErrorStatus = VhStopSingle.On;
                StatusChangeReport();
            }
            Send_Cmd194_AlarmReport(alarm.Id.ToString(), ErrorStatus.ErrSet);
        }
        public void ResetAllAlarmsToAgvc(object sender, EventArgs eventArgs)
        {
            if (theVehicle.ErrorStatus == VhStopSingle.On)
            {
                theVehicle.ErrorStatus = VhStopSingle.Off;
                StatusChangeReport();
            }
            Send_Cmd194_AlarmReport("0", ErrorStatus.ErrReset);

            //foreach (var alarm in alarms)
            //{
            //    Send_Cmd194_AlarmReport(alarm.Id.ToString(), ErrorStatus.ErrReset);
            //}
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
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            var lastPosition = aseMoveStatus.LastMapPosition;
            if (Math.Abs(lastPosition.X - lastReportPosition.X) <= agvcConnectorConfig.NeerlyNoMoveRangeMm && Math.Abs(lastPosition.Y - lastReportPosition.Y) <= agvcConnectorConfig.NeerlyNoMoveRangeMm)
            {
                return true;
            }
            else
            {
                lastReportPosition = lastPosition;
                return false;
            }
        }
        public void ReportLoadArrival(string cmdId)
        {
            SendRecv_Cmd136_TransferEventReport(EventType.LoadArrivals, cmdId);
        }
        public void Loading(string cmdId)
        {
            Send_Cmd136_TransferEventReport(EventType.Vhloading, cmdId);
        }
        public void CstIdReadReport(TransferStep transferStep, EnumCstIdReadResult result)
        {
            //SendRecv_Cmd136_CstIdReadReport();
            //return Send_Cmd136_CstIdReadReport(transferStep, result);
        }
        public void TransferComplete(AgvcTransCmd agvcTransCmd)
        {
            AgvcTransCmd cmd = agvcTransCmd;
            SendRecv_Cmd132_TransferCompleteReport(cmd, 0);
        }
        public void LoadComplete(string cmdId)
        {
            AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer[cmdId];
            agvcTransCmd.EnrouteState = CommandState.UnloadEnroute;
            StatusChangeReport();
            SendRecv_Cmd136_TransferEventReport(EventType.LoadComplete, cmdId);
        }
        public void ReportUnloadArrival(string cmdId)
        {
            SendRecv_Cmd136_TransferEventReport(EventType.UnloadArrivals, cmdId);
        }
        public void Unloading(string cmdId)
        {
            Send_Cmd136_TransferEventReport(EventType.Vhunloading, cmdId);
        }
        public void UnloadComplete(string cmdId)
        {
            AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer[cmdId];
            agvcTransCmd.EnrouteState = CommandState.None;
            StatusChangeReport();
            SendRecv_Cmd136_TransferEventReport(EventType.UnloadComplete, cmdId);
        }
        public void MoveArrival()
        {
            Send_Cmd134_TransferEventReport(EventType.AdrOrMoveArrivals);
        }
        public void AvoidComplete()
        {
            Send_Cmd152_AvoidCompleteReport(0);
            mainFlowHandler.GoNextTransferStep = true;
        }
        public void AvoidFail()
        {
            Send_Cmd152_AvoidCompleteReport(1);
        }
        public bool IsAskReserveAlive() => (thdAskReserve != null) && thdAskReserve.IsAlive;
        public void NoCommand()
        {
            //theVehicle.ActionStatus = VHActionStatus.NoCommand;
            StatusChangeReport();
        }
        public void Commanding()
        {
            //theVehicle.ActionStatus = VHActionStatus.Commanding;
            StatusChangeReport();
        }
        public void ReplyTransferCommand(string cmdId, CommandActionType type, ushort seqNum, int replyCode, string reason)
        {
            Send_Cmd131_TransferResponse(cmdId, type, seqNum, replyCode, reason);
        }
        public void ReplyAvoidCommand(AseMovingGuide aseMovingGuide, int replyCode, string reason)
        {
            Send_Cmd151_AvoidResponse(aseMovingGuide.SeqNum, replyCode, reason);
        }
        public void ChargHandshaking()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusHandshaking;
            StatusChangeReport();
        }
        public void Charging()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusCharging;
            StatusChangeReport();
        }
        public void ChargeOff()
        {
            theVehicle.ChargeStatus = VhChargeStatus.ChargeStatusNone;
            StatusChangeReport();
        }
        public void PauseReply(ushort seqNum, int replyCode, PauseEvent type)
        {
            Send_Cmd139_PauseResponse(seqNum, replyCode, type);
        }
        public void CancelAbortReply(ushort iSeqNum, int replyCode, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            Send_Cmd137_TransferCancelResponse(iSeqNum, replyCode, receive);
        }
        public void DoOverride(ID_31_TRANS_REQUEST transRequest, ushort iSeqNum)
        {
            //AgvcOverrideCmd agvcOverrideCmd = (AgvcOverrideCmd)ConvertAgvcTransCmdIntoPackage(transRequest, iSeqNum);
            //ShowTransferCmdToForm(agvcOverrideCmd);
            //OnOverrideCommandEvent?.Invoke(this, agvcOverrideCmd);
        }
        public void DoBasicTransferCmd(ID_31_TRANS_REQUEST transRequest, ushort iSeqNum)
        {
            AgvcTransCmd agvcTransCmd = ConvertAgvcTransCmdIntoPackage(transRequest, iSeqNum);
            ShowTransferCmdToForm(agvcTransCmd);
            OnInstallTransferCommandEvent?.Invoke(this, agvcTransCmd);
        }
        public void StatusChangeReport()
        {
            Send_Cmd144_StatusChangeReport();
        }
        private void ShowTransferCmdToForm(AgvcTransCmd agvcTransCmd)
        {
            var msg = $"\r\n Get {agvcTransCmd.AgvcTransCommandType},\r\n Load Adr={agvcTransCmd.LoadAddressId}, Load Port Id={agvcTransCmd.LoadPortId},\r\n Unload Adr={agvcTransCmd.UnloadAddressId}, Unload Port Id={agvcTransCmd.UnloadPortId}.";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
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
                if (theVehicle.AutoState == EnumAutoState.Auto)
                {
                    ID_194_ALARM_REPORT iD_194_ALARM_REPORT = new ID_194_ALARM_REPORT();
                    iD_194_ALARM_REPORT.ErrCode = alarmCode;
                    iD_194_ALARM_REPORT.ErrStatus = status;

                    WrapperMessage wrappers = new WrapperMessage();
                    wrappers.ID = WrapperMessage.AlarmRepFieldNumber;
                    wrappers.AlarmRep = iD_194_ALARM_REPORT;

                    SendCommandWrapper(wrappers);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd91_AlarmResetRequest(object sender, TcpIpEventArgs e)
        {
            ID_91_ALARM_RESET_REQUEST receive = (ID_91_ALARM_RESET_REQUEST)e.objPacket;

            alarmHandler.ResetAllAlarmFromAgvc();

            int replyCode = 0;
            Send_Cmd191_AlarmResetResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd191_AlarmResetResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_191_ALARM_RESET_RESPONSE iD_191_ALARM_RESET_RESPONSE = new ID_191_ALARM_RESET_RESPONSE();
                iD_191_ALARM_RESET_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AlarmResetRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.AlarmResetResp = iD_191_ALARM_RESET_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd74_AddressTeachResponse(object sender, TcpIpEventArgs e)
        {
            ID_74_ADDRESS_TEACH_RESPONSE receive = (ID_74_ADDRESS_TEACH_RESPONSE)e.objPacket;



        }
        public void Send_Cmd174_AddressTeachReport(string addressId, int position)
        {
            try
            {
                //TODO: Teaching port address

                ID_174_ADDRESS_TEACH_REPORT iD_174_ADDRESS_TEACH_REPORT = new ID_174_ADDRESS_TEACH_REPORT();
                iD_174_ADDRESS_TEACH_REPORT.Addr = addressId;
                iD_174_ADDRESS_TEACH_REPORT.Position = position;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AddressTeachRepFieldNumber;
                wrappers.AddressTeachRep = iD_174_ADDRESS_TEACH_REPORT;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                ID_152_AVOID_COMPLETE_REPORT iD_152_AVOID_COMPLETE_REPORT = new ID_152_AVOID_COMPLETE_REPORT();
                iD_152_AVOID_COMPLETE_REPORT.CmpStatus = completeStatus;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AvoidCompleteRepFieldNumber;
                wrappers.AvoidCompleteRep = iD_152_AVOID_COMPLETE_REPORT;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd51_AvoidRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_51_AVOID_REQUEST receive = (ID_51_AVOID_REQUEST)e.objPacket;
                OnMessageShowOnMainFormEvent?.Invoke(this, $" Get Avoid Command");
                AseMovingGuide aseMovingGuide = new AseMovingGuide(receive, e.iSeqNum);
                ShowAvoidRequestToForm(aseMovingGuide);
                OnAvoideRequestEvent?.Invoke(this, aseMovingGuide);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd151_AvoidResponse(ushort seqNum, int replyCode, string reason)
        {
            try
            {
                ID_151_AVOID_RESPONSE iD_151_AVOID_RESPONSE = new ID_151_AVOID_RESPONSE();
                iD_151_AVOID_RESPONSE.ReplyCode = replyCode;
                iD_151_AVOID_RESPONSE.NgReason = reason;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AvoidRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.AvoidResp = iD_151_AVOID_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void Receive_Cmd44_StatusRequest(object sender, TcpIpEventArgs e)
        {
            ID_44_STATUS_CHANGE_RESPONSE receive = (ID_44_STATUS_CHANGE_RESPONSE)e.objPacket; // Cmd43's object is empty
        }
        public void Send_Cmd144_StatusChangeReport()
        {
            try
            {
                ID_144_STATUS_CHANGE_REP report = GetCmd144ReportBody();

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = report;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }
        public void Send_Cmd144_StatusChangeReport(string sender, ushort batteryPercentage)
        {
            try
            {
                ID_144_STATUS_CHANGE_REP report = GetCmd144ReportBody();
                report.BatteryCapacity = batteryPercentage;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = report;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }
        private ID_144_STATUS_CHANGE_REP GetCmd144ReportBody()
        {
            ID_144_STATUS_CHANGE_REP report = new ID_144_STATUS_CHANGE_REP();
            report.ModeStatus = VHModeStatusParse(theVehicle.AutoState);
            report.PowerStatus = theVehicle.PowerStatus;
            report.ObstacleStatus = theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Block ? VhStopSingle.On : VhStopSingle.Off;
            report.ReserveStatus = theVehicle.AseMovingGuide.ReserveStop;
            report.BlockingStatus = theVehicle.BlockingStatus;
            report.PauseStatus = theVehicle.AseMovingGuide.PauseStatus;
            report.ErrorStatus = theVehicle.ErrorStatus;
            report.DrivingDirection = theVehicle.DrivingDirection;
            report.BatteryCapacity = (uint)theVehicle.AseBatteryStatus.Percentage;
            report.BatteryTemperature = (int)theVehicle.AseBatteryStatus.Temperature;
            report.ChargeStatus = VhChargeStatusParse(theVehicle.IsCharging);
            report.XAxis = theVehicle.AseMoveStatus.LastMapPosition.X;
            report.YAxis = theVehicle.AseMoveStatus.LastMapPosition.Y;
            report.Speed = theVehicle.AseMoveStatus.Speed;
            AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
            report.WillPassGuideSection.Clear();
            report.WillPassGuideSection.AddRange(aseMovingGuide.GuideSectionIds);

            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
            report.CurrentSecID = aseMoveStatus.LastSection.Id;
            report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
            report.DirectionAngle = aseMoveStatus.MovingDirection;
            report.VehicleAngle = aseMoveStatus.HeadDirection;

            List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
            report.CmdId1 = agvcTransCmds.Count > 0 ? agvcTransCmds[0].CommandId : "";
            report.CmsState1 = agvcTransCmds.Count > 0 ? agvcTransCmds[0].EnrouteState : CommandState.None;
            report.CmdId2 = agvcTransCmds.Count > 1 ? agvcTransCmds[1].CommandId : "";
            report.CmsState2 = agvcTransCmds.Count > 1 ? agvcTransCmds[1].EnrouteState : CommandState.None;

            report.HasCstL = theVehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
            report.CstIdL = theVehicle.AseCarrierSlotL.CarrierId;
            report.HasCstR = theVehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
            report.CstIdR = theVehicle.AseCarrierSlotR.CarrierId;

            report.ActionStatus = agvcTransCmds.Count > 0 ? VHActionStatus.Commanding : VHActionStatus.NoCommand;

            return report;
        }

        private void Receive_Cmd43_StatusRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                Send_Cmd143_StatusResponse(e.iSeqNum);

                ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket;
                var receiveTime = receive.SystemTime; //可以記錄AGVC最後發送時間
                SetSystemTime(receiveTime);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd143_StatusResponse(ushort seqNum)
        {
            try
            {
                ID_143_STATUS_RESPONSE response = new ID_143_STATUS_RESPONSE();
                response.ModeStatus = VHModeStatusParse(theVehicle.AutoState);

                response.PowerStatus = theVehicle.PowerStatus;
                response.ObstacleStatus = theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Block ? VhStopSingle.On : VhStopSingle.Off;
                response.ReserveStatus = theVehicle.AseMovingGuide.ReserveStop;
                response.BlockingStatus = theVehicle.BlockingStatus;
                response.PauseStatus = theVehicle.AseMovingGuide.PauseStatus;
                response.ErrorStatus = theVehicle.ErrorStatus;
                response.ObstDistance = theVehicle.ObstDistance;
                response.ObstVehicleID = theVehicle.ObstVehicleID;
                response.HasCstL = theVehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
                response.CstIdL = theVehicle.AseCarrierSlotL.CarrierId;
                response.HasCstR = theVehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty ? VhLoadCSTStatus.NotExist : VhLoadCSTStatus.Exist;
                response.CstIdR = theVehicle.AseCarrierSlotR.CarrierId;
                response.ChargeStatus = VhChargeStatusParse(theVehicle.IsCharging);
                response.BatteryCapacity = (uint)theVehicle.AseBatteryStatus.Percentage;
                response.BatteryTemperature = (int)theVehicle.AseBatteryStatus.Temperature;
                response.XAxis = theVehicle.AseMoveStatus.LastMapPosition.X;
                response.YAxis = theVehicle.AseMoveStatus.LastMapPosition.Y;
                response.DirectionAngle = theVehicle.AseMoveStatus.MovingDirection;
                response.VehicleAngle = theVehicle.AseMoveStatus.HeadDirection;
                response.Speed = theVehicle.AseMoveStatus.Speed;
                response.StoppedBlockID = theVehicle.StoppedBlockID;

                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                response.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                response.CurrentSecID = aseMoveStatus.LastSection.Id;
                response.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                response.DrivingDirection = DriveDirctionParse(aseMoveStatus.LastSection.CmdDirection);

                List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                response.CmdId1 = agvcTransCmds.Count > 0 ? agvcTransCmds[0].CommandId : "";
                response.CmsState1 = agvcTransCmds.Count > 0 ? agvcTransCmds[0].EnrouteState : CommandState.None;
                response.CmdId2 = agvcTransCmds.Count > 1 ? agvcTransCmds[1].CommandId : "";
                response.CmsState2 = agvcTransCmds.Count > 1 ? agvcTransCmds[1].EnrouteState : CommandState.None;
                response.ActionStatus = agvcTransCmds.Count > 0 ? VHActionStatus.Commanding : VHActionStatus.NoCommand;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.StatusReqResp = response;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd41_ModeChange(object sender, TcpIpEventArgs e)
        {
            ID_41_MODE_CHANGE_REQ receive = (ID_41_MODE_CHANGE_REQ)e.objPacket;
            int replyCode = 1;
            Send_Cmd141_ModeChangeResponse(e.iSeqNum, replyCode);
        }
        public void Send_Cmd141_ModeChangeResponse(ushort seqNum, int replyCode)
        {
            try
            {
                ID_141_MODE_CHANGE_RESPONSE iD_141_MODE_CHANGE_RESPONSE = new ID_141_MODE_CHANGE_RESPONSE();
                iD_141_MODE_CHANGE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ModeChangeRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ModeChangeResp = iD_141_MODE_CHANGE_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd39_PauseRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_39_PAUSE_REQUEST receive = (ID_39_PAUSE_REQUEST)e.objPacket;

                var msg = $"AgvcConnector :  Get [{receive.EventType}]Command.";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                switch (receive.EventType)
                {
                    case PauseEvent.Continue:
                        mainFlowHandler.AgvcConnector_OnCmdResumeEvent(e.iSeqNum, receive.EventType);
                        break;
                    case PauseEvent.Pause:
                        mainFlowHandler.AgvcConnector_OnCmdPauseEvent(e.iSeqNum, receive.EventType);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd139_PauseResponse(ushort seqNum, int replyCode, PauseEvent eventType)
        {
            try
            {
                ID_139_PAUSE_RESPONSE response = new ID_139_PAUSE_RESPONSE();
                response.EventType = eventType;
                response.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.PauseRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.PauseResp = response;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd38_GuideInfoResponse(object sender, TcpIpEventArgs e)
        {
            try
            {
                if (IsOptimizingSteps) return;

                ID_38_GUIDE_INFO_RESPONSE response = (ID_38_GUIDE_INFO_RESPONSE)e.objPacket;
                ShowGuideInfoResponse(response);
                theVehicle.AseMovingGuide = new AseMovingGuide(response);
                ShowAseMovigGuideSectionAndAddressList();
                IsAskReservePause = true;
                ClearAllReserve();
                mainFlowHandler.SetupAseMovingGuideMovingSections();
                SetupNeedReserveSections();
                theVehicle.AseMoveStatus.IsMoveEnd = false;
                IsAskReservePause = false;
                StatusChangeReport();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void ShowAseMovigGuideSectionAndAddressList()
        {
            try
            {
                OnMessageShowOnMainFormEvent?.Invoke(this, theVehicle.AseMovingGuide.GetInfo());
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
                string msg = $" Get Guide Address[{info.FromTo.From}]->[{info.FromTo.To}],Guide=[{info.GuideSections.Count}] Sections 和 [{info.GuideAddresses.Count}] Addresses.";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);
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
                ID_138_GUIDE_INFO_REQUEST request = new ID_138_GUIDE_INFO_REQUEST();
                FitGuideInfos(request.FromToAdrList, fromAddress, toAddress);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.GuideInfoReqFieldNumber;
                wrappers.GuideInfoReq = request;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
        public int SendRecv_Cmd138_GuideInfoRequest(string fromAddress, string toAddress)
        {
            try
            {

                ID_138_GUIDE_INFO_REQUEST request = new ID_138_GUIDE_INFO_REQUEST();
                FitGuideInfos(request.FromToAdrList, fromAddress, toAddress);

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.GuideInfoReqFieldNumber;
                wrapper.GuideInfoReq = request;

                LogSendMsg(wrapper);

                ID_38_GUIDE_INFO_RESPONSE response = new ID_38_GUIDE_INFO_RESPONSE();
                OnMessageShowOnMainFormEvent?.Invoke(this, $"Send GuideInfoRequest. [{fromAddress}]->[{toAddress}]");
                string rtnMsg = "";
                TrxTcpIp.ReturnCode returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(wrapper, out response, out rtnMsg);
                if (returnCode == TrxTcpIp.ReturnCode.Normal)
                {
                    OnMessageShowOnMainFormEvent?.Invoke(this, $"Recv GuideInfoResponse. [{fromAddress}]->[{toAddress}]");
                    return (int)response.GuideInfoList[0].Distance;
                }
                else
                {
                    OnMessageShowOnMainFormEvent?.Invoke(this, $"Recv GuideInfoResponse Timeout. [{fromAddress}]->[{toAddress}]");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                OnMessageShowOnMainFormEvent?.Invoke(this, $"Send GuideInfoRequest Exception. [{fromAddress}]->[{toAddress}]");

                return -1;
            }
        }

        public void Receive_Cmd37_TransferCancelRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                int replyCode = 0;
                ID_37_TRANS_CANCEL_REQUEST receive = (ID_37_TRANS_CANCEL_REQUEST)e.objPacket;

                var msg = $"AgvcConnector : Get [{receive.CancelAction}] command";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                var cmdId = receive.CmdID.Trim();

                if (theVehicle.AgvcTransCmdBuffer.Count == 0)
                {
                    replyCode = 1;
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive);
                    var ngMsg = $"AgvcConnector : Vehicle Idle, reject [{receive.CancelAction}].";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                if (receive.CancelAction == CancelActionType.CmdEms)
                {
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive);
                    alarmHandler.SetAlarmFromAgvm(000037);
                    OnStopClearAndResetEvent?.Invoke(this, default(EventArgs));
                    return;
                }

                if (!theVehicle.AgvcTransCmdBuffer.ContainsKey(cmdId))
                {
                    replyCode = 1;
                    Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive);
                    var ngMsg = $"AgvcConnector : No [{cmdId}] to cancel, reject [{receive.CancelAction}].";
                    OnMessageShowOnMainFormEvent?.Invoke(this, ngMsg);
                    return;
                }

                switch (receive.CancelAction)
                {
                    case CancelActionType.CmdCancel:
                    case CancelActionType.CmdAbort:
                        mainFlowHandler.AgvcConnector_OnCmdCancelAbortEvent(e.iSeqNum, receive);
                        break;
                    case CancelActionType.CmdCancelIdMismatch:
                    case CancelActionType.CmdCancelIdReadFailed:
                        alarmHandler.ResetAllAlarmsFromAgvm();
                        OnStopClearAndResetEvent?.Invoke(this, default(EventArgs));
                        break;
                    case CancelActionType.CmdNone:
                    default:
                        replyCode = 1;
                        Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd137_TransferCancelResponse(ushort seqNum, int replyCode, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            try
            {
                ID_137_TRANS_CANCEL_RESPONSE iD_137_TRANS_CANCEL_RESPONSE = new ID_137_TRANS_CANCEL_RESPONSE();
                iD_137_TRANS_CANCEL_RESPONSE.CmdID = receive.CmdID;
                iD_137_TRANS_CANCEL_RESPONSE.CancelAction = receive.CancelAction;
                iD_137_TRANS_CANCEL_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransCancelRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TransCancelResp = iD_137_TRANS_CANCEL_RESPONSE;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd36_TransferEventResponse(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_36_TRANS_EVENT_RESPONSE receive = (ID_36_TRANS_EVENT_RESPONSE)e.objPacket;
                if (receive.EventType == EventType.ReserveReq)
                {
                }
                else if (receive.EventType == EventType.Bcrread)
                {
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void Send_Cmd136_TransferEventReport(EventType eventType, string cmdId)
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = eventType;
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.CmdID = cmdId;


                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = report;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }
        private void SendRecv_Cmd136_TransferEventReport(EventType eventType, string cmdId)
        {
            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = eventType;
                report.CurrentAdrID = theVehicle.AseMoveStatus.LastAddress.Id;
                report.CurrentSecID = theVehicle.AseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)theVehicle.AseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.CmdID = cmdId;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrapper.ImpTransEventRep = report;

                //SendWaitWrapper sendWaitWrapper = new SendWaitWrapper(wrapper);
                //queSendWaitWrappers.Enqueue(sendWaitWrapper);

                LogSendMsg(wrapper);

                ID_36_TRANS_EVENT_RESPONSE response = new ID_36_TRANS_EVENT_RESPONSE();
                OnMessageShowOnMainFormEvent?.Invoke(this, $"Send transfer event report. [{eventType}]");
                string rtnMsg = "";

                TrxTcpIp.ReturnCode returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(wrapper, out response, out rtnMsg);
                if (returnCode == TrxTcpIp.ReturnCode.Normal)
                {
                    switch (eventType)
                    {
                        case EventType.AdrOrMoveArrivals:
                            OnAgvcAcceptMoveArrivalEvent?.Invoke(this, default(EventArgs));
                            break;
                        case EventType.LoadArrivals:
                            OnAgvcAcceptLoadArrivalEvent?.Invoke(this, default(EventArgs));
                            break;
                        case EventType.UnloadArrivals:
                            OnAgvcAcceptUnloadArrivalEvent?.Invoke(this, default(EventArgs));
                            break;
                        case EventType.LoadComplete:
                            //mainFlowHandler.IsAgvcReplySendWaitMessage = true;
                            OnAgvcAcceptLoadCompleteEvent?.Invoke(this, default(EventArgs));
                            break;
                        case EventType.UnloadComplete:
                            OnAgvcAcceptUnloadCompleteEvent?.Invoke(this, default(EventArgs));
                            break;
                        case EventType.Bcrread:
                            OnAgvcReplyCstIdReadEvent(response);
                            //mainFlowHandler.IsAgvcReplySendWaitMessage = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (eventType == EventType.Bcrread)
                    {
                        theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = GetCancelCompleteStatus(report.BCRReadResult, theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus);
                    }
                    string msg = $"TransferEvent[{eventType}] send wait timeout[{report.CmdID}]";
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                    OnSendRecvTimeoutEvent?.Invoke(this, default(EventArgs));
                    //mainFlowHandler.IsAgvcReplySendWaitMessage = true;
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        public void SendRecv_Cmd136_CstIdReadReport()
        {

            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
            RobotCommand robotCommand = (RobotCommand)mainFlowHandler.GetCurTransferStep();
            AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(robotCommand.SlotNumber);

            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = EventType.Bcrread;
                report.CSTID = aseCarrierSlotStatus.CarrierId;
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.BCRReadResult = BCRReadResultParse(ReadResult);

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrapper.ImpTransEventRep = report;

                //SendWaitWrapper sendWaitWrapper = new SendWaitWrapper(wrapper);
                //queSendWaitWrappers.Enqueue(sendWaitWrapper);

                LogSendMsg(wrapper);

                ID_36_TRANS_EVENT_RESPONSE response = new ID_36_TRANS_EVENT_RESPONSE();
                OnMessageShowOnMainFormEvent?.Invoke(this, $"Send transfer event report. [{report.EventType}]");
                string rtnMsg = "";

                TrxTcpIp.ReturnCode returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(wrapper, out response, out rtnMsg);
                if (returnCode == TrxTcpIp.ReturnCode.Normal)
                {
                    OnAgvcReplyCstIdReadEvent(response);
                }
                else
                {
                    string cmdId = mainFlowHandler.GetCurTransferStep().CmdId;
                    theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = GetCancelCompleteStatus(report.BCRReadResult, theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus);
                    string msg = $"TransferEvent[{report.EventType}] send wait timeout[{report.CmdID}]";
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                    OnSendRecvTimeoutEvent?.Invoke(this, default(EventArgs));
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void OnAgvcReplyCstIdReadEvent(ID_36_TRANS_EVENT_RESPONSE response)
        {
            if (response.ReplyAction != ReplyActionType.Continue)
            {
                OnMessageShowOnMainFormEvent?.Invoke(this, $"Load fail, [ReplyAction = {response.ReplyAction}][RenameCarrierID = {response.RenameCarrierID}]");
                alarmHandler.ResetAllAlarmsFromAgvm();
                var cmdId = mainFlowHandler.GetCurTransferStep().CmdId;
                if (!string.IsNullOrEmpty(response.RenameCarrierID))
                {
                    var slotNum = theVehicle.AgvcTransCmdBuffer[cmdId].SlotNumber;
                    var slot = theVehicle.GetAseCarrierSlotStatus(slotNum);
                    theVehicle.AgvcTransCmdBuffer[cmdId].CassetteId = response.RenameCarrierID;
                    slot.CarrierId = response.RenameCarrierID;
                }
                theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus = GetCancelCompleteStatus(response.ReplyAction, theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus);
                mainFlowHandler.BcrReadAbortCommand(cmdId, theVehicle.AgvcTransCmdBuffer[cmdId].CompleteStatus);
            }
            else
            {
                OnMessageShowOnMainFormEvent?.Invoke(this, $"Load Complete and BcrReadReplyOk");
                OnAgvcAcceptBcrReadReply?.Invoke(this, new EventArgs());
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
        private CompleteStatus GetCancelCompleteStatus(BCRReadResult readResult, CompleteStatus completeStatus)
        {
            switch (readResult)
            {
                case BCRReadResult.BcrReadFail:
                    return CompleteStatus.IdreadFailed;
                case BCRReadResult.BcrMisMatch:
                    return CompleteStatus.IdmisMatch;
                case BCRReadResult.BcrNormal:
                default:
                    break;
            }

            return completeStatus;
        }

        public void Send_Cmd136_AskReserve(MapSection mapSection)
        {
            var msg = $"Ask Reserve {mapSection.Id}";
            OnMessageShowOnMainFormEvent?.Invoke(this, msg);
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

            try
            {
                ID_136_TRANS_EVENT_REP report = new ID_136_TRANS_EVENT_REP();
                report.EventType = EventType.ReserveReq;
                FitReserveInfos(report.ReserveInfos, mapSection);
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = report;


                #region Ask reserve and wait reply
                LogSendMsg(wrappers);

                ID_36_TRANS_EVENT_RESPONSE response = new ID_36_TRANS_EVENT_RESPONSE();
                string rtnMsg = "";

                var returnCode = ClientAgent.TrxTcpIp.sendRecv_Google(wrappers, out response, out rtnMsg, agvcConnectorConfig.RecvTimeoutMs, 0);

                if (CanDoReserveWork())
                {
                    if (returnCode == TrxTcpIp.ReturnCode.Normal)
                    {
                        OnReceiveReserveReply(response);
                    }
                    else
                    {
                        ReserveOkAskNext = false;
                        string xxmsg = $"Ask Reserve{mapSection.Id}, Result=[{returnCode}][{rtnMsg}]";
                        OnMessageShowOnMainFormEvent?.Invoke(this, xxmsg);
                    }
                }
                else
                {
                    string xxmsg = $"Ask Reserve{mapSection.Id},  Thd State=[IsPause{IsAskReservePause}][IsMoveEnd{theVehicle.AseMoveStatus.IsMoveEnd}]";
                    OnMessageShowOnMainFormEvent?.Invoke(this, xxmsg);
                }

                #endregion

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void FitReserveInfos(RepeatedField<ReserveInfo> reserveInfos, MapSection mapSection)
        {
            reserveInfos.Clear();
            ReserveInfo reserveInfo = new ReserveInfo();
            reserveInfo.ReserveSectionID = mapSection.Id;
            if (mapSection.CmdDirection == EnumCommandDirection.Backward)
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirReverse;
            }
            else if (mapSection.CmdDirection == EnumCommandDirection.None)
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirNone;
            }
            else
            {
                reserveInfo.DriveDirction = DriveDirction.DriveDirForward;
            }
            reserveInfos.Add(reserveInfo);
        }
        private void OnReceiveReserveReply(ID_36_TRANS_EVENT_RESPONSE receive)
        {
            if (CanDoReserveWork())
            {
                IsAskReservePause = true;
                string sectionId = receive.ReserveInfos[0].ReserveSectionID;
                if (receive.IsReserveSuccess == ReserveResult.Success)
                {
                    string msg = $" Get Reserve Accept {sectionId}";
                    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                    if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.On)
                    {
                        theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.Off;
                        StatusChangeReport();
                    }
                    if (!theVehicle.AseMoveStatus.IsMoveEnd)
                    {
                        OnGetReserveOk(sectionId);
                    }
                }
                else
                {
                    string msg = $" Get Reserve Reject {sectionId}";
                    OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                    RefreshPartMoveSections();
                    SpinWait.SpinUntil(() => false, agvcConnectorConfig.AskReserveIntervalMs);
                    if (queReserveOkSections.Count == 0)
                    {
                        if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.Off)
                        {
                            theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.On;
                            StatusChangeReport();
                        }
                    }
                    else if (queReserveOkSections.Count == 1)
                    {
                        queReserveOkSections.TryPeek(out MapSection mapSection);
                        var moveSection = theVehicle.AseMovingGuide.MovingSections.Find(x => x.Id == mapSection.Id);
                        if (moveSection.CmdDirection == EnumCommandDirection.Forward)
                        {
                            if (theVehicle.AseMoveStatus.LastAddress.Id == moveSection.TailAddress.Id)
                            {
                                if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.Off)
                                {
                                    theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.On;
                                    StatusChangeReport();
                                }
                            }
                        }
                        else
                        {
                            if (theVehicle.AseMoveStatus.LastAddress.Id == moveSection.HeadAddress.Id)
                            {
                                if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.Off)
                                {
                                    theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.On;
                                    StatusChangeReport();
                                }
                            }
                        }
                    }
                    if (theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Idle)
                    {
                        if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.Off)
                        {
                            theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.On;
                            StatusChangeReport();
                        }
                    }
                }
                IsAskReservePause = false;
            }
        }

        public void Receive_Cmd35_CarrierIdRenameRequest(object sender, TcpIpEventArgs e)
        {
            ID_35_CST_ID_RENAME_REQUEST receive = (ID_35_CST_ID_RENAME_REQUEST)e.objPacket;
            var result = false;

            if (theVehicle.AseCarrierSlotL.CarrierId == receive.OLDCSTID.Trim())
            {
                AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.AseCarrierSlotL;
                aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
                aseCarrierSlotStatus.CarrierId = receive.NEWCSTID;
                OnRenameCassetteIdEvent?.Invoke(this, aseCarrierSlotStatus);
                //mainFlowHandler.RenameCstId(EnumSlotNumber.L, receive.NEWCSTID);
                result = true;
            }
            else if (theVehicle.AseCarrierSlotR.CarrierId == receive.OLDCSTID.Trim())
            {
                AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.AseCarrierSlotR;
                aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
                aseCarrierSlotStatus.CarrierId = receive.NEWCSTID;
                OnRenameCassetteIdEvent?.Invoke(this, aseCarrierSlotStatus);

                //mainFlowHandler.RenameCstId(EnumSlotNumber.R, receive.NEWCSTID);
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

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.CSTIDRenameRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.CSTIDRenameResp = response;

                SendCommandWrapper(wrappers, true);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void Send_Cmd134_TransferEventReport(EventType type)
        {
            AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

            try
            {
                ID_134_TRANS_EVENT_REP report = new ID_134_TRANS_EVENT_REP();
                report.EventType = type;
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.DrivingDirection = DriveDirctionParse(aseMoveStatus.LastSection.CmdDirection);
                report.XAxis = theVehicle.AseMoveStatus.LastMapPosition.X;
                report.YAxis = theVehicle.AseMoveStatus.LastMapPosition.Y;
                report.Speed = theVehicle.AseMoveStatus.Speed;
                report.DirectionAngle = theVehicle.AseMoveStatus.MovingDirection;
                report.VehicleAngle = theVehicle.AseMoveStatus.HeadDirection;

                mirleLogger.Log(new LogFormat("Info", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Angle=[{aseMoveStatus.MovingDirection}]"));

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = report;

                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd32_TransferCompleteResponse(object sender, TcpIpEventArgs e)
        {
            ID_32_TRANS_COMPLETE_RESPONSE receive = (ID_32_TRANS_COMPLETE_RESPONSE)e.objPacket;
            //theVehicle.ActionStatus = receive.ReplyCode == 0 ? VHActionStatus.NoCommand : VHActionStatus.Commanding;
            StatusChangeReport();
        }
        public void SendRecv_Cmd132_TransferCompleteReport(AgvcTransCmd agvcTransCmd, int delay = 0)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

                var msg = $"Transfer Complete, Complete Status={agvcTransCmd.CompleteStatus}, Command ID={agvcTransCmd.CommandId}";
                OnMessageShowOnMainFormEvent?.Invoke(this, msg);

                ID_132_TRANS_COMPLETE_REPORT report = new ID_132_TRANS_COMPLETE_REPORT();
                report.CmdID = agvcTransCmd.CommandId;
                report.CSTID = agvcTransCmd.CassetteId;
                report.CmpStatus = agvcTransCmd.CompleteStatus;
                report.CurrentAdrID = aseMoveStatus.LastAddress.Id;
                report.CurrentSecID = aseMoveStatus.LastSection.Id;
                report.SecDistance = (uint)aseMoveStatus.LastSection.VehicleDistanceSinceHead;
                report.CmdPowerConsume = theVehicle.CmdPowerConsume;
                report.CmdDistance = theVehicle.CmdDistance;
                report.XAxis = theVehicle.AseMoveStatus.LastMapPosition.X;
                report.YAxis = theVehicle.AseMoveStatus.LastMapPosition.Y;
                report.DirectionAngle = theVehicle.AseMoveStatus.MovingDirection;
                report.VehicleAngle = theVehicle.AseMoveStatus.HeadDirection;

                WrapperMessage wrapper = new WrapperMessage();
                wrapper.ID = WrapperMessage.TranCmpRepFieldNumber;
                wrapper.TranCmpRep = report;

                //SendCommandWrapper(wrappers, false, delay);

                SendWaitWrapper sendWaitWrapper = new SendWaitWrapper(wrapper);
                queSendWaitWrappers.Enqueue(sendWaitWrapper);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void Receive_Cmd31_TransferRequest(object sender, TcpIpEventArgs e)
        {
            try
            {
                ID_31_TRANS_REQUEST transRequest = (ID_31_TRANS_REQUEST)e.objPacket;
                OnMessageShowOnMainFormEvent?.Invoke(this, $" Get Transfer Command: {transRequest.CommandAction}");

                switch (transRequest.CommandAction)
                {
                    case CommandActionType.Move:
                    case CommandActionType.Load:
                    case CommandActionType.Unload:
                    case CommandActionType.Loadunload:
                    case CommandActionType.Home:
                    case CommandActionType.Movetocharger:
                        DoBasicTransferCmd(transRequest, e.iSeqNum);
                        break;
                    case CommandActionType.Override:
                        DoOverride(transRequest, e.iSeqNum);
                        return;
                    default:
                        var msg = $"Reject Transfer Command: {transRequest.CommandAction}";
                        OnMessageShowOnMainFormEvent?.Invoke(this, msg);
                        Send_Cmd131_TransferResponse(transRequest.CmdID, transRequest.CommandAction, e.iSeqNum, 1, "Unknow command.");
                        return;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }
        public void Send_Cmd131_TransferResponse(string cmdId, CommandActionType commandAction, ushort seqNum, int replyCode, string reason)
        {
            try
            {
                ID_131_TRANS_RESPONSE iD_131_TRANS_RESPONSE = new ID_131_TRANS_RESPONSE();
                iD_131_TRANS_RESPONSE.CmdID = cmdId;
                iD_131_TRANS_RESPONSE.CommandAction = commandAction;
                iD_131_TRANS_RESPONSE.ReplyCode = replyCode;
                iD_131_TRANS_RESPONSE.NgReason = reason;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TransResp = iD_131_TRANS_RESPONSE;

                SendCommandWrapper(wrappers, true);

                if (replyCode == 0)
                {
                    Commanding();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private AgvcTransCmd ConvertAgvcTransCmdIntoPackage(ID_31_TRANS_REQUEST transRequest, ushort iSeqNum)
        {
            //解析 Get 的ID_31_TRANS_REQUEST並且填入AgvcTransCmd     

            switch (transRequest.CommandAction)
            {
                case CommandActionType.Move:
                    return new AgvcMoveCmd(transRequest, iSeqNum);
                case CommandActionType.Load:
                    return new AgvcLoadCmd(transRequest, iSeqNum);
                case CommandActionType.Unload:
                    return new AgvcUnloadCmd(transRequest, iSeqNum);
                case CommandActionType.Loadunload:
                    return new AgvcLoadunloadCmd(transRequest, iSeqNum);
                case CommandActionType.Home:
                    break;
                case CommandActionType.Override:
                    return new AgvcOverrideCmd(transRequest, iSeqNum);
                case CommandActionType.Movetocharger:
                    return new AgvcMoveToChargerCmd(transRequest, iSeqNum);
                default:
                    break;
            }

            return new AgvcTransCmd(transRequest, iSeqNum);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return ErrorStatus.ErrReset;
            }
        }
        private VhLoadCSTStatus VhLoadCSTStatusParse(bool loading)
        {
            if (loading)
            {
                return VhLoadCSTStatus.Exist;
            }
            else
            {
                return VhLoadCSTStatus.NotExist;
            }
        }
        private BCRReadResult BCRReadResultParse(EnumCstIdReadResult readResult)
        {
            switch (readResult)
            {
                case EnumCstIdReadResult.Mismatch:
                    return BCRReadResult.BcrMisMatch;
                case EnumCstIdReadResult.Fail:
                    return BCRReadResult.BcrReadFail;
                case EnumCstIdReadResult.Normal:
                default:
                    return BCRReadResult.BcrNormal;
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
                case EnumAutoState.PreManual:
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return DriveDirction.DriveDirNone;
            }
        }
        #endregion


        #region Get/Set System Date Time

        // 用於設置系統時間
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        // 用於獲得系統時間
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetSystemTime(ref SYSTEMTIME st);

        private void SetSystemTime(string timeStamp)
        {
            try
            {
                SYSTEMTIME st = GetSYSTEMTIME(timeStamp);
                SetSystemTime(ref st);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private SYSTEMTIME GetSYSTEMTIME(string timeStamp)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            st.wYear = short.Parse(timeStamp.Substring(0, 4));
            st.wMonth = short.Parse(timeStamp.Substring(4, 2));
            st.wDay = short.Parse(timeStamp.Substring(6, 2));

            int hour = (int.Parse(timeStamp.Substring(8, 2)) + 8) % 24;
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
            LogFormat logFormat = new LogFormat("Error", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", exMsg);
            OnLogMsgEvent?.Invoke(this, logFormat);
        }

        private void LogDebug(string classMethodName, string msg)
        {
            LogFormat logFormat = new LogFormat("Debug", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", msg);
            OnLogMsgEvent?.Invoke(this, logFormat);
        }

        private void LogComm(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Comm", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", msg));
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
}
