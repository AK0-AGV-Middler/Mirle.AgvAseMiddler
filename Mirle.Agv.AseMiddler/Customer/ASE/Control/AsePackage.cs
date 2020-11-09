using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using Mirle.Tools;
using PSDriver.PSDriver;
using SimpleWifi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class AsePackage
    {
        public PSWrapperXClass psWrapper;
        public MirleLogger mirleLogger = MirleLogger.Instance;
        public Dictionary<string, PSMessageXClass> psMessageMap = new Dictionary<string, PSMessageXClass>();
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;
        public string LocalLogMsg { get; set; } = "";
        public string MoveStopResult { get; set; } = "";
        public RobotCommand RobotCommand { get; set; }
        public DateTime LastDisconnectedTimeStamp { get; set; } = DateTime.Now;
        public long LastUpdateLeftSlotStatusSystemByte { get; set; } = 0;
        public long LastUpdateRightSlotStatusSystemByte { get; set; } = 0;
        public long LastUpdatePositionSystemByte { get; set; } = 0;
        public bool ResetSystemByte { get; set; } = true;

        private Thread thdWatchWifiSignalStrength;
        public bool IsWatchWifiSignalStrengthPause { get; set; } = false;

        private Thread thdSchedule;
        public bool IsSchedulePause { get; set; } = false;

        private Thread thdWatchPosition;
        public bool IsWatchPositionPause { get; private set; } = false;

        private Thread thdWatchBatteryState;
        public bool IsWatchBatteryStatusPause { get; private set; } = false;

        public ConcurrentQueue<PSMessageXClass> PrimarySendQueue { get; set; } = new ConcurrentQueue<PSMessageXClass>();
        public ConcurrentQueue<PSTransactionXClass> SecondarySendQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PSTransactionXClass> PrimaryReceiveQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<PSTransactionXClass> SecondaryReceiveQueue { get; set; } = new ConcurrentQueue<PSTransactionXClass>();
        public ConcurrentQueue<AsePositionArgs> ReceivePositionArgsQueue { get; set; } = new ConcurrentQueue<AsePositionArgs>();
        public ConcurrentQueue<PSMessageXClass> PrimaryTimeoutQueue { get; set; } = new ConcurrentQueue<PSMessageXClass>();

        public event EventHandler<string> ImportantPspLog;
        public event EventHandler<string> OnStatusChangeReportEvent;
        public event EventHandler<EnumAutoState> OnModeChangeEvent;
        public event EventHandler<AseCarrierSlotStatus> OnUpdateSlotStatusEvent;
        public event EventHandler<int> OnAlarmCodeSetEvent;
        public event EventHandler<int> OnAlarmCodeResetEvent;
        public event EventHandler OnAlarmCodeAllResetEvent;
        public event EventHandler<double> OnBatteryPercentageChangeEvent;
        public event EventHandler<bool> OnOpPauseOrResumeEvent;
        public event EventHandler<EnumRobotEndType> OnRobotEndEvent;

        public AsePackage()
        {
            InitialWrapper();
            InitialThreads();
        }

        private void InitialThreads()
        {
            thdWatchWifiSignalStrength = new Thread(WatchWifiSignalStrength);
            thdWatchWifiSignalStrength.IsBackground = true;
            thdWatchWifiSignalStrength.Start();

            thdSchedule = new Thread(Schedule);
            thdSchedule.IsBackground = true;
            thdSchedule.Start();

            thdWatchPosition = new Thread(WatchPosition);
            thdWatchPosition.IsBackground = true;
            thdWatchPosition.Start();

            thdWatchBatteryState = new Thread(WatchBatteryState);
            thdWatchBatteryState.IsBackground = true;
            thdWatchBatteryState.Start();
        }

        private void InitialWrapper()
        {
            try
            {
                LoadAutoReply();
                GetConfigWrapper();
                BindPsWrapperEvent();

                if (!Vehicle.MainFlowConfig.IsSimulation || Vehicle.PspConnectionConfig.IsServer)
                {
                    psWrapper.Open();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void BindPsWrapperEvent()
        {
            try
            {
                psWrapper.OnConnectionStateChange += PsWrapper_OnConnectionStateChange;
                psWrapper.OnPrimarySent += PsWrapper_OnPrimarySent;
                psWrapper.OnPrimaryReceived += PsWrapper_OnPrimaryReceived;
                psWrapper.OnSecondarySent += PsWrapper_OnSecondarySent;
                psWrapper.OnSecondaryReceived += PsWrapper_OnSecondaryReceived;
                psWrapper.OnTransactionError += PsWrapper_OnTransactionError;
                psWrapper.OnDebugLog += PsWrapper_OnDebugLog;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #region Threads

        private void WatchWifiSignalStrength()
        {
            while (true)
            {
                try
                {
                    //if (IsWatchWifiSignalStrengthPause)
                    //{
                    //    SpinWait.SpinUntil(() => false, Vehicle.AsePackageConfig.WatchWifiSignalIntervalMs);

                    //    continue;
                    //}

                    CalculateWifiSignalStrength();
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                SpinWait.SpinUntil(()=>false,Vehicle.AsePackageConfig.WatchWifiSignalIntervalMs);
            }
        }

        private void CalculateWifiSignalStrength()
        {
            uint result = 0;
            try
            {
                if (Vehicle.IsAgvcConnect)
                {
                    List<AccessPoint> accessPoints = new Wifi().GetAccessPoints().ToList();
                    if (accessPoints.Any())
                    {
                        foreach (var item in accessPoints)
                        {
                            if (item.IsConnected)
                            {
                                result = Math.Max(item.SignalStrength, 0);
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
            finally
            {
                Vehicle.WifiSignalStrength = result;
                if (Vehicle.IsLocalConnect)
                {
                    SendWifiSignalStrength(Vehicle.WifiSignalStrength);
                }
            }
        }

        private void SendWifiSignalStrength(uint signalStrength)
        {
            try
            {
                PrimarySendEnqueue("P19", signalStrength.ToString().PadLeft(3, '0'));
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Schedule()
        {
            while (true)
            {
                try
                {
                    //if (IsSchedulePause)
                    //{
                    //    SpinWait.SpinUntil(() => !IsSchedulePause, Vehicle.AsePackageConfig.ScheduleIntervalMs);

                    //    continue;
                    //}

                    if (Vehicle.IsLocalConnect)
                    {
                        while (PrimaryTimeoutQueue.Any())
                        {
                            PrimaryTimeoutQueue.TryDequeue(out PSMessageXClass psMessage);
                            PrimarySendQueue.Enqueue(psMessage);
                        }

                        if (PrimarySendQueue.Any())
                        {
                            PrimarySendQueue.TryDequeue(out PSMessageXClass psMessageObj);
                            PrimarySend(psMessageObj);
                        }

                        if (SecondarySendQueue.Any())
                        {
                            SecondarySendQueue.TryDequeue(out PSTransactionXClass psTransaction);
                            SecondarySend(psTransaction);
                        }

                        if (PrimaryReceiveQueue.Any())
                        {
                            PrimaryReceiveQueue.TryDequeue(out PSTransactionXClass psTransaction);
                            AutoReplyFromPsMessageMap(psTransaction);
                            DealPrimaryReceived(psTransaction);

                        }

                        if (SecondaryReceiveQueue.Any())
                        {
                            SecondaryReceiveQueue.TryDequeue(out PSTransactionXClass psTransaction);
                            DealSecondaryReceived(psTransaction);
                        }

                        SpinWait.SpinUntil(()=>false,10);
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                SpinWait.SpinUntil(()=>false,Vehicle.AsePackageConfig.ScheduleIntervalMs);

            }
        }

        private void WatchPosition()
        {
            while (true)
            {
                try
                {
                    //if (IsWatchPositionPause)
                    //{
                    //    SpinWait.SpinUntil(() => !IsWatchPositionPause, Vehicle.AseMoveConfig.WatchPositionInterval);
                    //    continue;
                    //}

                    if (Vehicle.IsLocalConnect)
                    {
                        if (!ReceivePositionArgsQueue.Any())
                        {
                            SendPositionReportRequest();
                        }
                        else
                        {
                            SpinWait.SpinUntil(()=>false,500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                SpinWait.SpinUntil(()=>false,Vehicle.AseMoveConfig.WatchPositionInterval);
            }
        }

        public void SendPositionReportRequest()
        {
            try
            {
                PrimarySendEnqueue("P33", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void WatchBatteryState()
        {
            while (true)
            {
                try
                {
                    if (Vehicle.IsLocalConnect)
                    {
                        SendBatteryStatusRequest();
                    }
                }
                catch (Exception ex)
                {
                    LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                }

                if (Vehicle.IsCharging)
                {
                    SpinWait.SpinUntil(()=>false,Vehicle.AseBatteryConfig.WatchBatteryStateIntervalInCharging);
                }
                else
                {
                    SpinWait.SpinUntil(()=>false,Vehicle.AseBatteryConfig.WatchBatteryStateInterval);
                }
            }
        }

        public void SendBatteryStatusRequest()
        {
            try
            {
                PrimarySendEnqueue("P35", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region PrimarySend

        private void PsWrapper_OnPrimarySent(ref PSTransactionXClass transaction)
        {
            try
            {
                //string msg = $"PrimarySent : [{transaction.PSPrimaryMessage.ToString()}]";
                //AllPspLog?.Invoke(this, msg);
                //LogPsWrapper(msg);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PrimarySendEnqueue(string index, string message)
        {
            try
            {
                PSMessageXClass psMessage = new PSMessageXClass();
                psMessage.Type = index.Substring(0, 1);
                psMessage.Number = index.Substring(1, 2);
                psMessage.PSMessage = message;

                PrimarySendQueue.Enqueue(psMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PrimarySend(PSMessageXClass psMessageObj)
        {
            try
            {
                PSTransactionXClass psTransaction = new PSTransactionXClass();
                psTransaction.PSPrimaryMessage = psMessageObj;

                psWrapper.PrimarySent(ref psTransaction);

                LogPsWrapper($"PSEND : [{psTransaction.PSPrimaryMessage.ToString()}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AseControl_OnPrimarySendEvent(object sender, PSTransactionXClass psTransaction)
        {
            try
            {
                var primaryMessage = psTransaction.PSPrimaryMessage;
                PrimarySendEnqueue(primaryMessage.Type + primaryMessage.Number, primaryMessage.PSMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }

        }

        public void SetLocalDateTime()
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                PrimarySendEnqueue("P15", timeStamp);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AllAgvlStatusReportRequest()
        {
            try
            {
                PrimarySendEnqueue("P31", "0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SetTransferCommandInfoRequest(AgvcTransferCommand transferCommand, EnumCommandInfoStep commandInfoStep)
        {
            try
            {
                string commandStep = ((int)commandInfoStep).ToString();
                string commandId = transferCommand.CommandId.PadLeft(20, '0');
                string fromPortNum = transferCommand.LoadAddressId.PadLeft(5, '0').Substring(0, 5);
                string toPortNum = transferCommand.UnloadAddressId.PadLeft(5, '0').Substring(0, 5);
                string vehicleSlot = transferCommand.SlotNumber.ToString();
                string lotId = transferCommand.LotId.PadLeft(40, '0').Substring(0, 40);
                string cassetteId = transferCommand.CassetteId;
                string transferCommandInfo = string.Concat(commandStep, commandId, fromPortNum, toPortNum, vehicleSlot, lotId, cassetteId); ;
                PrimarySendEnqueue("P37", transferCommandInfo);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void DoRobotCommand(string robotCommandInfo)
        {
            try
            {
                PrimarySendEnqueue("P45", robotCommandInfo);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SetAlarmCode(int alarmCode, bool isSet)
        {
            try
            {
                string isSetString = isSet ? "1" : "0";
                string alarmCodeString = alarmCode.ToString(new string('0', 6));
                string psMessage = string.Concat(isSetString, alarmCodeString);
                PrimarySendEnqueue("P61", psMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ResetAllAlarmCode()
        {
            try
            {
                PrimarySendEnqueue("P63", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void BuzzerOff()
        {
            try
            {
                PrimarySendEnqueue("P65", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ClearRobotCommand()
        {
            try
            {
                PrimarySendEnqueue("P49", "");
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
                PrimarySendEnqueue("P31", "3");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void DoRobotCommand(RobotCommand robotCommand)
        {
            try
            {
                RobotCommand = robotCommand;
                string robotCommandString = GetRobotCommandString();
                PrimarySendEnqueue("P45", robotCommandString);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void RefreshRobotState()
        {
            try
            {
                PrimarySendEnqueue("P31", "2");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void RefreshCarrierSlotState()
        {
            try
            {
                PrimarySendEnqueue("P31", "3");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string GetRobotCommandString()
        {
            try
            {
                string pioDirection = ((int)RobotCommand.PioDirection).ToString();
                string fromPort = "";
                string toPort = "";
                switch (RobotCommand.GetTransferStepType())
                {
                    case EnumTransferStepType.Load:
                        fromPort = RobotCommand.PortAddressId.PadLeft(5, '0');
                        toPort = RobotCommand.SlotNumber.ToString().PadLeft(5, '0');
                        break;
                    case EnumTransferStepType.Unload:
                        fromPort = RobotCommand.SlotNumber.ToString().PadLeft(5, '0');
                        toPort = RobotCommand.PortAddressId.PadLeft(5, '0');
                        break;
                    case EnumTransferStepType.Move:
                    case EnumTransferStepType.MoveToCharger:
                    case EnumTransferStepType.Empty:
                    default:
                        throw new Exception($"Robot command type error.[{RobotCommand.GetTransferStepType()}]");
                }

                string gateType = RobotCommand.GateType.Substring(0, 1);
                string portNumber = RobotCommand.PortNumber.Substring(0, 1);

                if (RobotCommand.IsBindUnload)
                {
                    string fromPort2 = RobotCommand.SlotNumber == EnumSlotNumber.L ? "0000R" : "0000L";
                    string toPort2 = fromPort;

                    return string.Concat(pioDirection, fromPort, toPort, gateType, portNumber, fromPort2, toPort2, portNumber).PadRight(24, '0');
                }
                else
                {
                    return string.Concat(pioDirection, fromPort, toPort, gateType, portNumber).PadRight(24, '0');
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        public void ChargeStatusRequest()
        {
            try
            {
                PrimarySendEnqueue("P31", "4");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PartMove(MapPosition mapPosition, int headAngle, int speed, EnumAseMoveCommandIsEnd isEnd, EnumIsExecute keepOrGo, EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            try
            {
                string isEndString = ((int)isEnd).ToString();
                string positionX = GetPositionString(mapPosition.X);
                string positionY = GetPositionString(mapPosition.Y);
                string thetaString = GetNumberToString(headAngle, 3);
                string speedString = GetNumberToString(speed, 4);
                string openSlotString = ((int)openSlot).ToString();
                string keepOrGoString = keepOrGo.ToString().Substring(0, 1).ToUpper();
                string message = string.Concat(isEndString, positionX, positionY, thetaString, speedString, openSlotString, keepOrGoString);

                PrimarySendEnqueue("P41", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void PartMove(EnumAseMoveCommandIsEnd enumAseMoveCommandIsEnd, EnumSlotSelect openSlot = EnumSlotSelect.None)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Model.Vehicle.Instance.AseMoveStatus);
                string beginString = ((int)enumAseMoveCommandIsEnd).ToString();
                string positionX = GetPositionString(aseMoveStatus.LastAddress.Position.X);
                string positionY = GetPositionString(aseMoveStatus.LastAddress.Position.Y);
                string thetaString = GetNumberToString((int)aseMoveStatus.LastAddress.VehicleHeadAngle, 3);
                string speedString = GetNumberToString((int)aseMoveStatus.LastSection.Speed, 4);
                string openSlotString = ((int)openSlot).ToString();
                string keepOrGoString = "G";
                string message = string.Concat(beginString, positionX, positionY, thetaString, speedString, openSlotString, keepOrGoString);

                PrimarySendEnqueue("P41", message);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string GetPositionString(double x)
        {
            try
            {
                string result = x >= 0 ? "P" : "N";
                string number = GetNumberToString((int)(Math.Abs(x)), 8);
                result = string.Concat(result, number);
                return result;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        private string GetNumberToString(int value, ushort digit)
        {
            try
            {
                string valueFormat = new string('0', digit);

                return value.ToString(valueFormat);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return "";
            }
        }

        #endregion

        #region PrimaryReceived

        //private void CheckPrimaryReceiveQueue()
        //{
        //    lock (PrimaryReceiveQueue)
        //    {
        //        primaryReceiveTransactions = PrimaryReceiveQueue.ToList();
        //        PrimaryReceiveQueue = new ConcurrentQueue<PSTransactionXClass>();
        //    }

        //    if (primaryReceiveTransactions.Any())
        //    {
        //        foreach (PSTransactionXClass psTransaction in primaryReceiveTransactions)
        //        {
        //            AutoReplyFromPsMessageMap(psTransaction);
        //        }
        //    }
        //}

        private void PsWrapper_OnPrimaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                LogPsWrapper($"PRECV : [{transaction.PSPrimaryMessage.ToString()}]");
                PrimaryReceiveQueue.Enqueue(transaction);

                //AutoReplyFromPsMessageMap(transaction);
                //DealPrimaryReceived(transaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void DealPrimaryReceived(PSTransactionXClass transaction)
        {
            try
            {
                if (transaction.PSPrimaryMessage == null)
                {
                    throw new Exception("Primary message is empty.");
                }

                if (transaction.PSPrimaryMessage.Type.ToUpper() != "P")
                {
                    throw new Exception("Primary message type is not P.");
                }

                if (ResetSystemByte)
                {
                    ResetSystemByte = false;
                    LastUpdateLeftSlotStatusSystemByte = transaction.PSPrimaryMessage.SystemBytes;
                    LastUpdateRightSlotStatusSystemByte = transaction.PSPrimaryMessage.SystemBytes;
                }

                switch (transaction.PSPrimaryMessage.Number)
                {
                    case "11":
                        SetVehicleAuto();
                        break;
                    case "13":
                        SetVehicleManual();
                        break;
                    case "21":
                        UpdateMoveStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "23":
                        UpdateRobotStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "25":
                        UpdateCarrierSlotStatus(transaction.PSPrimaryMessage.PSMessage, transaction.PSPrimaryMessage.SystemBytes);
                        break;
                    case "29":
                        UpdateChargeStatus(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "43":
                        ReceiveMoveAppendArrivalReport(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "53":
                        ReceiveRobotCommandFinishedReport(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "61":
                        AlarmReport(transaction.PSPrimaryMessage.PSMessage);
                        break;
                    case "63":
                        AllAlarmReset();
                        break;
                    case "97":
                        ShowTestMsg();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void ReceiveRobotCommandFinishedReport(string psMessage)
        {
            try
            {
                string finishedMsg = psMessage.Trim();
                if (Enum.TryParse(finishedMsg, out EnumRobotEndType robotEndType))
                {
                    OnRobotEndEvent?.Invoke(this, robotEndType);
                }
                else
                {
                    throw new Exception($"Can not parse robot command finished report.[{finishedMsg}]");
                }
                //switch (finishedMsg)
                //{
                //    case "Finished":
                //        OnRobotCommandFinishEvent?.Invoke(this, RobotCommand);
                //        break;
                //    case "InterlockError":
                //        OnRobotInterlockErrorEvent?.Invoke(this, RobotCommand);
                //        break;
                //    case "RobotError":
                //        OnRobotCommandErrorEvent?.Invoke(this, RobotCommand);
                //        break;
                //    default:
                //        throw new Exception($"Can not parse robot command finished report.[{finishedMsg}]");
                //}
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
                OnRobotEndEvent?.Invoke(this, EnumRobotEndType.RobotError);
            }
        }

        private void AutoReplyFromPsMessageMap(PSTransactionXClass psTransaction)
        {
            try
            {
                if (!IsPrimaryEmpty(psTransaction))
                {
                    if (IsPrimaryInMessageMap(psTransaction))
                    {
                        ReplyFromMsgMap(psTransaction);
                    }

                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void ReplyFromMsgMap(PSTransactionXClass psTransaction)
        {
            try
            {
                PSMessageXClass primaryMessage = psTransaction.PSPrimaryMessage;
                int primaryNumber = int.Parse(primaryMessage.Number);
                string secondaryTypeNumber = "S" + (primaryNumber + 1).ToString("00");

                if (psMessageMap.ContainsKey(secondaryTypeNumber))
                {
                    psTransaction.PSSecondaryMessage = psMessageMap[secondaryTypeNumber];
                    //psWrapper.SecondarySent(ref psTransaction);
                    SecondarySendQueue.Enqueue(psTransaction);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private bool IsPrimaryInMessageMap(PSTransactionXClass psTransaction)
        {
            try
            {
                if (psMessageMap.Count <= 0)
                {
                    throw new Exception("PsMessageMap is empty");
                }

                PSMessageXClass primaryMessage = psTransaction.PSPrimaryMessage;
                if (primaryMessage.Type.ToUpper() != "P")
                {
                    throw new Exception($"PrimaryReceive transaction.primaryMessage.type is not P.[{primaryMessage.Type}]");
                }

                return psMessageMap.ContainsKey(primaryMessage.Type + primaryMessage.Number);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        private bool IsPrimaryEmpty(PSTransactionXClass psTransaction)
        {
            return psTransaction.PSPrimaryMessage == null;
        }

        private void ShowTestMsg()
        {
            ImportantPspLog?.Invoke(this, "A test msg from AGVL.");
        }

        private void ReceiveMoveAppendArrivalReport(string psMessage)
        {
            try
            {
                AsePositionArgs positionArgs = new AsePositionArgs();

                positionArgs.Arrival = GetArrivalStatus(psMessage.Substring(0, 1));

                double x = GetPositionFromPsMessage(psMessage.Substring(1, 9));
                double y = GetPositionFromPsMessage(psMessage.Substring(10, 9));
                positionArgs.MapPosition = new MapPosition(x, y);

                if (int.TryParse(psMessage.Substring(19, 3), out int headAngle))
                {
                    positionArgs.HeadAngle = headAngle;
                }

                if (int.TryParse(psMessage.Substring(22, 3), out int movingDirection))
                {
                    positionArgs.MovingDirection = movingDirection;
                }

                if (int.TryParse(psMessage.Substring(25, 4), out int speed))
                {
                    positionArgs.Speed = speed;
                }

                if (positionArgs.Arrival != EnumAseArrival.Arrival)
                {
                    ImportantPspLog?.Invoke(this, $"ReceiveMoveAppendArrivalReport. [{psMessage.Substring(0, 1)}][{positionArgs.Arrival}][({(int)x},{(int)y})]");
                }

                ReceivePositionArgsQueue.Enqueue(positionArgs);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private EnumAseArrival GetArrivalStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumAseArrival.Fail;
                case "1":
                    return EnumAseArrival.Arrival;
                case "2":
                    return EnumAseArrival.EndArrival;
                default:
                    throw new Exception($"Can not parse arrival report.[{v}]");
            }
        }

        private int GetIntTryParse(string v)
        {
            try
            {
                return int.Parse(v);
            }
            catch (Exception)
            {
                throw new Exception($"Can not parse int.[{v}]");
            }
        }

        private double GetPositionFromPsMessage(string v)
        {
            try
            {
                string isPositive = v.Substring(0, 1);
                double value = double.Parse(v.Substring(1, 8));
                return IsValuePositive(isPositive) ? value : -value;
            }
            catch (Exception)
            {
                throw new Exception($"Can not parse position report.[{v}]");
            }
        }

        private void AllAlarmReset()
        {
            try
            {
                OnStatusChangeReportEvent?.Invoke(this, $"AllAlarmReset:");

                OnAlarmCodeAllResetEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void AlarmReport(string psMessage)
        {
            try
            {
                bool isAlarmSet = psMessage.Substring(0, 1) == "1";
                int alarmCode = int.Parse(psMessage.Substring(1, Vehicle.AsePackageConfig.ErrorCodeLength));

                if (alarmCode.ToString().Equals(Vehicle.AsePackageConfig.RemoteControlPauseErrorCode))
                {
                    OnOpPauseOrResumeEvent?.Invoke(this, isAlarmSet);
                }

                if (isAlarmSet)
                {
                    OnAlarmCodeSetEvent?.Invoke(this, alarmCode);
                }
                else
                {
                    OnAlarmCodeResetEvent?.Invoke(this, alarmCode);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void UpdateChargeStatus(string psMessage)
        {
            try
            {
                bool isCharging = psMessage.Substring(0, 1) == "1";
                if (isCharging)
                {
                    Vehicle.CheckStartChargeReplyEnd = true;
                }
                else
                {
                    Vehicle.CheckStopChargeReplyEnd = true;
                }
                Vehicle.IsCharging = isCharging;
                OnStatusChangeReportEvent?.Invoke(this, $"Local Update Charge Status :[{ Vehicle.IsCharging }]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void UpdateCarrierSlotStatus(string psMessage, uint systemByte)
        {
            EnumSlotNumber slotNumber = EnumSlotNumber.L;
            AseCarrierSlotStatus aseCarrierSlotStatus = new AseCarrierSlotStatus();

            try
            {
                slotNumber = psMessage.Substring(1, 1) == "L" ? EnumSlotNumber.L : EnumSlotNumber.R;
                if (!CheckSlotSystemByte(slotNumber, systemByte))//200827 dabid+ Log
                {
                    LogPsWrapper($"dabid Log {slotNumber.ToString()} systemByte :{systemByte.ToString()} is old. psMessage : {psMessage}");
                    return;
                }

                aseCarrierSlotStatus.SlotNumber = slotNumber;

                bool manualDeleteCst = psMessage.Substring(0, 1) == "1";
                aseCarrierSlotStatus.ManualDeleteCST = manualDeleteCst;
                if (manualDeleteCst)
                {
                    OnUpdateSlotStatusEvent?.Invoke(this, aseCarrierSlotStatus);
                    return;
                }

                aseCarrierSlotStatus.CarrierSlotStatus = GetCarrierSlotStatus(psMessage.Substring(2, 1));
                aseCarrierSlotStatus.CarrierId = psMessage.Substring(3);
                if (aseCarrierSlotStatus.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
                {
                    if (string.IsNullOrEmpty(aseCarrierSlotStatus.CarrierId.Trim()))
                    {
                        aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                    }
                    else if (aseCarrierSlotStatus.CarrierId == "ReadIdFail")
                    {
                        aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.ReadFail;
                    }
                    else if (aseCarrierSlotStatus.CarrierId == "PositionError")
                    {
                        aseCarrierSlotStatus.CarrierSlotStatus = EnumAseCarrierSlotStatus.PositionError;
                    }
                }

                OnUpdateSlotStatusEvent?.Invoke(this, aseCarrierSlotStatus);
            }
            catch (Exception ex)
            {
                string msg = "Carrier Slot Report, " + ex.Message;
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
                ImportantPspLog?.Invoke(this, msg);
            }
        }

        private bool CheckSlotSystemByte(EnumSlotNumber slotNumber, uint systemByte)
        {
            long thd = 100;//Vehicle.AsePackageConfig.MaxLocalSystemByte / 4;
            if (slotNumber == EnumSlotNumber.L)
            {
                long longSystemByte = (long)systemByte;
                if (longSystemByte > LastUpdateLeftSlotStatusSystemByte)
                {
                    //200904 dabid+ SlotStatusSystemByte最新已經是輪一圈歸0後的值，如果longSystemByte的值落在SystemByte回推thd以內就當是舊資料
                    //---L------------------------------S
                    if (LastUpdateLeftSlotStatusSystemByte < thd)
                    {
                        if (longSystemByte > (Vehicle.AsePackageConfig.MaxLocalSystemByte - (thd - LastUpdateLeftSlotStatusSystemByte)))
                        {
                            LogPsWrapper($"dabid Log {slotNumber.ToString()} - CurSlotSystemByte {LastUpdateLeftSlotStatusSystemByte.ToString()}");
                            return false;
                        }
                    }
                    //---L---S--------------------------
                    LastUpdateLeftSlotStatusSystemByte = longSystemByte;
                    LogPsWrapper($"dabid Log LastUpdateLeftSlotStatusSystemByte {LastUpdateLeftSlotStatusSystemByte.ToString()}");
                    return true;
                }
                else if (longSystemByte < LastUpdateLeftSlotStatusSystemByte)
                {

                    if (LastUpdateLeftSlotStatusSystemByte - longSystemByte > thd)
                    {
                        LastUpdateLeftSlotStatusSystemByte = longSystemByte;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                long longSystemByte = (long)systemByte;

                if (longSystemByte > LastUpdateRightSlotStatusSystemByte)
                {
                    //200904 dabid+ SlotStatusSystemByte最新已經是輪一圈歸0後的值，如果longSystemByte的值落在SystemByte回推thd以內就當是舊資料
                    //---L------------------------------S
                    if (LastUpdateRightSlotStatusSystemByte < thd)
                    {
                        if (longSystemByte > (Vehicle.AsePackageConfig.MaxLocalSystemByte - (thd - LastUpdateRightSlotStatusSystemByte)))
                        {
                            LogPsWrapper($"dabid Log {slotNumber.ToString()} - CurSlotSystemByte {LastUpdateRightSlotStatusSystemByte.ToString()}");
                            return false;
                        }
                    }
                    //---L---S--------------------------
                    LastUpdateRightSlotStatusSystemByte = longSystemByte;
                    LogPsWrapper($"dabid Log LastUpdateRightSlotStatusSystemByte {LastUpdateRightSlotStatusSystemByte.ToString()}");
                    return true;
                }
                else if (longSystemByte < LastUpdateRightSlotStatusSystemByte)
                {

                    if (LastUpdateRightSlotStatusSystemByte - longSystemByte > thd)
                    {
                        LastUpdateRightSlotStatusSystemByte = longSystemByte;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private EnumAseCarrierSlotStatus GetCarrierSlotStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumAseCarrierSlotStatus.Empty;
                case "1":
                    return EnumAseCarrierSlotStatus.Loading;
                case "2":
                    return EnumAseCarrierSlotStatus.PositionError;
                default:
                    throw new Exception($"Can not parse position report.[{v}]");
            }
        }

        private void UpdateRobotStatus(string psMessage)
        {
            try
            {
                AseRobotStatus aseRobotStatus = new AseRobotStatus();
                aseRobotStatus.RobotState = GetRobotStatus(psMessage.Substring(0, 1));
                aseRobotStatus.IsHome = psMessage.Substring(1, 1) == "1";

                Vehicle.AseRobotStatus = aseRobotStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateRobotStatus:[{Vehicle.AseRobotStatus.RobotState}][RobotHome={Vehicle.AseRobotStatus.IsHome}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private EnumAseRobotState GetRobotStatus(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumAseRobotState.Idle;
                case "1":
                    return EnumAseRobotState.Busy;
                case "2":
                    return EnumAseRobotState.Error;
                default:
                    return EnumAseRobotState.Error;
            }
        }

        private void UpdateMoveStatus(string psMessage)
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                aseMoveStatus.AseMoveState = GetMoveState(psMessage.Substring(0, 1));
                aseMoveStatus.HeadDirection = GetIntTryParse(psMessage.Substring(1, 3));
                Vehicle.AseMoveStatus = aseMoveStatus;

                OnStatusChangeReportEvent?.Invoke(this, $"UpdateMoveStatus:[{Vehicle.AseMoveStatus.AseMoveState}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);

            }
        }

        private EnumAseMoveState GetMoveState(string v)
        {
            switch (v)
            {
                case "0":
                    return EnumAseMoveState.Idle;
                case "1":
                    return EnumAseMoveState.Working;
                case "2":
                    return EnumAseMoveState.Pausing;
                case "3":
                    return EnumAseMoveState.Pause;
                case "4":
                    return EnumAseMoveState.Stoping;
                case "5":
                    return EnumAseMoveState.Block;
                case "6":
                    return EnumAseMoveState.Error;
                default:
                    return EnumAseMoveState.Error;
            }
        }

        private void SetVehicleManual()
        {
            OnModeChangeEvent?.Invoke(this, EnumAutoState.Manual);
            ImportantPspLog?.Invoke(this, $"ModeChange : Manual");
        }

        private void SetVehicleAuto()
        {
            OnModeChangeEvent?.Invoke(this, EnumAutoState.Auto);
            ImportantPspLog?.Invoke(this, $"ModeChange : Auto");
        }

        public void SetVehicleAutoScenario()
        {
            AllAgvlStatusReportRequest();
            SpinWait.SpinUntil(() => false, 50);

            SendPositionReportRequest();
            SpinWait.SpinUntil(() => false, 50);

            SendBatteryStatusRequest();
            SpinWait.SpinUntil(() => false, 50);
        }

        public void RequestVehicleToManual()
        {
            try
            {
                PrimarySendEnqueue("P13", "");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        public void CstRename(AseCarrierSlotStatus slotStatus)
        {
            try
            {
                string slotNumber = slotStatus.SlotNumber.ToString().Substring(0, 1);
                string cstRenameString = string.Concat(slotNumber, slotStatus.CarrierId);
                PrimarySendEnqueue("P27", cstRenameString);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region SecondarySend

        private void PsWrapper_OnSecondarySent(ref PSTransactionXClass transaction)
        {
            try
            {
                LogPsWrapper($"SSEND : [{transaction.PSSecondaryMessage.ToString()}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SecondarySend(PSTransactionXClass psTransaction)
        {
            try
            {
                if (Vehicle.IsLocalConnect)
                {
                    psWrapper.SecondarySent(ref psTransaction);
                }
                else
                {
                    SecondarySendQueue.Enqueue(psTransaction);
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region SecondaryReceived

        private void DealSecondaryReceived(PSTransactionXClass transaction)
        {
            try
            {
                if (transaction.PSSecondaryMessage == null)
                {
                    throw new Exception("Secondary message is empty.");
                }

                if (transaction.PSSecondaryMessage.Type.ToUpper() != "S")
                {
                    throw new Exception("Secondary message type is not S.");
                }


                switch (transaction.PSSecondaryMessage.Number)
                {
                    case "42":
                        ReceiveMoveAppendRequestAck(transaction.PSSecondaryMessage.PSMessage);
                        break;
                    case "34":
                        {
                            if (!CheckPositionSystemByte(transaction.PSPrimaryMessage.SystemBytes))
                            {
                                LogPsWrapper($"dabid Log systemByte :{transaction.PSPrimaryMessage.SystemBytes.ToString()} is old. Last = {LastUpdatePositionSystemByte}.");
                                break;
                            }
                            ReceivePositionReportRequestAck(transaction.PSSecondaryMessage.PSMessage);
                        }
                        break;
                    case "36":
                        ReceiveBatteryStatusRequestAck(transaction.PSSecondaryMessage.PSMessage);
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

        private bool CheckPositionSystemByte(uint ReceiveSystemByte)
        {
            long longSystemByte = (long)ReceiveSystemByte;
            if (longSystemByte > LastUpdatePositionSystemByte)
            {
                LastUpdatePositionSystemByte = longSystemByte;
                return true;
            }
            else if (longSystemByte < LastUpdatePositionSystemByte)
            {
                long thd = Vehicle.AsePackageConfig.MaxLocalSystemByte / 4;
                if (LastUpdatePositionSystemByte - longSystemByte > thd)
                {
                    LastUpdatePositionSystemByte = longSystemByte;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void ReceiveMoveAppendRequestAck(string psMessage)
        {
            try
            {
                if (int.TryParse(psMessage, out int result))
                {
                    if (result == 0)
                    {
                        ImportantPspLog?.Invoke(this, $"[片段移動 被拒絕] MoveAppend rejected by local. Do Move Fail Flow.");
                        AseMoveStatus moveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                        AsePositionArgs positionArgs = new AsePositionArgs()
                        {
                            Arrival = EnumAseArrival.Fail,
                            MapPosition = moveStatus.LastMapPosition,
                            HeadAngle = moveStatus.HeadDirection,
                            MovingDirection = moveStatus.MovingDirection,
                            Speed = 0
                        };
                        ReceivePositionArgsQueue.Enqueue(positionArgs);
                    }
                    else
                    {
                        ImportantPspLog?.Invoke(this, $"[片段移動 回應接受] MoveAppend accept by local. Wait arrival report.");
                    }
                }
                else
                {
                    ImportantPspLog?.Invoke(this, $"[片段移動 回應異常] MoveAppend reply unknow by local. Do Move Fail Flow.");
                    AseMoveStatus moveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                    AsePositionArgs positionArgs = new AsePositionArgs()
                    {
                        Arrival = EnumAseArrival.Fail,
                        MapPosition = moveStatus.LastMapPosition,
                        HeadAngle = moveStatus.HeadDirection,
                        MovingDirection = moveStatus.MovingDirection,
                        Speed = 0
                    };
                    ReceivePositionArgsQueue.Enqueue(positionArgs);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void ReceiveBatteryStatusRequestAck(string psMessage)
        {
            try
            {
                AseBatteryStatus aseBatteryStatus = new AseBatteryStatus(Vehicle.AseBatteryStatus);
                aseBatteryStatus.Percentage = GetIntTryParse(psMessage.Substring(0, 3));
                aseBatteryStatus.Voltage = GetIntTryParse(psMessage.Substring(3, 4)) * 0.01;
                aseBatteryStatus.Temperature = GetIntTryParse(psMessage.Substring(7, 3));
                //200522 dabid+ To Report 144 While Percentage diff
                if (Vehicle.AseBatteryStatus.Percentage != aseBatteryStatus.Percentage)
                {
                    SetPercentage(aseBatteryStatus.Percentage);
                }

                Vehicle.AseBatteryStatus = aseBatteryStatus;

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private void ReceivePositionReportRequestAck(string psMessage)
        {
            try
            {
                EnumAseMoveState moveState = GetMoveState(psMessage.Substring(0, 1));
                Vehicle.AseMoveStatus.AseMoveState = moveState;

                AsePositionArgs positionArgs = new AsePositionArgs();

                EnumAseArrival arrival = EnumAseArrival.Arrival;
                switch (moveState)
                {
                    case EnumAseMoveState.Idle:
                    case EnumAseMoveState.Working:
                    case EnumAseMoveState.Pausing:
                    case EnumAseMoveState.Pause:
                    case EnumAseMoveState.Block:
                        break;
                    case EnumAseMoveState.Stoping:
                    case EnumAseMoveState.Error:
                        {
                            arrival = EnumAseArrival.Fail;
                        }
                        break;
                    default:
                        break;
                }

                positionArgs.Arrival = arrival;

                double x = GetPositionFromPsMessage(psMessage.Substring(1, 9));
                double y = GetPositionFromPsMessage(psMessage.Substring(10, 9));
                positionArgs.MapPosition = new MapPosition(x, y);

                if (int.TryParse(psMessage.Substring(19, 3), out int headAngle))
                {
                    positionArgs.HeadAngle = headAngle;
                }

                if (int.TryParse(psMessage.Substring(22, 3), out int movingDirection))
                {
                    positionArgs.MovingDirection = movingDirection;
                }

                if (int.TryParse(psMessage.Substring(25, 4), out int speed))
                {
                    positionArgs.Speed = speed;
                }

                if (arrival == EnumAseArrival.Fail)
                {
                    ImportantPspLog?.Invoke(this, $"ReceivePositionReportRequestAck. [{psMessage.Substring(0, 1)}][{arrival.ToString()}][({x.ToString("F0")},{y.ToString("F0")})]");
                }

                ReceivePositionArgsQueue.Enqueue(positionArgs);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                //ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        private bool IsValuePositive(string v)
        {
            switch (v)
            {
                case "P":
                    return true;
                case "N":
                    return false;
                default:
                    throw new Exception($"My positive parse fail. {v}");
            }
        }

        private void PsWrapper_OnSecondaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                LogPsWrapper($"SRECV : [{transaction.PSSecondaryMessage.ToString()}]");

                SecondaryReceiveQueue.Enqueue(transaction);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                ImportantPspLog?.Invoke(this, ex.Message);
            }
        }

        #endregion

        #region PsWrapper

        private void PsWrapper_OnDebugLog(string msg)
        {
            //ImportantPspLog?.Invoke(this, msg);
        }

        private void PsWrapper_OnTransactionError(string errorString, ref PSMessageXClass psMessage)
        {
            if (psMessage == null) return;

            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, errorString + "\r\n" + psMessage.ToString());
            ImportantPspLog?.Invoke(this, $"PsWrapper_OnTransactionError. P{psMessage.Number}. AGVL DisConnect");

            PrimaryTimeoutQueue.Enqueue(psMessage);
        }

        private void PsWrapper_OnConnectionStateChange(enumConnectState state)
        {
            try
            {
                switch (state)
                {
                    case enumConnectState.CheckConnectMode:
                        {
                            if (Vehicle.IsLocalConnect)
                            {
                                Vehicle.IsLocalConnect = false;
                                LastDisconnectedTimeStamp = DateTime.Now;
                                string msg = $"PsWrapper connection state changed. [{state}]";
                                LogPsWrapper(msg);
                                ImportantPspLog?.Invoke(this, msg);
                                System.Threading.Tasks.Task.Run(() =>
                                {
                                    SpinWait.SpinUntil(() => Vehicle.IsLocalConnect, Vehicle.AsePackageConfig.DisconnectTimeoutSec * 1000);
                                    if (!Vehicle.IsLocalConnect)
                                    {
                                        OnAlarmCodeSetEvent?.Invoke(this, 57);
                                        LastDisconnectedTimeStamp = DateTime.Now;
                                    }
                                });
                            }
                            else
                            {
                                if ((DateTime.Now - LastDisconnectedTimeStamp).TotalSeconds > Vehicle.AsePackageConfig.DisconnectTimeoutSec)
                                {
                                    OnAlarmCodeSetEvent?.Invoke(this, 57);
                                    LastDisconnectedTimeStamp = DateTime.Now;
                                }
                            }
                        }
                        break;
                    case enumConnectState.ActiveWaitConnected:
                        break;
                    case enumConnectState.PassiveWaitConnected:
                        break;
                    case enumConnectState.Connected:
                        {
                            Vehicle.IsLocalConnect = true;
                            string msg = $"PsWrapper connection state changed. [{state}]";
                            LogPsWrapper(msg);
                            ImportantPspLog?.Invoke(this, msg);
                        }
                        break;
                    case enumConnectState.Quit:
                        {
                            Vehicle.IsLocalConnect = false;
                            string msg = $"PsWrapper connection state changed. [{state}]";
                            LogPsWrapper(msg);
                            ImportantPspLog?.Invoke(this, msg);
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

        private void GetConfigWrapper()
        {
            try
            {
                psWrapper = new PSWrapperXClass();
                psWrapper.Address = Vehicle.PspConnectionConfig.Ip;
                psWrapper.Port = Vehicle.PspConnectionConfig.Port;
                psWrapper.ConnectMode = Vehicle.PspConnectionConfig.IsServer ? enumConnectMode.Passive : enumConnectMode.Active;
                psWrapper.T3 = Vehicle.PspConnectionConfig.T6Timeout;
                psWrapper.T6 = Vehicle.PspConnectionConfig.T6Timeout;
                psWrapper.LinkTestIntervalMs = Vehicle.PspConnectionConfig.LinkTestIntervalMs;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LoadAutoReply()
        {
            try
            {
                if (File.Exists(Vehicle.AsePackageConfig.AutoReplyFilePath))
                {
                    LoadAutoReplyFileToMyMessageMap();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LoadAutoReplyFileToMyMessageMap()
        {
            try
            {
                string[] allRows = File.ReadAllLines(Vehicle.AsePackageConfig.AutoReplyFilePath);

                foreach (string oneRow in allRows)
                {
                    var parts = oneRow.Split(',');
                    string key = parts[0];
                    string description = parts[1];
                    string autoMessage = parts[2];

                    PSMessageXClass psMessage = new PSMessageXClass();
                    psMessage.Type = key.Substring(0, 1);
                    psMessage.Number = key.Substring(1, 2);
                    psMessage.Description = description;
                    psMessage.PSMessage = autoMessage;

                    psMessageMap.Add(key, psMessage);
                }

                PsMessageMapAddVersionInfo();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void PsMessageMapAddVersionInfo()
        {
            PSMessageXClass versionInfoRequestPsMessage = new PSMessageXClass();
            versionInfoRequestPsMessage.Type = "P";
            versionInfoRequestPsMessage.Number = "17";
            versionInfoRequestPsMessage.Description = "Version Info Request";
            versionInfoRequestPsMessage.PSMessage = "";
            psMessageMap.Add("P17", versionInfoRequestPsMessage);

            string versionInfo = string.Concat("Sw", Vehicle.SoftwareVersion.PadRight(13), "Sp", Vehicle.PspConnectionConfig.SpecVersion.PadRight(5));
            PSMessageXClass versionInfoRequestAckPsMessage = new PSMessageXClass();
            versionInfoRequestAckPsMessage.Type = "S";
            versionInfoRequestAckPsMessage.Number = "18";
            versionInfoRequestAckPsMessage.Description = "Version Info Request Ack.";
            versionInfoRequestAckPsMessage.PSMessage = versionInfo;
            psMessageMap.Add("S18", versionInfoRequestAckPsMessage);
        }

        //public bool IsConnected()
        //{
        //    return psWrapper.ConnectionState == enumConnectState.Connected;
        //}

        public void Connect()
        {
            psWrapper.Open();
        }

        public void DisConnect()
        {
            psWrapper.Close();

        }

        public void ReportAgvcDisConnect()
        {
            SendWifiSignalStrength(0);
        }

        #endregion

        #region Battery Control

        public void SetPercentage(int percentage)
        {
            try
            {
                if (Math.Abs(percentage - Vehicle.AseBatteryStatus.Percentage) >= 1)
                {
                    Vehicle.AseBatteryStatus.Percentage = percentage;
                    OnBatteryPercentageChangeEvent?.Invoke(this, percentage);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void StopCharge()
        {
            try
            {
                PrimarySendEnqueue("P47", "0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void StartCharge(EnumAddressDirection chargeDirection)
        {
            try
            {
                string chargeDirectionString;
                switch (chargeDirection)
                {
                    case EnumAddressDirection.Left:
                        chargeDirectionString = "1";
                        break;
                    case EnumAddressDirection.Right:
                        chargeDirectionString = "2";
                        break;
                    case EnumAddressDirection.None:
                    default:
                        throw new Exception($"Start charge command direction error.[{chargeDirection}]");
                }
                PrimarySendEnqueue("P47", chargeDirectionString);

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Move Control

        public void MoveStop()
        {
            try
            {
                PrimarySendEnqueue("P51", "2");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void MoveContinue()
        {
            try
            {
                PrimarySendEnqueue("P51", "1");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void MovePause()
        {
            try
            {
                PrimarySendEnqueue("P51", "0");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void RefreshMoveState()
        {
            try
            {
                PrimarySendEnqueue("P31", "1");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region Logger

        public void AppendPspLogMsg(string msg)
        {
            try
            {
                LocalLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", msg, "\r\n", LocalLogMsg);

                if (LocalLogMsg.Length > 65535)
                {
                    LocalLogMsg = LocalLogMsg.Substring(65535);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, "Device", "CarrierID", exMsg));
        }

        public void LogPsWrapper(string msg)
        {
            mirleLogger.Log(new LogFormat("PsWrapper", "5", "AsePackage", Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", msg));
            AppendPspLogMsg(msg);
        }

        #endregion

    }
}
