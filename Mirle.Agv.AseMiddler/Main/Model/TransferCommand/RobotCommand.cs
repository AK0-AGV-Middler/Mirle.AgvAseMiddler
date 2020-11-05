using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.AseMiddler.Controller;

namespace Mirle.Agv.AseMiddler.Model.TransferSteps
{
    public class RobotCommand : TransferStep
    {
        public string PortAddressId { get; set; } = "";
        public string CassetteId { get; set; } = "";
        public EnumSlotNumber SlotNumber { get; set; } = EnumSlotNumber.L;
        public EnumAddressDirection PioDirection { get; set; } = EnumAddressDirection.None;
        public string GateType { get; set; } = "0";
        public string PortNumber { get; set; } = "1";
        public bool IsBindUnload { get; set; } = false;

        public RobotCommand(AgvcTransferCommand transferCommand) : base(transferCommand.CommandId)
        {
            this.CassetteId = transferCommand.CassetteId;
            this.SlotNumber = transferCommand.SlotNumber;
        }    
    }
}
