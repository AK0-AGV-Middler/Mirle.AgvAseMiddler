using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Controller;
using System.Reflection;
using System.Collections.Concurrent;
using NUnit.Framework.Constraints;

namespace Mirle.Agv.AseMiddler.Model
{

    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }
        public ConcurrentDictionary<string, AgvcTransferCommand> mapTransferCommands { get; set; } = new ConcurrentDictionary<string, AgvcTransferCommand>();
        public AgvcTransferCommand TransferCommand { get; set; } = new AgvcTransferCommand();
        public EnumAutoState AutoState { get; set; } = EnumAutoState.Manual;

        public string SoftwareVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public bool IsAgvcConnect { get; set; } = false;
        public EnumLoginLevel LoginLevel { get; set; } = EnumLoginLevel.Op;
        public EnumChargingStage ChargingStage { get; set; } = EnumChargingStage.Idle;
        public MapInfo Mapinfo { get; private set; } = new MapInfo();
        public bool IsIgnoreAppendDebug { get; set; } = false;

        #region 200824 dabid for Watch Not AUTO Charge
        public bool VehicleIdle { get; set; } = false;
        public bool LowPower { get; set; } = false;
        public bool LowPowerStartChargeTimeout { get; set; } = false;
        public bool ArrivalCharge { get; set; } = false;
        public bool IsCharger { get; set; } = false;
        public string LastAddress { get; set; } = "";
        public int LowPowerRepeatedlyChargeCounter { get; set; } = 0;
        public bool LowPowerArrivalChargeFlag { get; set; } = false;

        #endregion
        #region 200828 dabid for Watch Not AskAllSectionsReserveInOnce

        //public bool TMP_IsHome { get; set; } = false;
        //public bool TMP_IsCharging { get; set; } = false;
        //public bool TMP_IsSendWaitSchedulePause { get; set; } = false;
        public string TMP_e { get; set; } = "NONE";
        #endregion
        #region AsePackage

        public bool IsLocalConnect { get; set; } = false;
        public AseMoveStatus AseMoveStatus { get; set; } = new AseMoveStatus();
        public AseRobotStatus AseRobotStatus { get; set; } = new AseRobotStatus();
        public AseCarrierSlotStatus AseCarrierSlotL { get; set; } = new AseCarrierSlotStatus();
        public AseCarrierSlotStatus AseCarrierSlotR { get; set; } = new AseCarrierSlotStatus(EnumSlotNumber.R);
        public bool IsCharging { get; set; } = false;
        public AseBatteryStatus AseBatteryStatus { get; set; } = new AseBatteryStatus();
        public AseMovingGuide AseMovingGuide { get; set; } = new AseMovingGuide();
        public string PspSpecVersion { get; set; } = "1.0";

        public bool CheckStartChargeReplyEnd { get; set; } = true;
        public bool CheckStopChargeReplyEnd { get; set; } = true;

        #endregion

        #region Comm Property
        //public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public VhStopSingle BlockingStatus { get; set; } = VhStopSingle.Off;
        public VhChargeStatus ChargeStatus { get; set; } = VhChargeStatus.ChargeStatusNone;
        public DriveDirction DrivingDirection { get; set; } = DriveDirction.DriveDirNone;
        public VhStopSingle ObstacleStatus { get; set; } = VhStopSingle.Off;
        public int ObstDistance { get; set; }
        public string ObstVehicleID { get; set; } = "";
        public VhPowerStatus PowerStatus { get; set; } = VhPowerStatus.PowerOn;
        public string StoppedBlockID { get; set; } = "";
        public VhStopSingle ErrorStatus { get; set; } = VhStopSingle.Off;
        public uint CmdPowerConsume { get; set; }
        public int CmdDistance { get; set; }
        public string TeachingFromAddress { get; internal set; } = "";
        public string TeachingToAddress { get; internal set; } = "";
        public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public BCRReadResult LeftReadResult { get; set; } = BCRReadResult.BcrReadFail;
        public BCRReadResult RightReadResult { get; set; } = BCRReadResult.BcrReadFail;
        public VhStopSingle OpPauseStatus { get; set; } = VhStopSingle.Off;
        public ConcurrentDictionary<PauseType, bool> PauseFlags = new ConcurrentDictionary<PauseType, bool>(Enum.GetValues(typeof(PauseType)).Cast<PauseType>().ToDictionary(x => x, x => false));
        public uint WifiSignalStrength { get; set; } = 0;
        public List<PortInfo> PortInfos { get; set; } = new List<PortInfo>();

        #endregion

        #region Configs

        //Main Configs

        public MainFlowConfig MainFlowConfig { get; set; } = new MainFlowConfig();
        public AgvcConnectorConfig AgvcConnectorConfig { get; set; } = new AgvcConnectorConfig();
        public MapConfig MapConfig { get; set; } = new MapConfig();
        public AlarmConfig AlarmConfig { get; set; } = new AlarmConfig();
        public BatteryLog BatteryLog { get; set; } = new BatteryLog();

        // AsePackage Configs

        public AsePackageConfig AsePackageConfig { get; set; } = new AsePackageConfig();
        public AseBatteryConfig AseBatteryConfig { get; set; } = new AseBatteryConfig();
        public AseMoveConfig AseMoveConfig { get; set; } = new AseMoveConfig();
        public PspConnectionConfig PspConnectionConfig { get; set; } = new PspConnectionConfig();

        #endregion

        private Vehicle() { }

        public AseCarrierSlotStatus GetAseCarrierSlotStatus(EnumSlotNumber slotNumber)
        {
            switch (slotNumber)
            {
                case EnumSlotNumber.R:
                    return this.AseCarrierSlotR;
                case EnumSlotNumber.L:
                default:
                    return this.AseCarrierSlotL;
            }
        }

        public bool IsPause()
        {
            return PauseFlags.Values.Any(x => x);
        }

        public void ResetPauseFlags()
        {
            PauseFlags = new ConcurrentDictionary<PauseType, bool>(Enum.GetValues(typeof(PauseType)).Cast<PauseType>().ToDictionary(x => x, x => false));
        }
    }
}
