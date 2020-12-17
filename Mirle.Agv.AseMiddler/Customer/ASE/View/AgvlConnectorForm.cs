using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Controller;
using PSDriver.PSDriver;
using Mirle.Tools;
using System.Reflection;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AgvlConnectorForm : Form
    {
        public string PspLogMsg { get; set; } = "";
        private AsePackage asePackage;

        //private MirleLogger mirleLogger = MirleLogger.Instance;
        private Vehicle Vehicle { get; set; } = Vehicle.Instance;
        private string deviceID = Vehicle.Instance.AgvcConnectorConfig.ClientName;
        public MapInfo MapInfo { get; set; }

        public AgvlConnectorForm(AsePackage asePackage, MapInfo mapInfo)
        {
            InitializeComponent();
            this.asePackage = asePackage;
            this.MapInfo = mapInfo;
            LoadPspConfig();
            InitialBoxPspMessageMap();
            InitialBoxPsMessageType();
            InitialBoxKeepOrGo();
            InitialBoxIsEnd();
            InitialBoxSlotOpen();
            InitialUcAskPositionIntervalMs();
            InitialBoxPioDirection();
            InitialBoxLDUD1();
            InitialBoxLDUD2();
            IniialBoxSlotSelect1();
            IniialBoxSlotSelect2();
            InitialBoxChargeDirection();
            InitialUcWatchSocIntervalMs();
        }

        private void InitialUcWatchSocIntervalMs()
        {
            ucWatchSocIntervalMs.TagValue = Vehicle.AseBatteryConfig.WatchBatteryStateInterval.ToString();
        }

        private void InitialBoxChargeDirection()
        {
            boxChargeDirection.DataSource = Enum.GetValues(typeof(EnumAddressDirection));
            boxChargeDirection.SelectedIndex = (int)EnumAddressDirection.Right;
        }

        private void IniialBoxSlotSelect2()
        {
            boxSlotSelect2.DataSource = Enum.GetValues(typeof(EnumSlotSelect));
            boxSlotSelect2.SelectedIndex = (int)EnumSlotSelect.Left;
        }

        private void IniialBoxSlotSelect1()
        {
            boxSlotSelect1.DataSource = Enum.GetValues(typeof(EnumSlotSelect));
            boxSlotSelect1.SelectedIndex = (int)EnumSlotSelect.Left;
        }

        private void InitialBoxLDUD2()
        {
            boxLDUD2.DataSource = Enum.GetValues(typeof(EnumLDUD));
            boxLDUD2.SelectedIndex = (int)EnumLDUD.None;
        }

        private void InitialBoxLDUD1()
        {
            boxLDUD1.DataSource = Enum.GetValues(typeof(EnumLDUD));
            boxLDUD1.SelectedIndex = (int)EnumLDUD.LD;
        }

        private void InitialBoxPioDirection()
        {
            boxPioDirection.DataSource = Enum.GetValues(typeof(EnumAddressDirection));
            boxPioDirection.SelectedIndex = (int)EnumAddressDirection.Right;
        }

        private void InitialUcAskPositionIntervalMs()
        {
            ucAskPositionInterval.TagValue = Vehicle.AseMoveConfig.WatchPositionInterval.ToString();
        }

        private void InitialBoxKeepOrGo()
        {
            boxKeepOrGo.DataSource = Enum.GetValues(typeof(EnumIsExecute));
            boxKeepOrGo.SelectedIndex = (int)EnumIsExecute.Go;
        }

        private void InitialBoxIsEnd()
        {
            boxIsEnd.DataSource = Enum.GetValues(typeof(EnumAseMoveCommandIsEnd));
            boxIsEnd.SelectedIndex = (int)EnumAseMoveCommandIsEnd.End;
        }

        private void InitialBoxSlotOpen()
        {
            boxSlotOpen.DataSource = Enum.GetValues(typeof(EnumSlotSelect));
            boxSlotOpen.SelectedIndex = (int)EnumSlotSelect.None;
        }

        private void LoadPspConfig()
        {
            try
            {
                var localConfig = Vehicle.PspConnectionConfig;
                ucIp.TagValue = localConfig.Ip;
                ucPort.TagValue = localConfig.Port.ToString();
                ucIsServer.TagValue = localConfig.IsServer ? "Server" : "Client";
                ucT3Timeout.TagValue = localConfig.T3Timeout.ToString();
                ucT6Timeout.TagValue = localConfig.T6Timeout.ToString();
                ucLinkTestIntervalMs.TagValue = localConfig.LinkTestIntervalMs.ToString();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateInfoPage();
            UpdateMovePage();
            UpdateRobotPage();
            UpdateChargePage();
            lock (asePackage.SbPsWrapperMsg)
            {
                textBox1.Text = asePackage.SbPsWrapperMsg.ToString();
            }            
        }

        private void UpdateRobotPage()
        {
            try
            {
                AseRobotStatus robotStatus = new AseRobotStatus(Vehicle.AseRobotStatus);
                AseCarrierSlotStatus slotStatusL = new AseCarrierSlotStatus(Vehicle.AseCarrierSlotL);
                AseCarrierSlotStatus slotStatusR = new AseCarrierSlotStatus(Vehicle.AseCarrierSlotR);

                ucRobotRobotState.TagValue = robotStatus.RobotState.ToString();
                ucRobotIsHome.TagValue = robotStatus.IsHome.ToString();

                ucRobotSlotLState.TagValue = slotStatusL.CarrierSlotStatus.ToString();
                ucRobotSlotLId.TagValue = slotStatusL.CarrierId;

                ucRobotSlotRState.TagValue = slotStatusR.CarrierSlotStatus.ToString();
                ucRobotSlotRId.TagValue = slotStatusR.CarrierId;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateChargePage()
        {
            try
            {
                AseBatteryStatus batteryStatus = new AseBatteryStatus(Vehicle.AseBatteryStatus);
                ucBatteryPercentage.TagValue = batteryStatus.Percentage.ToString();
                ucBatteryVoltage.TagValue = batteryStatus.Voltage.ToString("00.00");
                ucBatteryTemperature.TagValue = batteryStatus.Temperature.ToString();
                ucBatteryCharging.TagValue = Vehicle.IsCharging ? "Yes" : "No";
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateMovePage()
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                AseMovingGuide aseMovingGuide = new AseMovingGuide(Vehicle.AseMovingGuide);

                var lastPos = aseMoveStatus.LastMapPosition;
                string lastPosX = lastPos.X.ToString("F2");
                ucMovePositionX.TagValue = lastPosX;
                string lastPosY = lastPos.Y.ToString("F2");
                ucMovePositionY.TagValue = lastPosY;

                var lastAddress = aseMoveStatus.LastAddress;
                ucMoveLastAddress.TagValue = lastAddress.Id;

                var lastSection = aseMoveStatus.LastSection;
                ucMoveLastSection.TagValue = lastSection.Id;

                ucMoveIsMoveEnd.TagValue = aseMoveStatus.IsMoveEnd.ToString();

                ucMoveMoveState.TagValue = aseMoveStatus.AseMoveState.ToString();

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateInfoPage()
        {
            try
            {
                if (Vehicle.IsLocalConnect)
                {
                    txtConnection.Text = "Online";
                    txtConnection.BackColor = Color.YellowGreen;
                }
                else
                {
                    txtConnection.Text = "Offline";
                    txtConnection.BackColor = Color.Pink;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }

        private void InitialBoxPspMessageMap()
        {
            try
            {
                if (asePackage.psMessageMap.Count > 0)
                {
                    foreach (var item in asePackage.psMessageMap)
                    {
                        string pspMessageListItem = string.Concat(item.Key, ",", item.Value.Description);
                        boxPspMessageMap.Items.Add(pspMessageListItem);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitialBoxPsMessageType()
        {
            boxPsMessageType.DataSource = Enum.GetValues(typeof(PsMessageType));
            boxPsMessageType.SelectedItem = PsMessageType.P;
        }

        private void btnSingleMessageSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (boxPsMessageType.Text.Equals("P"))
                {
                    string number = numPsMessageNumber.Value.ToString("00");
                    asePackage.PrimarySendEnqueue("P" + number, txtPsMessageText.Text);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void boxPspMessageMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedItem = boxPspMessageMap.SelectedItem.ToString().Split(',')[0];
                PSMessageXClass pspMessage = asePackage.psMessageMap[selectedItem];
                boxPsMessageType.SelectedItem = pspMessage.Type == "P" ? PsMessageType.P : PsMessageType.S;
                numPsMessageNumber.Value = decimal.Parse(pspMessage.Number);
                txtPsMessageText.Text = pspMessage.PSMessage;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnSaveAutoReplyMessage_Click(object sender, EventArgs e)
        {
            try
            {
                string messageType = boxPsMessageType.Text.Trim();
                string number = numPsMessageNumber.Value.ToString("00");
                string index = messageType + number;
                string messageText = txtPsMessageText.Text;
                if (asePackage.psMessageMap.ContainsKey(index))
                {
                    asePackage.psMessageMap[index].PSMessage = messageText;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnLoadLocalConfig_Click(object sender, EventArgs e)
        {
            LoadPspConfig();
        }

        private void btnSearchMapAddress_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMapAddress.Text.Trim())) return;

                string addressId = txtMapAddress.Text.Trim();

                if (MapInfo.addressMap.ContainsKey(addressId))
                {
                    MapAddress mapAddress = Vehicle.Mapinfo.addressMap[addressId];
                    MapAddressFitMoveCommand(mapAddress);
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void MapAddressFitMoveCommand(MapAddress mapAddress)
        {
            numMovePositionX.Value = Convert.ToDecimal(mapAddress.Position.X);
            numMovePositionY.Value = Convert.ToDecimal(mapAddress.Position.Y);
            numHeadAngle.Value = Convert.ToDecimal((int)mapAddress.VehicleHeadAngle);
        }

        private void btnSendMove_Click(object sender, EventArgs e)
        {
            try
            {
                MapPosition mapPosition = new MapPosition(decimal.ToDouble(numMovePositionX.Value), decimal.ToDouble(numMovePositionY.Value));
                int headAngle = decimal.ToInt32(numHeadAngle.Value);
                int speed = decimal.ToInt32(numMoveSpeed.Value);
                EnumAseMoveCommandIsEnd isEnd = (EnumAseMoveCommandIsEnd)boxIsEnd.SelectedItem;
                EnumIsExecute isExecute = (EnumIsExecute)boxKeepOrGo.SelectedItem;
                EnumSlotSelect slotOpen = (EnumSlotSelect)boxSlotOpen.SelectedItem;

                asePackage.PartMove(mapPosition, headAngle, speed, isEnd, isExecute, slotOpen);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnSendRobot_Click(object sender, EventArgs e)
        {
            try
            {
                string pioDirection = boxPioDirection.SelectedIndex.ToString("0");
                string gateType = numGateType.Value.ToString("0");
                EnumLDUD loadunload1 = (EnumLDUD)boxLDUD1.SelectedItem;
                EnumSlotSelect slotSelect1 = (EnumSlotSelect)boxSlotSelect1.SelectedItem;
                if (slotSelect1 == EnumSlotSelect.None || slotSelect1 == EnumSlotSelect.Both) return;
                string slot1 = slotSelect1 == EnumSlotSelect.Left ? "0000L" : "0000R";
                string portNumber1 = numPortNumber1.Value.ToString("0");
                string addressId1 = txtAddressId1.Text.Trim().PadLeft(5, '0').Substring(0, 5);

                string robotCommandInfo = "";

                switch (loadunload1)
                {
                    case EnumLDUD.LD:
                        {
                            robotCommandInfo = string.Concat(pioDirection, addressId1, slot1, gateType, portNumber1);
                        }
                        break;
                    case EnumLDUD.UD:
                        {
                            robotCommandInfo = string.Concat(pioDirection, slot1, addressId1, gateType, portNumber1);
                        }
                        break;
                    case EnumLDUD.None:
                    default:
                        return;
                }

                EnumLDUD loadunload2 = (EnumLDUD)boxLDUD2.SelectedItem;
                EnumSlotSelect slotSelect2 = (EnumSlotSelect)boxSlotSelect2.SelectedItem;
                if (slotSelect2 == EnumSlotSelect.Both) return;
                string slot2 = slotSelect2 == EnumSlotSelect.Left ? "0000L" : "0000R";
                string portNumber2 = numPortNumber2.Value.ToString("0");
                string addressId2 = txtAddressId2.Text.Trim().PadLeft(5, '0').Substring(0, 5);

                switch (loadunload2)
                {
                    case EnumLDUD.LD:
                        {
                            robotCommandInfo = string.Concat(robotCommandInfo, addressId2, slot2, portNumber2);
                        }
                        break;
                    case EnumLDUD.UD:
                        {
                            robotCommandInfo = string.Concat(robotCommandInfo, slot2, addressId2, portNumber2);
                        }
                        break;
                    case EnumLDUD.None:
                        {
                            robotCommandInfo = robotCommandInfo.PadRight(24, '0');
                        }
                        break;
                }

                asePackage.DoRobotCommand(robotCommandInfo);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnRefreshRobotState_Click(object sender, EventArgs e)
        {
            RefreshStatus(sender);
        }

        private void RefreshStatus(object sender)
        {
            try
            {
                var btn = sender as Button;
                btn.Enabled = false;
                asePackage.AllAgvlStatusReportRequest();
                System.Threading.SpinWait.SpinUntil(() => false, 50);
                btn.Enabled = true;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnRefreshBatterySate_Click(object sender, EventArgs e)
        {
            RefreshStatus(sender);
        }

        private void btnRefreshPosition_Click(object sender, EventArgs e)
        {
            RefreshStatus(sender);
        }

        private void btnSearchChargeAddress_Click(object sender, EventArgs e)
        {
            try
            {
                string addressId = txtChargeAddress.Text;
                if (MapInfo.addressMap.ContainsKey(addressId))
                {
                    boxChargeDirection.SelectedItem = Vehicle.Mapinfo.addressMap[addressId].ChargeDirection.ToString();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnStartCharge_Click(object sender, EventArgs e)
        {
            try
            {
                EnumAddressDirection chargeDirection = (EnumAddressDirection)boxChargeDirection.SelectedItem;

                asePackage.StartCharge(chargeDirection);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnStopCharge_Click(object sender, EventArgs e)
        {
            try
            {
                asePackage.StopCharge();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private NLog.Logger _PsWrapperLogger = NLog.LogManager.GetLogger("PsWrapper");


        private void LogException(string classMethodName, string exMsg)
        {
            _PsWrapperLogger.Error($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{classMethodName}][{exMsg}]");

        }
    }
}
