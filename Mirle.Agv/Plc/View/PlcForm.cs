﻿using ClsMCProtocol;
using Mirle.AgvAseMiddler.Controller;
using Mirle.AgvAseMiddler.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.AgvAseMiddler.View
{
    public partial class PlcForm : IntegrateCommandForm
    {
        private MCProtocol mcProtocol;
        private PlcAgent plcAgent;
        private EnumAutoState IpcAutoState;
        public override bool IsSimulation
        {
            get => base.IsSimulation; set
            {
                base.IsSimulation = value;
                chkFakeForking.Checked = value;
            }
        }

        public PlcForm(AuoIntegrateControl auoIntegrateControl)
        {
            InitializeComponent();
            plcAgent = auoIntegrateControl.GetPlcAgent();
            mcProtocol = plcAgent.GetMCProtocol();

            tabPage1.Controls.Add(mcProtocol);         
            FormControlAddToEnityClass();
            EventInitial();

            mcProtocol.LoadXMLConfig();

            mcProtocol.OperationMode = MCProtocol.enOperationMode.NORMAL_MODE;

            mcProtocol.Height = tabPage1.Height;
            mcProtocol.Width = tabPage1.Width;
        }

        //public PlcForm(MCProtocol aMcProtocol, PlcAgent aPlcAgent)
        //{
        //    InitializeComponent();
        //    //mcProtocol = new MCProtocol();
        //    //mcProtocol.Name = "MCProtocol";
        //    //mcProtocol.OnDataChangeEvent += MCProtocol_OnDataChangeEvent;
        //    mcProtocol = aMcProtocol;
        //    tabPage1.Controls.Add(mcProtocol);

        //    plcAgent = aPlcAgent;

        //    //plcAgent = new PlcAgent(mcProtocol, null);
        //    //一些Form Control要給進去Entity物件
        //    FormControlAddToEnityClass();
        //    EventInitial();


        //    mcProtocol.LoadXMLConfig();

        //    mcProtocol.OperationMode = MCProtocol.enOperationMode.NORMAL_MODE;

        //    //aMCProtocol.Open("127.0.0.1", "3001");
        //    //aMCProtocol.ConnectToPLC("127.0.0.1", "5000");

        //    mcProtocol.Height = tabPage1.Height;
        //    mcProtocol.Width = tabPage1.Width;


        //}

        private void PlcForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void EventInitial()
        {
            mcProtocol.OnDataChangeEvent += McProtocol_OnDataChangeEvent;

            plcAgent.OnForkCommandExecutingEvent += PlcAgent_OnForkCommandExecutingEvent;
            plcAgent.OnForkCommandFinishEvent += PlcAgent_OnForkCommandFinishEvent;
            plcAgent.OnForkCommandErrorEvent += PlcAgent_OnForkCommandErrorEvent;
            plcAgent.OnForkCommandInterlockErrorEvent += PlcAgent_OnForkCommandInterlockErrorEvent;
            plcAgent.OnCassetteIDReadFinishEvent += PlcAgent_OnCassetteIDReadFinishEvent;
            plcAgent.OnIpcAutoManualChangeEvent += PlcAgent_OnIpcAutoManualChangeEvent;
        }

        private Control findControlByID(string strID)
        {
            Control aControl = null;
            aControl = grpF.Controls[strID];
            if (aControl != null)
            {
                return aControl;
            }

            aControl = grpB.Controls[strID];
            if (aControl != null)
            {
                return aControl;
            }

            aControl = grpL.Controls[strID];
            if (aControl != null)
            {
                return aControl;
            }

            aControl = grpR.Controls[strID];
            if (aControl != null)
            {
                return aControl;
            }

            return aControl;
        }


        private void LabelAddToSideBeamSensor(List<PlcBeamSensor> listBeamSensor)
        {
            foreach (PlcBeamSensor aBeamSensor in listBeamSensor)
            {
                aBeamSensor.FormLabel = (Label)findControlByID("lblBeamSensor" + aBeamSensor.Id);
                ((Label)findControlByID("lblBeamSensor" + aBeamSensor.Id)).Tag = aBeamSensor;
            }
        }

        private void FormControlAddToEnityClass()
        {
            //BeamSensor
            LabelAddToSideBeamSensor(this.plcAgent.APLCVehicle.listFrontBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.APLCVehicle.listBackBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.APLCVehicle.listLeftBeamSensor);
            LabelAddToSideBeamSensor(this.plcAgent.APLCVehicle.listRightBeamSensor);
            //Bumper
            foreach (PlcBumper aBumper in this.plcAgent.APLCVehicle.listBumper)
            {
                aBumper.FormLabel = (Label)findControlByID("lblBump" + aBumper.Id);
                ((Label)findControlByID("lblBump" + aBumper.Id)).Tag = aBumper;
            }
            //EMO
            foreach (PlcEmo aPlcEmo in this.plcAgent.APLCVehicle.listPlcEmo)
            {
                aPlcEmo.FormLabel = (Label)findControlByID("lblEMO" + aPlcEmo.Id);
                ((Label)findControlByID("lblEMO" + aPlcEmo.Id)).Tag = aPlcEmo;
            }
        }
        //
        private void PlcAgent_OnForkCommandErrorEvent(Object sender, PlcForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandErrorEvent";
        }

        private void PlcAgent_OnForkCommandExecutingEvent(Object sender, PlcForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandExecutingEvent";
        }

        private void PlcAgent_OnForkCommandFinishEvent(Object sender, PlcForkCommand aForkCommand)
        {
            triggerEvent = "PLCAgent_OnForkCommandFinishEvent";
        }

        private void PlcAgent_OnForkCommandInterlockErrorEvent(Object sender, PlcForkCommand aForkCommand)
        {
            triggerEvent = "OnForkCommandInterlockErrorEvent";
        }

        private void PlcAgent_OnCassetteIDReadFinishEvent(Object sender, String cassetteID)
        {
            triggerEvent = "PLCAgent_OnCassetteIDReadFinishEvent cassetteID = " + cassetteID;
        }

        private string triggerEvent;
        private void McProtocol_OnDataChangeEvent(string sMessage, ClsMCProtocol.clsColParameter oColParam)
        {
            //int tagChangeCount = oColParam.Count();
            //for (int i=1;i<= tagChangeCount; i++)
            //{
            //    triggerEvent = oColParam.Item(i).DataName.ToString() + " = " + oColParam.Item(i).AsBoolean.ToString();



            //}



        }

        private void btnForkCommandExecute_Click(object sender, EventArgs e)
        {
            //this.aPLCAgent.WriteForkCommand(Convert.ToUInt16(txtCommandNo.Text), (EnumForkCommand)Enum.Parse(typeof(EnumForkCommand), cmbOperationType.Text, false), txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));

            Task.Run(() =>
            {
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, true);
                System.Threading.Thread.Sleep(1000);
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Start, false);

            });
        }

        private void frmPLCAgent_Load(object sender, EventArgs e)
        {
            cmbOperationType.Items.Clear();

            foreach (string item in Enum.GetNames(typeof(EnumForkCommand)))
            {
                cmbOperationType.Items.Add(item);
            }
            cmbOperationType.SelectedIndex = 2;

            cmbDirection.Items.Clear();
            foreach (string item in Enum.GetNames(typeof(EnumStageDirection)))
            {
                cmbDirection.Items.Add(item);
            }
            cmbDirection.SelectedIndex = 1;
            //  //cmbChargeDirection

            cmbChargeDirection.Items.Clear();
            foreach (string item in Enum.GetNames(typeof(EnumChargeDirection)))
            {
                cmbChargeDirection.Items.Add(item);
            }
            cmbChargeDirection.SelectedIndex = 1;
            txtAutoChargeLowSOC.Text = this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc.ToString();

            cmbEQPIO.Items.Clear();
            cmbEQPIO.Items.Add(bool.TrueString);
            cmbEQPIO.Items.Add(bool.FalseString);
            cmbEQPIO.SelectedIndex = 0;

            //this.WindowState = FormWindowState.Minimized;
            timGUIRefresh.Enabled = true;

            FillSVToBatteryParamTbx();
            FillPVToBatteryParamTbx();

            FillSVToForkCommParamTbx();
            FillPVToForkCommParamTbx();
        }

        private void btnForkCommandClear_Click(object sender, EventArgs e)
        {
            txtCommandNo.Text = "1";
            cmbOperationType.Text = EnumForkCommand.Load.ToString();
            txtStageNo.Text = "1";
            cmbDirection.Text = EnumStageDirection.Left.ToString();
            cmbEQPIO.Text = bool.TrueString;
            txtForkSpeed.Text = "100";
            this.plcAgent.WriteForkCommandInfo(0, EnumForkCommand.None, "0", EnumStageDirection.None, true, 100);

        }

        private void btnForkCommandWrite_Click(object sender, EventArgs e)
        {
            try
            {
                this.plcAgent.WriteForkCommandInfo(Convert.ToUInt16(txtCommandNo.Text), (EnumForkCommand)Enum.Parse(typeof(EnumForkCommand), cmbOperationType.Text, false), txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));

            }
            catch (Exception ex)
            {

                this.triggerEvent = ex.ToString();
            }

        }

        private void btnForkCommandReadRequest_Click(object sender, EventArgs e)
        {

            Task.Run(() =>
            {
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, true);
                System.Threading.Thread.Sleep(1000);
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Read_Request, false);

            });
        }

        private void btnChargeLeftStart_Click(object sender, EventArgs e)
        {
            this.plcAgent.ChargeStartCommand(EnumChargeDirection.Left);
        }

        private void btnChargeRightStart_Click(object sender, EventArgs e)
        {
            this.plcAgent.ChargeStartCommand(EnumChargeDirection.Right);

        }

        private void btnChargeStop_Click(object sender, EventArgs e)
        {
            this.plcAgent.ChargeStopCommand();

        }

        private void btnForkCommandFinishAck_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, true);
                System.Threading.Thread.Sleep(1000);
                this.plcAgent.WriteForkCommandActionBit(EnumForkCommandExecutionType.Command_Finish_Ack, false);

            });
        }
        private void PlcAgent_OnIpcAutoManualChangeEvent(Object sender, EnumAutoState state)
        {
            IpcAutoState = state;
        }
        private void IpcModeObjEnabled(bool status)
        {
            grpForkCommdAndStat.Enabled = status;
            //grpCastIDReader.Enabled = status;
            grpForkCycleRun.Enabled = status;

            //========================================================//
            //鎖btnChargeLeftStart,btnChargeRightStart
            //foreach( Control c in pnlCharg.Controls)
            //{
            //    if (c is Button)
            //    {
            //        if (c.Name == "btnChargeLeftStart" || c.Name == "btnChargeRightStart")
            //        {
            //            c.Enabled = status;
            //        }                   
            //    }
            //}

            //========================================================//

            //pnlCharg.Enabled = status;
            //grpB.Enabled = status;
            //grpL.Enabled = status;
            //grpR.Enabled = status;
            //grpF.Enabled = status;
            //grpSafety.Enabled = status;
            //grpAutoSleep.Enabled = status;
            //pnlMove.Enabled = status;
            //palForkParams.Enabled = status;
            //palChargParams.Enabled = status;
            //if (!status)
            //{
            //    rdoSafetyEnable.Checked = true;
            //    rdoBeamSensorAutoSleepEnable.Checked = true;
            //    chkMoveFront.Checked = false;
            //    chkMoveBack.Checked = false;
            //    chkMoveLeft.Checked = false;
            //    chkMoveRight.Checked = false;
            //}
        }
        private EnumAutoState beforeIpcAutoState;
        private void timGUIRefresh_Tick(object sender, EventArgs e)
        {
            chkBeamSensorDisableNormalSpeed.Checked = this.plcAgent.APLCVehicle.BeamSensorDisableNormalSpeed;
            
            labIPcStatus.Text = Enum.GetName(typeof(EnumAutoState), Vehicle.Instance.AutoState);
            if (IpcAutoState == EnumAutoState.Auto)
            {
                if (beforeIpcAutoState != IpcAutoState)
                {
                    beforeIpcAutoState = IpcAutoState;
                    IpcModeObjEnabled(false);
                }
            }
            else
            {
                if (beforeIpcAutoState != IpcAutoState)
                {
                    beforeIpcAutoState = IpcAutoState;
                    IpcModeObjEnabled(true);
                }
            }
            //Battery Information
            txtCellNumber.Text = plcAgent.APLCVehicle.plcBatterys.Cell_number.ToString();
            txtTempNumber.Text = plcAgent.APLCVehicle.plcBatterys.Temperature_sensor_number.ToString();

            txtTempMOSFET.Text = plcAgent.APLCVehicle.plcBatterys.Temperature_1_MOSFET.ToString();
            txtTempCell.Text = plcAgent.APLCVehicle.plcBatterys.Temperature_2_Cell.ToString();
            txtTempMCU.Text = plcAgent.APLCVehicle.plcBatterys.Temperature_3_MCU.ToString();
            txtBatteryCurrent.Text = plcAgent.APLCVehicle.plcBatterys.BatteryCurrent.ToString();
            txtBatteryVoltage.Text = plcAgent.APLCVehicle.plcBatterys.Packet_Voltage.ToString();

            txtBatteryRC.Text = plcAgent.APLCVehicle.plcBatterys.Remain_Capacity.ToString();
            txtBatteryDC.Text = plcAgent.APLCVehicle.plcBatterys.Design_Capacity.ToString();
            txtBatterySOCFromPLC.Text = plcAgent.APLCVehicle.plcBatterys.BatterySOCFormPlc.ToString();
            txtBatterySOHFromPLC.Text = plcAgent.APLCVehicle.plcBatterys.BatterySOHFormPlc.ToString();

            foreach (Control c in tlpCellVoltage.Controls)
            {
                if (c is TextBox)
                {
                    double d = plcAgent.APLCVehicle.plcBatterys.BatteryCells[Convert.ToUInt16(c.Tag)].Voltage;
                    c.Text = d.ToString();
                    if (d <= plcAgent.APLCVehicle.plcBatterys.Battery_Cell_Low_Voltage)
                        c.BackColor = Color.Pink;
                    else c.BackColor = SystemColors.Control;
                }
            }

            PlcForkCommand plcForkCommand = plcAgent.APLCVehicle.Robot.ExecutingCommand;
            if (plcForkCommand != null)
            {
                tbxCommandNo_PV.Text = plcForkCommand.CommandNo.ToString();
                tbxForkCommandType_PV.Text = plcForkCommand.ForkCommandType.ToString();
                tbxDirection_PV.Text = plcForkCommand.Direction.ToString();
                tbxStageNo_PV.Text = plcForkCommand.StageNo.ToString();
                tbxIsEqPio_PV.Text = plcForkCommand.IsEqPio.ToString();
                tbxForkSpeed_PV.Text = plcForkCommand.ForkSpeed.ToString();
                tbxReason_PV.Text = plcForkCommand.Reason.ToString();
            }
            else
            {
                tbxCommandNo_PV.Text = "Null";
                tbxForkCommandType_PV.Text = "Null";
                tbxDirection_PV.Text = "Null";
                tbxStageNo_PV.Text = "Null";
                tbxIsEqPio_PV.Text = "Null";
                tbxForkSpeed_PV.Text = "Null";
                tbxReason_PV.Text = "Null";
            }

            tbxAlignmentP_AV.Text = plcAgent.APLCVehicle.Robot.ForkAlignmentP.ToString();
            tbxAlignmentY_AV.Text = plcAgent.APLCVehicle.Robot.ForkAlignmentY.ToString();
            tbxAlignmentPhi_AV.Text = plcAgent.APLCVehicle.Robot.ForkAlignmentPhi.ToString();
            tbxAlignmentF_AV.Text = plcAgent.APLCVehicle.Robot.ForkAlignmentF.ToString();
            tbxAlignmentCode_AV.Text = plcAgent.APLCVehicle.Robot.ForkAlignmentCode.ToString();
            tbxAlignmentC_AV.Text = plcAgent.APLCVehicle.Robot.ForkAlignmentC.ToString();
            tbxAlignmentB_AV.Text = plcAgent.APLCVehicle.Robot.ForkAlignmentB.ToString();
            

            if (this.plcAgent.APLCVehicle.RobotHome)
                lblForkHome.BackColor = Color.LightGreen;
            else
                lblForkHome.BackColor = Color.Silver;

            tbxLogView.Text = plcAgent.logMsg;

            txtTriggerEvent.Text = triggerEvent;
            if (this.plcAgent.APLCVehicle.plcBatterys.BatteryType == EnumBatteryType.Gotech)
            {
                this.lblGotech.BackColor = Color.LightGreen;
            }
            else
            {
                this.lblGotech.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.plcBatterys.BatteryType == EnumBatteryType.Yinda)
            {
                this.lblYinda.BackColor = Color.LightGreen;
            }
            else
            {
                this.lblYinda.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.Batterys.Charging)
            {
                lblCharging.BackColor = Color.LightGreen;
            }
            else
            {
                lblCharging.BackColor = Color.Silver;
            }

            txtCurrent.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.MeterCurrent);
            txtVoltage.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.MeterVoltage);
            txtWatt.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.MeterWatt);
            txtWattHour.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.MeterWattHour);
            txtAH.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.MeterAh);
            txtSOC.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.Percentage);
            txtAHWorkingRange.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.AhWorkingRange);
            txtCCModeAH.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.CcModeAh);

            txtCCModeCounter.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.CcModeCounter);
            txtMaxCCmodeCounter.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.MaxResetAhCcounter);
            txtFullChargeIndex.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.FullChargeIndex);

            txtFBatteryTemp.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.FBatteryTemperature);
            txtBBatteryTemp.Text = Convert.ToString(this.plcAgent.APLCVehicle.plcBatterys.BBatteryTemperature);

            txtErrorReason.Text = this.plcAgent.getErrorReason();

            txtCassetteID.Text = this.plcAgent.APLCVehicle.CarrierSlot.CarrierId;

            if (this.plcAgent.APLCVehicle.Robot.ForkBusy)
            {
                lblForkBusy.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkBusy.BackColor = Color.Silver;
            }
            if (this.plcAgent.APLCVehicle.Robot.ForkReady)
            {
                lblForkReady.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkReady.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.Robot.ForkFinish)
            {
                lblForkFinish.BackColor = Color.LightGreen;
            }
            else
            {
                lblForkFinish.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.CarrierSlot.Loading)
            {
                lblLoading.BackColor = Color.LightGreen;
            }
            else
            {
                lblLoading.BackColor = Color.Silver;
            }

            if (this.plcAgent.APLCVehicle.SafetyDisable)
            {
                tabSafety.BackColor = Color.Pink;
            }
            else
            {
                tabSafety.BackColor = Color.Transparent;
            }

            txtSafetyAction.Text = plcAgent.APLCVehicle.VehicleSafetyAction.ToString();
            //BeamSensor color
            showSideBeamcolor(plcAgent.APLCVehicle.listFrontBeamSensor);
            showSideBeamcolor(plcAgent.APLCVehicle.listBackBeamSensor);
            showSideBeamcolor(plcAgent.APLCVehicle.listLeftBeamSensor);
            showSideBeamcolor(plcAgent.APLCVehicle.listRightBeamSensor);

            //Bumper color
            foreach (PlcBumper aPLCBumper in plcAgent.APLCVehicle.listBumper)
            {
                if (aPLCBumper.Disable)
                {
                    aPLCBumper.FormLabel.BorderStyle = BorderStyle.None;
                }
                else
                {
                    aPLCBumper.FormLabel.BorderStyle = BorderStyle.FixedSingle;
                }

                if (!aPLCBumper.Signal)
                {
                    aPLCBumper.FormLabel.BackColor = this.lblNoDetect.BackColor;
                }
                else
                {
                    aPLCBumper.FormLabel.BackColor = this.lblNearDetect.BackColor;
                }
            }
            //20190730_Rudy 新增EMO Status 顯示
            if (plcAgent.APLCVehicle.PlcEmoStatus)
            {
                lblEMO.BackColor = Color.Pink;
            }
            else
            {
                lblEMO.BackColor = Color.LightGreen;
            }

            //EMO color
            foreach (PlcEmo aPlcEmo in plcAgent.APLCVehicle.listPlcEmo)
            {
                if (aPlcEmo.Disable)
                {
                    aPlcEmo.FormLabel.BorderStyle = BorderStyle.None;
                }
                else
                {
                    aPlcEmo.FormLabel.BorderStyle = BorderStyle.FixedSingle;
                }

                if (aPlcEmo.Signal)
                {
                    aPlcEmo.FormLabel.BackColor = this.lblNoDetect.BackColor;
                }
                else
                {
                    aPlcEmo.FormLabel.BackColor = this.lblNearDetect.BackColor;
                }
            }

            if (plcAgent.APLCVehicle.MoveFront)
            {
                chkMoveFront.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveFront.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.MoveBack)
            {
                chkMoveBack.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveBack.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.MoveLeft)
            {
                chkMoveLeft.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveLeft.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.MoveRight)
            {
                chkMoveRight.BackColor = Color.LightGreen;
            }
            else
            {
                chkMoveRight.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.FrontBeamSensorDisable)
            {
                grpF.BackColor = Color.Pink;
            }
            else
            {
                grpF.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.BackBeamSensorDisable)
            {
                grpB.BackColor = Color.Pink;
            }
            else
            {
                grpB.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.LeftBeamSensorDisable)
            {
                grpL.BackColor = Color.Pink;
            }
            else
            {
                grpL.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.RightBeamSensorDisable)
            {
                grpR.BackColor = Color.Pink;
            }
            else
            {
                grpR.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.BeamSensorAutoSleep)
            {
                rdoBeamSensorAutoSleepEnable.BackColor = Color.LightGreen;
                rdoBeamSensorAutoSleepDisable.BackColor = Color.Transparent;
            }
            else
            {
                rdoBeamSensorAutoSleepEnable.BackColor = Color.Transparent;
                rdoBeamSensorAutoSleepDisable.BackColor = Color.LightGreen;
            }

            if (plcAgent.APLCVehicle.BumperAlarmStatus)
            {
                lblBumperAlarm.BackColor = Color.Pink;
            }
            else
            {
                lblBumperAlarm.BackColor = Color.LightGreen;
            }

            #region tab JogOperation

            displayTabJogOperation_OperateStatus();
            displayTabJogOperation_ElmoStatus();
            displayTabJogOperation_VehicleOperation();

            #endregion
            
        }


        #region Display and change label bgcolor functions
        private void displayTabJogOperation_OperateStatus()
        {
            if (plcAgent.APLCVehicle.JogOperation.ModeOperation == true)
            {
                lblPlcModeAuto.BackColor = Color.LightGreen;
                lblPlcModeManual.BackColor = Color.Transparent;
            }
            else
            {
                lblPlcModeAuto.BackColor = Color.Transparent;
                lblPlcModeManual.BackColor = Color.LightGreen;
            }

            if (plcAgent.APLCVehicle.JogOperation.ModeVehicle == EnumJogVehicleMode.Auto)
            {
                lblVehicleModeAuto.BackColor = Color.LightGreen;
                lblVehicleModeManual.BackColor = Color.Transparent;
            }
            else
            {
                lblVehicleModeAuto.BackColor = Color.Transparent;
                lblVehicleModeManual.BackColor = Color.LightGreen;
            }
        }

        private void displayTabJogOperation_ElmoStatus()
        {
            switch (plcAgent.APLCVehicle.JogOperation.JogElmoFunction)
            {
                case EnumJogElmoFunction.Disable:
                    lblElmoDisable.BackColor = Color.LightGreen;
                    lblElmoEnable.BackColor = Color.Transparent;
                    lblElmoAllReset.BackColor = Color.Transparent;
                    break;
                case EnumJogElmoFunction.Enable:
                    lblElmoDisable.BackColor = Color.Transparent;
                    lblElmoEnable.BackColor = Color.LightGreen;
                    lblElmoAllReset.BackColor = Color.Transparent;
                    break;
                case EnumJogElmoFunction.All_Reset:
                    lblElmoDisable.BackColor = Color.Transparent;
                    lblElmoEnable.BackColor = Color.Transparent;
                    lblElmoAllReset.BackColor = Color.LightGreen;
                    break;
                default:
                    lblElmoDisable.BackColor = Color.Transparent;
                    lblElmoEnable.BackColor = Color.Transparent;
                    lblElmoAllReset.BackColor = Color.Transparent;
                    break;
            }
        }
        private void displayTabJogOperation_VehicleOperation()
        {

            switch (plcAgent.APLCVehicle.JogOperation.JogRunMode)
            {
                case EnumJogRunMode.Normal:
                    lblVehicleOperationModeNormal.BackColor = Color.LightGreen;
                    lblVehicleOperationModeTFWheel.BackColor = Color.Transparent;
                    lblVehicleOperationModeTBWheel.BackColor = Color.Transparent;
                    lblVehicleOperationModeSpinTurn.BackColor = Color.Transparent;
                    break;
                case EnumJogRunMode.ForwardWheel:
                    lblVehicleOperationModeNormal.BackColor = Color.Transparent;
                    lblVehicleOperationModeTFWheel.BackColor = Color.LightGreen;
                    lblVehicleOperationModeTBWheel.BackColor = Color.Transparent;
                    lblVehicleOperationModeSpinTurn.BackColor = Color.Transparent;
                    break;
                case EnumJogRunMode.BackwardWheel:
                    lblVehicleOperationModeNormal.BackColor = Color.Transparent;
                    lblVehicleOperationModeTFWheel.BackColor = Color.Transparent;
                    lblVehicleOperationModeTBWheel.BackColor = Color.LightGreen;
                    lblVehicleOperationModeSpinTurn.BackColor = Color.Transparent;
                    break;
                case EnumJogRunMode.SpinTurn:
                    lblVehicleOperationModeNormal.BackColor = Color.Transparent;
                    lblVehicleOperationModeTFWheel.BackColor = Color.Transparent;
                    lblVehicleOperationModeTBWheel.BackColor = Color.Transparent;
                    lblVehicleOperationModeSpinTurn.BackColor = Color.LightGreen;
                    break;
                default:
                    lblVehicleOperationModeNormal.BackColor = Color.Transparent;
                    lblVehicleOperationModeTFWheel.BackColor = Color.Transparent;
                    lblVehicleOperationModeTBWheel.BackColor = Color.Transparent;
                    lblVehicleOperationModeSpinTurn.BackColor = Color.Transparent;
                    break;
            }

            switch (plcAgent.APLCVehicle.JogOperation.JogTurnSpeed)
            {
                case EnumJogTurnSpeed.High:
                    lblTSpeedFast.BackColor = Color.LightGreen;
                    lblTSpeedMedium.BackColor = Color.Transparent;
                    lblTSpeedSlow.BackColor = Color.Transparent;
                    break;
                case EnumJogTurnSpeed.Medium:
                    lblTSpeedFast.BackColor = Color.Transparent;
                    lblTSpeedMedium.BackColor = Color.LightGreen;
                    lblTSpeedSlow.BackColor = Color.Transparent;
                    break;
                case EnumJogTurnSpeed.Low:
                    lblTSpeedFast.BackColor = Color.Transparent;
                    lblTSpeedMedium.BackColor = Color.Transparent;
                    lblTSpeedSlow.BackColor = Color.LightGreen;
                    break;
                default:
                    lblTSpeedFast.BackColor = Color.Transparent;
                    lblTSpeedMedium.BackColor = Color.Transparent;
                    lblTSpeedSlow.BackColor = Color.Transparent;
                    break;
            }

            switch (plcAgent.APLCVehicle.JogOperation.JogMoveVelocity)
            {
                case EnumJogMoveVelocity.ThreeHundred:
                    lblMVelocity300.BackColor = Color.LightGreen;
                    lblMVelocity100.BackColor = Color.Transparent;
                    lblMVelocity50.BackColor = Color.Transparent;
                    lblMVelocity10.BackColor = Color.Transparent;
                    break;
                case EnumJogMoveVelocity.OneHundred:
                    lblMVelocity300.BackColor = Color.Transparent;
                    lblMVelocity100.BackColor = Color.LightGreen;
                    lblMVelocity50.BackColor = Color.Transparent;
                    lblMVelocity10.BackColor = Color.Transparent;
                    break;
                case EnumJogMoveVelocity.Fifty:
                    lblMVelocity300.BackColor = Color.Transparent;
                    lblMVelocity100.BackColor = Color.Transparent;
                    lblMVelocity50.BackColor = Color.LightGreen;
                    lblMVelocity10.BackColor = Color.Transparent;
                    break;
                case EnumJogMoveVelocity.Ten:
                    lblMVelocity300.BackColor = Color.Transparent;
                    lblMVelocity100.BackColor = Color.Transparent;
                    lblMVelocity50.BackColor = Color.Transparent;
                    lblMVelocity10.BackColor = Color.LightGreen;
                    break;
                default:
                    lblMVelocity300.BackColor = Color.Transparent;
                    lblMVelocity100.BackColor = Color.Transparent;
                    lblMVelocity50.BackColor = Color.Transparent;
                    lblMVelocity10.BackColor = Color.Transparent;
                    break;

        }

            switch (plcAgent.APLCVehicle.JogOperation.JogOperation)
            {
                case EnumJogOperation.TurnLeft:
                    lblOperationTurnLeft.BackColor = Color.LightGreen;
                    lblOperationTurnRight.BackColor = Color.Transparent;
                    lblOperationMoveForward.BackColor = Color.Transparent;
                    lblOperationMoveBackward.BackColor = Color.Transparent;
                    lblOperationStop.BackColor = Color.Transparent;
                    break;
                case EnumJogOperation.TurnRight:
                    lblOperationTurnLeft.BackColor = Color.Transparent;
                    lblOperationTurnRight.BackColor = Color.LightGreen;
                    lblOperationMoveForward.BackColor = Color.Transparent;
                    lblOperationMoveBackward.BackColor = Color.Transparent;
                    lblOperationStop.BackColor = Color.Transparent;
                    break;
                case EnumJogOperation.MoveForward:
                    lblOperationTurnLeft.BackColor = Color.Transparent;
                    lblOperationTurnRight.BackColor = Color.Transparent;
                    lblOperationMoveForward.BackColor = Color.LightGreen;
                    lblOperationMoveBackward.BackColor = Color.Transparent;
                    lblOperationStop.BackColor = Color.Transparent;
                    break;
                case EnumJogOperation.MoveBackward:
                    lblOperationTurnLeft.BackColor = Color.Transparent;
                    lblOperationTurnRight.BackColor = Color.Transparent;
                    lblOperationMoveForward.BackColor = Color.Transparent;
                    lblOperationMoveBackward.BackColor = Color.LightGreen;
                    lblOperationStop.BackColor = Color.Transparent;
                    break;
                case EnumJogOperation.Stop:
                    lblOperationTurnLeft.BackColor = Color.Transparent;
                    lblOperationTurnRight.BackColor = Color.Transparent;
                    lblOperationMoveForward.BackColor = Color.Transparent;
                    lblOperationMoveBackward.BackColor = Color.Transparent;
                    lblOperationStop.BackColor = Color.LightGreen;
                    break;
                default:
                    lblOperationTurnLeft.BackColor = Color.Transparent;
                    lblOperationTurnRight.BackColor = Color.Transparent;
                    lblOperationMoveForward.BackColor = Color.Transparent;
                    lblOperationMoveBackward.BackColor = Color.Transparent;
                    lblOperationStop.BackColor = Color.Transparent;
                    break;

            }

            if (plcAgent.APLCVehicle.JogOperation.JogMoveOntimeRevise) {
                lblOperationRevise.BackColor = Color.LightGreen;
            }else
            {
                lblOperationRevise.BackColor = Color.Transparent;
            }

            if (plcAgent.APLCVehicle.JogOperation.JogMaxDistance != 0)
            {
                lblDistanceMm.Text = plcAgent.APLCVehicle.JogOperation.JogMaxDistance.ToString() + "  mm";
                lblDistanceMm.BackColor = Color.LightGreen;
            }else
            {
                lblDistanceMm.Text = "000000  mm";
                lblDistanceMm.BackColor = Color.Transparent;
            }
            
        }

        #endregion

        
        private void showSideBeamcolor(List<PlcBeamSensor> listBeamSensor)
        {
            foreach (PlcBeamSensor aBeamSensor in listBeamSensor)
            {
                if (aBeamSensor.Disable)
                {
                    aBeamSensor.FormLabel.BorderStyle = BorderStyle.None;
                }
                else
                {
                    aBeamSensor.FormLabel.BorderStyle = BorderStyle.FixedSingle;
                }

                if (aBeamSensor.ReadSleepSignal)
                {
                    aBeamSensor.FormLabel.BackColor = this.lblSleep.BackColor;
                }
                else
                {
                    if (aBeamSensor.NearSignal)
                    {
                        if (aBeamSensor.FarSignal)
                        {
                            aBeamSensor.FormLabel.BackColor = this.lblNoDetect.BackColor;
                        }
                        else
                        {
                            aBeamSensor.FormLabel.BackColor = this.lblFarDetect.BackColor;
                        }
                    }
                    else
                    {
                        aBeamSensor.FormLabel.BackColor = this.lblNearDetect.BackColor;
                    }
                }

                if( aBeamSensor==lblBeamSensorSelect.Tag)
                {
                    lblBeamSensorSelect.BackColor = aBeamSensor.FormLabel.BackColor;
                }

            }
        }



        private void btnMeterAHReset_Click(object sender, EventArgs e)
        {
            plcAgent.SetMeterAHToZero();

        }

        private void btnForkCommandExecuteFlow_Click(object sender, EventArgs e)
        {
            try
            {
                if (!plcAgent.IsForkCommandExist())
                {
                    PlcForkCommand aForkCommand = new PlcForkCommand(Convert.ToUInt16(txtCommandNo.Text), (EnumForkCommand)Enum.Parse(typeof(EnumForkCommand), cmbOperationType.Text, false), txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
                    plcAgent.AddForkComand(aForkCommand);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

                this.triggerEvent = ex.ToString();
            }


        }

        private void btnClearForkCommand_Click(object sender, EventArgs e)
        {
            try
            {
                plcAgent.ClearExecutingForkCommand();
            }
            catch (Exception ex)
            {

                this.triggerEvent = ex.ToString();
            }

        }

        private void btnBuzzerStop_Click(object sender, EventArgs e)
        {
            plcAgent.WritePLCBuzzserStop();
        }

        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            plcAgent.WritePLCAlarmReset();
        }

        private void btnTriggerCassetteReader_Click(object sender, EventArgs e)
        {
            string CassetteID = "";
            this.plcAgent.triggerCassetteIDReader(ref CassetteID);
        }

        private void btnCycle_Click(object sender, EventArgs e)
        {
            if (btnCycle.Text == "Cycle Start")
            {
                cycleForkCommandCount = 0;
                cycleChargeCommandCount = 0;
                timCycle.Enabled = true;
                btnCycle.Text = "Cycle Stop";
            }
            else
            {
                timCycle.Enabled = false;
                btnCycle.Text = "Cycle Start";
            }
        }

        private UInt64 cycleForkCommandCount = 0;
        private UInt64 cycleChargeCommandCount = 0;

        private void timCycle_Tick(object sender, EventArgs e)
        {
            if (this.plcAgent.APLCVehicle.Robot.ForkReady && this.plcAgent.APLCVehicle.Robot.ForkBusy == false)
            {
                if (!plcAgent.IsForkCommandExist())
                {
                    //判斷loading 決定load/unload
                    if (plcAgent.APLCVehicle.CarrierSlot.Loading)
                    {
                        PlcForkCommand aForkCommand = new PlcForkCommand(Convert.ToUInt16(txtCommandNo.Text), EnumForkCommand.Unload, txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
                        plcAgent.AddForkComand(aForkCommand);
                        cycleForkCommandCount++;
                        txtCycleForkCommandCount.Text = cycleForkCommandCount.ToString();
                    }
                    else
                    {
                        PlcForkCommand aForkCommand = new PlcForkCommand(Convert.ToUInt16(txtCommandNo.Text), EnumForkCommand.Load, txtStageNo.Text, (EnumStageDirection)Enum.Parse(typeof(EnumStageDirection), cmbDirection.Text, false), Convert.ToBoolean(cmbEQPIO.Text), Convert.ToUInt16(txtForkSpeed.Text));
                        plcAgent.AddForkComand(aForkCommand);
                        cycleForkCommandCount++;
                        txtCycleForkCommandCount.Text = cycleForkCommandCount.ToString();
                    }


                }
            }

            if (this.plcAgent.APLCVehicle.Batterys.Charging == false)
            {
                if (this.plcAgent.APLCVehicle.Batterys.Percentage < this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc)
                {
                    //自動充電
                    if ((EnumChargeDirection)Enum.Parse(typeof(EnumChargeDirection), cmbChargeDirection.Text, false) == EnumChargeDirection.Left)
                    {
                        this.plcAgent.ChargeStartCommand(EnumChargeDirection.Left);
                        cycleChargeCommandCount++;
                        txtCycleChargeCommandCount.Text = cycleChargeCommandCount.ToString();
                    }
                    else if ((EnumChargeDirection)Enum.Parse(typeof(EnumChargeDirection), cmbChargeDirection.Text, false) == EnumChargeDirection.Right)
                    {
                        this.plcAgent.ChargeStartCommand(EnumChargeDirection.Right);
                        cycleChargeCommandCount++;
                        txtCycleChargeCommandCount.Text = cycleChargeCommandCount.ToString();
                    }
                    else
                    {
                        timCycle.Enabled = false;
                        btnCycle.Text = "Cycle Start";
                    }
                }
                else
                {

                }

            }

        }

        private void btnSafetySet_Click(object sender, EventArgs e)
        {
            if (rdoSafetyDisable.Checked)
            {
                this.plcAgent.APLCVehicle.SafetyDisable = true;
            }
            else
            {
                this.plcAgent.APLCVehicle.SafetyDisable = false;
            }
        }


        private void lblBump_DoubleClick(object sender, EventArgs e)
        {
            //PlcBumper aPLCBumper = (PlcBumper)((Label)sender).Tag;
            //aPLCBumper.Disable = !aPLCBumper.Disable;
        }

        private void lblBeamSensor_DoubleClick(object sender, EventArgs e)
        {
            PlcBeamSensor aPlcBeamSensor = (PlcBeamSensor)((Label)sender).Tag;
            aPlcBeamSensor.Disable = !aPlcBeamSensor.Disable;
        }

        private void lblBeamSensor_Click(object sender, EventArgs e)
        {
            PlcBeamSensor aPlcBeamSensor = (PlcBeamSensor)((Label)sender).Tag;
            lblBeamSensorSelect.Text = ((Label)sender).Text;
            lblBeamSensorSelect.Tag = aPlcBeamSensor;
            lblBeamSensorSelect.BackColor = ((Label)sender).BackColor;

            chkBeamSensorFarSet.Checked = !aPlcBeamSensor.FarSignal;
            chkBeamSensorNearSet.Checked = !aPlcBeamSensor.NearSignal;
        }

        private void chkBeamSensorFarSet_CheckedChanged(object sender, EventArgs e)
        {
            if (lblBeamSensorSelect.Tag != null)
            {
                PlcBeamSensor aPlcBeamSensor = (PlcBeamSensor)lblBeamSensorSelect.Tag;
                if (chkBeamSensorFarSet.Checked)
                {
                    aPlcBeamSensor.FarSignal = false;
                }
                else
                {
                    aPlcBeamSensor.FarSignal = true;
                }
            }
        }

        private void chkBeamSensorNearSet_CheckedChanged(object sender, EventArgs e)
        {
            if (lblBeamSensorSelect.Tag != null)
            {
                PlcBeamSensor aPlcBeamSensor = (PlcBeamSensor)lblBeamSensorSelect.Tag;
                if (chkBeamSensorNearSet.Checked)
                {
                    aPlcBeamSensor.NearSignal = false;
                }
                else
                {
                    aPlcBeamSensor.NearSignal = true;
                }
            }
        }

        private void chkMoveFront_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveFront.Checked)
            {
                plcAgent.APLCVehicle.MoveFront = true;

            }
            else
            {
                plcAgent.APLCVehicle.MoveFront = false;
            }
        }

        private void chkMoveBack_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveBack.Checked)
            {
                plcAgent.APLCVehicle.MoveBack = true;
            }
            else
            {
                plcAgent.APLCVehicle.MoveBack = false;
            }
        }

        private void chkMoveLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveLeft.Checked)
            {
                plcAgent.APLCVehicle.MoveLeft = true;
            }
            else
            {
                plcAgent.APLCVehicle.MoveLeft = false;
            }
        }

        private void chkMoveRight_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveRight.Checked)
            {
                plcAgent.APLCVehicle.MoveRight = true;

            }
            else
            {
                plcAgent.APLCVehicle.MoveRight = false;
            }
        }

        private void pnlF_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.APLCVehicle.FrontBeamSensorDisable)
            {
                plcAgent.APLCVehicle.FrontBeamSensorDisable = false;
            }
            else
            {
                plcAgent.APLCVehicle.FrontBeamSensorDisable = true;
            }
        }

        private void pnlL_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.APLCVehicle.LeftBeamSensorDisable)
            {
                plcAgent.APLCVehicle.LeftBeamSensorDisable = false;
            }
            else
            {
                plcAgent.APLCVehicle.LeftBeamSensorDisable = true;
            }
        }

        private void pnlB_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.APLCVehicle.BackBeamSensorDisable)
            {
                plcAgent.APLCVehicle.BackBeamSensorDisable = false;
            }
            else
            {
                plcAgent.APLCVehicle.BackBeamSensorDisable = true;
            }
        }

        private void pnlR_DoubleClick(object sender, EventArgs e)
        {
            if (plcAgent.APLCVehicle.RightBeamSensorDisable)
            {
                plcAgent.APLCVehicle.RightBeamSensorDisable = false;
            }
            else
            {
                plcAgent.APLCVehicle.RightBeamSensorDisable = true;
            }
        }

        private void btnBeamSensorAutoSleepSet_Click(object sender, EventArgs e)
        {
            if (rdoBeamSensorAutoSleepEnable.Checked)
            {
                plcAgent.APLCVehicle.BeamSensorAutoSleep = true;
            }
            else
            {
                plcAgent.APLCVehicle.BeamSensorAutoSleep = false;
            }
        }



        private void btnSOCSet_Click(object sender, EventArgs e)
        {
            //plcAgent.APLCVehicle.Batterys.SetCcModeAh(plcAgent.APLCVehicle.Batterys.MeterAh + plcAgent.APLCVehicle.Batterys.AhWorkingRange * (100.0 - Convert.ToDouble(txtSOCSet.Text)) / 100.00, false);
            plcAgent.setSOC(Convert.ToDouble(txtSOCSet.Text));
        }

        private void lblEMO_DoubleClick(object sender, EventArgs e)
        {
            PlcEmo aPlcEMOSensor = (PlcEmo)((Label)sender).Tag;
            aPlcEMOSensor.Disable = !aPlcEMOSensor.Disable;
        }

        private enum ParamtTbxType
        {
            BatteryPV,
            BatterySV,
            ForkCommPV,
            ForkCommSV
        }
        //20190802_Rudy 新增XML Param 可修改   
        private void BatteryParamTbxFillToList(ref List<TextBox> tboxes, ParamtTbxType TbxType = ParamtTbxType.BatteryPV)
        {
            switch (TbxType)
            {
                case ParamtTbxType.BatteryPV:
                    tboxes.Add(tbxBatteryCellLowVoltage_PV);
                    tboxes.Add(tbxCCModeStopVoltage_PV);
                    tboxes.Add(tbxChargingOffDelay_PV);
                    tboxes.Add(tbxBatterysChargingTimeOut_PV);
                    tboxes.Add(tbxBatLoggerInterval_PV);
                    tboxes.Add(tbxAHWorkingRange_PV);
                    tboxes.Add(tbxMaxCCModeCounter_PV);
                    tboxes.Add(txtAutoChargeLowSOC_PV);
                    tboxes.Add(tbxResetAHTimeout_PV);
                    break;
                case ParamtTbxType.BatterySV:
                    tboxes.Add(tbxBatteryCellLowVoltage_SV);
                    tboxes.Add(tbxCCModeStopVoltage_SV);
                    tboxes.Add(tbxChargingOffDelay_SV);
                    tboxes.Add(tbxBatterysChargingTimeOut_SV);
                    tboxes.Add(tbxBatLoggerInterval_SV);
                    tboxes.Add(tbxAHWorkingRange_SV);
                    tboxes.Add(tbxMaxCCModeCounter_SV);
                    tboxes.Add(txtAutoChargeLowSOC_SV);
                    tboxes.Add(tbxResetAHTimeout_SV);
                    break;
                case ParamtTbxType.ForkCommPV:
                    tboxes.Add(tbxReadCassetteID_PV);
                    tboxes.Add(tbxCommReadTimeout_PV);
                    tboxes.Add(tbxCommBusyTimeout_PV);
                    tboxes.Add(tbxCommMovingTimeout_PV);
                    break;
                case ParamtTbxType.ForkCommSV:
                    tboxes.Add(new TextBox());
                    tboxes.Add(tbxCommReadTimeout_SV);
                    tboxes.Add(tbxCommBusyTimeout_SV);
                    tboxes.Add(tbxCommMovingTimeout_SV);
                    break;
            }
        }
        private void FillPVToBatteryParamTbx()
        {
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.BatteryPV);

            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxBatteryCellLowVoltage_PV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.Battery_Cell_Low_Voltage)).ToString();
                        break;
                    case "tbxCCModeStopVoltage_PV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.CCModeStopVoltage)).ToString();
                        break;
                    case "tbxChargingOffDelay_PV":
                        box.Text = (this.plcAgent.APLCVehicle.plcBatterys.Charging_Off_Delay).ToString();
                        break;
                    case "tbxBatterysChargingTimeOut_PV":
                        box.Text = (this.plcAgent.APLCVehicle.plcBatterys.Batterys_Charging_Time_Out / 60000).ToString();
                        break;
                    case "tbxBatLoggerInterval_PV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.Battery_Logger_Interval) / 1000).ToString();
                        break;
                    case "tbxAHWorkingRange_PV":
                        box.Text = this.plcAgent.APLCVehicle.plcBatterys.AhWorkingRange.ToString();
                        break;
                    case "tbxMaxCCModeCounter_PV":
                        box.Text = this.plcAgent.APLCVehicle.plcBatterys.MaxResetAhCcounter.ToString();
                        break;
                    case "txtAutoChargeLowSOC_PV":
                        box.Text = this.plcAgent.APLCVehicle.plcBatterys.PortAutoChargeLowSoc.ToString();
                        break;
                    case "tbxResetAHTimeout_PV":
                        box.Text = (this.plcAgent.APLCVehicle.plcBatterys.ResetAhTimeout / 1000).ToString();
                        break;
                }
            }
            liTextbox.Clear();
        }
        private void FillSVToBatteryParamTbx()
        {
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.BatterySV);

            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxBatteryCellLowVoltage_SV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.Battery_Cell_Low_Voltage)).ToString();
                        break;
                    case "tbxCCModeStopVoltage_SV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.CCModeStopVoltage)).ToString();
                        break;
                    case "tbxChargingOffDelay_SV":
                        box.Text = (this.plcAgent.APLCVehicle.plcBatterys.Charging_Off_Delay).ToString();
                        break;
                    case "tbxBatterysChargingTimeOut_SV":
                        box.Text = (this.plcAgent.APLCVehicle.plcBatterys.Batterys_Charging_Time_Out / 60000).ToString();
                        break;
                    case "tbxBatLoggerInterval_SV":
                        box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.Battery_Logger_Interval) / 1000).ToString();
                        break;
                    case "tbxAHWorkingRange_SV":
                        box.Text = this.plcAgent.APLCVehicle.plcBatterys.AhWorkingRange.ToString();
                        break;
                    case "tbxMaxCCModeCounter_SV":
                        box.Text = this.plcAgent.APLCVehicle.plcBatterys.MaxResetAhCcounter.ToString();
                        break;
                    case "txtAutoChargeLowSOC_SV":
                        box.Text = this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc.ToString();
                        break;
                    case "tbxResetAHTimeout_SV":
                        box.Text = (this.plcAgent.APLCVehicle.plcBatterys.ResetAhTimeout / 1000).ToString();
                        break;
                }
            }
            liTextbox.Clear();
        }
        private void FillPVToForkCommParamTbx()
        {
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.ForkCommPV);

            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxReadCassetteID_PV":
                        if (this.plcAgent.IsNeedReadCassetteID)
                            box.Text = "TRUE";
                        else
                            box.Text = "FALSE";
                        break;
                    case "tbxCommReadTimeout_PV":
                        box.Text = (this.plcAgent.ForkCommandReadTimeout / 1000).ToString();
                        break;
                    case "tbxCommBusyTimeout_PV":
                        box.Text = (this.plcAgent.ForkCommandBusyTimeout / 1000).ToString();
                        break;
                    case "tbxCommMovingTimeout_PV":
                        box.Text = (this.plcAgent.ForkCommandMovingTimeout / 1000).ToString();
                        break;
                }
            }
            liTextbox.Clear();
        }

        private void FillSVToForkCommParamTbx()
        {
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.ForkCommSV);

            if (this.plcAgent.IsNeedReadCassetteID)
            {
                chbCassetteID_SV.Checked = true;
                chbCassetteID_SV.Text = "TRUE";
            }
            else
            {
                chbCassetteID_SV.Checked = false;
                chbCassetteID_SV.Text = "FALSE";
            }

            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxCommReadTimeout_SV":
                        box.Text = (this.plcAgent.ForkCommandReadTimeout / 1000).ToString();
                        break;
                    case "tbxCommBusyTimeout_SV":
                        box.Text = (this.plcAgent.ForkCommandBusyTimeout / 1000).ToString();
                        break;
                    case "tbxCommMovingTimeout_SV":
                        box.Text = (this.plcAgent.ForkCommandMovingTimeout / 1000).ToString();
                        break;
                }
            }
            liTextbox.Clear();
        }
        private bool CheckBatteryParamSVInput()
        {
            bool result = true;
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.BatterySV);
            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxBatteryCellLowVoltage_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.Battery_Cell_Low_Voltage)).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxCCModeStopVoltage_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.CCModeStopVoltage)).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxChargingOffDelay_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.APLCVehicle.plcBatterys.Charging_Off_Delay).ToString();
                                result = false;
                            }
                        }
                        break;

                    case "tbxBatterysChargingTimeOut_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.APLCVehicle.plcBatterys.Batterys_Charging_Time_Out / 60000).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxBatLoggerInterval_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = (Convert.ToDouble(this.plcAgent.APLCVehicle.plcBatterys.Battery_Logger_Interval) / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxAHWorkingRange_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = this.plcAgent.APLCVehicle.plcBatterys.AhWorkingRange.ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxMaxCCModeCounter_SV":
                        {
                            if (!ushort.TryParse(box.Text, out ushort value))
                            {
                                box.Text = this.plcAgent.APLCVehicle.plcBatterys.MaxResetAhCcounter.ToString();
                                result = false;
                            }
                        }
                        break;
                    case "txtAutoChargeLowSOC_SV":
                        {
                            if (!double.TryParse(box.Text, out double value))
                            {
                                box.Text = this.plcAgent.APLCVehicle.Batterys.PortAutoChargeLowSoc.ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxResetAHTimeout_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.APLCVehicle.plcBatterys.ResetAhTimeout / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                }
            }
            liTextbox.Clear();
            return result;
        }
        private bool CheckForkCommParamSVInput()
        {
            bool result = true;
            List<TextBox> liTextbox = new List<TextBox>();
            BatteryParamTbxFillToList(ref liTextbox, ParamtTbxType.ForkCommSV);
            foreach (TextBox box in liTextbox)
            {
                switch (box.Name)
                {
                    case "tbxCommReadTimeout_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.ForkCommandReadTimeout / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxCommBusyTimeout_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.ForkCommandBusyTimeout / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                    case "tbxCommMovingTimeout_SV":
                        {
                            if (!uint.TryParse(box.Text, out uint value))
                            {
                                box.Text = (this.plcAgent.ForkCommandMovingTimeout / 1000).ToString();
                                result = false;
                            }
                        }
                        break;
                }
            }
            liTextbox.Clear();
            return result;
        }
        private void btnBatteryParamSet_Click(object sender, EventArgs e)
        {
            if (!CheckBatteryParamSVInput()) return;
            Dictionary<string, string> dicSetValue = new Dictionary<string, string>()
            {
                {"Battery_Cell_Low_Voltage",tbxBatteryCellLowVoltage_SV.Text},
                {"CCMode_Stop_Voltage",tbxCCModeStopVoltage_SV.Text},
                {"Charging_Off_Delay",tbxChargingOffDelay_SV.Text},
                {"Batterys_Charging_Time_Out",tbxBatterysChargingTimeOut_SV.Text},
                {"Battery_Logger_Interval",tbxBatLoggerInterval_SV.Text },
                {"SOC_AH",tbxAHWorkingRange_SV.Text },
                {"Ah_Reset_CCmode_Counter", tbxMaxCCModeCounter_SV.Text},
                {"Port_AutoCharge_Low_SOC",txtAutoChargeLowSOC_SV.Text},
                {"Ah_Reset_Timeout", tbxResetAHTimeout_SV.Text}
            };
            plcAgent.WritePlcConfigToXML(dicSetValue);

            FillPVToBatteryParamTbx();
            FillSVToBatteryParamTbx();
            dicSetValue.Clear();
        }

        private void btnForkCommParamSet_Click(object sender, EventArgs e)
        {
            if (!CheckForkCommParamSVInput()) return;
            string strReadCassetteID = "";
            if (chbCassetteID_SV.Checked) strReadCassetteID = "true"; else strReadCassetteID = "false";

            Dictionary<string, string> dicSetValue = new Dictionary<string, string>()
            {
                {"IsNeedReadCassetteID", strReadCassetteID},
                {"Fork_Command_Read_Timeout", tbxCommReadTimeout_SV.Text},
                {"Fork_Command_Busy_Timeout",tbxCommBusyTimeout_SV.Text},
                {"Fork_Command_Moving_Timeout", tbxCommMovingTimeout_SV.Text}
            };
            plcAgent.WritePlcConfigToXML(dicSetValue);
            FillPVToForkCommParamTbx();
            FillSVToForkCommParamTbx();
            dicSetValue.Clear();
        }
        private void chbCassetteID_SV_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked) ((CheckBox)sender).Text = "TRUE";
            else ((CheckBox)sender).Text = "FALSE";
        }

        private void btnFormHide_Click(object sender, EventArgs e)
        {
            Hide();
        }
        private bool bIPcStatusChange = false;
        private void labIPcStatusManual_Click(object sender, EventArgs e)
        {
            bIPcStatusChange = !bIPcStatusChange;
            if (bIPcStatusChange)
                Vehicle.Instance.AutoState = EnumAutoState.Manual;
            else
                Vehicle.Instance.AutoState = EnumAutoState.Auto;
        }

        private void chkFakeForking_CheckedChanged(object sender, EventArgs e)
        {
            if (this.plcAgent != null)
            {
                this.plcAgent.IsFakeForking = chkFakeForking.Checked;
            }
        }

        private void DirectionalLight(object sender, EventArgs e)
        {
            bool CheckedStatus = ((CheckBox)sender).Checked;
            switch (((CheckBox)sender).Name)
            {
                case "chkMoveFrontLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.Forward = true;
                    else plcAgent.APLCVehicle.Forward = false;
                    break;
                case "chkMoveBackLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.Backward = true;
                    else plcAgent.APLCVehicle.Backward = false;
                    break;
                case "chkSpinTurnLeftLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.SpinTurnLeft = true;
                    else plcAgent.APLCVehicle.SpinTurnLeft = false;
                    break;
                case "chkSpinTurnRightLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.SpinTurnRight = true;
                    else plcAgent.APLCVehicle.SpinTurnRight = false;
                    break;
                case "chkTraverseLeftLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.TraverseLeft = true;
                    else plcAgent.APLCVehicle.TraverseLeft = false;
                    break;
                case "chkTraverseRightLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.TraverseRight = true;
                    else plcAgent.APLCVehicle.TraverseRight = false;
                    break;
                case "chkSteeringFLLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.SteeringFL = true;
                    else plcAgent.APLCVehicle.SteeringFL = false;
                    break;
                case "chkSteeringFRLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.SteeringFR = true;
                    else plcAgent.APLCVehicle.SteeringFR = false;
                    break;
                case "chkSteeringBLLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.SteeringBL = true;
                    else plcAgent.APLCVehicle.SteeringBL = false;
                    break;
                case "chkSteeringBRLight":
                    if (CheckedStatus) plcAgent.APLCVehicle.SteeringBR = true;
                    else plcAgent.APLCVehicle.SteeringBR = false;
                    break;
            }
        }

        private void btnCstIDSet_Click(object sender, EventArgs e)
        {
            string strCstID = txtCassetteIDSet.Text;
            if (strCstID=="")
            {
                this.plcAgent.APLCVehicle.CarrierSlot.CarrierId = "";
            }
            else
            {
                this.plcAgent.testTriggerCassetteIDReader(ref strCstID);
            }
        }

        private void btnForce_ELMO_Servo_Off_Click(object sender, EventArgs e)
        {
            this.plcAgent.SetForcELMOServoOffOn();
        }

        private void chkBeamSensorDisableNormalSpeed_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBeamSensorDisableNormalSpeed.Checked)
            {
                this.plcAgent.APLCVehicle.BeamSensorDisableNormalSpeed = true;
            }
            else
            {
                this.plcAgent.APLCVehicle.BeamSensorDisableNormalSpeed = false;
            }
        }

        private void btnSimulationPLCConnect_Click(object sender, EventArgs e)
        {
            btnSimulationPLCConnect.Enabled = false;
            this.plcAgent.SimulationPLCConnect();
        }

        private void btnSaveBeamSensorDisableNormalSpeed_Click(object sender, EventArgs e)
        {

            string strSaveBeamSensorDisableNormalSpeed = "";
            if (chkBeamSensorDisableNormalSpeed.Checked) strSaveBeamSensorDisableNormalSpeed = "true"; else strSaveBeamSensorDisableNormalSpeed = "false";


            Dictionary<string, string> dicSetValue = new Dictionary<string, string>()
            {
                {"Beam_Sensor_Disable_Normal_Speed",strSaveBeamSensorDisableNormalSpeed},
                
            };
            plcAgent.WritePlcConfigToXML(dicSetValue);
        }

        private void btnFakeInterlockError_Click(object sender, EventArgs e)
        {
            plcAgent.triggerForkCommandInterlockErrorEvent();
        }

        private void ckbHmiThreadControl_CheckedChanged(object sender, EventArgs e)
        {
            plcAgent.plcOperationRun_ThreadControl(ckbHmiThreadControl.Checked);
        }

    }
}
