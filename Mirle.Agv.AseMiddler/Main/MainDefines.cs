using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler
{
    #region MainEnums
    public enum EnumSectionType
    {
        None,
        Horizontal,
        Vertical,
        R2000
    }
    public enum EnumMoveToEndReference
    {
        Load,
        Unload,
        Avoid
    }

    public enum EnumCommandDirection
    {
        None,
        Forward,
        Backward
    }

    public enum EnumTransferStepType
    {
        Move,
        MoveToCharger,
        Load,
        Unload,
        Empty
    }

    public enum EnumAgvcTransCommandType
    {
        Move,
        Load,
        Unload,
        LoadUnload,
        Override,
        MoveToCharger,
        Else
    }

    public enum EnumAutoState
    {
        Auto,
        Manual,
        None
    }

    public enum EnumCommandInfoStep
    {
        Begin,
        End
    }

    public enum EnumLoginLevel
    {
        Op,
        Engineer,
        Admin,
        OneAboveAll
    }

    public enum EnumCmdNums
    {
        Cmd000_EmptyCommand = 0,
        Cmd11_CouplerInfoReport = 11,
        Cmd31_TransferRequest = 31,
        Cmd32_TransferCompleteResponse = 32,
        Cmd35_CarrierIdRenameRequest = 35,
        Cmd36_TransferEventResponse = 36,
        Cmd37_TransferCancelRequest = 37,
        Cmd38_GuideInfoResponse = 38,
        Cmd39_PauseRequest = 39,
        Cmd41_ModeChange = 41,
        Cmd43_StatusRequest = 43,
        Cmd44_StatusRequest = 44,
        Cmd45_PowerOnoffRequest = 45,
        Cmd51_AvoidRequest = 51,
        Cmd52_AvoidCompleteResponse = 52,
        Cmd71_RangeTeachRequest = 71,
        Cmd72_RangeTeachCompleteResponse = 72,
        Cmd74_AddressTeachResponse = 74,
        Cmd91_AlarmResetRequest = 91,
        Cmd94_AlarmResponse = 94,
        Cmd111_CouplerInfoResponse = 111,
        Cmd131_TransferResponse = 131,
        Cmd132_TransferCompleteReport = 132,
        Cmd133_ControlZoneCancelResponse = 133,
        Cmd134_TransferEventReport = 134,
        Cmd135_CarrierIdRenameResponse = 135,
        Cmd136_TransferEventReport = 136,
        Cmd137_TransferCancelResponse = 137,
        Cmd138_GuideInfoRequest = 138,
        Cmd139_PauseResponse = 139,
        Cmd141_ModeChangeResponse = 141,
        Cmd143_StatusResponse = 143,
        Cmd144_StatusReport = 144,
        Cmd145_PowerOnoffResponse = 145,
        Cmd151_AvoidResponse = 151,
        Cmd152_AvoidCompleteReport = 152,
        Cmd171_RangeTeachResponse = 171,
        Cmd172_RangeTeachCompleteReport = 172,
        Cmd174_AddressTeachReport = 174,
        Cmd191_AlarmResetResponse = 191,
        Cmd194_AlarmReport = 194,
    }

    public enum EnumAlarmLevel
    {
        Warn,
        Alarm
    }

    public enum EnumCstIdReadResult
    {
        Normal,
        Mismatch,
        Fail
    }

    public enum EnumBeamDirection
    {
        Front,
        Back,
        Left,
        Right
    }

    public enum EnumAseMoveCommandIsEnd
    {
        None,
        End,
        Begin
    }

    public enum EnumAddressDirection
    {
        None = 0,
        Left = 1,
        Right = 2
    }

    public enum EnumSlotSelect
    {
        None,
        Left,
        Right,
        Both
    }


    public enum PsMessageType
    {
        P,
        S
    }

    public enum EnumAseRobotState
    {
        Idle,
        Busy,
        Error
    }

    public enum EnumAseMoveState
    {
        Idle,
        Working,
        Pausing,
        Pause,
        Stoping,
        Block,
        Error
    }

    public enum EnumAseCarrierSlotStatus
    {
        Empty,
        Loading,
        PositionError,
        ReadFail
    }

    public enum EnumMoveComplete
    {
        Success,
        Fail,
        Pause,
        Cancel
    }

    public enum EnumSlotNumber
    {
        L,
        R
    }

    public enum EnumAseArrival
    {
        Fail,
        Arrival,
        EndArrival
    }

    public enum EnumIsExecute
    {
        Keep,
        Go
    }

    public enum EnumLDUD
    {
        LD,
        UD,
        None
    }

    public enum EnumChargingStage
    {
        Idle,
        ArrivalCharge,
        WaitChargingOn,
        LowPowerCharge,
        DisCharge,
        WaitChargingOff
    }

    public enum EnumTransferStep
    {
        Idle,
        MoveToAddress,
        MoveToLoad,
        MoveToUnload,
        MoveToAvoid,
        MoveToAvoidWaitArrival,
        AvoidMoveComplete,
        MoveToAddressWaitArrival,
        MoveToAddressWaitEnd,
        WaitMoveArrivalVitualPortReply,
        LoadArrival,
        WaitLoadArrivalReply,
        Load,
        LoadWaitEnd,
        WaitLoadCompleteReply,
        WaitCstIdReadReply,
        UnloadArrival,
        WaitUnloadArrivalReply,
        Unload,
        UnloadWaitEnd,
        WaitUnloadCompleteReply,
        TransferComplete,      
        MoveFail,
        WaitOverrideToContinue,
        RobotFail,
        Abort,
        GetNext
    }

    public enum EnumRobotEndType
    {
        Finished,
        InterlockError,
        RobotError
    }

    public enum EnumAgvcReplyCode
    {
        Accept,
        Reject,
        Unknow
    }

    #endregion    

    public static class ExtensionMethods
    {
        public static string GetJsonInfo(this object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
