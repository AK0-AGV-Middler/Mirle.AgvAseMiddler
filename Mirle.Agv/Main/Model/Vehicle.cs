﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Model.TransferSteps;
using TcpIpClientSample;
using Mirle.Agv.Model.Configs;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class Vehicle
    {
        private static readonly Vehicle theVehicle = new Vehicle();
        public static Vehicle Instance { get { return theVehicle; } }

        public MiddleAgent ThdMiddleAgent { get; set; }       
        public MapInfo TheMapInfo { get; set; } = new MapInfo();
        public PlcVehicle ThePlcVehicle { get; private set; } = new PlcVehicle();
        private AgvcTransCmd curAgvcTransCmd;
        public AgvcTransCmd CurAgvcTransCmd
        {
            get
            {
                if (curAgvcTransCmd == null)
                {
                    return new AgvcTransCmd();
                }
                else
                {
                    return curAgvcTransCmd;
                }
            }
            set
            {
                curAgvcTransCmd = value;
            }
        }
        public AgvcTransCmd LastAgvcTransCmd { get; set; } = new AgvcTransCmd();
        public TransferStep CurTrasferStep { get; set; } = new EmptyTransferStep();
        public VehiclePosition CurVehiclePosition { get; set; } = new VehiclePosition();
        private EnumAutoState autoState = EnumAutoState.Manual;
        public EnumAutoState AutoState
        {
            get { return autoState; }
            set
            {
                if (value != autoState)
                {
                    autoState = value;
                    if (value == EnumAutoState.Auto)
                    {
                        ModeStatus = VHModeStatus.AutoRemote;
                    }
                    else
                    {
                        ModeStatus = VHModeStatus.Manual;
                    }
                    if (ThdMiddleAgent != null && value != EnumAutoState.PreManual)
                    {
                        ThdMiddleAgent.Send_Cmd144_StatusChangeReport();
                    }
                }
            }
        }
        public EnumThreadStatus VisitTransferStepsStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus TrackPositionStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus WatchLowPowerStatus { get; set; } = EnumThreadStatus.None;
        public EnumThreadStatus AskReserveStatus { get; set; } = EnumThreadStatus.None;
        public bool HasAlarm { get; set; } = false;
        public bool HasWarn { get; set; } = false;

        private bool frontBeamDisable = false;
        public bool FrontBeamDisable
        {
            get { return frontBeamDisable; }
            set
            {
                if (value != frontBeamDisable)
                {
                    frontBeamDisable = value;
                    ThePlcVehicle.FrontBeamSensorDisable = value;
                }
            }
        }
        private bool backBeamDisable = false;
        public bool BackBeamDisable
        {
            get { return backBeamDisable; }
            set
            {
                if (value != backBeamDisable)
                {
                    backBeamDisable = value;
                    ThePlcVehicle.BackBeamSensorDisable = value;
                }
            }
        }
        private bool leftBeamDisable = false;
        public bool LeftBeamDisable
        {
            get { return leftBeamDisable; }
            set
            {
                if (value != leftBeamDisable)
                {
                    leftBeamDisable = value;
                    ThePlcVehicle.LeftBeamSensorDisable = value;
                }
            }
        }
        private bool rightBeamDisable = false;
        public bool RightBeamDisable
        {
            get { return rightBeamDisable; }
            set
            {
                if (value != rightBeamDisable)
                {
                    rightBeamDisable = value;
                    ThePlcVehicle.RightBeamSensorDisable = value;
                }
            }
        }

        #region Comm Property
        public VHActionStatus ActionStatus { get; set; } = VHActionStatus.NoCommand;
        public VhStopSingle BlockingStatus { get; set; }
        public VhChargeStatus ChargeStatus { get; set; }
        public DriveDirction DrivingDirection { get; set; }
        public VHModeStatus ModeStatus { get; set; }
        public VhStopSingle ObstacleStatus { get; set; }
        public int ObstDistance { get; set; }
        public string ObstVehicleID { get; set; } = "";
        public VhStopSingle PauseStatus { get; set; }
        public VhPowerStatus PowerStatus { get; set; }
        public VhStopSingle ReserveStatus { get; set; }
        public string StoppedBlockID { get; set; } = "";
        public VhStopSingle ErrorStatus { get; set; }
        public ActiveType Cmd131ActType { get; set; }
        public CompleteStatus CompleteStatus { get; set; }
        public uint CmdPowerConsume { get; set; }
        public int CmdDistance { get; set; }
        public EventType Cmd134EventType { get; set; }
        public CMDCancelType Cmd137ActType { get; set; }
        public PauseEvent Cmd139EventType { get; set; }
        public string TeachingFromAddress { get; internal set; } = "";
        public string TeachingToAddress { get; internal set; } = "";

        #endregion

        private Vehicle()
        {
        }

        #region Getter

        public PlcVehicle GetPlcVehicle() { return ThePlcVehicle; }

        #endregion

    }
}
