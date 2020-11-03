using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Controller;
using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Google.Protobuf.Collections;
using System.Reflection;

using Mirle.Tools;
using System.Drawing;

namespace Mirle.Agv.AseMiddler.Model
{

    public class AgvcTransferCommand
    {
        public EnumTransferStep TransferStep { get; set; } = EnumTransferStep.Idle;
        public string CommandId { get; set; } = "";
        public EnumAgvcTransCommandType AgvcTransCommandType { get; set; } = EnumAgvcTransCommandType.Else;
        public string LoadAddressId { get; set; } = "";
        public string UnloadAddressId { get; set; } = "";
        public string CassetteId { get; set; } = "";
        public ushort SeqNum { get; set; }
        public CompleteStatus CompleteStatus { get; set; }
        public EnumSlotNumber SlotNumber { get; set; } = EnumSlotNumber.L;
        public CommandState EnrouteState { get; set; } = CommandState.None;
        public string LotId { get; set; } = "";
        public string LoadPortId { get; set; } = "";
        public string UnloadPortId { get; set; } = "";
        public bool IsRobotEnd { get; set; } = false;
        public bool IsStopAndClear { get; set; } = false;
        public bool IsLoadArrivalReply { get; set; } = false;
        public bool IsLoadCompleteReply { get; set; } = false;
        public bool IsCstIdReadReply { get; set; } = false;
        public bool IsUnloadArrivalReply { get; set; } = false;
        public bool IsUnloadCompleteReply { get; set; } = false;
        public bool IsVitualPortUnloadArrivalReply { get; set; } = false;
        public bool IsE84ContinueLoadAndUnlaod { get; set; } = false;
        public string E84ContinueCommandId { get; set; } = "";


        public AgvcTransferCommand()
        {
        }

        public AgvcTransferCommand(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum)
        {
            CommandId = transRequest.CmdID.Trim();
            CassetteId = string.IsNullOrEmpty(transRequest.CSTID) ? "" : transRequest.CSTID.Trim();
            SeqNum = aSeqNum;
            LotId = transRequest.LOTID.Trim();

            InitialCommand(transRequest.CommandAction);

            LoadAddressId = string.IsNullOrEmpty(transRequest.LoadAdr) ? "" : transRequest.LoadAdr.Trim();
            UnloadAddressId = string.IsNullOrEmpty(transRequest.DestinationAdr) ? "" : transRequest.DestinationAdr.Trim();
            LoadPortId = string.IsNullOrEmpty(transRequest.LoadPortID) ? "" : transRequest.LoadPortID.Trim();
            UnloadPortId = string.IsNullOrEmpty(transRequest.UnloadPortID) ? "" : transRequest.UnloadPortID.Trim();
        }

        private void InitialCommand(CommandActionType commandAction)
        {
            switch (commandAction)
            {
                case CommandActionType.Move:
                    AgvcTransCommandType = EnumAgvcTransCommandType.Move;
                    CompleteStatus = CompleteStatus.Move;
                    TransferStep = EnumTransferStep.MoveToAddress;
                    EnrouteState = CommandState.None;
                    break;
                case CommandActionType.Load:
                    AgvcTransCommandType = EnumAgvcTransCommandType.Load;
                    CompleteStatus = CompleteStatus.Load;
                    TransferStep = EnumTransferStep.MoveToLoad;
                    EnrouteState = CommandState.LoadEnroute;
                    break;
                case CommandActionType.Unload:
                    AgvcTransCommandType = EnumAgvcTransCommandType.Unload;
                    CompleteStatus = CompleteStatus.Unload;
                    TransferStep = EnumTransferStep.MoveToUnload;
                    EnrouteState = CommandState.UnloadEnroute;
                    break;
                case CommandActionType.Loadunload:
                    AgvcTransCommandType = EnumAgvcTransCommandType.LoadUnload;
                    CompleteStatus = CompleteStatus.Loadunload;
                    TransferStep = EnumTransferStep.MoveToLoad;
                    EnrouteState = CommandState.LoadEnroute;
                    break;
                case CommandActionType.Movetocharger:
                    AgvcTransCommandType = EnumAgvcTransCommandType.MoveToCharger;
                    CompleteStatus = CompleteStatus.MoveToCharger;
                    TransferStep = EnumTransferStep.MoveToAddress;
                    EnrouteState = CommandState.None;
                    break;
                case CommandActionType.Home:
                    break;
                case CommandActionType.Override:
                    AgvcTransCommandType = EnumAgvcTransCommandType.Override;
                    //CompleteStatus = CompleteStatus.Loadunload;
                    break;
                default:
                    break;
            }
        }

        public CommandActionType GetCommandActionType()
        {
            switch (AgvcTransCommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    return CommandActionType.Move;
                case EnumAgvcTransCommandType.Load:
                    return CommandActionType.Load;
                case EnumAgvcTransCommandType.Unload:
                    return CommandActionType.Unload;
                case EnumAgvcTransCommandType.LoadUnload:
                    return CommandActionType.Loadunload;
                case EnumAgvcTransCommandType.Override:
                    return CommandActionType.Override;
                case EnumAgvcTransCommandType.MoveToCharger:
                    return CommandActionType.Movetocharger;
                case EnumAgvcTransCommandType.Else:
                default:
                    return CommandActionType.Home;
            }
        }

        protected void LogException(string source, string exMsg)
        {
            MirleLogger.Instance.Log(new LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));
        }

        public bool IsAbortByAgvc()
        {
            switch (CompleteStatus)
            {
                case CompleteStatus.InterlockError:
                case CompleteStatus.IdreadFailed:
                case CompleteStatus.IdmisMatch:
                case CompleteStatus.VehicleAbort:
                case CompleteStatus.Abort:
                case CompleteStatus.Cancel:
                    return true;
                case CompleteStatus.Move:
                case CompleteStatus.Load:
                case CompleteStatus.Unload:
                case CompleteStatus.Loadunload:
                case CompleteStatus.MoveToCharger:
                case CompleteStatus.LongTimeInaction:
                case CompleteStatus.ForceFinishByOp:
                default:
                    return false;
            }
        }

        public string EnrouteAddressId()
        {
            switch (EnrouteState)
            {
                case CommandState.UnloadEnroute:
                case CommandState.None:
                    return UnloadAddressId.Trim();
                case CommandState.LoadEnroute:
                    return LoadAddressId.Trim();
            }
            return "";
        }
    }

    //public class AgvcMoveCmd : AgvcTransCmd
    //{
    //    public AgvcMoveCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
    //    {
    //        try
    //        {
    //            UnloadAddressId = transRequest.DestinationAdr;
    //        }
    //        catch (Exception ex)
    //        {
    //            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
    //        }
    //    }
    //}
    //public class AgvcMoveToChargerCmd : AgvcMoveCmd
    //{
    //    public AgvcMoveToChargerCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
    //    {

    //    }
    //}

    //public class AgvcLoadCmd : AgvcTransCmd
    //{
    //    public AgvcLoadCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
    //    {
    //        try
    //        {
    //            LoadAddressId = transRequest.LoadAdr;
    //            EnrouteState = CommandState.LoadEnroute;
    //        }
    //        catch (Exception ex)
    //        {
    //            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
    //        }
    //    }
    //}

    //public class AgvcUnloadCmd : AgvcTransCmd
    //{
    //    public AgvcUnloadCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
    //    {
    //        try
    //        {
    //            UnloadAddressId = transRequest.DestinationAdr;
    //            EnrouteState = CommandState.UnloadEnroute;
    //        }
    //        catch (Exception ex)
    //        {
    //            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
    //        }
    //    }
    //}

    //public class AgvcLoadunloadCmd : AgvcTransCmd
    //{
    //    public AgvcLoadunloadCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
    //    {
    //        try
    //        {
    //            LoadAddressId = transRequest.LoadAdr;
    //            UnloadAddressId = transRequest.DestinationAdr;
    //            EnrouteState = CommandState.LoadEnroute;
    //        }
    //        catch (Exception ex)
    //        {
    //            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
    //        }
    //    }
    //}

    //public class AgvcHomeCmd : AgvcTransCmd
    //{
    //    public AgvcHomeCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
    //    {
    //    }
    //}

    //public class AgvcOverrideCmd : AgvcTransCmd
    //{
    //    public AgvcOverrideCmd(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum) : base(transRequest, aSeqNum)
    //    {
    //        try
    //        {
    //            if (!string.IsNullOrEmpty(transRequest.LoadAdr))
    //            {
    //                LoadAddressId = transRequest.LoadAdr;
    //            }
    //            if (!string.IsNullOrEmpty(transRequest.DestinationAdr))
    //            {
    //                UnloadAddressId = transRequest.DestinationAdr;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
    //        }
    //    }
    //}
}
