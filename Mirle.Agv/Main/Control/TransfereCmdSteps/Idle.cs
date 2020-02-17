﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Model;
using Mirle.AgvAseMiddler.Model.TransferSteps;

namespace Mirle.AgvAseMiddler.Controller.Handler.TransCmdsSteps
{
    [Serializable]
    public class Idle : ITransferStatus
    {
        public void DoTransfer(MainFlowHandler mainFlowHandler)
        {
            EnumTransferStepType type = mainFlowHandler.GetCurrentTransferStepType();
            switch (type)
            {
                case EnumTransferStepType.Move:
                case EnumTransferStepType.MoveToCharger:
                    mainFlowHandler.SetTransCmdsStep(new Move());
                    mainFlowHandler.DoTransfer();
                    break;              
                case EnumTransferStepType.Load:
                    mainFlowHandler.SetTransCmdsStep(new Load());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferStepType.Unload:
                    mainFlowHandler.SetTransCmdsStep(new Unload());
                    mainFlowHandler.DoTransfer();
                    break;
                case EnumTransferStepType.Empty:
                default:
                    mainFlowHandler.IdleVisitNext();
                    break;
            }

        }
    }
}
