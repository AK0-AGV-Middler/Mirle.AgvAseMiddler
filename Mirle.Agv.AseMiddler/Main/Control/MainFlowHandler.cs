﻿using Google.Protobuf.Collections;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using Mirle.Agv.AseMiddler.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Mirle.Tools;
using com.mirle.iibg3k0.ttc.Common;
using System.Collections.Concurrent;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class MainFlowHandler
    {
        #region Configs
        private AgvcConnectorConfig agvcConnectorConfig;
        private MainFlowConfig mainFlowConfig;
        private MapConfig mapConfig;
        private AlarmConfig alarmConfig;
        public BatteryLog batteryLog;
        #endregion

        #region TransCmds
        private List<TransferStep> transferSteps = new List<TransferStep>();
        public List<TransferStep> PtransferSteps//200523 dabid+
        {
            get { return transferSteps; }
        }

        public bool GoNextTransferStep { get; set; }
        public int TransferStepsIndex { get; private set; } = 0;
        public bool IsOverrideMove { get; set; }
        public bool IsAvoidMove { get; set; }

        public bool IsReportingPosition { get; set; }
        public bool IsReserveMechanism { get; set; } = true;

        public bool IsAgvcReplySendWaitMessage { get; set; } = false;

        #endregion

        #region Controller

        private AgvcConnector agvcConnector;
        private MirleLogger mirleLogger = null;
        private AlarmHandler alarmHandler;
        private MapHandler mapHandler;
        private XmlHandler xmlHandler = new XmlHandler();
        private AsePackage asePackage;

        #endregion

        #region Threads
        private Thread thdVisitTransferSteps;
        public bool IsVisitTransferStepPause { get; set; } = false;

        private Thread thdTrackPosition;
        //private ManualResetEvent trackPositionShutdownEvent = new ManualResetEvent(false);
        //private ManualResetEvent trackPositionPauseEvent = new ManualResetEvent(true);
        private bool IsTrackPositionPause { get; set; } = false;
        private bool IsTrackPositionStop { get; set; } = false;
        public bool IsResetUpdatePositionReportTimer { get; set; } = false;
        public EnumThreadStatus TrackPositionStatus { get; private set; } = EnumThreadStatus.None;

        public EnumThreadStatus PreTrackPositionStatus { get; private set; } = EnumThreadStatus.None;

        private Thread thdWatchLowPower;
        private ManualResetEvent watchLowPowerShutdownEvent = new ManualResetEvent(false);

        private ManualResetEvent watchLowPowerPauseEvent = new ManualResetEvent(true);
        public EnumThreadStatus WatchLowPowerStatus { get; private set; } = EnumThreadStatus.None;
        public EnumThreadStatus PreWatchLowPowerStatus { get; private set; } = EnumThreadStatus.None;
        #endregion

        #region Events
        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;
        public event EventHandler<string> OnMessageShowEvent;
        public event EventHandler<bool> OnAgvlConnectionChangedEvent;
        #endregion

        #region Models
        public Vehicle theVehicle;
        private bool isIniOk;
        public MapInfo Mapinfo { get; private set; } = new MapInfo();
        public int InitialSoc { get; set; } = 70;
        public bool IsFirstAhGet { get; set; }
        public EnumCstIdReadResult ReadResult { get; set; } = EnumCstIdReadResult.Normal;
        public bool NeedRename { get; set; } = false;
        public bool IsSimulation { get; set; }
        public string MainFlowAbnormalMsg { get; set; }

        private ConcurrentQueue<AseMoveStatus> FakeReserveOkAseMoveStatus { get; set; } = new ConcurrentQueue<AseMoveStatus>();
        #endregion

        public MainFlowHandler()
        {
            isIniOk = true;
        }

        #region InitialComponents

        public void InitialMainFlowHandler()
        {
            XmlInitial();
            LoggersInitial();
            VehicleInitial();
            ControllersInitial();
            EventInitial();

            VehicleLocationInitialAndThreadsInitial();

            if (isIniOk)
            {
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "全部"));
            }
        }

        private void XmlInitial()
        {
            try
            {
                mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"MainFlow.xml");
                mapConfig = xmlHandler.ReadXml<MapConfig>(@"Map.xml");
                agvcConnectorConfig = xmlHandler.ReadXml<AgvcConnectorConfig>(@"AgvcConnectorConfig.xml");
                alarmConfig = xmlHandler.ReadXml<AlarmConfig>(@"Alarm.xml");
                batteryLog = xmlHandler.ReadXml<BatteryLog>(@"BatteryLog.xml");
                //mirleLogger.CreateXmlLogger(batteryLog, @"BatteryLog.xml");
                InitialSoc = batteryLog.InitialSoc;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "讀寫設定檔"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "讀寫設定檔"));
            }
        }

        private void LoggersInitial()
        {
            try
            {
                string loggerConfigPath = "Log.ini";
                if (File.Exists(loggerConfigPath))
                {
                    mirleLogger = MirleLogger.Instance;
                }
                else
                {
                    throw new Exception();
                }

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "紀錄器"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "紀錄器缺少Log.ini"));
            }
        }

        private void ControllersInitial()
        {
            try
            {
                alarmHandler = new AlarmHandler(this);
                mapHandler = new MapHandler(mapConfig);
                Mapinfo = mapHandler.theMapInfo;
                agvcConnector = new AgvcConnector(this);
                asePackage = new AsePackage(Mapinfo.gateTypeMap);
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void VehicleInitial()
        {
            try
            {
                theVehicle = Vehicle.Instance;
                theVehicle.IsSimulation = mainFlowConfig.IsSimulation;
                IsFirstAhGet = theVehicle.IsSimulation;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "台車"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "台車"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息, Send to MainFlow(this)'mapHandler
                agvcConnector.OnInstallTransferCommandEvent += AgvcConnector_OnInstallTransferCommandEvent;
                agvcConnector.OnOverrideCommandEvent += AgvcConnector_OnOverrideCommandEvent;
                agvcConnector.OnAvoideRequestEvent += AgvcConnector_OnAvoideRequestEvent;
                agvcConnector.OnLogMsgEvent += LogMsgHandler;
                agvcConnector.OnRenameCassetteIdEvent += AgvcConnector_OnRenameCassetteIdEvent;
                //agvcConnector.OnCassetteIdReadReplyAbortCommandEvent += AgvcConnector_OnCassetteIdReadReplyAbortCommandEvent;
                agvcConnector.OnStopClearAndResetEvent += AgvcConnector_OnStopClearAndResetEvent;
                agvcConnector.OnConnectionChangeEvent += AgvcConnector_OnConnectionChangeEvent;

                agvcConnector.OnAgvcAcceptMoveArrivalEvent += AgvcConnector_OnAgvcAcceptMoveArrivalEvent;
                agvcConnector.OnAgvcAcceptLoadArrivalEvent += AgvcConnector_OnAgvcAcceptLoadArrivalEvent;
                agvcConnector.OnAgvcAcceptLoadCompleteEvent += AgvcConnector_OnAgvcAcceptLoadCompleteEvent;
                agvcConnector.OnAgvcAcceptBcrReadReply += AgvcConnector_OnAgvcContinueBcrReadEvent;
                agvcConnector.OnAgvcAcceptUnloadArrivalEvent += AgvcConnector_OnAgvcAcceptUnloadArrivalEvent;
                agvcConnector.OnAgvcAcceptUnloadCompleteEvent += AgvcConnector_OnAgvcAcceptUnloadCompleteEvent;
                agvcConnector.OnSendRecvTimeoutEvent += AgvcConnector_OnSendRecvTimeoutEvent;

                //來自MoveControl的移動結束訊息, Send to MainFlow(this)'middleAgent'mapHandler
                asePackage.OnPositionChangeEvent += AsePackage_OnPositionChangeEvent;
                asePackage.OnPartMoveArrivalEvent += AsePackage_OnPartMoveArrivalEvent;
                asePackage.aseMoveControl.OnMoveFinishedEvent += AseMoveControl_OnMoveFinished;
                //asePackage.aseMoveControl.OnRetryMoveFinishEvent += AseMoveControl_OnRetryMoveFinished;

                asePackage.OnAgvlErrorEvent += AsePackage_OnAgvlErrorEvent;

                //來自IRobotControl的取放貨結束訊息, Send to MainFlow(this)'middleAgent'mapHandler
                asePackage.aseRobotControl.OnRobotInterlockErrorEvent += AseRobotControl_OnRobotInterlockErrorEvent;
                asePackage.aseRobotControl.OnRobotCommandFinishEvent += AseRobotContorl_OnRobotCommandFinishEvent;
                asePackage.aseRobotControl.OnRobotCommandErrorEvent += AseRobotControl_OnRobotCommandErrorEvent;

                //來自IRobot的CarrierId讀取訊息, Send to middleAgent
                asePackage.aseRobotControl.OnReadCarrierIdFinishEvent += AseRobotControl_OnReadCarrierIdFinishEvent;

                //來自IBatterysControl的電量改變訊息, Send to middleAgent
                asePackage.aseBatteryControl.OnBatteryPercentageChangeEvent += agvcConnector.AseBatteryControl_OnBatteryPercentageChangeEvent;
                asePackage.aseBatteryControl.OnBatteryPercentageChangeEvent += AseBatteryControl_OnBatteryPercentageChangeEvent;

                asePackage.OnStatusChangeReportEvent += AsePackage_OnStatusChangeReportEvent;

                //來自AlarmHandler的SetAlarm/ResetOneAlarm/ResetAllAlarm發生警告, Send to MainFlow,middleAgent               

                alarmHandler.SetAlarmToAgvl += asePackage.aseBuzzerControl.AlarmHandler_OnSetAlarmEvent;
                alarmHandler.SetAlarmToAgvc += agvcConnector.SetlAlarmToAgvc;
                alarmHandler.ResetAllAlarmsToAgvl += asePackage.aseBuzzerControl.ResetAllAlarmCode;
                alarmHandler.ResetAllAlarmsToAgvc += agvcConnector.ResetAllAlarmsToAgvc;

                asePackage.aseBuzzerControl.OnAlarmCodeSetEvent += AseBuzzerControl_OnAlarmCodeSetEvent1;
                asePackage.aseBuzzerControl.OnAlarmCodeResetEvent += AseBuzzerControl_OnAlarmCodeResetEvent;

                theVehicle.OnAutoStateChangeEvent += TheVehicle_OnAutoStateChangeEvent;

                asePackage.OnConnectionChangeEvent += AsePackage_OnConnectionChangeEvent;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "事件"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "事件"));

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AgvcConnector_OnConnectionChangeEvent(object sender, bool e)
        {
            theVehicle.IsAgvcConnect = e;
        }

        private void AsePackage_OnAgvlErrorEvent(object sender, EventArgs e)
        {
            alarmHandler.SetAlarmFromAgvl(40);
        }

        private void AseBuzzerControl_OnAlarmCodeResetEvent(object sender, int e)
        {
            alarmHandler.ResetAllAlarmsFromAgvl();
        }

        private void AseBuzzerControl_OnAlarmCodeSetEvent1(object sender, int id)
        {
            alarmHandler.SetAlarmFromAgvl(id);
        }

        private void AsePackage_OnPositionChangeEvent(object sender, AseMoveStatus aseMoveStatus)
        {
            UpdateVehicleDistanceSinceHead(aseMoveStatus);
        }

        private void AsePackage_OnPartMoveArrivalEvent(object sender, AseMoveStatus aseMoveStatus)
        {
            IsResetUpdatePositionReportTimer = UpdateVehiclePositionInMovingStep(aseMoveStatus);
        }

        private void AsePackage_OnStatusChangeReportEvent(object sender, string e)
        {
            OnMessageShowEvent?.Invoke(this, e);
            agvcConnector.StatusChangeReport();
        }

        private void AsePackage_OnConnectionChangeEvent(object sender, bool e)
        {
            OnAgvlConnectionChangedEvent?.Invoke(this, e);
        }

        private void TheVehicle_OnAutoStateChangeEvent(object sender, EnumAutoState autoState)
        {
            switch (autoState)
            {
                case EnumAutoState.Auto:
                    if (!theVehicle.IsSimulation)
                    {
                        alarmHandler.ResetAllAlarmsFromAgvm();
                        ResumeTransfer();
                    }
                    else
                    {
                        alarmHandler.ResetAllAlarmsFromAgvm();
                        ResumeTransfer();
                    }
                    break;
                case EnumAutoState.Manual:
                    StopClearAndReset();
                    break;
                case EnumAutoState.PreManual:
                    break;
                default:
                    break;
            }
            agvcConnector.StatusChangeReport();
        }

        private void VehicleLocationInitialAndThreadsInitial()
        {
            if (mainFlowConfig.IsSimulation)
            {
                try
                {
                    theVehicle.AseMoveStatus.LastMapPosition = Mapinfo.addressMap.First(x => x.Key != "").Value.Position;
                }
                catch (Exception ex)
                {
                    theVehicle.AseMoveStatus.LastMapPosition = new MapPosition();
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
            }
            else
            {
                if (asePackage.IsConnected())
                {
                    asePackage.AllAgvlStatusReportRequest();
                }
            }
            StartVisitTransferSteps();
            StartTrackPosition();
            StartWatchLowPower();
            var msg = $"讀取到的電量為{batteryLog.InitialSoc}";
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
        }
        private bool IsRealPositionEmpty()
        {
            if (theVehicle.AseMoveStatus.LastMapPosition == null)
            {
                return true;
            }

            if (theVehicle.AseMoveStatus.LastMapPosition.X == 0 && theVehicle.AseMoveStatus.LastMapPosition.Y == 0)
            {
                return true;
            }

            return false;
        }

        public void ReloadConfig()
        {
            XmlInitial();
        }

        #endregion

        #region Thd Visit TransferSteps
        private void VisitTransferSteps()
        {
            while (true)
            {
                if (IsVisitTransferStepPause)
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.VisitTransferStepsSleepTimeMs);
                    continue;
                }
                try
                {
                    if (GoNextTransferStep)
                    {
                        GoNextTransferStep = false;
                        if (TransferStepsIndex < 0)
                        {
                            TransferStepsIndex = 0;
                            GoNextTransferStep = true;
                            SpinWait.SpinUntil(() => false, mainFlowConfig.VisitTransferStepsSleepTimeMs);

                            continue;
                        }
                        if (transferSteps.Count == 0)
                        {
                            if (theVehicle.AgvcTransCmdBuffer.Count > 0)
                            {
                                if (!theVehicle.IsOptimize)
                                    GetTransferCommandToDo();
                            }
                            else
                            {
                                transferSteps.Add(new EmptyTransferStep());
                            }
                            GoNextTransferStep = true;
                            SpinWait.SpinUntil(() => false, mainFlowConfig.VisitTransferStepsSleepTimeMs);

                            continue;
                        }

                        if (TransferStepsIndex < transferSteps.Count)
                        {
                            DoTransferStep(transferSteps[TransferStepsIndex]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.VisitTransferStepsSleepTimeMs);
                }
            }
        }

        private void GetTransferCommandToDo()
        {
            transferSteps = new List<TransferStep>();
            TransferStepsIndex = 0;

            if (theVehicle.AgvcTransCmdBuffer.Count > 1)
            {
                if (!mainFlowConfig.DualCommandOptimize)
                {
                    List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                    AgvcTransCmd onlyCmd = agvcTransCmds[0];

                    switch (onlyCmd.EnrouteState)
                    {
                        case CommandState.None:
                            TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                            transferSteps.Add(new EmptyTransferStep());
                            break;
                        case CommandState.LoadEnroute:
                            TransferStepsAddMoveCmdInfo(onlyCmd.LoadAddressId, onlyCmd.CommandId);
                            TransferStepsAddLoadCmdInfo(onlyCmd);
                            break;
                        case CommandState.UnloadEnroute:
                            TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                            TransferStepsAddUnloadCmdInfo(onlyCmd);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                    AgvcTransCmd cmd001 = agvcTransCmds[0];

                    switch (cmd001.EnrouteState)
                    {
                        case CommandState.None:
                            {
                                TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                transferSteps.Add(new EmptyTransferStep());
                            }
                            break;
                        case CommandState.LoadEnroute:
                            {
                                var disCmd001Load = DistanceFromLastPosition(cmd001.LoadAddressId);

                                AgvcTransCmd cmd002 = agvcTransCmds[1];
                                if (cmd002.EnrouteState == CommandState.LoadEnroute)
                                {
                                    var disCmd002Load = DistanceFromLastPosition(cmd002.LoadAddressId);
                                    if (disCmd001Load <= disCmd002Load)
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd001.LoadAddressId, cmd001.CommandId);
                                        TransferStepsAddLoadCmdInfo(cmd001);
                                    }
                                    else
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd002.LoadAddressId, cmd002.CommandId);
                                        TransferStepsAddLoadCmdInfo(cmd002);
                                    }
                                }
                                else if (cmd002.EnrouteState == CommandState.UnloadEnroute)
                                {
                                    var disCmd002Unload = DistanceFromLastPosition(cmd002.UnloadAddressId);
                                    if (disCmd001Load <= disCmd002Unload)
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd001.LoadAddressId, cmd001.CommandId);
                                        TransferStepsAddLoadCmdInfo(cmd001);
                                    }
                                    else
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd002.UnloadAddressId, cmd002.CommandId);
                                        TransferStepsAddUnloadCmdInfo(cmd002);
                                    }
                                }
                                else
                                {
                                    StopClearAndReset();
                                    alarmHandler.SetAlarmFromAgvm(1);
                                }
                            }
                            break;
                        case CommandState.UnloadEnroute:
                            {
                                var disCmd001Unload = DistanceFromLastPosition(cmd001.UnloadAddressId);

                                AgvcTransCmd cmd002 = agvcTransCmds[1];
                                if (cmd002.EnrouteState == CommandState.LoadEnroute)
                                {
                                    var disCmd002Load = DistanceFromLastPosition(cmd002.LoadAddressId);
                                    if (disCmd001Unload <= disCmd002Load)
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                        TransferStepsAddUnloadCmdInfo(cmd001);
                                    }
                                    else
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd002.LoadAddressId, cmd002.CommandId);
                                        TransferStepsAddLoadCmdInfo(cmd002);
                                    }
                                }
                                else if (cmd002.EnrouteState == CommandState.UnloadEnroute)
                                {
                                    var disCmd002Unload = DistanceFromLastPosition(cmd002.UnloadAddressId);
                                    if (disCmd001Unload <= disCmd002Unload)
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                        TransferStepsAddUnloadCmdInfo(cmd001);
                                    }
                                    else
                                    {
                                        TransferStepsAddMoveCmdInfo(cmd002.UnloadAddressId, cmd002.CommandId);
                                        TransferStepsAddUnloadCmdInfo(cmd002);
                                    }
                                }
                                else
                                {
                                    StopClearAndReset();
                                    alarmHandler.SetAlarmFromAgvm(1);
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }
            }
            else if (theVehicle.AgvcTransCmdBuffer.Count == 1)
            {
                List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                AgvcTransCmd onlyCmd = agvcTransCmds[0];

                switch (onlyCmd.EnrouteState)
                {
                    case CommandState.None:
                        TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                        transferSteps.Add(new EmptyTransferStep());
                        break;
                    case CommandState.LoadEnroute:
                        TransferStepsAddMoveCmdInfo(onlyCmd.LoadAddressId, onlyCmd.CommandId);
                        TransferStepsAddLoadCmdInfo(onlyCmd);
                        break;
                    case CommandState.UnloadEnroute:
                        TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                        TransferStepsAddUnloadCmdInfo(onlyCmd);
                        break;
                    default:
                        break;
                }
            }

        }

        private void DoTransferStep(TransferStep transferStep)
        {
            switch (transferStep.GetTransferStepType())
            {
                case EnumTransferStepType.Move:
                case EnumTransferStepType.MoveToCharger:
                    MoveCmdInfo moveCmdInfo = (MoveCmdInfo)transferStep;
                    StopCharge();
                    if (moveCmdInfo.EndAddress.Id == theVehicle.AseMoveStatus.LastAddress.Id)
                    {
                        if (theVehicle.IsSimulation)
                        {
                            AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                        }
                        else
                        {
                            //asePackage.aseMoveControl.PartMove(EnumAseMoveCommandIsEnd.Begin);
                            if (theVehicle.IsReAuto)
                            {
                                theVehicle.IsReAuto = false;
                                asePackage.aseMoveControl.PartMove(EnumAseMoveCommandIsEnd.End);
                            }
                            else
                            {
                                AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                            }
                        }
                    }
                    else
                    {
                        theVehicle.AseMovingGuide.CommandId = moveCmdInfo.CmdId;
                        agvcConnector.ReportSectionPass();
                        theVehicle.AseMoveStatus.IsMoveEnd = false;
                        asePackage.aseMoveControl.PartMove(EnumAseMoveCommandIsEnd.Begin);
                        agvcConnector.AskGuideAddressesAndSections(moveCmdInfo);
                    }
                    break;
                case EnumTransferStepType.Load:
                    Load((LoadCmdInfo)transferStep);
                    break;
                case EnumTransferStepType.Unload:
                    Unload((UnloadCmdInfo)transferStep);
                    break;
                case EnumTransferStepType.Empty:
                    CheckTransferSteps();
                    break;
                default:
                    break;
            }
        }

        private void CheckTransferSteps()
        {
            if (TransferStepsIndex + 1 < transferSteps.Count)
            {
                transferSteps.RemoveAt(TransferStepsIndex);
            }

            GoNextTransferStep = true;
        }

        public void StartVisitTransferSteps()
        {
            TransferStepsIndex = 0;
            thdVisitTransferSteps = new Thread(VisitTransferSteps);
            thdVisitTransferSteps.IsBackground = true;
            thdVisitTransferSteps.Start();

            var msg = $"MainFlow : StartVisitTransferSteps";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void PauseVisitTransferSteps()
        {
            IsVisitTransferStepPause = true;
            if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
            {
                theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.On;
                agvcConnector.StatusChangeReport();
            }
            var msg = $"MainFlow : PauseVisitTransferSteps";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void ResumeVisitTransferSteps()
        {
            IsVisitTransferStepPause = false;
            if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.On)
            {
                theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                agvcConnector.StatusChangeReport();
            }
            var msg = $"MainFlow : ResumeVisitTransferSteps";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void ClearTransferSteps()
        {
            IsVisitTransferStepPause = true;
            GoNextTransferStep = false;
            TransferStepsIndex = 0;
            transferSteps = new List<TransferStep>();
            GoNextTransferStep = true;
            IsVisitTransferStepPause = false;

            var msg = $"MainFlow : ClearTransferSteps";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void ClearTransferSteps(string cmdId)
        {
            //IsVisitTransferStepPause = true;
            //SpinWait.SpinUntil(() => false, 2000);
            //GoNextTransferStep = false;
            //int transferStepCountBeforeRemove = transferSteps.Count;
            //transferSteps.RemoveAll(x => x.CmdId == cmdId);
            //TransferStepsIndex = TransferStepsIndex + transferSteps.Count - transferStepCountBeforeRemove;
            //if (transferSteps.Count == 0)
            //{
            //    TransferStepsIndex = -1;
            //    transferSteps.Add(new EmptyTransferStep());
            //}

            //GoNextTransferStep = true;
            //IsVisitTransferStepPause = false;

            List<TransferStep> tempTransferSteps = new List<TransferStep>();
            foreach (var step in transferSteps)
            {
                if (step.CmdId != cmdId)
                {
                    tempTransferSteps.Add(step);
                }
            }
            transferSteps = tempTransferSteps;

            var msg = $"MainFlow : Clear Finished Command[{cmdId}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        private void PauseTransfer()
        {
            agvcConnector.PauseAskReserve();
            PauseVisitTransferSteps();
        }

        private void ResumeTransfer()
        {
            ResumeVisitTransferSteps();
            agvcConnector.ResumeAskReserve();
        }

        #region Handle Transfer Command

        private void AgvcConnector_OnInstallTransferCommandEvent(object sender, AgvcTransCmd agvcTransCmd)
        {
            var msg = $"MainFlow : Get [{agvcTransCmd.AgvcTransCommandType}] command [{agvcTransCmd.CommandId}]";
            OnMessageShowEvent?.Invoke(this, msg);

            #region 檢查搬送Command
            try
            {
                switch (agvcTransCmd.AgvcTransCommandType)
                {
                    case EnumAgvcTransCommandType.Move:
                    case EnumAgvcTransCommandType.MoveToCharger:
                        CheckMoveEndAddress(agvcTransCmd.UnloadAddressId);
                        break;
                    case EnumAgvcTransCommandType.Load:
                        CheckLoadPortAddress(agvcTransCmd.LoadAddressId);
                        agvcTransCmd.SlotNumber = CheckVehicleSlot();
                        break;
                    case EnumAgvcTransCommandType.Unload:
                        CheckUnloadPortAddress(agvcTransCmd.UnloadAddressId);
                        agvcTransCmd.SlotNumber = CheckUnloadCstId(agvcTransCmd.CassetteId);
                        break;
                    case EnumAgvcTransCommandType.LoadUnload:
                        CheckLoadPortAddress(agvcTransCmd.LoadAddressId);
                        CheckUnloadPortAddress(agvcTransCmd.UnloadAddressId);
                        agvcTransCmd.SlotNumber = CheckVehicleSlot();
                        break;
                    case EnumAgvcTransCommandType.Override:
                        CheckOverrideAddress(agvcTransCmd);
                        break;
                    case EnumAgvcTransCommandType.Else:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 1, ex.Message);
                string reason = $"MainFlow : Reject {agvcTransCmd.AgvcTransCommandType} Command. {ex.Message}";
                OnMessageShowEvent?.Invoke(this, reason);
                return;
            }
            #endregion

            #region 搬送流程更新
            try
            {
                PauseTransfer();
                theVehicle.AgvcTransCmdBuffer.TryAdd(agvcTransCmd.CommandId, agvcTransCmd);
                agvcConnector.Commanding();
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 0, "");
                asePackage.SetTransferCommandInfoRequest();
                if (theVehicle.AgvcTransCmdBuffer.Count == 1)
                {
                    InitialTransferSteps(agvcTransCmd);
                    GoNextTransferStep = true;
                }
                ResumeTransfer();
                var okMsg = $"MainFlow : Get  {agvcTransCmd.AgvcTransCommandType}Command{agvcTransCmd.CommandId}  checked .";
                OnMessageShowEvent?.Invoke(this, okMsg);
            }
            catch (Exception ex)
            {
                agvcConnector.ReplyTransferCommand(agvcTransCmd.CommandId, agvcTransCmd.GetCommandActionType(), agvcTransCmd.SeqNum, 1, "");
                var ngMsg = $"MainFlow :  Get  {agvcTransCmd.AgvcTransCommandType}Command{agvcTransCmd.CommandId}  fail .";
                OnMessageShowEvent?.Invoke(this, ngMsg);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
            #endregion
        }

        private EnumSlotNumber CheckVehicleSlot()
        {
            var slotLState = theVehicle.AseCarrierSlotL.CarrierSlotStatus;
            var slotRState = theVehicle.AseCarrierSlotR.CarrierSlotStatus;
            var agvcTransCmdBuffer = theVehicle.AgvcTransCmdBuffer.Values.ToList();

            if (slotLState != EnumAseCarrierSlotStatus.Empty && slotRState != EnumAseCarrierSlotStatus.Empty)
            {
                throw new Exception("Vehicle no Slot to load.");
            }
            else if (theVehicle.AgvcTransCmdBuffer.Count >= 2)
            {
                throw new Exception("Vehicle no Slot to load.");
            }
            else if (theVehicle.AgvcTransCmdBuffer.Count == 1)
            {
                var curCmd = theVehicle.AgvcTransCmdBuffer.Values.First();
                switch (curCmd.EnrouteState)
                {

                    case CommandState.LoadEnroute:
                    case CommandState.UnloadEnroute:
                        if (curCmd.SlotNumber == EnumSlotNumber.L)
                        {
                            if (slotRState != EnumAseCarrierSlotStatus.Empty)
                            {
                                throw new Exception("Vehicle no Slot to load.");
                            }
                            else
                            {
                                return EnumSlotNumber.R;
                            }
                        }
                        else
                        {
                            if (slotLState != EnumAseCarrierSlotStatus.Empty)
                            {
                                throw new Exception("Vehicle no Slot to load.");
                            }
                            else
                            {
                                return EnumSlotNumber.L;
                            }
                        }
                    case CommandState.None:
                    default:
                        if (slotLState == EnumAseCarrierSlotStatus.Empty)
                        {
                            return EnumSlotNumber.L;
                        }
                        else
                        {
                            return EnumSlotNumber.R;
                        }
                }
            }
            else
            {
                if (slotLState == EnumAseCarrierSlotStatus.Empty)
                {
                    return EnumSlotNumber.L;
                }
                else
                {
                    return EnumSlotNumber.R;
                }
            }
        }

        private EnumSlotNumber CheckUnloadCstId(string cassetteId)
        {
            if (theVehicle.AseCarrierSlotL.CarrierId.ToUpper().Trim() == cassetteId)
            {
                return EnumSlotNumber.L;
            }
            else if (theVehicle.AseCarrierSlotR.CarrierId.ToUpper().Trim() == cassetteId)
            {
                return EnumSlotNumber.R;
            }
            else
            {
                throw new Exception($"No [{cassetteId}] to unload.");
            }
        }

        private void CheckOverrideAddress(AgvcTransCmd agvcTransCmd)
        {
            return;
        }

        private void CheckUnloadPortAddress(string unloadAddressId)
        {
            CheckMoveEndAddress(unloadAddressId);
            MapAddress unloadAddress = Mapinfo.addressMap[unloadAddressId];
            if (!unloadAddress.IsTransferPort())
            {
                throw new Exception($"{unloadAddressId} can not unload.");
            }
        }

        private void CheckLoadPortAddress(string loadAddressId)
        {
            CheckMoveEndAddress(loadAddressId);
            MapAddress loadAddress = Mapinfo.addressMap[loadAddressId];
            if (!loadAddress.IsTransferPort())
            {
                throw new Exception($"{loadAddressId} can not load.");
            }
        }

        private void CheckMoveEndAddress(string unloadAddressId)
        {
            if (!Mapinfo.addressMap.ContainsKey(unloadAddressId))
            {
                throw new Exception($"{unloadAddressId} is not in the map.");
            }
        }

        private void AgvcConnector_OnOverrideCommandEvent(object sender, AgvcOverrideCmd agvcOverrideCmd)
        {
            var msg = $"MainFlow :  Get [ Override ]Command[{agvcOverrideCmd.CommandId}],  start check .";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        private void RejectTransferCommandAndResume(int alarmCode, string reason, AgvcTransCmd agvcTransferCmd)
        {
            try
            {
                alarmHandler.SetAlarmFromAgvm(alarmCode);
                agvcConnector.ReplyTransferCommand(agvcTransferCmd.CommandId, agvcTransferCmd.GetCommandActionType(), agvcTransferCmd.SeqNum, 1, reason);
                reason = $"MainFlow : Reject {agvcTransferCmd.AgvcTransCommandType} Command, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                if (IsVisitTransferStepPause)
                {
                    ResumeVisitTransferSteps();
                    agvcConnector.ResumeAskReserve();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void RejectOverrideCommandAndResume(int alarmCode, string reason, AgvcOverrideCmd agvcOverrideCmd)
        {
            try
            {
                alarmHandler.SetAlarmFromAgvm(alarmCode);
                agvcConnector.ReplyTransferCommand(agvcOverrideCmd.CommandId, agvcOverrideCmd.GetCommandActionType(), agvcOverrideCmd.SeqNum, 1, reason);
                reason = $"MainFlow : Reject {agvcOverrideCmd.AgvcTransCommandType} Command, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AgvcConnector_OnAvoideRequestEvent(object sender, AseMovingGuide aseMovingGuide)
        {
            #region 避車檢查
            try
            {
                var msg = $"MainFlow :  Get Avoid Command, End Adr=[{aseMovingGuide.ToAddressId}],  start check .";
                OnMessageShowEvent?.Invoke(this, msg);

                agvcConnector.PauseAskReserve();

                if (theVehicle.AgvcTransCmdBuffer.Count == 0)
                {
                    throw new Exception("Vehicle has no Command, can not Avoid");
                }

                if (!IsMoveStep())
                {
                    throw new Exception("Vehicle is not moving, can not Avoid");
                }

                if (!IsMoveStopByNoReserve() && !theVehicle.AseMovingGuide.IsAvoidComplete)
                {
                    throw new Exception($"Vehicle is not stop by no reserve, can not Avoid");
                }

                //if (!IsAvoidCommandMatchTheMap(agvcMoveCmd))
                //{
                //    var reason = "避車路徑中 Port Adr 與路段不合圖資";
                //    RejectAvoidCommandAndResume(000018, reason, agvcMoveCmd);
                //    return;
                //}
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                RejectAvoidCommandAndResume(000036, ex.Message, aseMovingGuide);
            }
            #endregion

            #region 避車Command生成
            try
            {
                agvcConnector.PauseAskReserve();
                agvcConnector.ClearAllReserve();
                theVehicle.AseMovingGuide = aseMovingGuide;
                SetupAseMovingGuideMovingSections();
                agvcConnector.SetupNeedReserveSections();
                agvcConnector.ReplyAvoidCommand(aseMovingGuide, 0, "");
                var okmsg = $"MainFlow : Get 避車Command checked , 終點[{aseMovingGuide.ToAddressId}].";
                OnMessageShowEvent?.Invoke(this, okmsg);
                IsAvoidMove = true;
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                StopClearAndReset();
                var reason = "避車Exception";
                RejectAvoidCommandAndResume(000036, reason, aseMovingGuide);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

            #endregion
        }

        private bool IsMoveStopByNoReserve()
        {
            return theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.On;
        }

        private void RejectAvoidCommandAndResume(int alarmCode, string reason, AseMovingGuide aseMovingGuide)
        {
            try
            {
                alarmHandler.SetAlarmFromAgvm(alarmCode);
                agvcConnector.ReplyAvoidCommand(aseMovingGuide, 1, reason);
                reason = $"MainFlow : Reject Avoid Command, " + reason;
                OnMessageShowEvent?.Invoke(this, reason);
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public bool IsAgvcTransferCommandEmpty()
        {
            return theVehicle.AgvcTransCmdBuffer.Count == 0;
        }

        #endregion

        #region Convert AgvcTransferCommand to TransferSteps

        private void InitialTransferSteps(AgvcTransCmd agvcTransCmd)
        {
            TransferStepsIndex = 0;

            switch (agvcTransCmd.AgvcTransCommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    transferSteps.Add(new EmptyTransferStep());
                    break;
                case EnumAgvcTransCommandType.Load:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.LoadAddressId, agvcTransCmd.CommandId);
                    TransferStepsAddLoadCmdInfo(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.Unload:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    TransferStepsAddUnloadCmdInfo(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.LoadUnload:
                    TransferStepsAddMoveCmdInfo(agvcTransCmd.LoadAddressId, agvcTransCmd.CommandId);
                    TransferStepsAddLoadCmdInfo(agvcTransCmd);
                    break;
                case EnumAgvcTransCommandType.MoveToCharger:
                    TransferStepsAddMoveToChargerCmdInfo(agvcTransCmd.UnloadAddressId, agvcTransCmd.CommandId);
                    transferSteps.Add(new EmptyTransferStep());
                    break;
                case EnumAgvcTransCommandType.Override:
                case EnumAgvcTransCommandType.Else:
                default:
                    break;
            }
        }

        private void TransferStepsAddUnloadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            UnloadCmdInfo unloadCmdInfo = new UnloadCmdInfo(agvcTransCmd);
            MapAddress unloadAddress = Mapinfo.addressMap[unloadCmdInfo.PortAddressId];
            unloadCmdInfo.GateType = unloadAddress.GateType;
            if (string.IsNullOrEmpty(agvcTransCmd.UnloadPortId) || !unloadAddress.PortIdMap.ContainsKey(agvcTransCmd.UnloadPortId))
            {
                unloadCmdInfo.PortNumber = "1";
            }
            else
            {
                unloadCmdInfo.PortNumber = unloadAddress.PortIdMap[agvcTransCmd.UnloadPortId];
            }
            transferSteps.Add(unloadCmdInfo);
        }

        private void TransferStepsAddLoadCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            LoadCmdInfo loadCmdInfo = new LoadCmdInfo(agvcTransCmd);
            MapAddress loadAddress = Mapinfo.addressMap[loadCmdInfo.PortAddressId];
            loadCmdInfo.GateType = loadAddress.GateType;
            if (string.IsNullOrEmpty(agvcTransCmd.LoadPortId) || !loadAddress.PortIdMap.ContainsKey(agvcTransCmd.LoadPortId))
            {
                loadCmdInfo.PortNumber = "1";
            }
            else
            {
                loadCmdInfo.PortNumber = loadAddress.PortIdMap[agvcTransCmd.LoadPortId];
            }
            transferSteps.Add(loadCmdInfo);
        }

        private void TransferStepsAddMoveCmdInfo(string endAddressId, string cmdId)
        {
            MapAddress endAddress = Mapinfo.addressMap[endAddressId];
            MoveCmdInfo moveCmd = new MoveCmdInfo(endAddress, cmdId);
            transferSteps.Add(moveCmd);
        }

        private void TransferStepsAddMoveToChargerCmdInfo(string endAddressId, string cmdId)
        {
            MapAddress endAddress = Mapinfo.addressMap[endAddressId];
            MoveToChargerCmdInfo moveCmd = new MoveToChargerCmdInfo(endAddress, cmdId);
            transferSteps.Add(moveCmd);
        }

        private MoveToChargerCmdInfo GetMoveToChargerCmdInfo(AgvcTransCmd agvcTransCmd)
        {
            MapAddress endAddress = Mapinfo.addressMap[agvcTransCmd.UnloadAddressId];
            return new MoveToChargerCmdInfo(endAddress, agvcTransCmd.CommandId);
        }

        private EnumStageDirection EnumStageDirectionParse(EnumPioDirection pioDirection)
        {
            switch (pioDirection)
            {
                case EnumPioDirection.Left:
                    return EnumStageDirection.Left;
                case EnumPioDirection.Right:
                    return EnumStageDirection.Right;
                case EnumPioDirection.None:
                    return EnumStageDirection.None;
                default:
                    return EnumStageDirection.None;
            }
        }

        #endregion

        #endregion

        #region Thd Watch LowPower

        private void WatchLowPower()
        {
            Stopwatch sw = new Stopwatch();
            long total = 0;
            while (true)
            {
                try
                {
                    sw.Restart();

                    #region Pause And Stop Check
                    watchLowPowerPauseEvent.WaitOne(Timeout.Infinite);
                    if (watchLowPowerShutdownEvent.WaitOne(0)) break;
                    #endregion

                    WatchLowPowerStatus = EnumThreadStatus.Working;

                    if (theVehicle.AutoState == EnumAutoState.Auto && transferSteps.Count == 0)
                    {
                        if (IsLowPower())
                        {
                            LowPowerStartCharge(theVehicle.AseMoveStatus.LastAddress);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.WatchLowPowerSleepTimeMs);
                    sw.Stop();
                    total += sw.ElapsedMilliseconds;
                }
            }
            sw.Stop();
            AfterWatchLowPower(total);
        }

        public void StartWatchLowPower()
        {
            watchLowPowerPauseEvent.Set();
            watchLowPowerShutdownEvent.Reset();
            thdWatchLowPower = new Thread(WatchLowPower);
            thdWatchLowPower.IsBackground = true;
            thdWatchLowPower.Start();
            WatchLowPowerStatus = EnumThreadStatus.Start;
            var msg = $"MainFlow : 開始監看自動充電, [Power={theVehicle.AseBatteryStatus.Percentage}][LowSocGap={theVehicle.AutoChargeLowThreshold}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void PauseWatchLowPower()
        {
            watchLowPowerPauseEvent.Reset();
            PreWatchLowPowerStatus = WatchLowPowerStatus;
            WatchLowPowerStatus = EnumThreadStatus.Pause;
            var msg = $"MainFlow : 暫停監看自動充電, [Power={theVehicle.AseBatteryStatus.Percentage}][LowSocGap={theVehicle.AutoChargeLowThreshold}]";
            OnMessageShowEvent?.Invoke(this, msg);
            //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
            //    , msg));

        }

        public void ResumeWatchLowPower()
        {
            watchLowPowerPauseEvent.Set();
            var tempStatus = WatchLowPowerStatus;
            WatchLowPowerStatus = PreWatchLowPowerStatus;
            PreWatchLowPowerStatus = tempStatus;
            var msg = $"MainFlow : 恢復監看自動充電, [Power={theVehicle.AseBatteryStatus.Percentage}][LowSocGap={theVehicle.AutoChargeLowThreshold}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public void StopWatchLowPower()
        {
            if (WatchLowPowerStatus != EnumThreadStatus.None)
            {
                WatchLowPowerStatus = EnumThreadStatus.Stop;
            }
            var msg = $"MainFlow : 停止監看自動充電, [Power={theVehicle.AseBatteryStatus.Percentage}][LowSocGap={theVehicle.AutoChargeLowThreshold}]";
            OnMessageShowEvent?.Invoke(this, msg);
            watchLowPowerShutdownEvent.Set();
            watchLowPowerPauseEvent.Set();
        }

        public void AfterWatchLowPower(long total)
        {
            WatchLowPowerStatus = EnumThreadStatus.None;
            var msg = $"MainFlow : 監看自動充電 後處理, [ThreadStatus={WatchLowPowerStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        private bool IsLowPower()
        {
            return theVehicle.AseBatteryStatus.Percentage <= theVehicle.AutoChargeLowThreshold;
        }

        private bool IsHighPower()
        {
            return theVehicle.AseBatteryStatus.Percentage >= theVehicle.AutoChargeHighThreshold;
        }

        private bool IsWatchLowPowerStop()
        {
            return WatchLowPowerStatus == EnumThreadStatus.Stop || WatchLowPowerStatus == EnumThreadStatus.StopComplete || WatchLowPowerStatus == EnumThreadStatus.None;
        }

        #endregion

        #region Thd Track Position

        private void TrackPosition()
        {
            Stopwatch sw = new Stopwatch();
            long total = 0;
            while (true)
            {
                try
                {
                    sw.Start();

                    #region Pause And Stop Check
                    if (IsTrackPositionPause)
                    {
                        SpinWait.SpinUntil(() => false, mainFlowConfig.TrackPositionSleepTimeMs);
                        continue;
                    }
                    if (IsTrackPositionStop) break;
                    #endregion

                    TrackPositionStatus = EnumThreadStatus.Working;

                    AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                    if (theVehicle.AseMoveStatus.LastMapPosition == null) continue;

                    if (theVehicle.AutoState == EnumAutoState.Auto)
                    {
                        if (transferSteps.Count > 0)
                        {
                            //有搬送Command時, 比對當前Position與搬送路徑Sections計算LastSection/LastAddress/Distance                           
                            if (IsMoveStep())
                            {
                                if (!theVehicle.AseMoveStatus.IsMoveEnd)
                                {
                                    if (!theVehicle.IsSimulation)
                                    {
                                        if (IsResetUpdatePositionReportTimer)
                                        {
                                            IsResetUpdatePositionReportTimer = false;
                                            sw.Reset();
                                        }
                                    }
                                    else
                                    {
                                        if (FakeReserveOkAseMoveStatus.Any())
                                        {
                                            if (theVehicle.AseMovingGuide.ReserveStop == VhStopSingle.On)
                                            {
                                                theVehicle.AseMovingGuide.ReserveStop = VhStopSingle.Off;
                                                agvcConnector.StatusChangeReport();
                                            }
                                            FakeMoveToReserveOkPositions();
                                            sw.Reset();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //無搬送Command時, 比對當前Position與全地圖Sections確定section-distance
                        UpdateVehiclePositionManual();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
                finally
                {
                    SpinWait.SpinUntil(() => false, mainFlowConfig.TrackPositionSleepTimeMs);
                }

                sw.Stop();
                if (sw.ElapsedMilliseconds > mainFlowConfig.ReportPositionIntervalMs)
                {
                    agvcConnector.ReportAddressPass();
                    total += sw.ElapsedMilliseconds;
                    sw.Reset();
                }
            }

            AfterTrackPosition(total);
        }

        public void StartTrackPosition()
        {
            IsTrackPositionPause = false;
            IsTrackPositionStop = false;
            thdTrackPosition = new Thread(TrackPosition);
            thdTrackPosition.IsBackground = true;
            thdTrackPosition.Start();
            TrackPositionStatus = EnumThreadStatus.Start;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : 開始追蹤座標, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }

        public void PauseTrackPosition()
        {
            IsTrackPositionPause = true;
            PreTrackPositionStatus = TrackPositionStatus;
            TrackPositionStatus = EnumThreadStatus.Pause;
            OnMessageShowEvent?.Invoke(this, $"MainFlow : 暫停追蹤座標, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }

        public void ResumeTrackPosition()
        {
            if (thdTrackPosition != null && thdTrackPosition.IsAlive)
            {
                IsTrackPositionPause = false;
                var tempStatus = TrackPositionStatus;
                TrackPositionStatus = PreTrackPositionStatus;
                PreTrackPositionStatus = tempStatus;
                OnMessageShowEvent?.Invoke(this, $"MainFlow : 恢復追蹤座標, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
            }
            else
            {
                StartTrackPosition();
            }
        }

        public void StopTrackPosition()
        {
            IsTrackPositionPause = false;
            IsTrackPositionStop = true;
            if (TrackPositionStatus != EnumThreadStatus.None)
            {
                TrackPositionStatus = EnumThreadStatus.Stop;
            }

            OnMessageShowEvent?.Invoke(this, $"MainFlow : 停止追蹤座標, [TrackPositionStatus={TrackPositionStatus}][PreTrackPositionStatus={PreTrackPositionStatus}]");
        }

        private void AfterTrackPosition(long total)
        {
            TrackPositionStatus = EnumThreadStatus.None;
            var msg = $"MainFlow : 追蹤座標 後處理, [ThreadStatus={TrackPositionStatus}][TotalSpendMs={total}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        private void FakeMoveToReserveOkPositions()
        {
            theVehicle.AseMoveStatus.AseMoveState = EnumAseMoveState.Working;
            SpinWait.SpinUntil(() => false, 2000);
            FakeReserveOkAseMoveStatus.TryDequeue(out AseMoveStatus targetAseMoveStatus);
            AseMoveStatus tempMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

            if (targetAseMoveStatus.LastSection.Id != tempMoveStatus.LastSection.Id)
            {
                tempMoveStatus.LastSection = targetAseMoveStatus.LastSection;
                GetFakeSectionDistance(tempMoveStatus);
                theVehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();

                SpinWait.SpinUntil(() => false, 1000);

                tempMoveStatus.LastMapPosition = targetAseMoveStatus.LastMapPosition;
                tempMoveStatus.Speed = targetAseMoveStatus.Speed;
                tempMoveStatus.HeadDirection = targetAseMoveStatus.HeadDirection;
                tempMoveStatus.LastAddress = targetAseMoveStatus.LastAddress;
                GetFakeSectionDistance(tempMoveStatus);
                theVehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();

                theVehicle.AseMovingGuide.MovingSectionsIndex++;
                UpdateAgvcConnectorGotReserveOkSections(targetAseMoveStatus.LastSection.Id);
                SpinWait.SpinUntil(() => false, 1000);
            }

            if (targetAseMoveStatus.LastAddress.Id != theVehicle.AseMoveStatus.LastAddress.Id)
            {
                SpinWait.SpinUntil(() => false, 1000);

                tempMoveStatus.LastMapPosition = targetAseMoveStatus.LastMapPosition;
                tempMoveStatus.Speed = targetAseMoveStatus.Speed;
                tempMoveStatus.HeadDirection = targetAseMoveStatus.HeadDirection;
                tempMoveStatus.LastAddress = targetAseMoveStatus.LastAddress;
                GetFakeSectionDistance(tempMoveStatus);
                theVehicle.AseMoveStatus = tempMoveStatus;
                agvcConnector.ReportSectionPass();
            }

            if (FakeReserveOkAseMoveStatus.IsEmpty)
            {
                theVehicle.AseMoveStatus.AseMoveState = EnumAseMoveState.Idle;
            }

            if (targetAseMoveStatus.IsMoveEnd)
            {
                AseMoveControl_OnMoveFinished(this, EnumMoveComplete.Success);
                theVehicle.AseMoveStatus.IsMoveEnd = true;
            }
        }

        private static void GetFakeSectionDistance(AseMoveStatus tempMoveStatus)
        {
            if (tempMoveStatus.LastAddress.Id == tempMoveStatus.LastSection.TailAddress.Id)
            {
                tempMoveStatus.LastSection.VehicleDistanceSinceHead = tempMoveStatus.LastSection.HeadToTailDistance;
            }
            else
            {
                tempMoveStatus.LastSection.VehicleDistanceSinceHead = 0;
            }
        }

        public bool IsPositionInThisAddress(MapPosition realPosition, MapPosition addressPosition)
        {
            return mapHandler.IsPositionInThisAddress(realPosition, addressPosition);
        }

        public bool IsAddressInThisSection(MapSection mapSection, MapAddress mapAddress)
        {
            return mapSection.InsideAddresses.FindIndex(x => x.Id == mapAddress.Id) > -1;
        }

        #endregion

        public bool CanVehMove()
        {
            if (theVehicle.IsCharging)//dabid
            {
                StopCharge();
            }
            return theVehicle.AseRobotStatus.IsHome && !theVehicle.IsCharging;
        }

        public void AgvcConnector_GetReserveOkUpdateMoveControlNextPartMovePosition(MapSection mapSection, EnumKeepOrGo keepOrGo)
        {
            try
            {
                int sectionIndex = theVehicle.AseMovingGuide.GuideSectionIds.FindIndex(x => x == mapSection.Id);
                MapAddress address = Mapinfo.addressMap[theVehicle.AseMovingGuide.GuideAddressIds[sectionIndex + 1]];

                bool isEnd = false;
                EnumAddressDirection addressDirection = EnumAddressDirection.None;
                if (address.Id == theVehicle.AseMovingGuide.ToAddressId)
                {
                    addressDirection = address.TransferPortDirection;
                    isEnd = true;
                }
                int headAngle = (int)address.VehicleHeadAngle;
                int speed = (int)mapSection.Speed;

                if (theVehicle.IsSimulation)
                {
                    AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                    aseMoveStatus.AseMoveState = isEnd ? EnumAseMoveState.Idle : EnumAseMoveState.Working;
                    aseMoveStatus.LastAddress = address;
                    aseMoveStatus.LastMapPosition = address.Position;
                    aseMoveStatus.LastSection = mapSection;
                    aseMoveStatus.HeadDirection = headAngle;
                    aseMoveStatus.Speed = speed;
                    aseMoveStatus.IsMoveEnd = isEnd;
                    FakeReserveOkAseMoveStatus.Enqueue(aseMoveStatus);
                }
                else
                {
                    EnumAseMoveCommandIsEnd moveCommandIsEnd = isEnd ? EnumAseMoveCommandIsEnd.End : EnumAseMoveCommandIsEnd.None;
                    asePackage.aseMoveControl.PartMove(addressDirection, address.Position, headAngle, speed, moveCommandIsEnd, keepOrGo);
                }

                OnMessageShowEvent?.Invoke(this, $"Send to MoveControl get reserve {mapSection.Id} ok , next end point [{address.Id}]({Convert.ToInt32(address.Position.X)},{Convert.ToInt32(address.Position.Y)}).");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public bool IsMoveStep() => GetCurrentTransferStepType() == EnumTransferStepType.Move || GetCurrentTransferStepType() == EnumTransferStepType.MoveToCharger;

        public bool IsRobotStep() => GetCurrentTransferStepType() == EnumTransferStepType.Load || GetCurrentTransferStepType() == EnumTransferStepType.Unload;

        public void AseMoveControl_OnMoveFinished(object sender, EnumMoveComplete status)
        {
            try
            {
                theVehicle.AseMoveStatus.IsMoveEnd = true;
                #region Not EnumMoveComplete.Success
                if (status == EnumMoveComplete.Fail)
                {
                    alarmHandler.SetAlarmFromAgvm(6);
                    agvcConnector.ClearAllReserve();
                    theVehicle.AseMovingGuide = new AseMovingGuide();
                    if (IsAvoidMove)
                    {
                        agvcConnector.AvoidFail();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Avoid Fail. ");
                        IsAvoidMove = false;
                        return;
                    }
                    else if (IsOverrideMove)
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow :  Override Move Fail. ");
                        IsOverrideMove = false;
                        return;
                    }
                    else
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Fail. ");
                        return;
                    }
                }

                if (status == EnumMoveComplete.Pause)
                {
                    //VisitTransferStepsStatus = EnumThreadStatus.PauseComplete;
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Pause.  checked ");
                    agvcConnector.PauseAskReserve();
                    PauseVisitTransferSteps();
                    return;
                }

                if (status == EnumMoveComplete.Cancel)
                {
                    StopClearAndReset();
                    if (IsAvoidMove)
                    {
                        agvcConnector.AvoidFail();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Avoid Move Cancel checked ");
                        return;
                    }
                    else
                    {
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move Cancel checked ");
                        return;
                    }
                }
                #endregion

                #region EnumMoveComplete.Success
                theVehicle.IsReAuto = false;

                agvcConnector.ClearAllReserve();

                if (IsAvoidMove)
                {
                    theVehicle.AseMovingGuide = new AseMovingGuide();
                    theVehicle.AseMovingGuide.IsAvoidComplete = true;
                    agvcConnector.AvoidComplete();
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Avoid Move End Ok.");
                }
                else
                {
                    MoveCmdInfo moveCmdInfo = (MoveCmdInfo)GetCurTransferStep();
                    agvcConnector.MoveArrival();
                    ArrivalStartCharge(moveCmdInfo.EndAddress);
                    theVehicle.AseMovingGuide = new AseMovingGuide();

                    if (IsNextTransferStepIdle())
                    {
                        TransferComplete(moveCmdInfo.CmdId);
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Move End Ok.");
                    }
                    else
                    {
                        VisitNextTransferStep();
                    }
                }

                IsAvoidMove = false;
                IsOverrideMove = false;

                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void TransferComplete(string cmdId)
        {
            try
            {
                IsVisitTransferStepPause = true;
                AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer[cmdId];
                agvcTransCmd.EnrouteState = CommandState.None;
                ReportAgvcTransferComplete(agvcTransCmd);
                ClearTransferSteps(cmdId);
                AgvcTransCmd cmd = new AgvcTransCmd();
                theVehicle.AgvcTransCmdBuffer.TryRemove(cmdId, out cmd);
                if (theVehicle.AgvcTransCmdBuffer.Count == 0)
                {
                    agvcConnector.NoCommand();
                }
                asePackage.SetTransferCommandInfoRequest();
                OptimizeTransferStepsAfterTransferComplete();
                GoNextTransferStep = true;
                IsVisitTransferStepPause = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void ArrivalStartCharge(MapAddress endAddress)
        {
            try
            {
                try
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(500);
                        StartCharge(endAddress);
                    });

                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void LogRetry(int forkNgRetryTimes)
        {
            try
            {
                var msg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), ",\t", alarmHandler.LastAlarm.AlarmText, ",\t", forkNgRetryTimes);
                mirleLogger.LogString("RetryLog", msg);
            }
            catch (Exception)
            {
            }
        }

        private bool IsNextTransferStepUnload() => GetNextTransferStepType() == EnumTransferStepType.Unload;
        private bool IsNextTransferStepLoad() => GetNextTransferStepType() == EnumTransferStepType.Load;
        private bool IsNextTransferStepMove() => GetNextTransferStepType() == EnumTransferStepType.Move || GetNextTransferStepType() == EnumTransferStepType.MoveToCharger;
        private bool IsNextTransferStepIdle() => GetNextTransferStepType() == EnumTransferStepType.Empty;

        private void VisitNextTransferStep()
        {
            TransferStepsIndex++;
            GoNextTransferStep = true;
        }

        public TransferStep GetCurTransferStep()
        {
            TransferStep transferStep = new EmptyTransferStep();
            if (TransferStepsIndex < transferSteps.Count)
            {
                transferStep = transferSteps[TransferStepsIndex];
            }
            return transferStep;
        }

        public TransferStep GetPreTransferStep()
        {
            TransferStep transferStep = new EmptyTransferStep();
            var preTransferStepsIndex = TransferStepsIndex - 1;
            if (preTransferStepsIndex < transferSteps.Count && preTransferStepsIndex >= 0)
            {
                transferStep = transferSteps[preTransferStepsIndex];
            }
            return transferStep;
        }

        private void ReportAgvcTransferComplete(AgvcTransCmd agvcTransCmd)
        {
            IsAgvcReplySendWaitMessage = false;
            agvcConnector.TransferComplete(agvcTransCmd);
            while (!IsAgvcReplySendWaitMessage)
            {
                Thread.Sleep(500);
                //SpinWait.SpinUntil(() => IsAgvcReplySendWaitMessage, 500);
            }
            IsAgvcReplySendWaitMessage = false;
        }

        private void GetPioDirection(RobotCommand robotCommand)
        {
            try
            {
                MapAddress portAddress = Mapinfo.addressMap[robotCommand.PortAddressId];
                robotCommand.PioDirection = portAddress.PioDirection;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #region Load Unload

        public void Load(LoadCmdInfo loadCmd)
        {
            try
            {
                GetPioDirection(loadCmd);

                AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(loadCmd.SlotNumber);

                OnMessageShowEvent?.Invoke(this, $"PreLoadSlotCheck, [slotNum={aseCarrierSlotStatus.SlotNumber}][slotState={aseCarrierSlotStatus.CarrierSlotStatus}][loadCmdId={loadCmd.CmdId}]");

                if (aseCarrierSlotStatus.CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty)
                {
                    alarmHandler.SetAlarmFromAgvm(000016);
                    return;
                }

                ReportLoadArrival(loadCmd);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReportLoadArrival(LoadCmdInfo loadCmdInfo)
        {
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Load Arrival, [Port Adr = {loadCmdInfo.PortAddressId}][PioDirection = {loadCmdInfo.PioDirection}][Port Num = {loadCmdInfo.PortNumber}][GateType = {loadCmdInfo.GateType}]");
            agvcConnector.ReportLoadArrival(loadCmdInfo.CmdId);
        }

        private void AgvcConnector_OnAgvcAcceptLoadArrivalEvent(object sender, EventArgs e)
        {
            var loadCmd = (LoadCmdInfo)GetCurTransferStep();

            AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(loadCmd.SlotNumber);

            try
            {
                agvcConnector.Loading(loadCmd.CmdId);
                ReadResult = EnumCstIdReadResult.Fail;//dabid

                if (theVehicle.IsSimulation)
                {
                    SimulationLoad(loadCmd, aseCarrierSlotStatus);
                }
                else
                {
                    Task.Run(() => asePackage.aseRobotControl.DoRobotCommand(loadCmd));
                    OnMessageShowEvent?.Invoke(this, $"Loading, [Direction={loadCmd.PioDirection}][SlotNum={loadCmd.SlotNumber}][Load Adr={loadCmd.PortAddressId}][Load Port Num={loadCmd.PortNumber}]");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        private void SimulationLoad(LoadCmdInfo loadCmd, AseCarrierSlotStatus aseCarrierSlotStatus)
        {
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Loading, [Direction={loadCmd.PioDirection}][SlotNum={loadCmd.SlotNumber}][PortNum={loadCmd.PortNumber}]");
            SpinWait.SpinUntil(() => false, 3000);
            aseCarrierSlotStatus.CarrierId = loadCmd.CassetteId;
            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
            ReadResult = EnumCstIdReadResult.Normal;//dabid
            AseRobotControl_OnReadCarrierIdFinishEvent(this, aseCarrierSlotStatus.SlotNumber);
            SpinWait.SpinUntil(() => false, 2000);
            AseRobotContorl_OnRobotCommandFinishEvent(this, (RobotCommand)GetCurTransferStep());
        }

        public void Unload(UnloadCmdInfo unloadCmd)
        {
            //OptimizeTransferSteps();

            GetPioDirection(unloadCmd);

            AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(unloadCmd.SlotNumber);

            if (aseCarrierSlotStatus.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty)
            {
                alarmHandler.SetAlarmFromAgvm(000017);
                return;
            }

            ReportUnloadArrival(unloadCmd);
        }

        private void AgvcConnector_OnAgvcAcceptUnloadArrivalEvent(object sender, EventArgs e)
        {
            try
            {
                UnloadCmdInfo unloadCmd = (UnloadCmdInfo)GetCurTransferStep();
                AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(unloadCmd.SlotNumber);

                agvcConnector.Unloading(unloadCmd.CmdId);

                if (theVehicle.IsSimulation)
                {
                    SimulationUnload(unloadCmd, aseCarrierSlotStatus);
                }
                else
                {
                    Task.Run(() => asePackage.aseRobotControl.DoRobotCommand(unloadCmd));
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Unloading, [Direction{unloadCmd.PioDirection}][SlotNum={unloadCmd.SlotNumber}][Unload Adr={unloadCmd.PortAddressId}][Unload Port Num={unloadCmd.PortNumber}]");
                }
                batteryLog.LoadUnloadCount++;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void ReportUnloadArrival(UnloadCmdInfo unloadCmd)
        {
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Unload Arrival, [Port Adr = {unloadCmd.PortAddressId}][PioDirection = {unloadCmd.PioDirection}][PortNumber = {unloadCmd.PortNumber}]GateType = {unloadCmd.GateType}]");
            agvcConnector.ReportUnloadArrival(unloadCmd.CmdId);
        }

        private void SimulationUnload(UnloadCmdInfo unloadCmd, AseCarrierSlotStatus aseCarrierSlotStatus)
        {
            OnMessageShowEvent?.Invoke(this, $"MainFlow : Unloading, [Direction{unloadCmd.PioDirection}][SlotNum={unloadCmd.SlotNumber}][PortNum={unloadCmd.PortNumber}]");
            SpinWait.SpinUntil(() => false, 3000);
            aseCarrierSlotStatus.CarrierId = "";
            aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
            SpinWait.SpinUntil(() => false, 2000);
            AseRobotContorl_OnRobotCommandFinishEvent(this, (RobotCommand)GetCurTransferStep());
        }

        private void AseRobotControl_OnRobotCommandErrorEvent(object sender, RobotCommand robotCommand)
        {
            OnMessageShowEvent?.Invoke(this, "AseRobotControl_OnRobotCommandErrorEvent");
            StopClearAndReset();
        }

        public void AseRobotContorl_OnRobotCommandFinishEvent(object sender, RobotCommand robotCommand)
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, "AseRobotContorl_OnRobotCommandFinishEvent");
                EnumTransferStepType transferStepType = robotCommand.GetTransferStepType();
                if (transferStepType == EnumTransferStepType.Load)
                {
                    agvcConnector.ReadResult = ReadResult;
                    ReportAgvcLoadComplete(robotCommand.CmdId);

                }
                else if (transferStepType == EnumTransferStepType.Unload)
                {
                    OnMessageShowEvent?.Invoke(this, $"MainFlow : Unload Complete");

                    var slotNumber = robotCommand.SlotNumber;
                    AseCarrierSlotStatus aseCarrierSlotStatus = theVehicle.GetAseCarrierSlotStatus(slotNumber);
                    switch (aseCarrierSlotStatus.CarrierSlotStatus)
                    {
                        case EnumAseCarrierSlotStatus.Empty:
                            ReportAgvcUnloadComplete(robotCommand.CmdId);
                            break;
                        case EnumAseCarrierSlotStatus.Loading:
                        case EnumAseCarrierSlotStatus.ReadFail:
                            if (theVehicle.AgvcTransCmdBuffer.ContainsKey(robotCommand.CmdId))
                            {
                                theVehicle.AgvcTransCmdBuffer[robotCommand.CmdId].CompleteStatus = CompleteStatus.VehicleAbort;
                                TransferComplete(robotCommand.CmdId);
                            }
                            else
                            {
                                OnMessageShowEvent?.Invoke(this, $"AseRobotContorl_OnRobotCommandFinishEvent, can not find command [ID = {robotCommand.CmdId}]. Reset all.");
                                StopClearAndReset();
                            }
                            break;
                        case EnumAseCarrierSlotStatus.PositionError:
                            StopClearAndReset();
                            break;
                    }
                }
                else
                {
                    OnMessageShowEvent?.Invoke(this, $"[{transferStepType}]");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AgvcConnector_OnAgvcAcceptLoadCompleteEvent(object sender, EventArgs e)
        {
            try
            {
                ReportAgvcBcrRead();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void ReportAgvcUnloadComplete(string cmdId)
        {
            //IsAgvcReplySendWaitMessage = false;
            agvcConnector.UnloadComplete(cmdId);
            //while (!IsAgvcReplySendWaitMessage)
            //{
            //    SpinWait.SpinUntil(() => IsAgvcReplySendWaitMessage, 500);
            //}
            //IsAgvcReplySendWaitMessage = false;
        }
        private void ReportAgvcLoadComplete(string cmdId)
        {
            //IsAgvcReplySendWaitMessage = false;
            agvcConnector.LoadComplete(cmdId);
            //while (!IsAgvcReplySendWaitMessage)
            //{
            //    var xx = agvcConnector.queSendWaitWrappers.Count;
            //    Thread.Sleep(500);
            //    //SpinWait.SpinUntil(() => IsAgvcReplySendWaitMessage, 500);
            //}
            //IsAgvcReplySendWaitMessage = false;
        }
        private void ReportAgvcBcrRead()
        {
            //IsAgvcReplySendWaitMessage = false;
            agvcConnector.SendRecv_Cmd136_CstIdReadReport();
            //while (!IsAgvcReplySendWaitMessage)
            //{
            //    SpinWait.SpinUntil(() => IsAgvcReplySendWaitMessage, 500);
            //}
            //IsAgvcReplySendWaitMessage = false;
        }

        private void AseRobotControl_OnReadCarrierIdFinishEvent(object sender, EnumSlotNumber slotNumber)
        {
            try
            {
                #region 2019.12.16 Report to Agvc when ForkFinished

                if (!IsRobotStep()) return;
                var robotCmdInfo = (RobotCommand)GetCurTransferStep();
                if (robotCmdInfo.SlotNumber != slotNumber) return;
                //if (robotCmdInfo.GetTransferStepType() == EnumTransferStepType.Unload) return;
                AseCarrierSlotStatus aseCarrierSlotStatus = slotNumber == EnumSlotNumber.L ? theVehicle.AseCarrierSlotL : theVehicle.AseCarrierSlotR;

                if (GetCurrentTransferStepType() == EnumTransferStepType.Load)
                {
                    switch (aseCarrierSlotStatus.CarrierSlotStatus)
                    {
                        case EnumAseCarrierSlotStatus.Empty:
                            ReadResult = EnumCstIdReadResult.Fail;
                            break;
                        case EnumAseCarrierSlotStatus.Loading:
                            if (robotCmdInfo.CassetteId.Trim() == aseCarrierSlotStatus.CarrierId.Trim())
                            {
                                OnMessageShowEvent?.Invoke(this, $"CST ID = [{aseCarrierSlotStatus.CarrierId.Trim()}] read ok.");
                                ReadResult = EnumCstIdReadResult.Normal;
                            }
                            else
                            {
                                OnMessageShowEvent?.Invoke(this, $"Read CST ID = [{aseCarrierSlotStatus.CarrierId}], unmatch command CST ID = [{robotCmdInfo.CassetteId}]");
                                alarmHandler.SetAlarmFromAgvm(000028);
                                ReadResult = EnumCstIdReadResult.Mismatch;
                            }
                            break;
                        case EnumAseCarrierSlotStatus.ReadFail:
                            OnMessageShowEvent?.Invoke(this, $"CST ID Read Fail");
                            alarmHandler.SetAlarmFromAgvm(000004);
                            ReadResult = EnumCstIdReadResult.Fail;
                            break;
                        case EnumAseCarrierSlotStatus.PositionError:
                            OnMessageShowEvent?.Invoke(this, $"CST Position Error.");
                            alarmHandler.SetAlarmFromAgvm(000051);
                            ReadResult = EnumCstIdReadResult.Fail;
                            StopClearAndReset();
                            break;
                        default:
                            break;
                    }
                }
                else if (GetCurrentTransferStepType() == EnumTransferStepType.Unload)
                {
                    switch (aseCarrierSlotStatus.CarrierSlotStatus)
                    {
                        case EnumAseCarrierSlotStatus.Empty:
                            OnMessageShowEvent?.Invoke(this, $"Slot [{slotNumber}] unload success.");
                            break;
                        case EnumAseCarrierSlotStatus.Loading:
                        case EnumAseCarrierSlotStatus.ReadFail:
                        case EnumAseCarrierSlotStatus.PositionError:
                            alarmHandler.SetAlarmFromAgvm(7);
                            OnMessageShowEvent?.Invoke(this, $"Slot [{slotNumber}] unload fail. [CST ID = {aseCarrierSlotStatus.CarrierId}][{aseCarrierSlotStatus.CarrierSlotStatus}]");
                            break;
                        default:
                            break;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AseRobotControl_OnRobotInterlockErrorEvent(object sender, RobotCommand robotCommand)
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, "AseRobotControl_OnRobotInterlockErrorEvent");
                alarmHandler.ResetAllAlarmsFromAgvm();
                var curCmdId = robotCommand.CmdId;
                if (theVehicle.AgvcTransCmdBuffer.ContainsKey(curCmdId))
                {
                    theVehicle.AgvcTransCmdBuffer[curCmdId].CompleteStatus = CompleteStatus.InterlockError;
                    TransferComplete(curCmdId);
                }
                else
                {
                    OnMessageShowEvent?.Invoke(this, $"AseRobotControl_OnRobotInterlockErrorEvent, can not find command [ID = {curCmdId}]. Reset all.");
                    StopClearAndReset();
                }
            }
            catch (Exception ex)
            {
                OnMessageShowEvent?.Invoke(this, $"InterlockError Exception.");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AgvcConnector_OnSendRecvTimeoutEvent(object sender, EventArgs e)
        {
            alarmHandler.SetAlarmFromAgvm(38);
            StopClearAndReset();
        }

        private void AgvcConnector_OnAgvcContinueBcrReadEvent(object sender, EventArgs e)
        {
            OptimizeTransferStepsAfterLoadEnd();
        }

        private void AgvcConnector_OnAgvcAcceptMoveArrivalEvent(object sender, EventArgs e)
        {
            OnMessageShowEvent(this, "MoveArrival");
            if (transferSteps.Count > 0)
            {
                VisitNextTransferStep();
            }
        }

        private void AgvcConnector_OnAgvcAcceptUnloadCompleteEvent(object sender, EventArgs e)
        {
            try
            {
                var transferStep = GetCurTransferStep();
                TransferComplete(transferStep.CmdId);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void OptimizeTransferStepsAfterLoadEnd()
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, $"Optimize Transfer Steps");

                theVehicle.IsOptimize = true;
                IsVisitTransferStepPause = true;

                var curCmdId = GetCurTransferStep().CmdId;
                var curCmd = theVehicle.AgvcTransCmdBuffer[curCmdId];
                var curStep = GetCurTransferStep();
                transferSteps = new List<TransferStep>();
                TransferStepsIndex = 1;
                transferSteps.Add(curStep);

                if (!mainFlowConfig.DualCommandOptimize)
                {
                    if (curCmd.AgvcTransCommandType == EnumAgvcTransCommandType.LoadUnload)
                    {
                        curCmd.EnrouteState = CommandState.UnloadEnroute;
                        TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmdId);
                        TransferStepsAddUnloadCmdInfo(curCmd);
                    }
                    else
                    {
                        transferSteps = new List<TransferStep>();
                        TransferStepsIndex = 0;
                        TransferComplete(curCmdId);
                    }
                }
                else
                {
                    if (theVehicle.AgvcTransCmdBuffer.Count <= 1)
                    {
                        if (curCmd.AgvcTransCommandType == EnumAgvcTransCommandType.LoadUnload)
                        {
                            curCmd.EnrouteState = CommandState.UnloadEnroute;
                            TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmdId);
                            TransferStepsAddUnloadCmdInfo(curCmd);
                        }
                        else
                        {
                            transferSteps = new List<TransferStep>();
                            TransferStepsIndex = 0;
                            TransferComplete(curCmdId);
                        }
                    }
                    else
                    {
                        if (curCmd.GetCommandActionType() == CommandActionType.Load)
                        {
                            transferSteps = new List<TransferStep>();
                            TransferStepsIndex = 0;
                            TransferComplete(curCmdId);
                        }
                        else
                        {
                            curCmd.EnrouteState = CommandState.UnloadEnroute;
                            var disCurCmdUnload = DistanceFromLastPosition(curCmd.UnloadAddressId);

                            var transferCommands = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                            int nextCmdIndex = transferCommands[0].CommandId == curCmdId ? 1 : 0;
                            var nextCmd = transferCommands[nextCmdIndex];
                            var nextCmdId = nextCmd.CommandId;

                            if (nextCmd.EnrouteState == CommandState.LoadEnroute)
                            {
                                var disNextCmdLoad = DistanceFromLastPosition(nextCmd.LoadAddressId);
                                if (disCurCmdUnload <= disNextCmdLoad)
                                {
                                    TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmdId);
                                    TransferStepsAddUnloadCmdInfo(curCmd);
                                }
                                else
                                {
                                    TransferStepsAddMoveCmdInfo(nextCmd.LoadAddressId, nextCmdId);
                                    TransferStepsAddLoadCmdInfo(nextCmd);
                                }
                            }
                            else if (nextCmd.EnrouteState == CommandState.UnloadEnroute)
                            {
                                var disNextCmdUnload = DistanceFromLastPosition(nextCmd.UnloadAddressId);
                                if (disCurCmdUnload <= disNextCmdUnload)
                                {
                                    TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmdId);
                                    TransferStepsAddUnloadCmdInfo(curCmd);
                                }
                                else
                                {
                                    TransferStepsAddMoveCmdInfo(nextCmd.UnloadAddressId, nextCmdId);
                                    TransferStepsAddUnloadCmdInfo(nextCmd);
                                }
                            }
                        }
                    }
                }

                theVehicle.IsOptimize = false;

                GoNextTransferStep = true;
                IsVisitTransferStepPause = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void BcrReadAbortCommand(string cmdId, CompleteStatus completeStatus)
        {
            IsVisitTransferStepPause = true;
            string curTransferId = GetCurTransferStep().CmdId;

            AgvcTransCmd cancelingAgvcTransCmd = theVehicle.AgvcTransCmdBuffer[cmdId];
            cancelingAgvcTransCmd.CompleteStatus = completeStatus;
            TransferComplete(cmdId);

            if (IsVisitTransferStepPause)
            {
                IsVisitTransferStepPause = false;
            }
        }

        private int DistanceFromLastPosition(string addressId)
        {
            var lastPosition = theVehicle.AseMoveStatus.LastMapPosition;
            var addressPosition = Mapinfo.addressMap[addressId].Position;
            return (int)mapHandler.GetDistance(lastPosition, addressPosition);
        }

        private void OptimizeTransferStepsAfterTransferComplete()
        {
            try
            {
                OnMessageShowEvent?.Invoke(this, $"OptimizeTransferStepsAfterTransferComplete");
                theVehicle.IsOptimize = true;
                transferSteps = new List<TransferStep>();
                TransferStepsIndex = 0;

                if (theVehicle.AgvcTransCmdBuffer.Count > 1)
                {
                    if (!mainFlowConfig.DualCommandOptimize)
                    {
                        List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                        AgvcTransCmd onlyCmd = agvcTransCmds[0];

                        switch (onlyCmd.EnrouteState)
                        {
                            case CommandState.None:
                                TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                                transferSteps.Add(new EmptyTransferStep());
                                break;
                            case CommandState.LoadEnroute:
                                TransferStepsAddMoveCmdInfo(onlyCmd.LoadAddressId, onlyCmd.CommandId);
                                TransferStepsAddLoadCmdInfo(onlyCmd);
                                break;
                            case CommandState.UnloadEnroute:
                                TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                                TransferStepsAddUnloadCmdInfo(onlyCmd);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                        AgvcTransCmd cmd001 = agvcTransCmds[0];

                        switch (cmd001.EnrouteState)
                        {
                            case CommandState.None:
                                {
                                    TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                    transferSteps.Add(new EmptyTransferStep());
                                }
                                break;
                            case CommandState.LoadEnroute:
                                {
                                    var disCmd001Load = DistanceFromLastPosition(cmd001.LoadAddressId);

                                    AgvcTransCmd cmd002 = agvcTransCmds[1];
                                    if (cmd002.EnrouteState == CommandState.LoadEnroute)
                                    {
                                        var disCmd002Load = DistanceFromLastPosition(cmd002.LoadAddressId);
                                        if (disCmd001Load <= disCmd002Load)
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd001.LoadAddressId, cmd001.CommandId);
                                            TransferStepsAddLoadCmdInfo(cmd001);
                                        }
                                        else
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd002.LoadAddressId, cmd002.CommandId);
                                            TransferStepsAddLoadCmdInfo(cmd002);
                                        }
                                    }
                                    else if (cmd002.EnrouteState == CommandState.UnloadEnroute)
                                    {
                                        var disCmd002Unload = DistanceFromLastPosition(cmd002.UnloadAddressId);
                                        if (disCmd001Load <= disCmd002Unload)
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd001.LoadAddressId, cmd001.CommandId);
                                            TransferStepsAddLoadCmdInfo(cmd001);
                                        }
                                        else
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd002.UnloadAddressId, cmd002.CommandId);
                                            TransferStepsAddUnloadCmdInfo(cmd002);
                                        }
                                    }
                                    else
                                    {
                                        StopClearAndReset();
                                        alarmHandler.SetAlarmFromAgvm(1);
                                    }
                                }
                                break;
                            case CommandState.UnloadEnroute:
                                {
                                    var disCmd001Unload = DistanceFromLastPosition(cmd001.UnloadAddressId);

                                    AgvcTransCmd cmd002 = agvcTransCmds[1];
                                    if (cmd002.EnrouteState == CommandState.LoadEnroute)
                                    {
                                        var disCmd002Load = DistanceFromLastPosition(cmd002.LoadAddressId);
                                        if (disCmd001Unload <= disCmd002Load)
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                            TransferStepsAddUnloadCmdInfo(cmd001);
                                        }
                                        else
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd002.LoadAddressId, cmd002.CommandId);
                                            TransferStepsAddLoadCmdInfo(cmd002);
                                        }
                                    }
                                    else if (cmd002.EnrouteState == CommandState.UnloadEnroute)
                                    {
                                        var disCmd002Unload = DistanceFromLastPosition(cmd002.UnloadAddressId);
                                        if (disCmd001Unload <= disCmd002Unload)
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd001.UnloadAddressId, cmd001.CommandId);
                                            TransferStepsAddUnloadCmdInfo(cmd001);
                                        }
                                        else
                                        {
                                            TransferStepsAddMoveCmdInfo(cmd002.UnloadAddressId, cmd002.CommandId);
                                            TransferStepsAddUnloadCmdInfo(cmd002);
                                        }
                                    }
                                    else
                                    {
                                        StopClearAndReset();
                                        alarmHandler.SetAlarmFromAgvm(1);
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                else if (theVehicle.AgvcTransCmdBuffer.Count == 1)
                {
                    List<AgvcTransCmd> agvcTransCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                    AgvcTransCmd onlyCmd = agvcTransCmds[0];

                    switch (onlyCmd.EnrouteState)
                    {
                        case CommandState.None:
                            TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                            transferSteps.Add(new EmptyTransferStep());
                            break;
                        case CommandState.LoadEnroute:
                            TransferStepsAddMoveCmdInfo(onlyCmd.LoadAddressId, onlyCmd.CommandId);
                            TransferStepsAddLoadCmdInfo(onlyCmd);
                            break;
                        case CommandState.UnloadEnroute:
                            TransferStepsAddMoveCmdInfo(onlyCmd.UnloadAddressId, onlyCmd.CommandId);
                            TransferStepsAddUnloadCmdInfo(onlyCmd);
                            break;
                        default:
                            break;
                    }
                }

                theVehicle.IsOptimize = false;

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        #endregion


        #region Simple Getters
        public AlarmHandler GetAlarmHandler() => alarmHandler;
        public AgvcConnector GetAgvcConnector() => agvcConnector;
        public AgvcConnectorConfig GetAgvcConnectorConfig() => agvcConnectorConfig;
        public MainFlowConfig GetMainFlowConfig() => mainFlowConfig;
        public MapConfig GetMapConfig() => mapConfig;
        public MapHandler GetMapHandler() => mapHandler;
        public AlarmConfig GetAlarmConfig() => alarmConfig;
        public AsePackage GetAsePackage() => asePackage;
        public AseMoveControl GetAseMoveControl() => asePackage.aseMoveControl;
        public string GetMoveControlStopResult() => asePackage.aseMoveControl.StopResult;
        #endregion

        public void SetupAseMovingGuideMovingSections()
        {
            try
            {
                //StopCharge();
                AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
                aseMovingGuide.MovingSections.Clear();
                for (int i = 0; i < theVehicle.AseMovingGuide.GuideSectionIds.Count; i++)
                {
                    MapSection mapSection = new MapSection();
                    string sectionId = aseMovingGuide.GuideSectionIds[i].Trim();
                    string addressId = aseMovingGuide.GuideAddressIds[i + 1].Trim();
                    if (!Mapinfo.sectionMap.ContainsKey(sectionId))
                    {
                        throw new Exception($"Map info has no this section ID.[{sectionId}]");
                    }
                    else if (!Mapinfo.addressMap.ContainsKey(addressId))
                    {
                        throw new Exception($"Map info has no this address ID.[{addressId}]");
                    }

                    mapSection = Mapinfo.sectionMap[sectionId];
                    mapSection.CmdDirection = addressId == mapSection.TailAddress.Id ? EnumCommandDirection.Forward : EnumCommandDirection.Backward;
                    aseMovingGuide.MovingSections.Add(mapSection);
                }
                theVehicle.AseMovingGuide = aseMovingGuide;
            }
            catch (Exception ex)
            {
                OnMessageShowEvent?.Invoke(this, ex.Message);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                theVehicle.AseMovingGuide.MovingSections = new List<MapSection>();
                alarmHandler.SetAlarmFromAgvm(18);
                StopClearAndReset();
            }
        }

        private bool UpdateVehiclePositionInMovingStep(AseMoveStatus aseMoveStatus)
        {
            try
            {
                AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);

                if (!aseMovingGuide.GuideSectionIds.Any())
                {
                    theVehicle.AseMoveStatus = aseMoveStatus;
                    UpdateVehiclePositionManual();
                    agvcConnector.ReportAddressPass();
                    return false;
                }

                double neerlyDistance = 999999;
                foreach (string addressId in aseMovingGuide.GuideAddressIds)
                {
                    MapAddress mapAddress = Mapinfo.addressMap[addressId];
                    double dis = mapHandler.GetDistance(aseMoveStatus.LastMapPosition, mapAddress.Position);

                    if (dis < neerlyDistance)
                    {
                        neerlyDistance = dis;
                        aseMoveStatus.LastAddress = mapAddress;
                    }
                }

                foreach (string sectionId in aseMovingGuide.GuideSectionIds)
                {
                    MapSection mapSection = Mapinfo.sectionMap[sectionId];
                    if (mapSection.InSection(aseMoveStatus.LastAddress.Id))
                    {
                        aseMoveStatus.LastSection = mapSection;
                    }
                }

                aseMoveStatus.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(aseMoveStatus.LastAddress.Position, aseMoveStatus.LastSection.HeadAddress.Position);

                theVehicle.AseMoveStatus = aseMoveStatus;

                agvcConnector.ReportAddressPass();

                UpdateAgvcConnectorGotReserveOkSections(aseMoveStatus.LastSection.Id);

                for (int i = 0; i < aseMovingGuide.MovingSections.Count; i++)
                {
                    if (aseMovingGuide.MovingSections[i].Id == aseMoveStatus.LastSection.Id)
                    {
                        theVehicle.AseMovingGuide.MovingSectionsIndex = i;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        private void UpdateVehicleDistanceSinceHead(AseMoveStatus aseMoveStatus)
        {
            if (theVehicle.AutoState == EnumAutoState.Manual)
            {
                theVehicle.AseMoveStatus = aseMoveStatus;
            }
            else if (theVehicle.AutoState == EnumAutoState.Manual)
            {
                if (mapHandler.IsPositionInThisSection(aseMoveStatus.LastSection, aseMoveStatus.LastMapPosition))
                {
                    aseMoveStatus.LastSection.VehicleDistanceSinceHead = mapHandler.GetDistance(aseMoveStatus.LastSection.HeadAddress.Position, aseMoveStatus.LastMapPosition);
                    theVehicle.AseMoveStatus = aseMoveStatus;
                }
            }
        }

        private void MakeUpAlreadyPassSectionReport(AseMoveStatus aseMoveStatus)
        {
            AseMovingGuide aseMovingGuide = new AseMovingGuide(theVehicle.AseMovingGuide);
            MapSection section = aseMovingGuide.MovingSections[aseMovingGuide.MovingSectionsIndex];
            aseMoveStatus.LastSection = section;
            if (section.CmdDirection == EnumCommandDirection.Backward)
            {
                aseMoveStatus.LastAddress = section.HeadAddress;
                aseMoveStatus.LastSection.VehicleDistanceSinceHead = 0;
            }
            else
            {
                aseMoveStatus.LastAddress = section.TailAddress;
                aseMoveStatus.LastSection.VehicleDistanceSinceHead = section.HeadToTailDistance;
            }
            theVehicle.AseMoveStatus = aseMoveStatus;
            agvcConnector.ReportSectionPass();
        }

        public void UpdateVehiclePositionManual()
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);

                #region find nearly address

                double neerlyDistance = 999999;
                foreach (MapAddress mapAddress in Mapinfo.addressMap.Values)
                {
                    double dis = mapHandler.GetDistance(aseMoveStatus.LastMapPosition, mapAddress.Position);

                    if (dis < neerlyDistance)
                    {
                        neerlyDistance = dis;
                        aseMoveStatus.NeerlyAddress = mapAddress;
                    }
                }

                #endregion

                #region find nearly section by nearly address

                List<MapSection> sectionsContainNearlyAddress = new List<MapSection>();
                foreach (MapSection mapSection in Mapinfo.sectionMap.Values)
                {
                    if (mapSection.InSection(aseMoveStatus.NeerlyAddress.Id))
                    {
                        sectionsContainNearlyAddress.Add(mapSection);
                    }
                }

                if (sectionsContainNearlyAddress.Count == 0)
                {
                    alarmHandler.SetAlarmFromAgvm(42);
                    throw new Exception($"Nearly Address({aseMoveStatus.NeerlyAddress.Id}) is isolate with sections.");
                }

                if (aseMoveStatus.LastSection.InSection(aseMoveStatus.NeerlyAddress.Id))
                {
                    aseMoveStatus.NeerlySection = aseMoveStatus.LastSection;
                }
                else
                {
                    aseMoveStatus.NeerlySection = sectionsContainNearlyAddress[0];
                }

                aseMoveStatus.NeerlySection.VehicleDistanceSinceHead = mapHandler.GetDistance(aseMoveStatus.NeerlyAddress.Position, aseMoveStatus.NeerlySection.HeadAddress.Position);

                #endregion

                #region IsReport

                bool isReport = false;
                if (aseMoveStatus.NeerlyAddress.Id != aseMoveStatus.LastAddress.Id)
                {
                    isReport = true;
                }
                if (aseMoveStatus.NeerlySection.Id != aseMoveStatus.LastSection.Id)
                {
                    isReport = true;
                }

                aseMoveStatus.LastSection = aseMoveStatus.NeerlySection;
                aseMoveStatus.LastAddress = aseMoveStatus.NeerlyAddress;

                theVehicle.AseMoveStatus = aseMoveStatus;

                if (isReport)
                {
                    agvcConnector.ReportAddressPass();
                }

                #endregion

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateAgvcConnectorGotReserveOkSections(string id)
        {
            int getReserveOkSectionIndex = 0;
            try
            {
                var getReserveOkSections = agvcConnector.GetReserveOkSections();
                getReserveOkSectionIndex = getReserveOkSections.FindIndex(x => x.Id == id);
                if (getReserveOkSectionIndex < 0) return;
                for (int i = 0; i < getReserveOkSectionIndex; i++)
                {
                    //Remove passed section in ReserveOkSection
                    agvcConnector.DequeueGotReserveOkSections();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"FAIL [SecId={id}][Index={getReserveOkSectionIndex}]");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }

        }

        private void StartCharge(MapAddress endAddress)
        {
            try
            {
                var address = endAddress;
                var percentage = theVehicle.AseBatteryStatus.Percentage;
                var highPercentage = theVehicle.AutoChargeHighThreshold;

                if (address.IsCharger())
                {
                    if (theVehicle.IsCharging)
                    {
                        var msg = $"Vehicle arrival {address.Id},Charge Direction = {address.ChargeDirection},Charge State = {theVehicle.IsCharging}, thus NOT send charge command.";
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                        return;
                    }

                    if (IsHighPower())
                    {
                        var msg = $"Vehicle arrival {address.Id},Charge Direction = {address.ChargeDirection},Precentage = {percentage:F2} > {highPercentage:F2}(High Threshold),  thus NOT send charge command.";
                        OnMessageShowEvent?.Invoke(this, msg);
                        return;
                    }
                    else
                    {
                        var msg = $"Vehicle arrival {address.Id},Charge Direction = {address.ChargeDirection},Precentage = {percentage:F2} < {highPercentage:F2}(High Threshold),  thus SEND charge command.";
                        OnMessageShowEvent?.Invoke(this, msg);
                    }

                    agvcConnector.ChargHandshaking();
                    if (theVehicle.IsSimulation)
                    {
                        theVehicle.IsCharging = true;
                    }

                    asePackage.aseBatteryControl.StartCharge(address.ChargeDirection);

                    int timeoutCount = 10;
                    while (timeoutCount > 0)
                    {
                        if (theVehicle.IsCharging) break;
                        timeoutCount--;
                        Thread.Sleep(mainFlowConfig.StartChargeWaitingTimeoutMs);
                    }

                    if (!theVehicle.IsCharging)
                    {
                        alarmHandler.SetAlarmFromAgvm(000013);
                    }
                    else
                    {
                        agvcConnector.Charging();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Arrival Port Adr [{address.Id}], and Charging.");
                        batteryLog.ChargeCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }
        private void LowPowerStartCharge(MapAddress lastAddress)
        {
            try
            {
                var address = lastAddress;
                var percentage = theVehicle.AseBatteryStatus.Percentage;
                var pos = theVehicle.AseMoveStatus.LastMapPosition;
                if (address.IsCharger() && mapHandler.IsPositionInThisAddress(pos, address.Position))
                {
                    if (theVehicle.IsCharging)
                    {
                        return;
                    }
                    else
                    {
                        var msg = $"Addr = {address.Id},and no transfer command,Charge Direction = {address.PioDirection},Precentage = {percentage:F2} < {theVehicle.AutoChargeLowThreshold:F2}(Low Threshold), SEND chsrge command";
                        OnMessageShowEvent?.Invoke(this, msg);
                    }

                    agvcConnector.ChargHandshaking();
                    if (theVehicle.IsSimulation)
                    {
                        theVehicle.IsCharging = true;
                    }

                    asePackage.aseBatteryControl.StartCharge(address.ChargeDirection);

                    int timeoutCount = 10;
                    while (timeoutCount > 0)
                    {
                        if (theVehicle.IsCharging) break;
                        timeoutCount--;
                        Thread.Sleep(mainFlowConfig.StartChargeWaitingTimeoutMs);
                    }
                    //do
                    //{
                    //    if (theVehicle.IsCharging) break;
                    //    timeoutCount--;
                    //    Thread.Sleep(mainFlowConfig.StartChargeWaitingTimeoutMs);
                    //    //SpinWait.SpinUntil(() => false, mainFlowConfig.StartChargeWaitingTimeoutMs);
                    //} while (timeoutCount >= 0);

                    if (!theVehicle.IsCharging)
                    {
                        alarmHandler.SetAlarmFromAgvm(000013);
                    }
                    else
                    {
                        agvcConnector.Charging();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Charging, [Address={address.Id}][IsCharging={theVehicle.IsCharging}]");
                        batteryLog.ChargeCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public bool StopCharge()
        {
            try
            {
                var beginMsg = $"MainFlow : Try STOP charge, [IsCharging={theVehicle.IsCharging}]";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, beginMsg);

                //if (!theVehicle.IsCharging) return true;//dabid-

                AseMoveStatus aseMoveStatus = new AseMoveStatus(theVehicle.AseMoveStatus);
                var address = aseMoveStatus.LastAddress;
                if (address.IsCharger())
                {
                    agvcConnector.ChargHandshaking();

                    if (theVehicle.IsSimulation)
                    {
                        theVehicle.IsCharging = false;
                    }

                    asePackage.aseBatteryControl.StopCharge();
                    int timeoutCount = 10;
                    while (timeoutCount > 0)
                    {
                        if (!theVehicle.IsCharging) break;
                        timeoutCount--;
                        Thread.Sleep(mainFlowConfig.StartChargeWaitingTimeoutMs);
                    }

                    if (theVehicle.IsCharging)
                    {
                        alarmHandler.SetAlarmFromAgvm(000014);
                        StopClearAndReset();//StopVehicle();//200525 dabid# //StopVehicle()
                        return false;
                    }
                    else
                    {
                        agvcConnector.ChargeOff();
                        OnMessageShowEvent?.Invoke(this, $"MainFlow : Stop Charge OK.");
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return false;
            }
        }

        private void AgvcConnector_OnStopClearAndResetEvent(object sender, EventArgs e)
        {
            StopClearAndReset();
        }

        public void IntoAuto()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void StopClearAndReset()
        {
            try
            {
                PauseTransfer();
                agvcConnector.ClearAllReserve();
                theVehicle.AseMovingGuide = new AseMovingGuide();
                StopVehicle();
                ReadResult = EnumCstIdReadResult.Fail;//dabid


                var transferCommands = theVehicle.AgvcTransCmdBuffer.Values.ToList();
                foreach (var transCmd in transferCommands)
                {
                    theVehicle.AgvcTransCmdBuffer[transCmd.CommandId].CompleteStatus = GetStopAndClearCompleteStatus(transCmd.CompleteStatus);
                    TransferComplete(transCmd.CommandId);
                }


                if (theVehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading || theVehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
                {
                    asePackage.aseRobotControl.ReadCarrierId();
                }

                if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.On)
                {
                    theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                    agvcConnector.StatusChangeReport();
                }

                var msg = $"MainFlow : Stop And Clear";
                OnMessageShowEvent?.Invoke(this, msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private CompleteStatus GetStopAndClearCompleteStatus(CompleteStatus completeStatus)
        {
            switch (completeStatus)
            {
                case CompleteStatus.Move:
                case CompleteStatus.Load:
                case CompleteStatus.Unload:
                case CompleteStatus.Loadunload:
                case CompleteStatus.MoveToCharger:
                    return CompleteStatus.VehicleAbort;
                case CompleteStatus.Cancel:
                case CompleteStatus.Abort:
                case CompleteStatus.VehicleAbort:
                case CompleteStatus.IdmisMatch:
                case CompleteStatus.IdreadFailed:
                case CompleteStatus.InterlockError:
                case CompleteStatus.LongTimeInaction:
                case CompleteStatus.ForceFinishByOp:
                default:
                    return completeStatus;
            }
        }

        private bool IsInterlockErrorOrBcrReadFail(AgvcTransCmd agvcTransCmd)
        {
            return agvcTransCmd.CompleteStatus == CompleteStatus.InterlockError || agvcTransCmd.CompleteStatus == CompleteStatus.IdmisMatch || agvcTransCmd.CompleteStatus == CompleteStatus.IdreadFailed;
        }

        public EnumTransferStepType GetCurrentTransferStepType()
        {
            try
            {
                if (transferSteps.Count > 0)
                {
                    if (TransferStepsIndex < transferSteps.Count)
                    {
                        return transferSteps[TransferStepsIndex].GetTransferStepType();
                    }
                }

                return EnumTransferStepType.Empty;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return EnumTransferStepType.Empty;
            }
        }
        public EnumTransferStepType GetNextTransferStepType()
        {
            try
            {
                if (transferSteps.Count > 0)
                {
                    if (TransferStepsIndex + 1 < transferSteps.Count)
                    {
                        return transferSteps[TransferStepsIndex + 1].GetTransferStepType();
                    }
                }

                return EnumTransferStepType.Empty;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
                return EnumTransferStepType.Empty;
            }
        }

        public int GetTransferStepsCount()
        {
            return transferSteps.Count;
        }

        public void StopVehicle()
        {
            asePackage.aseMoveControl.StopAndClear();
            asePackage.aseRobotControl.ClearRobotCommand();
            asePackage.aseBatteryControl.StopCharge();

            var msg = $"MainFlow : Stop Vehicle, [MoveState={theVehicle.AseMoveStatus.AseMoveState}][IsCharging={theVehicle.IsCharging}]";
            OnMessageShowEvent?.Invoke(this, msg);
        }

        public bool SetManualToAuto()
        {
            StopClearAndReset();
            //string reason = "";
            //if (!moveControlPlate.CanAuto(ref reason))
            //{
            //    reason = $"Manual switch to  Auto 失敗, 原因： " + reason;
            //    OnMessageShowEvent?.Invoke(this, reason);
            //    return false;
            //}
            //else
            //{
            //    string msg = $"Manual switch to  Auto  ok ";
            //    OnMessageShowEvent?.Invoke(this, msg);
            //    return true;
            //}

            string msg = $"Manual switch to  Auto  ok ";
            OnMessageShowEvent?.Invoke(this, msg);
            return true;

        }

        public void ResetAllarms()
        {
            alarmHandler.ResetAllAlarmsFromAgvm();
        }

        public void SetupVehicleSoc(int percentage)
        {
            asePackage.aseBatteryControl.SetPercentage(percentage);
        }

        private void AgvcConnector_OnRenameCassetteIdEvent(object sender, AseCarrierSlotStatus e)
        {
            try
            {
                AgvcTransCmd agvcTransCmd = theVehicle.AgvcTransCmdBuffer.Values.First(x => x.SlotNumber == e.SlotNumber);
                agvcTransCmd.CassetteId = e.CarrierId;
                theVehicle.AgvcTransCmdBuffer[agvcTransCmd.CommandId] = agvcTransCmd;

                if (transferSteps.Count > 0)
                {
                    foreach (var transferStep in transferSteps)
                    {
                        if (transferStep.CmdId == agvcTransCmd.CommandId && IsRobCommand(transferStep))
                        {
                            ((RobotCommand)transferStep).CassetteId = e.CarrierId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsRobCommand(TransferStep transferStep) => transferStep.GetTransferStepType() == EnumTransferStepType.Load || transferStep.GetTransferStepType() == EnumTransferStepType.Unload;

        public void AgvcConnector_OnCmdPauseEvent(ushort iSeqNum, PauseEvent type)
        {
            try
            {
                PauseTransfer();
                asePackage.aseMoveControl.VehclePause();
                var msg = $"MainFlow : Get [{type}]Command.";
                OnMessageShowEvent(this, msg);
                agvcConnector.PauseReply(iSeqNum, 0, PauseEvent.Pause);
                if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
                {
                    theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.On;
                    agvcConnector.StatusChangeReport();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void AgvcConnector_OnCmdResumeEvent(ushort iSeqNum, PauseEvent type)
        {
            try
            {
                var msg = $"MainFlow : Get [{type}]Command.";
                OnMessageShowEvent(this, msg);
                agvcConnector.PauseReply(iSeqNum, 0, PauseEvent.Continue);
                asePackage.aseMoveControl.VehcleContinue();
                ResumeVisitTransferSteps();
                agvcConnector.ResumeAskReserve();
                if (theVehicle.AseMovingGuide.PauseStatus == VhStopSingle.Off)
                {
                    theVehicle.AseMovingGuide.PauseStatus = VhStopSingle.Off;
                    agvcConnector.StatusChangeReport();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private bool IsMoveControllPause()
        {
            return (theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Pause || theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Pausing);
        }

        private bool IsMoveControlStop()
        {
            return (theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Idle || theVehicle.AseMoveStatus.AseMoveState == EnumAseMoveState.Stoping);
        }

        public void AgvcConnector_OnCmdCancelAbortEvent(ushort iSeqNum, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            try
            {
                var msg = $"MainFlow : Get [{receive.CancelAction}] Command.";
                OnMessageShowEvent(this, msg);
                PauseTransfer();
                agvcConnector.CancelAbortReply(iSeqNum, 0, receive);

                string abortCmdId = receive.CmdID.Trim();
                var step = GetCurTransferStep();
                bool abortCurCommand = GetCurTransferStep().CmdId == abortCmdId;
                var curCmd = theVehicle.AgvcTransCmdBuffer[abortCmdId];

                if (abortCurCommand)
                {
                    agvcConnector.ClearAllReserve();
                    asePackage.aseMoveControl.VehcleCancel();
                    curCmd.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.CancelAction);
                    TransferComplete(curCmd.CommandId);
                }
                else
                {
                    curCmd.EnrouteState = CommandState.None;
                    ReportAgvcTransferComplete(curCmd);
                    if (GetCurTransferStep().CmdId == abortCmdId)
                    {
                        StopClearAndReset();
                        alarmHandler.SetAlarmFromAgvm(1);
                    }
                    else
                    {
                        AgvcTransCmd cmd = new AgvcTransCmd();
                        theVehicle.AgvcTransCmdBuffer.TryRemove(abortCmdId, out cmd);
                        if (theVehicle.AgvcTransCmdBuffer.Count == 0)
                        {
                            agvcConnector.NoCommand();
                        }
                        asePackage.SetTransferCommandInfoRequest();
                        OptimizeTransferStepsAfterTransferComplete();
                        GoNextTransferStep = true;
                        IsVisitTransferStepPause = false;
                    }
                }

                ResumeTransfer();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void RefreshTransferStepsAfterNextCommandCancel()
        {
            OnMessageShowEvent?.Invoke(this, $"RefreshTransferStepsAfterNextCommandCancel");

            List<AgvcTransCmd> transferCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
            var curCmd = transferCmds[0];

            if (curCmd.EnrouteState == CommandState.LoadEnroute)
            {
                if (transferSteps.Count <= 2)
                {
                    TransferStepsAddMoveCmdInfo(curCmd.UnloadAddressId, curCmd.CommandId);
                    TransferStepsAddUnloadCmdInfo(curCmd);
                }
            }
        }

        private void RefreshTransferStepsAfterCurCommandCancel()
        {
            OnMessageShowEvent?.Invoke(this, $"RefreshTransferStepsAfterNextCommandCancel");

            transferSteps = new List<TransferStep>();
            TransferStepsIndex = 0;
            GoNextTransferStep = true;

            List<AgvcTransCmd> transferCmds = theVehicle.AgvcTransCmdBuffer.Values.ToList();
            var nextCmd = transferCmds[0];

            if (nextCmd.EnrouteState == CommandState.LoadEnroute)
            {
                TransferStepsAddMoveCmdInfo(nextCmd.LoadAddressId, nextCmd.CommandId);
                TransferStepsAddLoadCmdInfo(nextCmd);
            }
            else if (nextCmd.EnrouteState == CommandState.UnloadEnroute)
            {
                TransferStepsAddMoveCmdInfo(nextCmd.UnloadAddressId, nextCmd.CommandId);
                TransferStepsAddUnloadCmdInfo(nextCmd);
            }

        }

        private CompleteStatus GetCompleteStatusFromCancelRequest(CancelActionType cancelAction)
        {
            switch (cancelAction)
            {
                case CancelActionType.CmdCancel:
                    return CompleteStatus.Cancel;
                case CancelActionType.CmdCancelIdMismatch:
                    return CompleteStatus.IdmisMatch;
                case CancelActionType.CmdCancelIdReadFailed:
                    return CompleteStatus.IdreadFailed;
                case CancelActionType.CmdNone:
                case CancelActionType.CmdEms:
                case CancelActionType.CmdAbort:
                default:
                    return CompleteStatus.Abort;
            }
        }

        #region Save/Load Configs

        public void LoadMainFlowConfig()
        {

            mainFlowConfig = xmlHandler.ReadXml<MainFlowConfig>(@"MainFlow.xml");
        }

        public void SetMainFlowConfig(MainFlowConfig mainFlowConfig)
        {
            this.mainFlowConfig = mainFlowConfig;
            xmlHandler.WriteXml(mainFlowConfig, @"MainFlow.xml");
        }

        public void LoadAgvcConnectorConfig()
        {
            agvcConnectorConfig = xmlHandler.ReadXml<AgvcConnectorConfig>(@"AgvcConnector.xml");
        }

        public void SetAgvcConnectorConfig(AgvcConnectorConfig agvcConnectorConfig)
        {
            this.agvcConnectorConfig = agvcConnectorConfig;
            xmlHandler.WriteXml(this.agvcConnectorConfig, @"AgvcConnector.xml");
        }

        #endregion

        #region AsePackage

        private void AseBatteryControl_OnBatteryPercentageChangeEvent(object sender, double batteryPercentage)
        {
            try
            {
                batteryLog.InitialSoc = (int)batteryPercentage;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void ReadCarrierId()
        {
            try
            {
                asePackage.aseRobotControl.ReadCarrierId();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void BuzzOff()
        {
            try
            {
                asePackage.aseBuzzerControl.BuzzerOff();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        public void ResetMoveControlStopResult()
        {
            try
            {
                asePackage.aseMoveControl.StopResult = "";
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.StackTrace);
            }
        }

        private void AsePackage_OnMessageShowEvent(object sender, string e)
        {
            OnMessageShowEvent?.Invoke(this, e);
        }

        #endregion

        public void ResetBatteryLog()
        {
            BatteryLog tempBatteryLog = new BatteryLog();
            tempBatteryLog.ResetTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff");
            tempBatteryLog.InitialSoc = batteryLog.InitialSoc;
            batteryLog = tempBatteryLog;
            //TODO: AgvcConnector
        }

        #region Log

        private void LogException(string classMethodName, string exMsg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Error", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", exMsg));
            }
            catch (Exception)
            {
            }
        }

        private void LogDebug(string classMethodName, string msg)
        {
            try
            {
                mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, agvcConnectorConfig.ClientName, "CarrierID", msg));
            }
            catch (Exception)
            {
            }
        }

        private void LogMsgHandler(object sender, LogFormat e)
        {
            mirleLogger.Log(e);
        }

        #endregion



    }
}
