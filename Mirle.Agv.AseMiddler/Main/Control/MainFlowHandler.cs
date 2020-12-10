using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using Mirle.Tools;
using Newtonsoft.Json;
using NLog.Layouts;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class MainFlowHandler
    {
        #region TransCmds
        public bool IsOverrideMove { get; set; }
        public bool IsAvoidMove { get; set; }
        public bool IsArrivalCharge { get; set; } = false;

        #endregion

        #region Controller

        public AgvcConnector agvcConnector;
        //public MirleLogger mirleLogger = null;
        public AlarmHandler alarmHandler;
        public MapHandler mapHandler;
        public AsePackage asePackage;
        public UserAgent UserAgent { get; set; } = new UserAgent();

        #endregion

        #region Threads

        private Thread thdVisitTransferSteps;
        public bool IsVisitTransferStepPause { get; set; } = false;

        private Thread thdTrackPosition;
        public bool IsTrackPositionPause { get; set; } = false;

        private Thread thdWatchChargeStage;
        public bool IsWatchChargeStagePause { get; set; } = false;

        #endregion

        #region Events
        public event EventHandler<InitialEventArgs> OnComponentIntialDoneEvent;

        #endregion

        #region Models

        public Vehicle Vehicle;

        private bool isIniOk;
        public int InitialSoc { get; set; } = 70;
        public bool IsFirstAhGet { get; set; }
        public string CanAutoMsg { get; set; } = "";
        public DateTime StartChargeTimeStamp { get; set; } = DateTime.Now;
        public DateTime StopChargeTimeStamp { get; set; } = DateTime.Now;
        public bool WaitingTransferCompleteEnd { get; set; } = false;
        public string DebugLogMsg { get; set; } = "";
        public System.Text.StringBuilder SbDebugMsg { get; set; } = new System.Text.StringBuilder(short.MaxValue);
        public LastIdlePosition LastIdlePosition { get; set; } = new LastIdlePosition();
        public bool IsLowPowerStartChargeTimeout { get; set; } = false;
        public bool IsStopChargTimeoutInRobotStep { get; set; } = false;
        public DateTime LowPowerStartChargeTimeStamp { get; set; } = DateTime.Now;
        public int LowPowerRepeatedlyChargeCounter { get; set; } = 0;
        public bool IsStopCharging { get; set; } = false;

        public System.Timers.Timer TimerStopChargeInLastRobotStep { get; set; } = new System.Timers.Timer() { AutoReset = false };

        #endregion

        public MainFlowHandler()
        {
            isIniOk = true;
        }

        #region InitialComponents

        public void InitialMainFlowHandler()
        {
            Vehicle = Vehicle.Instance;
            LoggersInitial();
            ConfigInitial();
            VehicleInitial();
            ControllersInitial();
            EventInitial();

            VehicleLocationInitialAndThreadsInitial();

            if (isIniOk)
            {
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "全部"));
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Start Process Ok.");
            }
        }

        private void ConfigInitial()
        {
            try
            {
                //Main Configs 
                int minThreadSleep = 100;
                string allText = System.IO.File.ReadAllText("MainFlowConfig.json");
                Vehicle.MainFlowConfig = JsonConvert.DeserializeObject<MainFlowConfig>(allText);
                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    Vehicle.LoginLevel = EnumLoginLevel.Admin;
                }
                Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs, minThreadSleep);
                Vehicle.MainFlowConfig.TrackPositionSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.TrackPositionSleepTimeMs, minThreadSleep);
                Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs = Math.Max(Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs, minThreadSleep);

                allText = System.IO.File.ReadAllText("MapConfig.json");
                Vehicle.MapConfig = JsonConvert.DeserializeObject<MapConfig>(allText);

                allText = System.IO.File.ReadAllText("AgvcConnectorConfig.json");
                Vehicle.AgvcConnectorConfig = JsonConvert.DeserializeObject<AgvcConnectorConfig>(allText);
                Vehicle.AgvcConnectorConfig.ScheduleIntervalMs = Math.Max(Vehicle.AgvcConnectorConfig.ScheduleIntervalMs, minThreadSleep);
                Vehicle.AgvcConnectorConfig.AskReserveIntervalMs = Math.Max(Vehicle.AgvcConnectorConfig.AskReserveIntervalMs, minThreadSleep);

                allText = System.IO.File.ReadAllText("AlarmConfig.json");
                Vehicle.AlarmConfig = JsonConvert.DeserializeObject<AlarmConfig>(allText);

                allText = System.IO.File.ReadAllText("BatteryLog.json");
                Vehicle.BatteryLog = JsonConvert.DeserializeObject<BatteryLog>(allText);
                InitialSoc = Vehicle.BatteryLog.InitialSoc;

                //AsePackage Configs
                allText = System.IO.File.ReadAllText("AsePackageConfig.json");
                Vehicle.AsePackageConfig = JsonConvert.DeserializeObject<AsePackageConfig>(allText);
                Vehicle.AsePackageConfig.ScheduleIntervalMs = Math.Max(Vehicle.AsePackageConfig.ScheduleIntervalMs, minThreadSleep);
                Vehicle.AsePackageConfig.WatchWifiSignalIntervalMs = Math.Max(Vehicle.AsePackageConfig.WatchWifiSignalIntervalMs, minThreadSleep);
                Vehicle.AseMoveConfig.WatchPositionInterval = Math.Max(Vehicle.AseMoveConfig.WatchPositionInterval, minThreadSleep);
                Vehicle.AseBatteryConfig.WatchBatteryStateIntervalInCharging = Math.Max(Vehicle.AseBatteryConfig.WatchBatteryStateIntervalInCharging, minThreadSleep);
                Vehicle.AseBatteryConfig.WatchBatteryStateInterval = Math.Max(Vehicle.AseBatteryConfig.WatchBatteryStateInterval, minThreadSleep);

                allText = System.IO.File.ReadAllText("PspConnectionConfig.json");
                Vehicle.PspConnectionConfig = JsonConvert.DeserializeObject<PspConnectionConfig>(allText);

                allText = System.IO.File.ReadAllText("AseBatteryConfig.json");
                Vehicle.AseBatteryConfig = JsonConvert.DeserializeObject<AseBatteryConfig>(allText);

                allText = System.IO.File.ReadAllText("AseMoveConfig.json");
                Vehicle.AseMoveConfig = JsonConvert.DeserializeObject<AseMoveConfig>(allText);

                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    Vehicle.AseBatteryConfig.WatchBatteryStateInterval = 30 * 1000;
                    Vehicle.AseBatteryConfig.WatchBatteryStateIntervalInCharging = 30 * 1000;
                    Vehicle.AseMoveConfig.WatchPositionInterval = 5000;
                }

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
                //string loggerConfigPath = "Log.ini";
                //if (File.Exists(loggerConfigPath))
                //{
                //    mirleLogger = MirleLogger.Instance;
                //}
                //else
                //{
                //    throw new Exception();
                //}

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "紀錄器"));
            }
            catch (Exception)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "紀錄器缺少 Log.ini"));
            }
        }

        private void ControllersInitial()
        {
            try
            {
                alarmHandler = new AlarmHandler();
                mapHandler = new MapHandler();
                agvcConnector = new AgvcConnector(this);
                asePackage = new AsePackage();
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "控制層"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "控制層"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void VehicleInitial()
        {
            try
            {
                IsFirstAhGet = Vehicle.MainFlowConfig.IsSimulation;

                if (Vehicle.MainFlowConfig.ChargeIntervalInRobotingSec > 0)
                {
                    TimerStopChargeInLastRobotStep.Interval = Vehicle.MainFlowConfig.ChargeIntervalInRobotingSec * 1000;
                }

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "台車"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "台車"));
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void EventInitial()
        {
            try
            {
                //來自middleAgent的NewTransCmds訊息, Send to MainFlow(this)'mapHandler
                agvcConnector.OnInstallTransferCommandEvent += AgvcConnector_OnInstallTransferCommandEvent;
                agvcConnector.OnAvoideRequestEvent += AgvcConnector_OnAvoideRequestEvent;
                agvcConnector.OnRenameCassetteIdEvent += AgvcConnector_OnRenameCassetteIdEvent;

                //agvcConnector.OnSendRecvTimeoutEvent += AgvcConnector_OnSendRecvTimeoutEvent;
                agvcConnector.OnCstRenameEvent += AgvcConnector_OnCstRenameEvent;

                //來自MoveControl的移動結束訊息, Send to MainFlow(this)'middleAgent'mapHandler
                asePackage.OnUpdateSlotStatusEvent += AsePackage_OnUpdateSlotStatusEvent;
                asePackage.OnModeChangeEvent += AsePackage_OnModeChangeEvent;
                //asePackage.ImportantPspLog += AsePackage_ImportantPspLog;

                //來自IRobotControl的取放貨結束訊息, Send to MainFlow(this)'middleAgent'mapHandler
                asePackage.OnRobotEndEvent += AsePackage_OnRobotEndEvent;

                //來自IBatterysControl的電量改變訊息, Send to middleAgent
                asePackage.OnBatteryPercentageChangeEvent += agvcConnector.AseBatteryControl_OnBatteryPercentageChangeEvent;
                asePackage.OnBatteryPercentageChangeEvent += AseBatteryControl_OnBatteryPercentageChangeEvent;

                asePackage.OnStatusChangeReportEvent += AsePackage_OnStatusChangeReportEvent;
                asePackage.OnOpPauseOrResumeEvent += AsePackage_OnOpPauseOrResumeEvent;

                asePackage.OnAlarmCodeSetEvent += AsePackage_OnAlarmCodeSetEvent1;
                asePackage.OnAlarmCodeAllResetEvent += AsePackage_OnAlarmCodeResetEvent;

                TimerStopChargeInLastRobotStep.Elapsed += TimerStopChargeInLastRobotStep_Elapsed;

                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(true, "事件"));
            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "事件"));

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }


        private void VehicleLocationInitialAndThreadsInitial()
        {
            try
            {
                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    AsePositionArgs positionArgs = new AsePositionArgs()
                    {
                        Arrival = EnumAseArrival.Arrival,
                        MapPosition = Vehicle.Mapinfo.addressMap.First(/*x => x.Key != ""*/).Value.Position
                    };
                    asePackage.ReceivePositionArgsQueue.Enqueue(positionArgs);
                }
                else
                {
                    if (Vehicle.IsLocalConnect)
                    {
                        asePackage.AllAgvlStatusReportRequest();
                    }
                }
                StartVisitTransferSteps();
                StartTrackPosition();
                StartWatchChargeStage();
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"讀取到的電量為{Vehicle.BatteryLog.InitialSoc}");

            }
            catch (Exception ex)
            {
                isIniOk = false;
                OnComponentIntialDoneEvent?.Invoke(this, new InitialEventArgs(false, "事件"));

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Thd Visit TransferSteps

        public void StartVisitTransferSteps()
        {
            thdVisitTransferSteps = new Thread(VisitTransferSteps);
            thdVisitTransferSteps.IsBackground = true;
            thdVisitTransferSteps.Start();

            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : StartVisitTransferSteps");
        }
        public void PauseVisitTransferSteps()
        {
            IsVisitTransferStepPause = true;
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : PauseVisitTransferSteps");
        }
        public void ResumeVisitTransferSteps()
        {
            IsVisitTransferStepPause = false;
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : ResumeVisitTransferSteps");
        }
        private void VisitTransferSteps()
        {
            while (true)
            {
                try
                {
                    if (Vehicle.TransferCommand.IsStopAndClear)
                    {
                        ClearTransferTransferCommand();
                        SpinWait.SpinUntil(() => Vehicle.TransferCommand.IsStopAndClear, Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);
                        continue;
                    }
                    else if (IsVisitTransferStepPause)
                    {
                        SpinWait.SpinUntil(() => !IsVisitTransferStepPause, Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);
                        continue;
                    }

                    switch (Vehicle.TransferCommand.TransferStep)
                    {
                        case EnumTransferStep.Idle:
                            Idle();
                            break;
                        case EnumTransferStep.MoveToLoad:
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.移動.準備.至取貨站] MoveToLoad.");
                            MoveToAddress(Vehicle.TransferCommand.LoadAddressId, EnumMoveToEndReference.Load);
                            break;
                        case EnumTransferStep.MoveToAddress:
                        case EnumTransferStep.MoveToUnload:
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.移動.準備.至終站] MoveToUnload or MoveToAddress.");
                            MoveToAddress(Vehicle.TransferCommand.UnloadAddressId, EnumMoveToEndReference.Unload);
                            break;
                        case EnumTransferStep.MoveToAvoid:
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.移動.準備.至避車站] MoveToAvoid.");
                            MoveToAddress(Vehicle.AseMovingGuide.ToAddressId, EnumMoveToEndReference.Avoid);
                            break;
                        case EnumTransferStep.MoveToAvoidWaitArrival:
                            if (Vehicle.AseMoveStatus.IsMoveEnd)
                            {
                                MoveToAddressEnd();
                            }
                            else if (Vehicle.AseMoveStatus.LastAddress.Id == Vehicle.AseMovingGuide.ToAddressId)
                            {
                                Vehicle.AseMovingGuide.IsAvoidMove = true;
                                Vehicle.AseMovingGuide.MoveComplete = EnumMoveComplete.Success;
                                MoveToAddressEnd();
                            }
                            break;
                        case EnumTransferStep.AvoidMoveComplete:
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[避車.到站.回報.完成] AvoidMoveComplete.");
                            AvoidMoveComplete();
                            break;
                        case EnumTransferStep.MoveToAddressWaitArrival:
                            if (Vehicle.TransferCommand.SendWaitTimeout)
                            {
                                Vehicle.TransferCommand.IsStopAndClear = true;

                            }
                            else if (Vehicle.AseMoveStatus.IsMoveEnd)
                            {
                                MoveToAddressEnd();
                            }
                            else if (Vehicle.AseMoveStatus.LastAddress.Id == Vehicle.AseMovingGuide.ToAddressId)
                            {
                                MoveToAddressArrival();
                            }
                            break;
                        case EnumTransferStep.WaitMoveArrivalVitualPortReply:
                            if (Vehicle.TransferCommand.SendWaitTimeout)
                            {
                                Vehicle.TransferCommand.IsStopAndClear = true;

                            }
                            else if (Vehicle.TransferCommand.IsVitualPortUnloadArrivalReply)
                            {
                                DealVitualPortUnloadArrivalReply();
                            }
                            break;
                        case EnumTransferStep.MoveToAddressWaitEnd:
                            if (Vehicle.AseMoveStatus.IsMoveEnd)
                            {
                                MoveToAddressEnd();
                            }
                            break;
                        case EnumTransferStep.LoadArrival:
                            LoadArrival();
                            break;
                        case EnumTransferStep.WaitLoadArrivalReply:
                            if (Vehicle.TransferCommand.SendWaitTimeout)
                            {
                                Vehicle.TransferCommand.IsStopAndClear = true;

                            }
                            else if (Vehicle.TransferCommand.IsLoadArrivalReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.到站.回報.成功] AgvcConnector_LoadArrivalReply.");
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.Load;
                            }
                            break;
                        case EnumTransferStep.Load:
                            Load();
                            break;
                        case EnumTransferStep.LoadWaitEnd:
                            if (Vehicle.TransferCommand.IsRobotEnd)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.動作.結束] LoadComplete.");
                                LoadComplete();
                            }
                            break;
                        case EnumTransferStep.WaitLoadCompleteReply:
                            if (Vehicle.TransferCommand.SendWaitTimeout)
                            {
                                Vehicle.TransferCommand.IsStopAndClear = true;

                            }
                            else if (Vehicle.TransferCommand.IsLoadCompleteReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[取貨.完成.回報.成功] AgvcConnector_LoadCompleteReply.");
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitCstIdReadReply;
                                Vehicle.TransferCommand.IsCstIdReadReply = false;
                                agvcConnector.SendRecv_Cmd136_CstIdReadReport();
                            }
                            break;
                        case EnumTransferStep.WaitCstIdReadReply:
                            if (Vehicle.TransferCommand.SendWaitTimeout)
                            {
                                Vehicle.TransferCommand.IsStopAndClear = true;

                            }
                            else if (Vehicle.TransferCommand.IsCstIdReadReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[取貨.貨號回報.成功] AgvcConnector_CstIdReadReply.");

                                LoadEnd();
                            }
                            break;
                        case EnumTransferStep.UnloadArrival:
                            UnloadArrival();
                            break;
                        case EnumTransferStep.WaitUnloadArrivalReply:
                            if (Vehicle.TransferCommand.SendWaitTimeout)
                            {
                                Vehicle.TransferCommand.IsStopAndClear = true;

                            }
                            else if (Vehicle.TransferCommand.IsUnloadArrivalReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨.到站.回報.成功] AgvcConnector_OnAgvcAcceptUnloadArrivalEvent.");

                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.Unload;
                            }
                            break;
                        case EnumTransferStep.Unload:
                            Unload();
                            break;
                        case EnumTransferStep.UnloadWaitEnd:
                            if (Vehicle.TransferCommand.IsRobotEnd)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨.動作.結束] UnloadComplete.");
                                UnloadComplete();
                            }
                            break;
                        case EnumTransferStep.WaitUnloadCompleteReply:
                            if (Vehicle.TransferCommand.SendWaitTimeout)
                            {
                                Vehicle.TransferCommand.IsStopAndClear = true;

                            }
                            else if (Vehicle.TransferCommand.IsUnloadCompleteReply)
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[放貨.完成.回報.成功] AgvcConnector_UnloadCompleteReply.");

                                UnloadEnd();
                            }
                            break;
                        case EnumTransferStep.TransferComplete:
                            TransferCommandComplete();
                            break;
                        case EnumTransferStep.WaitOverrideToContinue:
                            break;
                        case EnumTransferStep.GetNext:
                            {
                                switch (Vehicle.TransferCommand.EnrouteState)
                                {
                                    case CommandState.None:
                                        Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddress;
                                        break;
                                    case CommandState.LoadEnroute:
                                        Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                                        break;
                                    case CommandState.UnloadEnroute:
                                        Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                                        break;
                                }
                            }
                            break;
                        case EnumTransferStep.MoveFail:
                        case EnumTransferStep.RobotFail:
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.VisitTransferStepsSleepTimeMs);
            }
        }

        #region Move Step

        private void MoveToAddress(string endAddressId, EnumMoveToEndReference endReference)
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.移動.準備] MoveToAddress.[{endAddressId}].[{endReference}]");

                LastIdlePosition.TimeStamp = DateTime.Now;
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitArrival;
                Vehicle.AseMovingGuide = new AseMovingGuide();
                Vehicle.AseMovingGuide.ToAddressId = endAddressId;

                if (endAddressId == Vehicle.AseMoveStatus.LastAddress.Id)
                {
                    if (Vehicle.AseMoveStatus.IsMoveEnd)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[原地到站] Same address end.");

                        Vehicle.AseMovingGuide.MoveComplete = EnumMoveComplete.Success;
                    }
                }
                else
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動前.斷充] Move Stop Charge");
                    StopCharge();

                    Vehicle.AseMovingGuide.CommandId = Vehicle.TransferCommand.CommandId;
                    agvcConnector.ReportSectionPass();
                    if (!Vehicle.IsCharging)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[退出.站點] Move Begin.");
                        Vehicle.AseMoveStatus.IsMoveEnd = false;
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"IsMoveEnd Need False And Cur IsMoveEnd = {Vehicle.AseMoveStatus.IsMoveEnd}");
                        if (Vehicle.AseMoveStatus.LastAddress.TransferPortDirection != EnumAddressDirection.None)
                        {
                            asePackage.PartMove(EnumAseMoveCommandIsEnd.Begin);
                        }

                        agvcConnector.ClearAllReserve();
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[詢問.路線] AskGuideAddressesAndSections.");
                        agvcConnector.AskGuideAddressesAndSections(endAddressId);

                        if (endReference == EnumMoveToEndReference.Avoid)
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAvoidWaitArrival;
                        }
                    }
                    else
                    {
                        SetAlarmFromAgvm(58);
                        SpinWait.SpinUntil(() => false, 3000);
                        if (endReference == EnumMoveToEndReference.Avoid)
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAvoid;
                        }
                        else
                        {
                            switch (Vehicle.TransferCommand.EnrouteState)
                            {
                                case CommandState.LoadEnroute:
                                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                                    break;
                                case CommandState.None:
                                case CommandState.UnloadEnroute:
                                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void MoveToAddressArrival()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.到達.站點] MoveToAddressArrival Address = {Vehicle.AseMoveStatus.LastAddress.Id}");

                agvcConnector.ClearAllReserve();
                if (Vehicle.IsCharging) StopCharge();

                if (!IsFirstOrderDealVitualPort())
                {
                    LastIdlePosition.TimeStamp = DateTime.Now;
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitEnd;

                    if (Vehicle.MainFlowConfig.IsSimulation)
                    {
                        Vehicle.AseMovingGuide.MoveComplete = EnumMoveComplete.Success;
                        Vehicle.AseMoveStatus.IsMoveEnd = true;

                        switch (Vehicle.TransferCommand.EnrouteState)
                        {
                            case CommandState.None:
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位] Move end.");
                                break;
                            case CommandState.LoadEnroute:
                                if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Left)
                                {
                                    CheckLoadEnrouteBindSamePortUnload();
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.左開蓋] Move end. Open slot left.");

                                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.L;
                                }
                                else if (Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Right)
                                {
                                    CheckLoadEnrouteBindSamePortUnload();
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.右開蓋] Move end. Open slot right.");

                                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.R;
                                }
                                else
                                {
                                    VehicleSlotFullFindFitUnloadCommand();
                                }
                                break;
                            case CommandState.UnloadEnroute:
                                if (Vehicle.TransferCommand.SlotNumber == EnumSlotNumber.L)
                                {
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.左開蓋] Move end. Open slot left.");
                                }
                                else
                                {
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.右開蓋] Move end. Open slot right.");
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (Vehicle.TransferCommand.EnrouteState)
                        {
                            case CommandState.None:
                                {
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位] Move end.");
                                    asePackage.PartMove(EnumAseMoveCommandIsEnd.End);
                                }
                                break;
                            case CommandState.LoadEnroute:
                                {
                                    if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Left)
                                    {
                                        CheckLoadEnrouteBindSamePortUnload();
                                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.左開蓋] Move end. Open slot left.");
                                        Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.L;
                                        asePackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Left);
                                    }
                                    else if (Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Right)
                                    {
                                        CheckLoadEnrouteBindSamePortUnload();
                                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.右開蓋] Move end. Open slot right.");
                                        Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.R;
                                        asePackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Right);
                                    }
                                    else
                                    {
                                        VehicleSlotFullFindFitUnloadCommand();
                                    }
                                }
                                break;
                            case CommandState.UnloadEnroute:
                                if (Vehicle.TransferCommand.SlotNumber == EnumSlotNumber.L)
                                {
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.左開蓋] Move end. Open slot left.");
                                    asePackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Left);
                                }
                                else
                                {
                                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[二次定位.右開蓋] Move end. Open slot right.");
                                    asePackage.PartMove(EnumAseMoveCommandIsEnd.End, EnumSlotSelect.Right);
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (!CheckUnloadEnrouteBindSamePortLoad())
                    {
                        Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitMoveArrivalVitualPortReply;
                        Vehicle.TransferCommand.IsVitualPortUnloadArrivalReply = false;
                        agvcConnector.ReportUnloadArrival();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private bool CheckUnloadEnrouteBindSamePortLoad()
        {
            var unloadPort = Vehicle.Mapinfo.portMap[Vehicle.TransferCommand.UnloadPortId];
            var agvStation = Vehicle.Mapinfo.agvStationMap[unloadPort.AgvStationId];
            foreach (var transferCommand in Vehicle.mapTransferCommands.Values.ToArray())
            {
                if (transferCommand.EnrouteState == CommandState.LoadEnroute)
                {
                    if (agvStation.IsInThisAgvStationForPortId(transferCommand.LoadPortId))
                    {
                        if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Left)
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                            transferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                            Vehicle.TransferCommand = transferCommand;

                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[雙命令.綁定.左開蓋] Check unload enroute bind same port load. Open slot left.");
                            return true;
                        }
                        else if (Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Right)
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                            transferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                            Vehicle.TransferCommand = transferCommand;

                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[雙命令.綁定.右開蓋] Check unload enroute bind same port load. Open slot right.");
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private void CheckLoadEnrouteBindSamePortUnload()
        {
            try
            {
                if (Vehicle.MainFlowConfig.IsE84Continue)
                {
                    if (string.IsNullOrEmpty(Vehicle.TransferCommand.LoadPortId))
                    {
                        throw new Exception("CheckLoadEnrouteBindSamePortUnload fail. Vehicle.TransferCommand.LoadPortId is empty");
                    }
                    if (!Vehicle.Mapinfo.portMap.ContainsKey(Vehicle.TransferCommand.LoadPortId.Trim()))
                    {
                        throw new Exception($"CheckLoadEnrouteBindSamePortUnload fail. !Vehicle.Mapinfo.portMap.ContainsKey {Vehicle.TransferCommand.LoadPortId}");
                    }
                    var loadPort = Vehicle.Mapinfo.portMap[Vehicle.TransferCommand.LoadPortId.Trim()];
                    if (loadPort.IsAgvStationPort())
                    {
                        var agvStation = Vehicle.Mapinfo.agvStationMap[loadPort.AgvStationId];
                        foreach (var transferCommand in Vehicle.mapTransferCommands.Values.ToArray())
                        {
                            if (transferCommand.EnrouteState == CommandState.UnloadEnroute)
                            {
                                var unloadPort = Vehicle.Mapinfo.portMap[transferCommand.UnloadPortId];
                                if (unloadPort.IsVitualPort)
                                {
                                    if (agvStation.IsInThisAgvStationForPortId(unloadPort.ID))
                                    {
                                        transferCommand.UnloadPortId = Vehicle.TransferCommand.LoadPortId;
                                        Vehicle.TransferCommand.IsE84ContinueLoadAndUnlaod = true;
                                        Vehicle.TransferCommand.E84ContinueCommandId = transferCommand.CommandId;
                                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[雙命令.綁定] Load enroute bind same port unload.[{transferCommand.CommandId}]");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void VehicleSlotFullFindFitUnloadCommand()
        {
            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;

            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[儲位已滿.無法取貨] Move end. No slot to load.");

            bool foundNextCommand = CheckSameStationUnloadCommand();

            SpinWait.SpinUntil(() => foundNextCommand, 2000);
        }
        private bool CheckSameStationUnloadCommand()
        {
            if (!string.IsNullOrEmpty(Vehicle.AseMoveStatus.LastAddress.AgvStationId))
            {
                var agvStationId = Vehicle.AseMoveStatus.LastAddress.AgvStationId;
                var agvStation = Vehicle.Mapinfo.agvStationMap[agvStationId];
                if (Vehicle.Mapinfo.agvStationMap.ContainsKey(Vehicle.AseMoveStatus.LastAddress.AgvStationId))
                {
                    foreach (var transferCommand in Vehicle.mapTransferCommands.Values.ToArray())
                    {
                        if (transferCommand.EnrouteState == CommandState.UnloadEnroute)
                        {
                            if (agvStation.IsInThisAgvStationForPortId(transferCommand.UnloadPortId))
                            {
                                transferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                                Vehicle.TransferCommand = transferCommand;

                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨檢查.儲位已滿] Vehicle slot full. Switch unlaod command.[{Vehicle.TransferCommand.CommandId}]");
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        private bool IsFirstOrderDealVitualPort()
        {
            try
            {
                if (Vehicle.TransferCommand.EnrouteState == CommandState.UnloadEnroute)
                {
                    if (!Vehicle.Mapinfo.portMap.ContainsKey(Vehicle.TransferCommand.UnloadPortId.Trim()))
                    {
                        throw new Exception($"IsFirstOrderDealVitualPort fail. !Vehicle.Mapinfo.portMap.ContainsKey {Vehicle.TransferCommand.UnloadPortId}");
                    }
                    return Vehicle.Mapinfo.portMap[Vehicle.TransferCommand.UnloadPortId].IsVitualPort;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }
        private void DealVitualPortUnloadArrivalReply()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Vitual Port Unload Arrival Replyed. [{Vehicle.PortInfos.Count}]");
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;

                bool foundUnloadPort = false;
                var readyPorts = Vehicle.PortInfos.Where(portInfo => portInfo.IsInputMode && portInfo.IsAGVPortReady).ToList();
                if (readyPorts.Any())
                {
                    var sameAddressReadyPorts = readyPorts.Where(portInfo => portInfo.ID == Vehicle.AseMoveStatus.LastAddress.Id).ToList();
                    if (sameAddressReadyPorts.Any())
                    {
                        Vehicle.TransferCommand.UnloadPortId = sameAddressReadyPorts[0].ID;
                        foundUnloadPort = true;
                    }
                    else
                    {
                        foreach (var portInfo in readyPorts)
                        {
                            if (Vehicle.Mapinfo.portMap.ContainsKey(portInfo.ID))
                            {
                                var port = Vehicle.Mapinfo.portMap[portInfo.ID];
                                Vehicle.TransferCommand.UnloadAddressId = port.ReferenceAddressId;
                                Vehicle.TransferCommand.UnloadPortId = portInfo.ID;
                                foundUnloadPort = true;
                                break;
                            }
                        }
                    }
                }

                if (!foundUnloadPort)
                {
                    VitualPortReplyUnreadyFindFitLoadCommand();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void VitualPortReplyUnreadyFindFitLoadCommand()
        {
            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;

            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[設備未準備.無法放貨] Station unready to unload.");

            bool foundNextCommand = CheckSameStationLoadCommand();

            SpinWait.SpinUntil(() => foundNextCommand, 2000);
        }
        private bool CheckSameStationLoadCommand()
        {
            if (!string.IsNullOrEmpty(Vehicle.AseMoveStatus.LastAddress.AgvStationId))
            {
                var agvStationId = Vehicle.AseMoveStatus.LastAddress.AgvStationId;
                var agvStation = Vehicle.Mapinfo.agvStationMap[agvStationId];
                if (Vehicle.Mapinfo.agvStationMap.ContainsKey(agvStationId))
                {
                    foreach (var transferCommand in Vehicle.mapTransferCommands.Values.ToArray())
                    {
                        if (transferCommand.EnrouteState == CommandState.LoadEnroute)
                        {
                            if (agvStation.IsInThisAgvStationForPortId(transferCommand.LoadPortId))
                            {
                                transferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                                Vehicle.TransferCommand = transferCommand;

                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Vitual port unload unready. Switch Load Command. [{Vehicle.TransferCommand.CommandId}][{Vehicle.PortInfos.Count}]");
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        private void MoveToAddressEnd()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.結束] MoveToAddressEnd IsMoveEnd = {Vehicle.AseMoveStatus.IsMoveEnd}");

                agvcConnector.ClearAllReserve();

                #region Not EnumMoveComplete.Success

                if (Vehicle.AseMovingGuide.MoveComplete == EnumMoveComplete.Fail)
                {
                    Vehicle.AseMoveStatus.IsMoveEnd = false;
                    if (Vehicle.AseMovingGuide.IsAvoidMove)
                    {
                        agvcConnector.AvoidFail();
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[避車.移動.失敗] : Avoid Fail. ");
                        Vehicle.AseMovingGuide.IsAvoidMove = false;
                    }
                    else if (Vehicle.AseMovingGuide.IsOverrideMove)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[變更路徑.移動失敗] :  Override Move Fail. ");
                        Vehicle.AseMovingGuide.IsOverrideMove = false;
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.失敗] : Move Fail. ");
                    }

                    SetAlarmFromAgvm(6);
                    agvcConnector.StatusChangeReport();

                    StopClearAndReset();

                    return;
                }

                #endregion

                #region EnumMoveComplete.Success

                if (Vehicle.AseMovingGuide.MoveComplete == EnumMoveComplete.Success)
                {
                    if (Vehicle.TransferCommand.IsCanceled)
                    {
                        Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
                    }
                    else if (Vehicle.AseMovingGuide.IsAvoidMove)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[避車.到站] AvoidMoveComplete.");

                        Vehicle.AseMovingGuide.IsAvoidMove = false;
                        Vehicle.TransferCommand.TransferStep = EnumTransferStep.AvoidMoveComplete;
                        Vehicle.AseMovingGuide.IsAvoidComplete = true;
                        agvcConnector.AvoidComplete();
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[移動.二次定位.到站] : Move End Ok.");
                        if (!Vehicle.IsCharging) ArrivalStartCharge(Vehicle.AseMoveStatus.LastAddress);
                        Vehicle.AseMovingGuide = new AseMovingGuide();
                        agvcConnector.StatusChangeReport();

                        switch (Vehicle.TransferCommand.EnrouteState)
                        {
                            case CommandState.None:
                                agvcConnector.MoveArrival();
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Move Arrival. [AddressId = {Vehicle.AseMoveStatus.LastAddress.Id}]");
                                break;
                            case CommandState.LoadEnroute:
                                Vehicle.TransferCommand.TransferStep = EnumTransferStep.LoadArrival;
                                break;
                            case CommandState.UnloadEnroute:
                                if (!IsFirstOrderDealVitualPort())
                                {
                                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.UnloadArrival;
                                }
                                else
                                {
                                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitMoveArrivalVitualPortReply;
                                    Vehicle.TransferCommand.IsVitualPortUnloadArrivalReply = false;
                                    agvcConnector.ReportUnloadArrival();
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void AvoidMoveComplete()
        {
            try
            {
                switch (Vehicle.TransferCommand.EnrouteState)
                {
                    case CommandState.None:
                        if (Vehicle.AseMoveStatus.LastAddress.Id == Vehicle.TransferCommand.UnloadAddressId)
                        {
                            agvcConnector.MoveArrival();
                            LastIdlePosition.TimeStamp = DateTime.Now;
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitArrival;
                            Vehicle.AseMovingGuide.ToAddressId = Vehicle.TransferCommand.UnloadAddressId;
                        }
                        else
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitOverrideToContinue;
                        }
                        break;
                    case CommandState.LoadEnroute:
                        if (Vehicle.AseMoveStatus.LastAddress.Id == Vehicle.TransferCommand.LoadAddressId)
                        {
                            LastIdlePosition.TimeStamp = DateTime.Now;
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitArrival;
                            Vehicle.AseMovingGuide.ToAddressId = Vehicle.TransferCommand.LoadAddressId;
                        }
                        else
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitOverrideToContinue;
                        }
                        break;
                    case CommandState.UnloadEnroute:
                        if (Vehicle.AseMoveStatus.LastAddress.Id == Vehicle.TransferCommand.UnloadAddressId)
                        {
                            LastIdlePosition.TimeStamp = DateTime.Now;
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToAddressWaitArrival;
                            Vehicle.AseMovingGuide.ToAddressId = Vehicle.TransferCommand.UnloadAddressId;
                        }
                        else
                        {
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitOverrideToContinue;
                        }
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
        private void AgvcConnector_OnOverrideCommandEvent(object sender, AgvcTransferCommand transferCommand)
        {
            try
            {
                var msg = $"MainFlow :  Get [ Override ]Command[{transferCommand.CommandId}],  start check .";
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void AgvcConnector_OnAvoideRequestEvent(object sender, AseMovingGuide aseMovingGuide)
        {
            #region 避車檢查
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow :  Get Avoid Command, End Adr=[{aseMovingGuide.ToAddressId}],  start check .");

                agvcConnector.PauseAskReserve();

                if (Vehicle.mapTransferCommands.IsEmpty)
                {
                    throw new Exception("Vehicle has no Command, can not Avoid");
                }

                if (!IsMoveStep())
                {
                    throw new Exception("Vehicle is not moving, can not Avoid");
                }

                if (!IsMoveStopByNoReserve() && !Vehicle.AseMovingGuide.IsAvoidComplete)
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
                IsAvoidMove = true;
                agvcConnector.ClearAllReserve();
                Vehicle.AseMovingGuide = aseMovingGuide;
                SetupAseMovingGuideMovingSections();
                agvcConnector.SetupNeedReserveSections();
                agvcConnector.StatusChangeReport();
                agvcConnector.ReplyAvoidCommand(aseMovingGuide.SeqNum, 0, "");
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : Get 避車Command checked , 終點[{aseMovingGuide.ToAddressId}].");
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                StopClearAndReset();
                var reason = "避車Exception";
                RejectAvoidCommandAndResume(000036, reason, aseMovingGuide);
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            #endregion
        }
        public bool IsMoveStep()
        {
            return Vehicle.TransferCommand.TransferStep == EnumTransferStep.MoveToAddressWaitEnd || Vehicle.TransferCommand.TransferStep == EnumTransferStep.MoveToAddressWaitArrival;
        }
        private bool IsMoveStopByNoReserve()
        {
            return Vehicle.AseMovingGuide.ReserveStop == VhStopSingle.On;
        }
        private void RejectAvoidCommandAndResume(int alarmCode, string reason, AseMovingGuide aseMovingGuide)
        {
            try
            {
                SetAlarmFromAgvm(alarmCode);
                agvcConnector.ReplyAvoidCommand(aseMovingGuide.SeqNum, 1, reason);
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, string.Concat($"MainFlow : Reject Avoid Command, ", reason));
                agvcConnector.ResumeAskReserve();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public bool CanVehMove()
        {
            try
            {
                if (Vehicle.IsCharging) //dabid
                {
                    StopCharge();
                }
            }
            catch
            {

            }
            return Vehicle.AseRobotStatus.IsHome && !Vehicle.IsCharging;

        }
        public void AgvcConnector_GetReserveOkUpdateMoveControlNextPartMovePosition(MapSection mapSection, EnumIsExecute keepOrGo)
        {
            try
            {
                int sectionIndex = Vehicle.AseMovingGuide.GuideSectionIds.FindIndex(x => x == mapSection.Id);
                MapAddress address = Vehicle.Mapinfo.addressMap[Vehicle.AseMovingGuide.GuideAddressIds[sectionIndex + 1]];

                int headAngle = (int)address.VehicleHeadAngle;
                int speed = (int)mapSection.Speed;

                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    Task.Run(() =>
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                        AsePositionArgs positionArgs = new AsePositionArgs()
                        {
                            Arrival = EnumAseArrival.Arrival,
                            MapPosition = address.Position
                        };
                        asePackage.ReceivePositionArgsQueue.Enqueue(positionArgs);
                    });
                }
                else
                {
                    asePackage.PartMove(address.Position, headAngle, speed, EnumAseMoveCommandIsEnd.None, keepOrGo, EnumSlotSelect.None);

                }

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Send to MoveControl get reserve {mapSection.Id} ok , next end point [{address.Id}]({Convert.ToInt32(address.Position.X)},{Convert.ToInt32(address.Position.Y)}).");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Robot Step

        private void LoadArrival()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.到站.回報] Load Arrival. [AddressId = {Vehicle.AseMoveStatus.LastAddress.Id}]");

                Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitLoadArrivalReply;
                Vehicle.TransferCommand.IsLoadArrivalReply = false;
                agvcConnector.ReportLoadArrival();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Load()
        {
            try
            {
                if (Vehicle.TransferCommand.IsStopAndClear) return;

                Vehicle.TransferCommand.IsRobotEnd = false;
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.LoadWaitEnd;

                if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Left)
                {
                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.L;
                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                }
                else if (Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty && Vehicle.MainFlowConfig.SlotDisable != EnumSlotSelect.Right)
                {
                    Vehicle.TransferCommand.SlotNumber = EnumSlotNumber.R;
                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                }
                else
                {
                    VehicleSlotFullFindFitUnloadCommand();
                    return;
                }

                LoadCmdInfo loadCmd = GetLoadCommand();
                if (Vehicle.AseMoveStatus.LastAddress.Id != Vehicle.TransferCommand.LoadAddressId)
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                    return;
                }

                if (loadCmd.PortNumber == "9")
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToLoad;
                    return;
                }

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.取貨] Loading, [Direction={loadCmd.PioDirection}][SlotNum={loadCmd.SlotNumber}][Load Adr={loadCmd.PortAddressId}][Load Port Num={loadCmd.PortNumber}][PortId = {Vehicle.TransferCommand.LoadPortId}]");
                agvcConnector.Loading();

                if (!Vehicle.TransferCommand.IsE84ContinueLoadAndUnlaod)
                {
                    CheckStopChargeInLastRobotStep();
                }

                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    SimulationLoad();
                }
                else
                {
                    loadCmd.IsBindUnload = Vehicle.TransferCommand.IsE84ContinueLoadAndUnlaod;
                    Task.Run(() => asePackage.DoRobotCommand(loadCmd));
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private LoadCmdInfo GetLoadCommand()
        {
            try
            {
                MapAddress portAddress = Vehicle.Mapinfo.addressMap[Vehicle.TransferCommand.LoadAddressId];
                LoadCmdInfo robotCommand = new LoadCmdInfo(Vehicle.TransferCommand);
                robotCommand.PioDirection = portAddress.PioDirection;
                robotCommand.GateType = portAddress.GateType;
                string portId = Vehicle.TransferCommand.LoadPortId.Trim();

                if (!Vehicle.Mapinfo.portMap.ContainsKey(portId))
                {
                    robotCommand.PortNumber = "1";
                }
                else
                {
                    var port = Vehicle.Mapinfo.portMap[portId];
                    if (port.IsVitualPort)
                    {
                        robotCommand.PortNumber = "9";
                    }
                    else
                    {
                        robotCommand.PortNumber = port.Number;
                        Vehicle.TransferCommand.LoadAddressId = port.ReferenceAddressId;
                    }
                }

                return robotCommand;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return new LoadCmdInfo(Vehicle.TransferCommand);
            }
        }

        private void SimulationLoad()
        {
            SpinWait.SpinUntil(() => false, 2000);

            switch (Vehicle.TransferCommand.SlotNumber)
            {
                case EnumSlotNumber.L:
                    Vehicle.AseCarrierSlotL.CarrierId = Vehicle.TransferCommand.CassetteId;
                    Vehicle.AseCarrierSlotL.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
                    break;
                case EnumSlotNumber.R:
                    Vehicle.AseCarrierSlotR.CarrierId = Vehicle.TransferCommand.CassetteId;
                    Vehicle.AseCarrierSlotR.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;
                    break;
            }

            agvcConnector.StatusChangeReport();

            SpinWait.SpinUntil(() => false, 2000);

            AsePackage_OnRobotEndEvent(this, EnumRobotEndType.Finished);

            var slotNum = Vehicle.TransferCommand.SlotNumber == EnumSlotNumber.L ? EnumSlotNumber.R : EnumSlotNumber.L;

            //if (Vehicle.TransferCommand.IsE84ContinueLoadAndUnlaod)
            //{
            //    var nextTransferCommand = Vehicle.mapTransferCommands[Vehicle.TransferCommand.E84ContinueCommandId.Trim()];
            //    nextTransferCommand.IsRobotEnd = false;

            //    Task.Run(() =>
            //    {
            //        SpinWait.SpinUntil(() => false, 2000);

            //        SimulationUnload();

            //    });
            //}
        }

        private void LoadComplete()
        {
            try
            {
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitLoadCompleteReply;
                ConfirmBcrReadResultInLoad(Vehicle.TransferCommand.SlotNumber);
                Vehicle.TransferCommand.IsLoadCompleteReply = false;
                Vehicle.TransferCommand.EnrouteState = CommandState.UnloadEnroute;
                agvcConnector.LoadComplete();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ConfirmBcrReadResultInLoad(EnumSlotNumber slotNumber)
        {
            try
            {
                AseCarrierSlotStatus slotStatus = Vehicle.GetAseCarrierSlotStatus(slotNumber);

                if (Vehicle.MainFlowConfig.BcrByPass)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.關閉] BcrByPass.");

                    switch (slotStatus.CarrierSlotStatus)
                    {
                        case EnumAseCarrierSlotStatus.Empty:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.失敗] BcrByPass, slot is empty.");

                                slotStatus.CarrierId = "";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;

                                }
                            }
                            break;
                        case EnumAseCarrierSlotStatus.ReadFail:
                        case EnumAseCarrierSlotStatus.Loading:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.成功] BcrByPass, loading is true.");
                                slotStatus.CarrierId = Vehicle.TransferCommand.CassetteId;
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Loading;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                                }
                            }
                            break;
                        case EnumAseCarrierSlotStatus.PositionError:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.凸片] CST Position Error.");

                                slotStatus.CarrierId = "PositionError";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.PositionError;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }

                                SetAlarmFromAgvm(000051);
                                return;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    switch (slotStatus.CarrierSlotStatus)
                    {
                        case EnumAseCarrierSlotStatus.Empty:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.失敗] CST ID is empty.");

                                slotStatus.CarrierId = "";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }

                                SetAlarmFromAgvm(000051);
                            }
                            break;
                        case EnumAseCarrierSlotStatus.Loading:
                            if (Vehicle.TransferCommand.CassetteId == slotStatus.CarrierId.Trim())
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.成功] CST ID = [{slotStatus.CarrierId.Trim()}] read ok.");

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                                }
                                else
                                {
                                    Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                                }
                            }
                            else
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.失敗] Read CST ID = [{slotStatus.CarrierId}], unmatched command CST ID = [{Vehicle.TransferCommand.CassetteId }]");

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.LeftReadResult = BCRReadResult.BcrMisMatch;
                                }
                                else
                                {
                                    Vehicle.RightReadResult = BCRReadResult.BcrMisMatch;
                                }

                                SetAlarmFromAgvm(000028);
                            }
                            break;
                        case EnumAseCarrierSlotStatus.ReadFail:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[取貨.讀取.失敗] ReadFail.");

                                slotStatus.CarrierId = "ReadFail";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }
                                SetAlarmFromAgvm(000004);
                            }
                            break;
                        case EnumAseCarrierSlotStatus.PositionError:
                            {
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨.讀取.凸片] CST Position Error.");

                                slotStatus.CarrierId = "PositionError";
                                slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.PositionError;

                                if (slotNumber == EnumSlotNumber.L)
                                {
                                    Vehicle.AseCarrierSlotL = slotStatus;
                                    Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                                }
                                else
                                {
                                    Vehicle.AseCarrierSlotR = slotStatus;
                                    Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                                }

                                SetAlarmFromAgvm(000051);
                                return;
                            }
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LoadEnd()
        {
            try
            {
                if (Vehicle.TransferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.Load)
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
                }
                else
                {
                    Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                    Vehicle.TransferCommand.EnrouteState = CommandState.UnloadEnroute;

                    if (Vehicle.MainFlowConfig.DualCommandOptimize)
                    {
                        LoadEndOptimize();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void LoadEndOptimize()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令選擇] Load End Optimize");

                if (Vehicle.TransferCommand.IsE84ContinueLoadAndUnlaod)
                {
                    var bindCommandId = Vehicle.TransferCommand.E84ContinueCommandId.Trim();
                    if (!string.IsNullOrEmpty(bindCommandId))
                    {
                        if (Vehicle.mapTransferCommands.ContainsKey(bindCommandId))
                        {
                            var transferCommand = Vehicle.mapTransferCommands[bindCommandId];
                            Vehicle.TransferCommand = transferCommand;
                            SetupBindUnloadCommand();

                            if (Vehicle.MainFlowConfig.IsSimulation)
                            {
                                SimulationUnload();
                            }

                            return;
                        }
                    }
                }

                var transferCommands = Vehicle.mapTransferCommands.Values.ToArray();

                foreach (var transferCommand in transferCommands)
                {
                    if (transferCommand.IsStopAndClear)
                    {
                        Vehicle.TransferCommand = transferCommand;

                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令切換.中止] Load End Stop And Clear.[{Vehicle.TransferCommand.CommandId}]");

                        return;
                    }
                }

                if (Vehicle.mapTransferCommands.Count > 1)
                {
                    bool foundNextCommand = false;

                    bool isEqLoad = IsEqFromAddressId(Vehicle.TransferCommand.LoadAddressId);

                    if (Vehicle.MainFlowConfig.IsEqPortOrderOptimize)
                    {
                        var samePortTypeTransferCommands = Vehicle.mapTransferCommands.Values.Where(x => IsSamePortTypeFromTransferCommand(x, isEqLoad)).OrderBy(x => DistanceFromLastPosition(x)).ToList();
                        if (samePortTypeTransferCommands.Any())
                        {
                            var transferCommand = samePortTypeTransferCommands.First();
                            transferCommand.TransferStep = EnumTransferStep.GetNext;
                            Vehicle.TransferCommand = transferCommand;
                            foundNextCommand = true;
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令切換] Load End Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                        }
                    }

                    if (!foundNextCommand)
                    {
                        var samePortTypeTransferCommands = Vehicle.mapTransferCommands.Values.Where(x => IsNotUnableToLoadBySlotFull(x)).OrderBy(x => DistanceFromLastPosition(x)).ToList();
                        if (samePortTypeTransferCommands.Any())
                        {
                            var transferCommand = samePortTypeTransferCommands.First();
                            transferCommand.TransferStep = EnumTransferStep.GetNext;
                            Vehicle.TransferCommand = transferCommand;
                            foundNextCommand = true;
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[取貨完成.命令切換] Load End Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void SetupBindUnloadCommand()
        {
            if (Vehicle.TransferCommand.IsStopAndClear) throw new Exception("Stop and Clear.");

            Vehicle.TransferCommand.IsRobotEnd = false;
            Vehicle.TransferCommand.TransferStep = EnumTransferStep.UnloadWaitEnd;

            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.雙命令.放貨] : Unloading, [SlotNum={Vehicle.TransferCommand.SlotNumber}][Unload Adr={Vehicle.TransferCommand.UnloadAddressId}][PortId = {Vehicle.TransferCommand.UnloadPortId}]");

            agvcConnector.ReportNonSendWaitUnloadArrival();
            agvcConnector.Unloading();

            CheckStopChargeInLastRobotStep();
        }

        private bool IsNotUnableToLoadBySlotFull(AgvcTransferCommand x)
        {
            if (x.EnrouteState == CommandState.LoadEnroute)
            {
                return !SlotIsFull();
            }
            return true;
        }

        private int DistanceFromLastPosition(AgvcTransferCommand x)
        {
            switch (x.EnrouteState)
            {
                case CommandState.None:
                    return int.MaxValue;
                case CommandState.LoadEnroute:
                    return DistanceFromLastPosition(x.LoadAddressId);
                case CommandState.UnloadEnroute:
                    return DistanceFromLastPosition(x.UnloadAddressId);
                default:
                    return int.MaxValue;
            }
        }

        private bool IsSamePortTypeFromTransferCommand(AgvcTransferCommand transferCommand, bool isEq)
        {
            switch (transferCommand.EnrouteState)
            {
                case CommandState.None:
                    return false;
                case CommandState.LoadEnroute:
                    {
                        var transferCommandIsEq = IsEqFromAddressId(transferCommand.LoadAddressId);
                        if (SlotIsFull() && transferCommandIsEq)
                        {
                            return false;
                        }
                        return transferCommandIsEq == isEq;
                    }
                case CommandState.UnloadEnroute:
                    return IsEqFromAddressId(transferCommand.UnloadAddressId) == isEq;
                default:
                    return false;
            }

        }

        private int DistanceFromLastPosition(string addressId)
        {
            var lastPosition = Vehicle.AseMoveStatus.LastMapPosition;
            var addressPosition = Vehicle.Mapinfo.addressMap[addressId].Position;
            return (int)mapHandler.GetDistance(lastPosition, addressPosition);
        }

        private void UnloadArrival()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Unload Arrival. [AddressId = {Vehicle.AseMoveStatus.LastAddress.Id}]");

                Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitUnloadArrivalReply;
                Vehicle.TransferCommand.IsUnloadArrivalReply = false;
                agvcConnector.ReportUnloadArrival();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Unload()
        {
            try
            {
                UnloadCmdInfo unloadCmd = PreUnloadCheck();

                if (Vehicle.MainFlowConfig.IsSimulation)
                {
                    SimulationUnload();
                }
                else
                {
                    Task.Run(() => asePackage.DoRobotCommand(unloadCmd));
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private UnloadCmdInfo PreUnloadCheck()
        {
            if (Vehicle.TransferCommand.IsStopAndClear) throw new Exception("Stop and Clear.");

            Vehicle.TransferCommand.IsRobotEnd = false;
            Vehicle.TransferCommand.TransferStep = EnumTransferStep.UnloadWaitEnd;

            CheckSlotStatusCanUnload();

            UnloadCmdInfo unloadCmd = GetUnloadCommand();
            if (Vehicle.AseMoveStatus.LastAddress.Id != Vehicle.TransferCommand.UnloadAddressId)
            {
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                throw new Exception("Move to unload.");
            }

            if (unloadCmd.PortNumber == "9")
            {
                Vehicle.TransferCommand.TransferStep = EnumTransferStep.MoveToUnload;
                throw new Exception("Find real unload port.");
            }

            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.放貨] : Unloading, [Direction{unloadCmd.PioDirection}][SlotNum={unloadCmd.SlotNumber}][Unload Adr={unloadCmd.PortAddressId}][Unload Port Num={unloadCmd.PortNumber}][PortId = {Vehicle.TransferCommand.UnloadPortId}]");
            agvcConnector.Unloading();

            CheckStopChargeInLastRobotStep();
            return unloadCmd;
        }

        private void CheckSlotStatusCanUnload()
        {
            switch (Vehicle.TransferCommand.SlotNumber)
            {
                case EnumSlotNumber.L:
                    if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨前.檢查.失敗] Pre Unload Check Fail. Slot is Empty.");

                        SetAlarmFromAgvm(000017);
                        throw new Exception("Pre Unload Check Fail SlotL.");
                    }
                    break;
                case EnumSlotNumber.R:
                    if (Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Empty)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨前.檢查.失敗] Pre Unload Check Fail. Slot is Empty.");

                        SetAlarmFromAgvm(000017);
                        throw new Exception("Pre Unload Check Fail SlotL.");
                    }
                    break;
            }
        }

        private void CheckStopChargeInLastRobotStep()
        {
            try
            {
                var otherCommands = Vehicle.mapTransferCommands.Values.Where(x => x.CommandId.Trim() != Vehicle.TransferCommand.CommandId.Trim()).ToList();
                if (!otherCommands.Any())
                {
                    StartTimerToStopChargeInRobotStep();
                }
                else
                {
                    var sameAddressCommands = otherCommands.Where(x => x.EnrouteAddressId() == Vehicle.AseMoveStatus.LastAddress.Id).ToList();
                    if (!sameAddressCommands.Any())
                    {
                        StartTimerToStopChargeInRobotStep();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void StartTimerToStopChargeInRobotStep()
        {
            try
            {
                if (!TimerStopChargeInLastRobotStep.Enabled && Vehicle.MainFlowConfig.ChargeIntervalInRobotingSec > 0)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[手臂命令.提早斷充] : StartTimerToStopChargeInRobotStep [ChargeTimeSec = {Vehicle.MainFlowConfig.ChargeIntervalInRobotingSec}].");

                    TimerStopChargeInLastRobotStep.Start();
                }

                //Task.Run(() =>
                //{
                //    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[手臂命令.提早斷充] : ChargeIntervalInRobotingSec [ChargeTimeSec = {Vehicle.MainFlowConfig.ChargeIntervalInRobotingSec}].");

                //    SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.ChargeIntervalInRobotingSec);

                //    StopCharge();
                //});
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void TimerStopChargeInLastRobotStep_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[手臂命令.提早斷充] : TimerStopChargeInLastRobotStep_Elapsed.");
            StopCharge();
        }

        private UnloadCmdInfo GetUnloadCommand()
        {
            try
            {
                MapAddress portAddress = Vehicle.Mapinfo.addressMap[Vehicle.TransferCommand.UnloadAddressId];
                UnloadCmdInfo robotCommand = new UnloadCmdInfo(Vehicle.TransferCommand);
                robotCommand.PioDirection = portAddress.PioDirection;
                robotCommand.GateType = portAddress.GateType;
                string portId = Vehicle.TransferCommand.UnloadPortId.Trim();

                if (!Vehicle.Mapinfo.portMap.ContainsKey(portId))
                {
                    robotCommand.PortNumber = "1";
                }
                else
                {
                    var port = Vehicle.Mapinfo.portMap[portId];
                    if (port.IsVitualPort)
                    {
                        robotCommand.PortNumber = "9";
                    }
                    else
                    {
                        robotCommand.PortNumber = port.Number;
                        Vehicle.TransferCommand.UnloadAddressId = port.ReferenceAddressId;
                    }
                }

                return robotCommand;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return new UnloadCmdInfo(Vehicle.TransferCommand);
            }
        }

        private void SimulationUnload()
        {
            SpinWait.SpinUntil(() => false, 2000);

            switch (Vehicle.TransferCommand.SlotNumber)
            {
                case EnumSlotNumber.L:
                    Vehicle.AseCarrierSlotL.CarrierId = "";
                    Vehicle.AseCarrierSlotL.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                    break;
                case EnumSlotNumber.R:
                    Vehicle.AseCarrierSlotR.CarrierId = "";
                    Vehicle.AseCarrierSlotR.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                    break;
            }

            agvcConnector.StatusChangeReport();

            SpinWait.SpinUntil(() => false, 2000);

            AsePackage_OnRobotEndEvent(this, EnumRobotEndType.Finished);
        }

        private void UnloadComplete()
        {
            try
            {
                var slotNumber = Vehicle.TransferCommand.SlotNumber;
                AseCarrierSlotStatus aseCarrierSlotStatus = Vehicle.GetAseCarrierSlotStatus(slotNumber);

                switch (aseCarrierSlotStatus.CarrierSlotStatus)
                {
                    case EnumAseCarrierSlotStatus.Empty:
                        {
                            Vehicle.TransferCommand.EnrouteState = CommandState.None;
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.WaitUnloadCompleteReply;
                            Vehicle.TransferCommand.IsUnloadCompleteReply = false;
                            agvcConnector.UnloadComplete();
                        }
                        break;
                    case EnumAseCarrierSlotStatus.Loading:
                    case EnumAseCarrierSlotStatus.ReadFail:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨.失敗] :[{slotNumber}][{aseCarrierSlotStatus.CarrierSlotStatus}].");
                            Vehicle.TransferCommand.IsStopAndClear = true;
                        }
                        break;
                    case EnumAseCarrierSlotStatus.PositionError:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放貨.失敗.凸片] : PositionError.");
                            SetAlarmFromAgvm(51);
                            Vehicle.TransferCommand.EnrouteState = CommandState.None;
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.RobotFail;
                        }
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

        private void UnloadEnd()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成 {Vehicle.TransferCommand.AgvcTransCommandType}] TransferComplete.");

                Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AsePackage_OnRobotEndEvent(object sender, EnumRobotEndType robotEndType)
        {
            try
            {
                switch (robotEndType)
                {
                    case EnumRobotEndType.Finished:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[手臂.命令.完成] AseRobotContorl_OnRobotCommandFinishEvent");

                            Vehicle.TransferCommand.IsRobotEnd = true;
                        }
                        break;
                    case EnumRobotEndType.InterlockError:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[手臂.交握.失敗] AseRobotControl_OnRobotInterlockErrorEvent");
                            ResetAllAlarmsFromAgvm();

                            Vehicle.TransferCommand.CompleteStatus = CompleteStatus.InterlockError;
                            Vehicle.TransferCommand.IsStopAndClear = true;

                            if (Vehicle.TransferCommand.IsE84ContinueLoadAndUnlaod)
                            {
                                if (!string.IsNullOrEmpty(Vehicle.TransferCommand.E84ContinueCommandId))
                                {
                                    if (Vehicle.mapTransferCommands.ContainsKey(Vehicle.TransferCommand.E84ContinueCommandId))
                                    {
                                        var transferCommand = Vehicle.mapTransferCommands[Vehicle.TransferCommand.E84ContinueCommandId];
                                        transferCommand.CompleteStatus = CompleteStatus.InterlockError;
                                        transferCommand.IsStopAndClear = true;
                                    }
                                }
                            }
                        }
                        break;
                    case EnumRobotEndType.RobotError:
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[手臂.命令.失敗] AseRobotControl_OnRobotCommandErrorEvent");
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.RobotFail;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AgvcConnector_OnSendRecvTimeoutEvent(object sender, EventArgs e)
        {
            // SetAlarmFromAgvm(38);
        }

        private void AgvcConnector_OnCstRenameEvent(object sender, EnumSlotNumber slotNumber)
        {
            try
            {
                AseCarrierSlotStatus slotStatus = slotNumber == EnumSlotNumber.L ? Vehicle.AseCarrierSlotL : Vehicle.AseCarrierSlotR;
                asePackage.CstRename(slotStatus);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Handle Transfer Command

        private void ClearTransferTransferCommand()
        {
            Vehicle.TransferCommand.IsStopAndClear = false;

            if (Vehicle.TransferCommand.TransferStep == EnumTransferStep.Idle) return;

            Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
            Vehicle.AseMovingGuide = new AseMovingGuide();

            if (!Vehicle.TransferCommand.IsAbortByAgvc())
            {
                Vehicle.TransferCommand.CompleteStatus = CompleteStatus.VehicleAbort;
            }

            TransferCommandComplete();
        }

        private void Idle()
        {
            if (Vehicle.mapTransferCommands.IsEmpty)
            {
                if (Vehicle.ActionStatus == VHActionStatus.Commanding)
                {
                    agvcConnector.NoCommand();
                }
            }
            else
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[發呆.結束.選擇.命令] Idle Pick Command To Do.");

                if (Vehicle.mapTransferCommands.Count == 1)
                {
                    Vehicle.TransferCommand = Vehicle.mapTransferCommands.Values.ToArray()[0];
                }
                else
                {
                    //Vehicle.TransferCommand = PickACommand();
                    Vehicle.TransferCommand = Vehicle.mapTransferCommands.Values.ToArray()[0];
                }
            }
        }

        private void TransferCommandComplete()
        {
            try
            {
                WaitingTransferCompleteEnd = true;

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令.結束] TransferComplete. [CommandId = {Vehicle.TransferCommand.CommandId}][CompleteStatus = {Vehicle.TransferCommand.CompleteStatus}]");

                if (!alarmHandler.dicHappeningAlarms.IsEmpty && !Vehicle.TransferCommand.IsAbortByAgvc())
                {
                    ResetAllAlarmsFromAgvm();
                }
                Vehicle.mapTransferCommands.TryRemove(Vehicle.TransferCommand.CommandId, out AgvcTransferCommand transferCommand);
                agvcConnector.TransferComplete(transferCommand);
                asePackage.SetTransferCommandInfoRequest(transferCommand, EnumCommandInfoStep.End);

                TransferCompleteOptimize();

                if (Vehicle.mapTransferCommands.IsEmpty)
                {
                    Vehicle.ResetPauseFlags();

                    agvcConnector.NoCommand();
                }
                else
                {
                    agvcConnector.StatusChangeReport();
                }

                WaitingTransferCompleteEnd = false;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void TransferCompleteOptimize()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇] TransferCompleteOptimize");

                bool isEqEnd = IsEqFromAddressId(Vehicle.AseMoveStatus.LastAddress.Id);

                if (!Vehicle.mapTransferCommands.IsEmpty)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令選擇.命令選取] Transfer Complete Select Another Transfer Command.");

                    if (Vehicle.mapTransferCommands.Count == 1)
                    {
                        Vehicle.TransferCommand = Vehicle.mapTransferCommands.Values.ToArray()[0];
                    }

                    if (Vehicle.mapTransferCommands.Count > 1)
                    {
                        var transferCommands = Vehicle.mapTransferCommands.Values.ToArray();

                        foreach (var transferCommand in transferCommands)
                        {
                            if (transferCommand.IsStopAndClear)
                            {
                                Vehicle.TransferCommand = transferCommand;

                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.命令切換] Transfer Complete Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");

                                return;
                            }
                        }

                        bool foundNextCommand = false;

                        bool isEqLoad = IsEqFromAddressId(Vehicle.TransferCommand.UnloadAddressId);

                        if (Vehicle.MainFlowConfig.IsEqPortOrderOptimize)
                        {
                            var samePortTypeTransferCommands = Vehicle.mapTransferCommands.Values.Where(x => IsSamePortTypeFromTransferCommand(x, isEqLoad)).OrderBy(x => DistanceFromLastPosition(x)).ToList();
                            if (samePortTypeTransferCommands.Any())
                            {
                                var transferCommand = samePortTypeTransferCommands.First();
                                transferCommand.TransferStep = EnumTransferStep.GetNext;
                                Vehicle.TransferCommand = transferCommand;
                                foundNextCommand = true;
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.命令切換] Transfer Complete Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                            }
                        }

                        if (!foundNextCommand)
                        {
                            var samePortTypeTransferCommands = Vehicle.mapTransferCommands.Values.Where(x => IsNotUnableToLoadBySlotFull(x)).OrderBy(x => DistanceFromLastPosition(x)).ToList();
                            if (samePortTypeTransferCommands.Any())
                            {
                                var transferCommand = samePortTypeTransferCommands.First();
                                transferCommand.TransferStep = EnumTransferStep.GetNext;
                                Vehicle.TransferCommand = transferCommand;
                                foundNextCommand = true;
                                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.命令切換] Transfer Complete Select Another Transfer Command.[{Vehicle.TransferCommand.CommandId}]");
                            }
                        }

                        if (!foundNextCommand)
                        {
                            Vehicle.TransferCommand = Vehicle.mapTransferCommands.Values.ToArray()[0];
                            Vehicle.TransferCommand.TransferStep = EnumTransferStep.GetNext;
                        }
                    }
                }
                else
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[命令完成.命令選擇.無命令] Transfer Complete into Idle.");
                    Vehicle.TransferCommand = new AgvcTransferCommand();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsEqFromPortId(string portId)
        {
            try
            {
                return !Vehicle.Mapinfo.portMap[portId].IsAgvStationPort();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);

                return true;
            }
        }

        private bool IsEqFromAddressId(string addressId)
        {
            try
            {
                return string.IsNullOrEmpty(Vehicle.Mapinfo.addressMap[addressId].AgvStationId);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);

                return true;
            }
        }

        private bool SlotIsFull()
        {
            return Vehicle.GetAseCarrierSlotStatus(EnumSlotNumber.L).CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty && Vehicle.GetAseCarrierSlotStatus(EnumSlotNumber.R).CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty;
        }

        public void StopClearAndReset()
        {
            PauseTransfer();

            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[停止.重置] Stop.Clear.Reset.");

                SpinWait.SpinUntil(() => false, 500);
                agvcConnector.ClearAllReserve();

                if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading || Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
                {
                    asePackage.ReadCarrierId();
                }

                foreach (var transCmd in Vehicle.mapTransferCommands.Values.ToList())
                {
                    transCmd.IsStopAndClear = true;
                }

                Vehicle.TransferCommand.IsStopAndClear = true;

                StopVehicle();

                agvcConnector.StatusChangeReport();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            ResumeTransfer();
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

        private static object installTransferCommandLocker = new object();

        private void AgvcConnector_OnInstallTransferCommandEvent(object sender, AgvcTransferCommand transferCommand)
        {
            lock (installTransferCommandLocker)
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[檢查搬送命令] Check Transfer Command [{transferCommand.CommandId}]");

                #region 檢查搬送Command
                try
                {
                    if (transferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.Override)
                    {
                        AgvcConnector_OnOverrideCommandEvent(sender, transferCommand);
                        return;
                    }

                    switch (transferCommand.AgvcTransCommandType)
                    {
                        case EnumAgvcTransCommandType.Move:
                        case EnumAgvcTransCommandType.MoveToCharger:
                            CheckVehicleTransferCommandMapEmpty();
                            CheckMoveEndAddress(transferCommand.UnloadAddressId);
                            break;
                        case EnumAgvcTransCommandType.Load:
                            CheckRobotPortAddress(transferCommand.LoadAddressId, transferCommand.LoadPortId);
                            CheckCstIdDuplicate(transferCommand.CassetteId);
                            CheckTransferCommandMap(transferCommand);
                            break;
                        case EnumAgvcTransCommandType.Unload:
                            CheckRobotPortAddress(transferCommand.UnloadAddressId, transferCommand.UnloadPortId);
                            transferCommand.SlotNumber = CheckUnloadCstId(transferCommand.CassetteId);
                            CheckTransferCommandMap(transferCommand);
                            break;
                        case EnumAgvcTransCommandType.LoadUnload:
                            CheckRobotPortAddress(transferCommand.LoadAddressId, transferCommand.LoadPortId);
                            CheckRobotPortAddress(transferCommand.UnloadAddressId, transferCommand.UnloadPortId);
                            CheckCstIdDuplicate(transferCommand.CassetteId);
                            CheckTransferCommandMap(transferCommand);
                            break;
                        case EnumAgvcTransCommandType.Override:
                            CheckOverrideAddress(transferCommand);
                            break;
                        case EnumAgvcTransCommandType.Else:
                            break;
                        default:
                            break;
                    }

                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[檢查搬送命令 成功] Check Transfer Command Ok. [{transferCommand.CommandId}]");

                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Reject, ex.Message);
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[檢查搬送命令 失敗] Check Transfer Command Fail. [{transferCommand.CommandId}] {ex.Message}");
                    return;
                }
                #endregion

                #region 搬送流程更新
                try
                {
                    var isMapTransferCommandsEmpty = Vehicle.mapTransferCommands.IsEmpty;
                    Vehicle.mapTransferCommands.TryAdd(transferCommand.CommandId, transferCommand);
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Accept, "");
                    asePackage.SetTransferCommandInfoRequest(transferCommand, EnumCommandInfoStep.Begin);
                    if (isMapTransferCommandsEmpty) agvcConnector.Commanding();
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[初始化搬送命令 成功] Initial Transfer Command Ok. [{transferCommand.CommandId}]");
                }
                catch (Exception ex)
                {
                    agvcConnector.ReplyTransferCommand(transferCommand.CommandId, transferCommand.GetCommandActionType(), transferCommand.SeqNum, (int)EnumAgvcReplyCode.Reject, "");
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[初始化搬送命令 失敗] Initial Transfer Command Fail. [{transferCommand.CommandId}]");
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
                #endregion
            }
        }

        private void CheckVehicleTransferCommandMapEmpty()
        {
            if (WaitingTransferCompleteEnd)
            {
                throw new Exception("Vehicle is waiting last transfer commmand end.");
            }

            if (!Vehicle.mapTransferCommands.IsEmpty)
            {
                throw new Exception("Vehicle transfer command map is not empty.");
            }
        }

        private void CheckCstIdDuplicate(string cassetteId)
        {
            var agvcTransCmdBuffer = Vehicle.mapTransferCommands.Values.ToList();
            for (int i = 0; i < agvcTransCmdBuffer.Count; i++)
            {
                if (agvcTransCmdBuffer[i].CassetteId == cassetteId)
                {
                    throw new Exception("Transfer command casette ID duplicate.");
                }
            }
        }

        private void CheckTransferCommandMap(AgvcTransferCommand transferCommand)
        {
            if (Vehicle.mapTransferCommands.Any(x => IsMoveTransferCommand(x.Value.AgvcTransCommandType)))
            {
                throw new Exception("Vehicle has move command, can not do loadunload.");
            }

            if (Vehicle.MainFlowConfig.SlotDisable == EnumSlotSelect.Both)
            {
                throw new Exception($"Vehicle has no empty slot to transfer cst. Left = Disable, Right = Disable.");
            }

            if (transferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.Load)
            {
                CheckEnoughSamePortTypeCommandsFromPortId(transferCommand, CommandState.LoadEnroute);
            }
            else if (transferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.Unload)
            {
                CheckEnoughSamePortTypeCommandsFromPortId(transferCommand, CommandState.UnloadEnroute);
            }
            else if (transferCommand.AgvcTransCommandType == EnumAgvcTransCommandType.LoadUnload)
            {
                CheckEnoughSamePortTypeCommandsFromPortId(transferCommand, CommandState.LoadEnroute);

                CheckEnoughSamePortTypeCommandsFromPortId(transferCommand, CommandState.UnloadEnroute);
            }
        }

        private void CheckEnoughSamePortTypeCommandsFromPortId(AgvcTransferCommand transferCommand, CommandState enroute)
        {
            //var isAgvStation = IsAgvStationFromPortId(transferCommand,enroute);

            if (IsAgvStationFromPortId(transferCommand, enroute))
            {
                var samePortTypeCommands = (from command in Vehicle.mapTransferCommands.Values.ToArray()
                                            where IsAgvStationFromPortId(command, enroute) /*== isAgvStation*/
                                            select (command)).ToList();

                if (samePortTypeCommands.Count >= 2)
                {
                    throw new Exception($"Vehicle has no enough slot to transfer. [SamePortTypeCommands={samePortTypeCommands.Count}][enroute={enroute}]");
                }
            }
        }

        private bool IsAgvStationFromPortId(AgvcTransferCommand transferCommand, CommandState enroute)
        {
            var portId = enroute == CommandState.LoadEnroute ? transferCommand.LoadPortId : transferCommand.UnloadPortId;
            if (!string.IsNullOrEmpty(portId))
            {
                if (Vehicle.Mapinfo.portMap.ContainsKey(portId))
                {
                    if (Vehicle.Mapinfo.portMap[portId].IsAgvStationPort())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsMoveTransferCommand(EnumAgvcTransCommandType agvcTransCommandType)
        {
            return agvcTransCommandType == EnumAgvcTransCommandType.Move || agvcTransCommandType == EnumAgvcTransCommandType.MoveToCharger;
        }

        private EnumSlotNumber CheckUnloadCstId(string cassetteId)
        {
            if (Vehicle.mapTransferCommands.Any(x => IsMoveTransferCommand(x.Value.AgvcTransCommandType)))
            {
                throw new Exception("Vehicle has move command, can not do loadunload.");
            }
            if (string.IsNullOrEmpty(cassetteId))
            {
                throw new Exception($"Unload CST ID is Empty.");
            }
            if (Vehicle.AseCarrierSlotL.CarrierId.Trim() == cassetteId)
            {
                return EnumSlotNumber.L;
            }
            else if (Vehicle.AseCarrierSlotR.CarrierId.Trim() == cassetteId)
            {
                return EnumSlotNumber.R;
            }
            else
            {
                throw new Exception($"No [{cassetteId}] to unload.");
            }
        }

        private void CheckOverrideAddress(AgvcTransferCommand transferCommand)
        {
            return;
        }

        private void CheckRobotPortAddress(string portAddressId, string portId)
        {
            CheckMoveEndAddress(portAddressId);
            MapAddress portAddress = Vehicle.Mapinfo.addressMap[portAddressId];
            if (!portAddress.IsTransferPort())
            {
                throw new Exception($"{portAddressId} can not unload.");
            }

            if (Vehicle.Mapinfo.portMap.ContainsKey(portId))
            {
                var port = Vehicle.Mapinfo.portMap[portId];
                if (port.ReferenceAddressId != portAddressId)
                {
                    throw new Exception($"{portAddressId} unmatch {portId}.");
                }
            }
        }

        private void CheckMoveEndAddress(string unloadAddressId)
        {
            if (!Vehicle.Mapinfo.addressMap.ContainsKey(unloadAddressId))
            {
                throw new Exception($"{unloadAddressId} is not in the map.");
            }
        }

        #endregion

        #endregion

        #region Thd Watch Charge Stage

        DateTime lastLowPowerArrivalChargeFlagOn = DateTime.Now;
        DateTime lastLowPowerArrivalChargeFlagOff = DateTime.Now;

        private void WatchChargeStage()
        {
            while (true)
            {
                try
                {
                    if (Vehicle.MainFlowConfig.IsSimulation)
                    {
                        SpinWait.SpinUntil(() => false, 100 * Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs);

                        continue;
                    }

                    if (!asePackage.psWrapper.IsConnected())
                    {
                        throw new Exception("AsePackage disconnect. Can not get Battery and Charging Status.");
                    }

                    UpdateBatteryAndChargingeStatus();

                    Vehicle.LowPower = IsBatteryLowerThanHighPower();//200824 dabid+stop
                    Vehicle.VehicleIdle = IsVehicleIdle();//200824 dabid+


                    if (Vehicle.AutoState == EnumAutoState.Auto)
                    {
                        if (IsVehicleIdle())
                        {
                            if (IsBatteryLowerThanHighPower())
                            {
                                LowPowerStartCharge(Vehicle.AseMoveStatus.LastAddress);
                            }
                        }
                        else
                        {
                            CheckVehicleLowPowerArrivalChargeFlag();
                        }
                    }

                    if (IsBatteryLowerThanLowPower() && !Vehicle.IsCharging) //200701 dabid+
                    {
                        throw new Exception($"[AutoState={Vehicle.AutoState}][IsVehicleIdle()={IsVehicleIdle()}][Percentage={Vehicle.AseBatteryStatus.Percentage}][LowThreshold={Vehicle.MainFlowConfig.LowPowerPercentage}]");
                    }
                }
                catch (Exception ex)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Low Power Charge Fail. {ex.Message}");

                    SetAlarmFromAgvm(2);
                }

                SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.WatchLowPowerSleepTimeMs);
            }
        }

        private void CheckVehicleLowPowerArrivalChargeFlag()
        {
            if (IsBatteryHigherThanHighPower())
            {
                Vehicle.LowPowerArrivalChargeFlag = false;
                lastLowPowerArrivalChargeFlagOff = DateTime.Now;
            }
            else if (IsBatteryLowerThanLowPower())
            {
                Vehicle.LowPowerArrivalChargeFlag = true;
                lastLowPowerArrivalChargeFlagOn = DateTime.Now;
            }
        }

        private bool IsBatteryLowerThanLowPower()
        {
            return Vehicle.AseBatteryStatus.Percentage <= Vehicle.MainFlowConfig.LowPowerPercentage;
        }

        private void UpdateBatteryAndChargingeStatus()
        {
            if (Vehicle.MainFlowConfig.IsSimulation) return;

            asePackage.SendChargeStatusRequest();
            asePackage.SendBatteryStatusRequest();
            SpinWait.SpinUntil(() => asePackage.IsBatteryRequestReply && asePackage.IsChargeStatusRequestReply, 15 * 1000);
            if (!asePackage.IsBatteryRequestReply)
            {
                throw new Exception("Battery status request no reply.");
            }

            if (!asePackage.IsChargeStatusRequestReply)
            {
                throw new Exception("Charge status request no reply.");
            }
        }

        private bool IsVehicleIdle()
        {
            if (Vehicle.TransferCommand.TransferStep == EnumTransferStep.Idle)
            {
                return true;
            }
            if (!Vehicle.mapTransferCommands.Any())
            {
                return true;
            }
            return false;
        }

        public void StartWatchChargeStage()
        {
            thdWatchChargeStage = new Thread(WatchChargeStage);
            thdWatchChargeStage.IsBackground = true;
            thdWatchChargeStage.Start();
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"StartWatchChargeStage");
        }

        private bool IsBatteryLowerThanHighPower()
        {
            return Vehicle.AseBatteryStatus.Percentage <= Vehicle.MainFlowConfig.HighPowerPercentage;
        }

        private bool IsBatteryHigherThanHighPower()
        {
            return Vehicle.AseBatteryStatus.Percentage > Vehicle.MainFlowConfig.HighPowerPercentage;
        }

        public void MainFormStartCharge()
        {
            StartCharge(Vehicle.AseMoveStatus.LastAddress);
        }

        private object _StartOrStopChargeLocker = new object();

        private void StartCharge(MapAddress endAddress, int chargeTimeSec = -1)
        {
            try
            {
                lock (_StartOrStopChargeLocker)
                {
                    if (endAddress.IsCharger())
                    {
                        if (IsBatteryHigherThanHighPower())
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Vehicle arrival {endAddress.Id},Charge Direction = {endAddress.ChargeDirection},Precentage = {Vehicle.AseBatteryStatus.Percentage} > {Vehicle.MainFlowConfig.HighPowerPercentage}(Threshold),  thus NOT send charge command.");
                            return;
                        }

                        agvcConnector.ChargHandshaking();
                        Vehicle.IsCharging = true;

                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $@"Start Charge, Vehicle arrival {endAddress.Id},Charge Direction = {endAddress.ChargeDirection},Precentage = {Vehicle.AseBatteryStatus.Percentage}.");

                        if (Vehicle.MainFlowConfig.IsSimulation)
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[充電.成功] Start Charge success.");
                            return;
                        }

                        asePackage.StartCharge(endAddress.ChargeDirection);

                        SpinWait.SpinUntil(() => asePackage.IsStartChargeReply, Vehicle.MainFlowConfig.StartChargeWaitingTimeoutMs);

                        if (asePackage.IsStartChargeReply)
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[充電.成功] Start Charge success.");
                        }
                        else
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[充電.失敗] Start Charge fail.[asePackage.IsStartChargeReply = {asePackage.IsStartChargeReply}]");
                            SetAlarmFromAgvm(000013);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            UpdateBatteryAndChargingeStatus();
        }

        private void LowPowerStartCharge(MapAddress lastAddress)
        {
            try
            {
                lock (_StartOrStopChargeLocker)
                {
                    Vehicle.ArrivalCharge = IsArrivalCharge;//200824 dabid for Watch Not AUTO Charge
                    Vehicle.LastAddress = lastAddress.Id;
                    Vehicle.IsCharger = lastAddress.IsCharger();//200824 dabid for Watch Not AUTO Charge

                    if (Vehicle.IsCharging)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[低電量閒置.自動充電] Vehicle is Charging.");
                        return;
                    }

                    if (!lastAddress.IsCharger())
                    {
                        throw new Exception($"At {lastAddress.Id} is not coupler.");
                    }

                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[低電量閒置.自動充電] Low power charging.");

                    Vehicle.IsCharging = true;

                    agvcConnector.ChargHandshaking();

                    if (Vehicle.MainFlowConfig.IsSimulation)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[低電量閒置.自動充電] Simulation Low power charging.");
                        return;
                    }

                    asePackage.StartCharge(lastAddress.ChargeDirection);

                    SpinWait.SpinUntil(() => asePackage.IsStartChargeReply, Vehicle.MainFlowConfig.StartChargeWaitingTimeoutMs);

                    if (asePackage.IsStartChargeReply)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "[低電量閒置.自動充電.成功] Low power charge success.");
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[低電量閒置.自動充電.失敗] Low power charge fail.[asePackage.IsStartChargeReply = {asePackage.IsStartChargeReply}]");
                        SetAlarmFromAgvm(000013);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Low Power Charge Fail, {ex.Message}");
            }

            UpdateBatteryAndChargingeStatus();
        }

        public void StopCharge()
        {
            try
            {
                lock (_StartOrStopChargeLocker)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $@"[斷充.開始] Try STOP charge.[IsCharging = {Vehicle.IsCharging}][AddressIsCharger={Vehicle.AseMoveStatus.LastAddress.IsCharger()}]");

                    TimerStopChargeInLastRobotStep.Stop();

                    if (Vehicle.AseMoveStatus.LastAddress.IsCharger() || Vehicle.IsCharging)
                    {
                        agvcConnector.ChargHandshaking();

                        if (Vehicle.MainFlowConfig.IsSimulation)
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[斷充.成功] Stop Charge success.");
                            Vehicle.IsCharging = false;
                            return;
                        }

                        asePackage.StopCharge();

                        SpinWait.SpinUntil(() => asePackage.IsStopChargeReply, Vehicle.MainFlowConfig.StopChargeWaitingTimeoutMs);

                        if (asePackage.IsStopChargeReply)
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[斷充.成功] Stop Charge success.");
                        }
                        else
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[斷充.逾時] Stop Charge Timeout.[asePackage.IsStopChargeReply = {asePackage.IsStopChargeReply}]");
                            SetAlarmFromAgvm(000014);
                        }
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $@"[斷充.成功] Vehicle is not charging.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            UpdateBatteryAndChargingeStatus();
        }

        private bool IsRobotStep()
        {
            switch (Vehicle.TransferCommand.TransferStep)
            {
                case EnumTransferStep.WaitLoadArrivalReply:
                case EnumTransferStep.Load:
                case EnumTransferStep.WaitLoadCompleteReply:
                case EnumTransferStep.WaitCstIdReadReply:
                case EnumTransferStep.UnloadArrival:
                case EnumTransferStep.WaitUnloadArrivalReply:
                case EnumTransferStep.Unload:
                case EnumTransferStep.WaitUnloadCompleteReply:
                case EnumTransferStep.LoadWaitEnd:
                case EnumTransferStep.UnloadWaitEnd:
                case EnumTransferStep.RobotFail:
                    return true;
                default:
                    return false; ;
            }
        }

        private void ArrivalStartCharge(MapAddress endAddress)
        {
            try
            {
                if (Vehicle.LowPowerArrivalChargeFlag)
                {
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[到站.充電] : ArrivalStartCharge.");

                    Task.Run(() =>
                    {
                        StartCharge(endAddress);
                    });
                }
                else
                {
                    string flagTimeStamp = "HH:mm:ss.fff";
                    LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[到站.不充電] : Arrival No Charge.[Flag Off][Battery={Vehicle.AseBatteryStatus.Percentage}][LowTh={Vehicle.MainFlowConfig.LowPowerPercentage}][LastOn={lastLowPowerArrivalChargeFlagOn.ToString(flagTimeStamp)}][LastOff={lastLowPowerArrivalChargeFlagOff.ToString(flagTimeStamp)}]");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool IsMoveEndRobotStep()
        {
            try
            {
                switch (Vehicle.TransferCommand.AgvcTransCommandType)
                {
                    case EnumAgvcTransCommandType.Load:
                    case EnumAgvcTransCommandType.Unload:
                    case EnumAgvcTransCommandType.LoadUnload:
                        return true;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            return false;
        }

        #endregion

        #region Thd Track Position

        private void TrackPosition()
        {
            while (true)
            {
                try
                {
                    //if (IsTrackPositionPause)
                    //{
                    //    SpinWait.SpinUntil(() => !IsTrackPositionPause, Vehicle.MainFlowConfig.TrackPositionSleepTimeMs);
                    //    continue;
                    //}

                    if (asePackage.ReceivePositionArgsQueue.Any())
                    {
                        asePackage.ReceivePositionArgsQueue.TryDequeue(out AsePositionArgs positionArgs);
                        DealAsePositionArgs(positionArgs);
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                SpinWait.SpinUntil(() => false, Vehicle.MainFlowConfig.TrackPositionSleepTimeMs);
            }
        }

        private void DealAsePositionArgs(AsePositionArgs positionArgs)
        {
            try
            {
                AseMovingGuide movingGuide = Vehicle.AseMovingGuide;
                AseMoveStatus moveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                moveStatus.LastMapPosition = positionArgs.MapPosition;
                CheckPositionUnchangeTimeout(positionArgs);

                if (movingGuide.GuideSectionIds.Any())
                {
                    if (positionArgs.Arrival == EnumAseArrival.EndArrival)
                    {
                        moveStatus.NearlyAddress = Vehicle.Mapinfo.addressMap[movingGuide.ToAddressId];
                        moveStatus.NearlySection = movingGuide.MovingSections.Last();
                        moveStatus.NearlySection.VehicleDistanceSinceHead = moveStatus.NearlySection.HeadAddress.MyDistance(moveStatus.NearlyAddress.Position);
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Update Position. [Section = {moveStatus.NearlySection.Id}][Address = {moveStatus.NearlyAddress.Id}].");
                    }
                    else
                    {
                        var nearlyDistance = 999999;
                        var reserveOkSections = agvcConnector.queReserveOkSections.ToList();
                        if (!reserveOkSections.Any())
                        {
                            foreach (string addressId in movingGuide.GuideAddressIds)
                            {
                                MapAddress mapAddress = Vehicle.Mapinfo.addressMap[addressId];
                                var dis = moveStatus.LastMapPosition.MyDistance(mapAddress.Position);

                                if (dis < nearlyDistance)
                                {
                                    nearlyDistance = dis;
                                    moveStatus.NearlyAddress = mapAddress;
                                }
                            }

                            foreach (string sectionId in movingGuide.GuideSectionIds)
                            {
                                MapSection mapSection = Vehicle.Mapinfo.sectionMap[sectionId];
                                if (mapSection.InSection(moveStatus.NearlyAddress.Id))
                                {
                                    moveStatus.NearlySection = mapSection;
                                }
                            }
                        }
                        else
                        {
                            List<MapAddress> reserveOkAddrs = new List<MapAddress>();
                            foreach (var mapSection in reserveOkSections)
                            {
                                reserveOkAddrs.AddRange(mapSection.InsideAddresses);
                            }

                            foreach (var mapAddress in reserveOkAddrs)
                            {
                                var dis = moveStatus.LastMapPosition.MyDistance(mapAddress.Position);

                                if (dis < nearlyDistance)
                                {
                                    nearlyDistance = dis;
                                    moveStatus.NearlyAddress = mapAddress;
                                }
                            }

                            foreach (var mapSection in reserveOkSections)
                            {
                                if (mapSection.InSection(moveStatus.NearlyAddress.Id))
                                {
                                    moveStatus.NearlySection = mapSection;
                                }
                            }

                        }

                        moveStatus.NearlySection.VehicleDistanceSinceHead = moveStatus.NearlyAddress.MyDistance(moveStatus.NearlySection.HeadAddress.Position);

                        if (moveStatus.NearlyAddress.Id != moveStatus.LastAddress.Id)
                        {
                            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Update Position. [Section = {moveStatus.NearlySection.Id}][Address = {moveStatus.NearlyAddress.Id}].");
                        }
                    }

                    moveStatus.LastAddress = moveStatus.NearlyAddress;
                    moveStatus.LastSection = moveStatus.NearlySection;
                    moveStatus.HeadDirection = positionArgs.HeadAngle;
                    moveStatus.MovingDirection = positionArgs.MovingDirection;
                    moveStatus.Speed = positionArgs.Speed;
                    Vehicle.AseMoveStatus = moveStatus;
                    agvcConnector.ReportSectionPass();

                    UpdateMovePassSections(moveStatus.LastSection.Id);

                    for (int i = 0; i < movingGuide.MovingSections.Count; i++)
                    {
                        if (movingGuide.MovingSections[i].Id == moveStatus.LastSection.Id)
                        {
                            Vehicle.AseMovingGuide.MovingSectionsIndex = i;
                        }
                    }
                }
                else
                {
                    moveStatus.NearlyAddress = Vehicle.Mapinfo.addressMap.Values.ToList().OrderBy(address => address.MyDistance(positionArgs.MapPosition)).ToArray().First();
                    moveStatus.NearlySection = Vehicle.Mapinfo.sectionMap.Values.ToList().FirstOrDefault(section => section.InSection(moveStatus.NearlyAddress));
                    moveStatus.NearlySection.VehicleDistanceSinceHead = moveStatus.NearlySection.HeadAddress.MyDistance(positionArgs.MapPosition);
                    moveStatus.LastMapPosition = positionArgs.MapPosition;
                    if (moveStatus.NearlyAddress.Id != moveStatus.LastAddress.Id)
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Update Position. [LastSection = {moveStatus.LastSection.Id}][LastAddress = {moveStatus.LastAddress.Id}] to [NearlySection = {moveStatus.NearlySection.Id}][NearlyAddress = {moveStatus.NearlyAddress.Id}]");
                    }
                    moveStatus.LastAddress = moveStatus.NearlyAddress;
                    moveStatus.LastSection = moveStatus.NearlySection;
                    moveStatus.HeadDirection = positionArgs.HeadAngle;
                    moveStatus.MovingDirection = positionArgs.MovingDirection;
                    moveStatus.Speed = positionArgs.Speed;
                    Vehicle.AseMoveStatus = moveStatus;
                    agvcConnector.ReportSectionPass();
                }

                switch (positionArgs.Arrival)
                {
                    case EnumAseArrival.Fail:
                        Vehicle.AseMovingGuide.MoveComplete = EnumMoveComplete.Fail;
                        Vehicle.AseMoveStatus.IsMoveEnd = true;
                        break;
                    case EnumAseArrival.Arrival:
                        break;
                    case EnumAseArrival.EndArrival:
                        Vehicle.AseMovingGuide.MoveComplete = EnumMoveComplete.Success;
                        Vehicle.AseMoveStatus.IsMoveEnd = true;
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

        private void CheckPositionUnchangeTimeout(AsePositionArgs positionArgs)
        {
            if (IsMoveStep())
            {
                if (LastIdlePosition.Position.MyDistance(positionArgs.MapPosition) <= Vehicle.MainFlowConfig.IdleReportRangeMm)
                {
                    if ((DateTime.Now - LastIdlePosition.TimeStamp).TotalMilliseconds >= Vehicle.MainFlowConfig.IdleReportIntervalMs)
                    {
                        UpdateLastIdlePositionAndTimeStamp(positionArgs);
                        SetAlarmFromAgvm(55);
                    }
                }
                else
                {
                    UpdateLastIdlePositionAndTimeStamp(positionArgs);
                }
            }
            else
            {
                LastIdlePosition.TimeStamp = DateTime.Now;
            }
        }

        private void UpdateLastIdlePositionAndTimeStamp(AsePositionArgs positionArgs)
        {
            LastIdlePosition lastIdlePosition = new LastIdlePosition();
            lastIdlePosition.Position = positionArgs.MapPosition;
            lastIdlePosition.TimeStamp = DateTime.Now;
            LastIdlePosition = lastIdlePosition;
        }

        public void StartTrackPosition()
        {
            thdTrackPosition = new Thread(TrackPosition);
            thdTrackPosition.IsBackground = true;
            thdTrackPosition.Start();
        }

        #endregion

        public void SetupAseMovingGuideMovingSections()
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[設定 路線] Setup MovingGuide.");

                AseMovingGuide aseMovingGuide = new AseMovingGuide(Vehicle.AseMovingGuide);
                aseMovingGuide.MovingSections.Clear();
                for (int i = 0; i < Vehicle.AseMovingGuide.GuideSectionIds.Count; i++)
                {
                    MapSection mapSection = new MapSection();
                    string sectionId = aseMovingGuide.GuideSectionIds[i].Trim();
                    string addressId = aseMovingGuide.GuideAddressIds[i + 1].Trim();
                    if (!Vehicle.Mapinfo.sectionMap.ContainsKey(sectionId))
                    {
                        throw new Exception($"Map info has no this section ID.[{sectionId}]");
                    }
                    else if (!Vehicle.Mapinfo.addressMap.ContainsKey(addressId))
                    {
                        throw new Exception($"Map info has no this address ID.[{addressId}]");
                    }

                    mapSection = Vehicle.Mapinfo.sectionMap[sectionId];
                    mapSection.CmdDirection = addressId == mapSection.TailAddress.Id ? EnumCommandDirection.Forward : EnumCommandDirection.Backward;
                    aseMovingGuide.MovingSections.Add(mapSection);
                }
                Vehicle.AseMovingGuide = aseMovingGuide;

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[設定 路線 成功] Setup MovingGuide OK.");
            }
            catch (Exception ex)
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[設定 路線 失敗] Setup MovingGuide Fail.");
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                Vehicle.AseMovingGuide.MovingSections = new List<MapSection>();
                SetAlarmFromAgvm(18);
                StopClearAndReset();
            }
        }

        private void UpdateMovePassSections(string id)
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        public void StopVehicle()
        {
            asePackage.MoveStop();
            asePackage.ClearRobotCommand();
            asePackage.StopCharge();

            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"MainFlow : Stop Vehicle, [MoveState={Vehicle.AseMoveStatus.AseMoveState}][IsCharging={Vehicle.IsCharging}]");
        }

        public void SetupVehicleSoc(int percentage)
        {
            asePackage.SetPercentage(percentage);
        }

        private void AgvcConnector_OnRenameCassetteIdEvent(object sender, AseCarrierSlotStatus e)
        {
            try
            {
                foreach (var transCmd in Vehicle.mapTransferCommands.Values.ToList())
                {
                    if (transCmd.SlotNumber == e.SlotNumber)
                    {
                        transCmd.CassetteId = e.CarrierId;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AgvcConnector_OnCmdPauseEvent(ushort iSeqNum, PauseType pauseType)
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.暫停] [{PauseEvent.Pause}][{pauseType}]");

                Vehicle.PauseFlags[pauseType] = true;
                PauseTransfer();
                asePackage.MovePause();
                agvcConnector.PauseReply(iSeqNum, (int)EnumAgvcReplyCode.Accept, PauseEvent.Pause);
                agvcConnector.StatusChangeReport();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AgvcConnector_OnCmdResumeEvent(ushort iSeqNum, PauseType pauseType)
        {
            try
            {
                if (pauseType == PauseType.All)
                {
                    Vehicle.ResetPauseFlags();
                    ResumeMiddler(iSeqNum, pauseType);
                }
                else
                {
                    Vehicle.PauseFlags[pauseType] = false;
                    agvcConnector.PauseReply(iSeqNum, (int)EnumAgvcReplyCode.Accept, PauseEvent.Continue);

                    if (!Vehicle.IsPause())
                    {
                        ResumeMiddler(iSeqNum, pauseType);
                    }
                    else
                    {
                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[尚有.其他.暫停旗標] [{PauseEvent.Continue}][{pauseType}]");
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ResumeMiddler(ushort iSeqNum, PauseType pauseType)
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.續行] [{PauseEvent.Continue}][{pauseType}]");

            agvcConnector.PauseReply(iSeqNum, (int)EnumAgvcReplyCode.Accept, PauseEvent.Continue);
            asePackage.MoveContinue();
            ResumeTransfer();
            agvcConnector.StatusChangeReport();
        }

        private void ResumeMiddler()
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.續行] By Op Resume.");

            asePackage.MoveContinue();
            ResumeTransfer();
            agvcConnector.StatusChangeReport();
        }

        public void AgvcConnector_OnCmdCancelAbortEvent(ushort iSeqNum, ID_37_TRANS_CANCEL_REQUEST receive)
        {
            PauseTransfer();
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[執行.命令.取消] Get [{receive.CancelAction}] Command.");

            string abortCmdId = receive.CmdID.Trim();
            bool IsAbortCurCommand = Vehicle.TransferCommand.CommandId == abortCmdId;
            var targetAbortCmd = Vehicle.mapTransferCommands[abortCmdId];

            if (IsAbortCurCommand)
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放棄.當前.命令] TransferComplete [{targetAbortCmd.CompleteStatus}].");

                CheckCanAbortCommand();

                agvcConnector.ClearAllReserve();
                //asePackage.MoveStop();
                Vehicle.AseMovingGuide = new AseMovingGuide();
                Vehicle.TransferCommand.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.CancelAction);
                //Vehicle.TransferCommand.TransferStep = EnumTransferStep.TransferComplete;
                Vehicle.TransferCommand.IsCanceled = true;
            }
            else
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"[放棄.背景.命令] TransferComplete [{targetAbortCmd.CompleteStatus}].");

                WaitingTransferCompleteEnd = true;

                targetAbortCmd.TransferStep = EnumTransferStep.Abort;
                targetAbortCmd.CompleteStatus = GetCompleteStatusFromCancelRequest(receive.CancelAction);

                Vehicle.mapTransferCommands.TryRemove(Vehicle.TransferCommand.CommandId, out AgvcTransferCommand transferCommand);
                agvcConnector.TransferComplete(transferCommand);
                asePackage.SetTransferCommandInfoRequest(transferCommand, EnumCommandInfoStep.End);

                if (Vehicle.mapTransferCommands.IsEmpty)
                {
                    Vehicle.ResetPauseFlags();

                    agvcConnector.NoCommand();

                    Vehicle.TransferCommand = new AgvcTransferCommand();
                }
                else
                {
                    agvcConnector.StatusChangeReport();
                }

                WaitingTransferCompleteEnd = false;
            }
            ResumeTransfer();
        }

        private void CheckCanAbortCommand()
        {
            switch (Vehicle.TransferCommand.TransferStep)
            {
                case EnumTransferStep.Idle:
                case EnumTransferStep.MoveToAddress:
                case EnumTransferStep.MoveToLoad:
                case EnumTransferStep.MoveToUnload:
                case EnumTransferStep.MoveToAvoid:
                case EnumTransferStep.MoveToAvoidWaitArrival:
                case EnumTransferStep.AvoidMoveComplete:
                case EnumTransferStep.MoveToAddressWaitArrival:
                case EnumTransferStep.MoveToAddressWaitEnd:
                case EnumTransferStep.WaitMoveArrivalVitualPortReply:
                case EnumTransferStep.MoveFail:
                case EnumTransferStep.WaitOverrideToContinue:
                case EnumTransferStep.GetNext:
                    throw new Exception($"[Vehicle.TransferCommand.TransferStep ={Vehicle.TransferCommand.TransferStep}]");
                case EnumTransferStep.LoadArrival:
                case EnumTransferStep.WaitLoadArrivalReply:
                case EnumTransferStep.Load:
                case EnumTransferStep.LoadWaitEnd:
                case EnumTransferStep.WaitLoadCompleteReply:
                case EnumTransferStep.WaitCstIdReadReply:
                case EnumTransferStep.UnloadArrival:
                case EnumTransferStep.WaitUnloadArrivalReply:
                case EnumTransferStep.Unload:
                case EnumTransferStep.UnloadWaitEnd:
                case EnumTransferStep.WaitUnloadCompleteReply:
                case EnumTransferStep.TransferComplete:
                case EnumTransferStep.RobotFail:
                case EnumTransferStep.Abort:
                    break;
                default:
                    break;
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

        public void AgvcDisconnected()
        {
            try
            {
                SetAlarmFromAgvm(56);
                asePackage.ReportAgvcDisConnect();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #region AsePackage

        private void AseBatteryControl_OnBatteryPercentageChangeEvent(object sender, double batteryPercentage)
        {
            try
            {
                Vehicle.BatteryLog.InitialSoc = (int)batteryPercentage;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ReadCarrierId()
        {
            try
            {
                asePackage.ReadCarrierId();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AsePackage_OnUpdateSlotStatusEvent(object sender, AseCarrierSlotStatus slotStatus)
        {
            try
            {
                if (slotStatus.ManualDeleteCST)
                {
                    slotStatus.CarrierId = "";
                    slotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                    switch (slotStatus.SlotNumber)
                    {
                        case EnumSlotNumber.L:
                            Vehicle.AseCarrierSlotL = slotStatus;
                            break;
                        case EnumSlotNumber.R:
                            Vehicle.AseCarrierSlotR = slotStatus;
                            break;
                    }

                    agvcConnector.Send_Cmd136_CstRemove(slotStatus.SlotNumber);
                }
                else
                {
                    switch (slotStatus.SlotNumber)
                    {
                        case EnumSlotNumber.L:
                            Vehicle.AseCarrierSlotL = slotStatus;
                            break;
                        case EnumSlotNumber.R:
                            Vehicle.AseCarrierSlotR = slotStatus;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public object _ModeChangeLocker = new object();

        public void AsePackage_OnModeChangeEvent(object sender, EnumAutoState autoState)
        {
            try
            {
                lock (_ModeChangeLocker)
                {
                    CheckLocalSwitchToManualAbortCommand(autoState);

                    StopClearAndReset();

                    if (Vehicle.AutoState != autoState)
                    {
                        switch (autoState)
                        {
                            case EnumAutoState.Auto:
                                asePackage.SetVehicleAutoScenario();
                                SpinWait.SpinUntil(() => false, 3000);  //500-->3000
                                CheckCanAuto();
                                UpdateSlotStatus();
                                Vehicle.AseMoveStatus.IsMoveEnd = false;
                                break;
                            case EnumAutoState.Manual:
                                asePackage.RequestVehicleToManual();
                                break;
                            case EnumAutoState.None:
                                break;
                            default:
                                break;
                        }

                        Vehicle.AutoState = autoState;
                        if (autoState == EnumAutoState.Auto) ResetAllAlarmsFromAgvm();

                        agvcConnector.StatusChangeReport();

                        LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Switch to {autoState}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (autoState == EnumAutoState.Auto)
                {
                    SetAlarmFromAgvm(31);
                    asePackage.RequestVehicleToManual();
                }
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void CheckLocalSwitchToManualAbortCommand(EnumAutoState autoState)
        {
            if (autoState == EnumAutoState.Manual)
            {
                if (Vehicle.mapTransferCommands.Any())
                {
                    SetAlarmFromAgvm(59);
                }
            }
        }

        private void CheckCanAuto()
        {

            if (Vehicle.AseMoveStatus.LastSection == null || string.IsNullOrEmpty(Vehicle.AseMoveStatus.LastSection.Id))
            {
                CanAutoMsg = "Section Lost";
                throw new Exception("CheckCanAuto fail. Section Lost.");
            }
            else if (Vehicle.AseMoveStatus.LastAddress == null || string.IsNullOrEmpty(Vehicle.AseMoveStatus.LastAddress.Id))
            {
                CanAutoMsg = "Address Lost";
                throw new Exception("CheckCanAuto fail. Address Lost.");
            }
            else if (Vehicle.AseMoveStatus.AseMoveState != EnumAseMoveState.Idle && Vehicle.AseMoveStatus.AseMoveState != EnumAseMoveState.Block)
            {
                CanAutoMsg = $"Move State = {Vehicle.AseMoveStatus.AseMoveState}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }
            else if (Vehicle.AseMoveStatus.LastAddress.MyDistance(Vehicle.AseMoveStatus.LastMapPosition) >= Vehicle.MainFlowConfig.InitialPositionRangeMm)
            {
                SetAlarmFromAgvm(54);
                CanAutoMsg = $"Initial Positon Too Far.";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }

            var aseRobotStatus = Vehicle.AseRobotStatus;
            if (aseRobotStatus.RobotState != EnumAseRobotState.Idle)
            {
                CanAutoMsg = $"Robot State = {aseRobotStatus.RobotState}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }
            else if (!aseRobotStatus.IsHome)
            {
                CanAutoMsg = $"Robot IsHome = {aseRobotStatus.IsHome}";
                throw new Exception($"CheckCanAuto fail. {CanAutoMsg}");
            }

            CanAutoMsg = "OK";
        }

        private void UpdateSlotStatus()
        {
            try
            {
                AseCarrierSlotStatus leftSlotStatus = new AseCarrierSlotStatus(Vehicle.AseCarrierSlotL);
                AseCarrierSlotStatus rightSlotStatus = new AseCarrierSlotStatus(Vehicle.AseCarrierSlotR);

                switch (leftSlotStatus.CarrierSlotStatus)
                {
                    case EnumAseCarrierSlotStatus.Empty:
                        {
                            leftSlotStatus.CarrierId = "";
                            leftSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                            Vehicle.AseCarrierSlotL = leftSlotStatus;
                            Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                        }
                        break;
                    case EnumAseCarrierSlotStatus.Loading:
                        {
                            if (string.IsNullOrEmpty(leftSlotStatus.CarrierId.Trim()))
                            {
                                leftSlotStatus.CarrierId = "ReadFail";
                                leftSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                                Vehicle.AseCarrierSlotL = leftSlotStatus;
                                Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                            }
                            else
                            {
                                Vehicle.LeftReadResult = BCRReadResult.BcrNormal;
                            }
                        }
                        break;
                    case EnumAseCarrierSlotStatus.PositionError:
                        {
                            SetAlarmFromAgvm(51);
                        }
                        return;
                    case EnumAseCarrierSlotStatus.ReadFail:
                        {
                            leftSlotStatus.CarrierId = "ReadFail";
                            leftSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                            Vehicle.AseCarrierSlotL = leftSlotStatus;
                            Vehicle.LeftReadResult = BCRReadResult.BcrReadFail;
                        }
                        break;
                    default:
                        break;
                }

                switch (rightSlotStatus.CarrierSlotStatus)
                {
                    case EnumAseCarrierSlotStatus.Empty:
                        {
                            rightSlotStatus.CarrierId = "";
                            rightSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.Empty;
                            Vehicle.AseCarrierSlotR = rightSlotStatus;
                            Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                        }
                        break;
                    case EnumAseCarrierSlotStatus.Loading:
                        {
                            if (string.IsNullOrEmpty(rightSlotStatus.CarrierId.Trim()))
                            {
                                rightSlotStatus.CarrierId = "ReadFail";
                                rightSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                                Vehicle.AseCarrierSlotR = rightSlotStatus;
                                Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                            }
                            else
                            {
                                Vehicle.RightReadResult = BCRReadResult.BcrNormal;
                            }
                        }
                        break;
                    case EnumAseCarrierSlotStatus.PositionError:
                        {
                            SetAlarmFromAgvm(51);
                        }
                        return;
                    case EnumAseCarrierSlotStatus.ReadFail:
                        {
                            rightSlotStatus.CarrierId = "ReadFail";
                            rightSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                            Vehicle.AseCarrierSlotR = rightSlotStatus;
                            Vehicle.RightReadResult = BCRReadResult.BcrReadFail;
                        }
                        break;
                    default:
                        break;
                }

                agvcConnector.CSTStatusReport(); //200625 dabid#
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AsePackage_ImportantPspLog(object sender, string msg)
        {
            try
            {
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
            }
            catch (System.Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AsePackage_OnAlarmCodeResetEvent(object sender, EventArgs e)
        {
            ResetAllAlarmsFromAgvl();
        }

        private void AsePackage_OnAlarmCodeSetEvent1(object sender, int id)
        {
            SetAlarmFromAgvl(id);
        }

        private void AsePackage_OnStatusChangeReportEvent(object sender, string e)
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, e);
            agvcConnector.StatusChangeReport();
        }

        private void AsePackage_OnOpPauseOrResumeEvent(object sender, bool e)
        {
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"AsePackage_OnOpPauseOrResumeEvent [{e}].");

            if (e)
            {
                Vehicle.OpPauseStatus = VhStopSingle.On;
                agvcConnector.StatusChangeReport();
            }
            else
            {
                Vehicle.OpPauseStatus = VhStopSingle.Off;
                Vehicle.ResetPauseFlags();
                ResumeMiddler();
            }

        }

        #endregion

        #region Set / Reset Alarm

        public void SetAlarmFromAgvm(int errorCode)
        {
            if (!alarmHandler.dicHappeningAlarms.ContainsKey(errorCode))
            {
                alarmHandler.SetAlarm(errorCode);
                asePackage.SetAlarmCode(errorCode, true);
                var IsAlarm = alarmHandler.IsAlarm(errorCode);

                agvcConnector.SetlAlarmToAgvc(errorCode, IsAlarm);
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, alarmHandler.GetAlarmText(errorCode));
            }
        }

        public void SetAlarmFromAgvl(int errorCode)
        {
            if (!alarmHandler.dicHappeningAlarms.ContainsKey(errorCode))
            {
                alarmHandler.SetAlarm(errorCode);
                var IsAlarm = alarmHandler.IsAlarm(errorCode);
                agvcConnector.SetlAlarmToAgvc(errorCode, IsAlarm);
                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, alarmHandler.GetAlarmText(errorCode));
            }
        }

        public void ResetAllAlarmsFromAgvm()
        {
            alarmHandler.ResetAllAlarms();
            asePackage.ResetAllAlarmCode();
            agvcConnector.ResetAllAlarmsToAgvc();
        }

        public void ResetAllAlarmsFromAgvc()
        {
            alarmHandler.ResetAllAlarms();
            asePackage.ResetAllAlarmCode();
        }

        public void ResetAllAlarmsFromAgvl()
        {
            alarmHandler.ResetAllAlarms();
            agvcConnector.ResetAllAlarmsToAgvc();
        }

        #endregion

        #region Log

        public void AppendDebugLog(string msg)
        {
            if (!Vehicle.IsIgnoreAppendDebug)
            {
                try
                {
                    //lock (DebugLogMsg)
                    //{
                    //    for (int i = 0; i < 100; i++)
                    //    {
                    //        DebugLogMsg = string.Concat(DateTime.Now.ToString("HH:mm:ss.fff"), "  ", msg, "\r\n", DebugLogMsg);

                    //        if (DebugLogMsg.Length > 65535)
                    //        {
                    //            DebugLogMsg = DebugLogMsg.Substring(65535);
                    //        }
                    //    }
                    //}


                    lock (SbDebugMsg)
                    {
                        SbDebugMsg.Insert(0, string.Concat(DateTime.Now.ToString("HH:mm:ss.fff"), "  ", msg, Environment.NewLine));
                        if (SbDebugMsg.Length > 20000)
                        {
                            SbDebugMsg.Remove(10000, 10000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }
            }
        }

        private NLog.Logger _transferLogger = NLog.LogManager.GetLogger("Transfer");


        private void LogException(string classMethodName, string exMsg)
        {
            try
            {
                _transferLogger.Error($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{classMethodName}][{exMsg}]");

            }
            catch (Exception) { }
        }

        public void LogDebug(string classMethodName, string msg)
        {
            try
            {
                _transferLogger.Debug($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{classMethodName}][{msg}]");

                AppendDebugLog(msg);
            }
            catch (Exception) { }
        }

        #endregion
    }

    public class LastIdlePosition
    {
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public MapPosition Position { get; set; } = new MapPosition();
    }
}