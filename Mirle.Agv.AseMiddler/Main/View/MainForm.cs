using Mirle.Agv.AseMiddler.Controller;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using Mirle.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class MainForm : Form
    {
        public MainFlowHandler mainFlowHandler;
        public MainForm mainForm;
        private AsePackage asePackage;
        private AgvcConnector agvcConnector;
        private AgvcConnectorForm agvcConnectorForm;
        private AlarmForm alarmForm;
        private AlarmHandler alarmHandler;
        private AgvlConnectorForm AgvlConnectorForm;
        private WarningForm warningForm;
        private ConfigForm configForm;
        private LoginForm loginForm;
        private Panel panelLeftUp;
        private Panel panelLeftDown;
        //PerformanceCounter performanceCounterCpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
        //PerformanceCounter performanceCounterRam = new PerformanceCounter("Memory", "Available MBytes");
        //PerformanceCounter performanceCounterRam = new PerformanceCounter("Memory", "% Committed Bytes in Use");
        private MirleLogger mirleLogger = MirleLogger.Instance;
        private Vehicle Vehicle = Vehicle.Instance;
        public bool IsEnableStartChargeButton { get; set; } = false;
        public bool IsEnableStopChargeButton { get; set; } = false;

        private List<TextBox> txtTransferCommands = new List<TextBox>();


        #region PaintingItems
        private Image image;
        private Graphics gra;
        private Dictionary<string, Pen> allPens = new Dictionary<string, Pen>();

        private Dictionary<string, UcSectionImage> allUcSectionImages = new Dictionary<string, UcSectionImage>();
        private Dictionary<string, UcAddressImage> allUcAddressImages = new Dictionary<string, UcAddressImage>();
        private double coefficient = 0.05f;
        private double deltaOrigion = 50;
        private UcVehicleImage ucVehicleImage = new UcVehicleImage();
        private MapPosition minPos = new MapPosition();
        private MapPosition maxPos = new MapPosition();
        private MapPosition tmpO = new MapPosition();
        private int mapYOffset = 0;

        private Point mouseDownPbPoint;
        private Point mouseDownScreenPoint;

        #endregion

        public MainForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
            alarmHandler = mainFlowHandler.alarmHandler;
            asePackage = mainFlowHandler.asePackage;
            agvcConnector = mainFlowHandler.agvcConnector;
            mainForm = this;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitialForms();
            InitialPaintingItems();
            InitialPanels();
            ResetImageAndPb();
            InitialSoc();
            asePackage.AllAgvlStatusReportRequest();
            asePackage.SendPositionReportRequest();
            asePackage.SendBatteryStatusRequest();
            InitialConnectionAndCarrierStatus();
            InitialDisableSlotCheckBox();
            btnKeyInPosition.Visible = Vehicle.MainFlowConfig.IsSimulation;
            btnKeyInSoc.Visible = Vehicle.MainFlowConfig.IsSimulation;
            txtLastAlarm.Text = "";
            txtTransferCommands = new List<TextBox>() { txtTransferCommand01, txtTransferCommand02, txtTransferCommand03, txtTransferCommand04 };
            var msg = "MainForm : 讀取主畫面";
            LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, msg);
        }

        private void InitialDisableSlotCheckBox()
        {
            var disable = Vehicle.MainFlowConfig.SlotDisable;
            switch (disable)
            {
                case EnumSlotSelect.None:
                    {
                        checkBoxDisableLeftSlot.Checked = false;
                        checkBoxDisableRightSlot.Checked = false;
                    }
                    break;
                case EnumSlotSelect.Left:
                    {
                        checkBoxDisableLeftSlot.Checked = true;
                        checkBoxDisableRightSlot.Checked = false;
                    }
                    break;
                case EnumSlotSelect.Right:
                    {
                        checkBoxDisableLeftSlot.Checked = false;
                        checkBoxDisableRightSlot.Checked = true;
                    }
                    break;
                case EnumSlotSelect.Both:
                    {
                        checkBoxDisableLeftSlot.Checked = true;
                        checkBoxDisableRightSlot.Checked = true;
                    }
                    break;
                default:
                    {
                        checkBoxDisableLeftSlot.Checked = false;
                        checkBoxDisableRightSlot.Checked = false;
                    }
                    break;
            }
        }

        private void InitialForms()
        {
            agvcConnectorForm = new AgvcConnectorForm(agvcConnector);
            agvcConnectorForm.WindowState = FormWindowState.Normal;
            agvcConnectorForm.Show();
            agvcConnectorForm.Hide();

            configForm = new ConfigForm();
            configForm.WindowState = FormWindowState.Normal;
            configForm.Show();
            configForm.Hide();

            var middlerConfig = Vehicle.AgvcConnectorConfig;
            tstextClientName.Text = $"[{ middlerConfig.ClientName}]";
            tstextRemoteIp.Text = $"[{middlerConfig.RemoteIp}]";
            tstextRemotePort.Text = $"[{middlerConfig.RemotePort}]";
            this.Text = $"主畫面 版本編號為[{Application.ProductVersion}]";

            alarmForm = new AlarmForm(mainFlowHandler);
            alarmForm.WindowState = FormWindowState.Normal;
            alarmForm.Show();
            alarmForm.Hide();

            warningForm = new WarningForm();
            warningForm.WindowState = FormWindowState.Normal;
            warningForm.Show();
            warningForm.Hide();

            numPositionX.Maximum = decimal.MaxValue;
            numPositionY.Maximum = decimal.MaxValue;

            InitialAseAgvlConnectorForm();
            InitialLoginForm();

        }

        private void InitialPaintingItems()
        {
            Pen aPen;
            aPen = new Pen(Color.Blue, 2);
            allPens.Add("Blue2", aPen);
            aPen = new Pen(Color.Black, 2);
            allPens.Add("Black2", aPen);
            aPen = new Pen(Color.Red, 2);
            allPens.Add("Red2", aPen);
            aPen = new Pen(Color.Green, 2);
            allPens.Add("Green2", aPen);
            aPen = new Pen(Color.YellowGreen, 2);
            allPens.Add("YellowGreen2", aPen);
            aPen = new Pen(Color.Black, 1);
            aPen.DashStyle = DashStyle.DashDot;
            allPens.Add("BlackDashDot1", aPen);

            aPen = new Pen(Color.Red, 4);
            allPens.Add("NotGetReserveSection", aPen);

            aPen = new Pen(Color.Green, 4);
            allPens.Add("GetReserveSection", aPen);

            aPen = new Pen(Color.Blue);
            allPens.Add("NormalSection", aPen);

            ucVehicleImage.Parent = pictureBox1;
            ucVehicleImage.BringToFront();
            ucVehicleImage.Show();
        }

        private void InitialPanels()
        {
            panelLeftUp = splitContainer3.Panel1;
            panelLeftUp.HorizontalScroll.Enabled = true;
            panelLeftUp.VerticalScroll.Enabled = true;

            panelLeftDown = splitContainer3.Panel2;
            panelLeftDown.HorizontalScroll.Enabled = true;
            panelLeftDown.VerticalScroll.Enabled = true;
        }

        private void InitialSoc()
        {
            timer_SetupInitialSoc.Enabled = true;
        }

        private void InitialConnectionAndCarrierStatus()
        {
            //IsAgvcConnect = agvcConnector.IsConnected();
            UpdateAgvcConnection();

            UpdateAgvlConnection();

            if (Vehicle.AseCarrierSlotL.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading || Vehicle.AseCarrierSlotR.CarrierSlotStatus == EnumAseCarrierSlotStatus.Loading)
            {
                mainFlowHandler.ReadCarrierId();
            }
        }

        public void DrawBasicMap()
        {
            try
            {
                SetupImageRegion();

                // Draw Sections in blueLine
                allUcSectionImages.Clear();

                var sectionMap = Vehicle.Mapinfo.sectionMap.Values.ToList();
                foreach (var section in sectionMap)
                {
                    var headPos = section.HeadAddress.Position;
                    var tailPos = section.TailAddress.Position;

                    MapPosition sectionLocation = new MapPosition(Math.Min(headPos.X, tailPos.X), Math.Max(headPos.Y, tailPos.Y));//200310 dabid#

                    UcSectionImage ucSectionImage = new UcSectionImage(Vehicle.Mapinfo, section);
                    if (!allUcSectionImages.ContainsKey(section.Id))
                    {
                        allUcSectionImages.Add(section.Id, ucSectionImage);
                    }
                    pictureBox1.Controls.Add(ucSectionImage);
                    ucSectionImage.Location = MapPixelExchange(sectionLocation);
                    switch (section.Type)
                    {
                        case EnumSectionType.Horizontal:
                            break;
                        case EnumSectionType.Vertical:
                            ucSectionImage.Location = new Point(ucSectionImage.Location.X - (ucSectionImage.labelSize.Width / 2 + 5), ucSectionImage.Location.Y);
                            break;
                        case EnumSectionType.R2000:
                        case EnumSectionType.None:
                        default:
                            break;
                    }

                    ucSectionImage.BringToFront();

                    ucSectionImage.MouseDown += UcSectionImage_MouseDown;
                    ucSectionImage.label1.MouseDown += UcSectionImageItem_MouseDown;
                    ucSectionImage.pictureBox1.MouseDown += UcSectionImageItem_MouseDown;
                }

                //Draw Addresses in BlackRectangle(Segment) RedCircle(Port) RedTriangle(Charger)
                allUcAddressImages.Clear();

                var addressMap = Vehicle.Mapinfo.addressMap.Values.ToList();
                foreach (var address in addressMap)
                {
                    UcAddressImage ucAddressImage = new UcAddressImage(Vehicle.Mapinfo, address);
                    if (!allUcAddressImages.ContainsKey(address.Id))
                    {
                        allUcAddressImages.Add(address.Id, ucAddressImage);
                    }
                    pictureBox1.Controls.Add(ucAddressImage);
                    ucAddressImage.Location = MapPixelExchange(address.Position);
                    ucAddressImage.FixToCenter();
                    ucAddressImage.BringToFront();
                    Label label = new Label();
                    label.AutoSize = false;
                    label.Size = new Size(35, 12);
                    label.Parent = pictureBox1;
                    label.Text = address.Id;
                    label.Location = new Point(ucAddressImage.Location.X, ucAddressImage.Location.Y + 2 * (ucAddressImage.Radius + 1));
                    label.BringToFront();


                    ucAddressImage.MouseDown += UcAddressImage_MouseDown;
                    //ucAddressImage.label1.MouseDown += UcAddressImageItem_MouseDown;
                    ucAddressImage.pictureBox1.MouseDown += UcAddressImageItem_MouseDown;
                    //ucAddressImage.pictureBox1.MouseDoubleClick += ucAddressImageItem_DoubleClick;
                }

                pictureBox1.SendToBack();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void SetupImageRegion()
        {
            #region 200318 dabid+ 
            //MapPosition tmpO = new MapPosition();
            var tmp = Vehicle.Mapinfo.addressMap.Values.FirstOrDefault();
            tmpO = tmp.Position;
            MapPosition tmpMaxY = new MapPosition();
            MapPosition tmpMinY = new MapPosition();
            foreach (var addr in Vehicle.Mapinfo.addressMap.Values)
            {
                if (addr.Position.X * addr.Position.X + addr.Position.Y * addr.Position.Y < tmpO.X * tmpO.X + tmpO.Y * tmpO.Y)
                {
                    tmpO = addr.Position;
                }
            }
            tmp = Vehicle.Mapinfo.addressMap.Values.FirstOrDefault();
            tmpMaxY.Y = tmpMinY.Y = tmp.Position.Y;
            foreach (var address in Vehicle.Mapinfo.addressMap.Values)
            {
                if (Math.Abs(address.Position.Y - tmpO.Y) > Math.Abs(tmpMaxY.Y - tmpO.Y))
                {
                    tmpMaxY = address.Position;
                }
                if (Math.Abs(address.Position.Y - tmpO.Y) < Math.Abs(tmpMinY.Y - tmpO.Y))
                {
                    tmpMinY = address.Position;
                }
            }
            mapYOffset = (Int32)Math.Abs(tmpMaxY.Y - tmpMinY.Y);
            #endregion

            double xMax = Vehicle.Mapinfo.addressMap.Values.ToList().Max(addr => addr.Position.X);
            double xMin = Vehicle.Mapinfo.addressMap.Values.ToList().Min(addr => addr.Position.X);
            double yMax = Vehicle.Mapinfo.addressMap.Values.ToList().Max(addr => addr.Position.Y);
            double yMin = Vehicle.Mapinfo.addressMap.Values.ToList().Min(addr => addr.Position.Y);
            maxPos.X = xMax;//200318 dabid+
            maxPos.Y = yMax;//200318 dabid+
            minPos.X = xMin;//200318 dabid+
            minPos.Y = yMin;//200318 dabid+
            var maxPosInPixel = MapPixelExchange(new MapPosition(xMax, yMax));
            var minPosInPixel = MapPixelExchange(new MapPosition(xMin, yMin));
            Point point = new Point(100 + Math.Abs(maxPosInPixel.X - minPosInPixel.X), 100 + Math.Abs(maxPosInPixel.Y - minPosInPixel.Y));
            pictureBox1.Size = new Size(point);
            image = new Bitmap(point.X, point.Y, PixelFormat.Format32bppArgb);
            gra = Graphics.FromImage(image);
        }
        public Point MapPixelExchange(MapPosition position)
        {
            var pixelX = Convert.ToInt32(((position.X - minPos.X)) * coefficient + deltaOrigion);//200318 dabid#
            var pixelY = Convert.ToInt32(((position.Y - minPos.Y) * (-1) + mapYOffset) * coefficient + deltaOrigion);//200318 dabid#

            return new Point(pixelX, pixelY);
        }

        public MapPosition MapPositionExchange(MapPosition pixel)
        {
            var posX = (pixel.X - deltaOrigion) / coefficient + minPos.X;// 200318 dabid#
            var posY = ((pixel.Y - deltaOrigion) / coefficient - mapYOffset) / (-1) + minPos.Y;// 200318 dabid#

            return new MapPosition(posX, posY);
        }

        private void UcAddressImageItem_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = ((Control)sender).Parent;
            UcAddressImage ucAddressImage = (UcAddressImage)control;
            mouseDownPbPoint = MapPixelExchange(ucAddressImage.Address.Position);
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void UcAddressImage_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            UcAddressImage ucAddressImage = (UcAddressImage)control;
            mouseDownPbPoint = MapPixelExchange(ucAddressImage.Address.Position);
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void UcSectionImageItem_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            Control parentControl = control.Parent;
            Point point = new Point(e.X + parentControl.Location.X, e.Y + parentControl.Location.Y);
            mouseDownPbPoint = point;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();

        }

        private void UcSectionImage_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            Point point = new Point(e.X + control.Location.X, e.Y + control.Location.Y);
            mouseDownPbPoint = point;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Environment.Exit(Environment.ExitCode);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AgvlConnectorPage_Click(object sender, EventArgs e)
        {
            if (AgvlConnectorForm.IsDisposed)
            {
                InitialAseAgvlConnectorForm();
            }
            if (AgvlConnectorForm != null)
            {
                AgvlConnectorForm.BringToFront();
                AgvlConnectorForm.Show();
            }
        }

        private void InitialAseAgvlConnectorForm()
        {
            AgvlConnectorForm = new AgvlConnectorForm(asePackage, Vehicle.Mapinfo);
        }

        private void AgvcConnectorPage_Click(object sender, EventArgs e)
        {
            if (agvcConnectorForm.IsDisposed)
            {
                agvcConnectorForm = new AgvcConnectorForm(agvcConnector);
            }
            agvcConnectorForm.BringToFront();
            agvcConnectorForm.Show();

        }

        private void btnRefreshStatus_Click(object sender, EventArgs e)
        {
            btnRefreshStatus.Enabled = false;
            asePackage.AllAgvlStatusReportRequest();
            SpinWait.SpinUntil(() => false, 50);
            asePackage.SendPositionReportRequest();
            SpinWait.SpinUntil(() => false, 50);
            asePackage.SendBatteryStatusRequest();
            btnRefreshStatus.Enabled = true;
        }

        private void btnRefreshMoveState_Click(object sender, EventArgs e)
        {
            btnRefreshMoveState.Enabled = false;

            asePackage.SendPositionReportRequest();

            SpinWait.SpinUntil(() => false, 50);

            asePackage.RefreshMoveState();

            btnRefreshMoveState.Enabled = true;
        }

        private void btnRefreshRobotState_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshRobotState.Enabled = false;

                asePackage.RefreshRobotState();

                SpinWait.SpinUntil(() => false, 50);

                asePackage.RefreshCarrierSlotState();

                btnRefreshRobotState.Enabled = true;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AseRobotControlForm_RefreshBatteryState(object sender, EventArgs e)
        {
            try
            {
                var btn = sender as Button;
                btn.Enabled = false;
                asePackage.AllAgvlStatusReportRequest();
                System.Threading.Thread.Sleep(50);
                btn.Enabled = true;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AlarmPage_Click(object sender, EventArgs e)
        {
            if (alarmForm.IsDisposed)
            {
                alarmForm = new AlarmForm(mainFlowHandler);
            }
            alarmForm.BringToFront();
            alarmForm.Show();
        }

        private void VehicleStatusPage_Click(object sender, EventArgs e)
        {
            if (configForm.IsDisposed)
            {
                configForm = new ConfigForm();
            }
            configForm.BringToFront();
            configForm.Show();
        }

        public delegate void DelRenewUI(Control control, string msg);
        public void RenewUI(Control control, string msg)
        {
            if (this.InvokeRequired)
            {
                DelRenewUI del = new DelRenewUI(RenewUI);
                del(control, msg);
            }
            else
            {
                control.Text = msg;
            }
        }

        #region Image Functions

        private void SetpupVehicleLocation()
        {
            var mdpx = mouseDownPbPoint.X;
            var mdpy = mouseDownPbPoint.Y;
            var mouseDownPointInPosition = MapPositionExchange(new MapPosition(mouseDownPbPoint.X, mouseDownPbPoint.Y));

            numPositionX.Value = (decimal)mouseDownPointInPosition.X;
            numPositionY.Value = (decimal)mouseDownPointInPosition.Y;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPbPoint = e.Location;
            mouseDownScreenPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));
            SetpupVehicleLocation();
        }

        private void PbLoadImage()
        {
            pictureBox1.Image = image;
        }

        public void ResetImageAndPb()
        {
            DrawBasicMap();
            PbLoadImage();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetImageAndPb();

            RenewUI(this, "Reset");
        }

        #endregion

        private void timeUpdateUI_Tick(object sender, EventArgs e)
        {
            try
            {
                //UpdatePerformanceCounter(performanceCounterCpu, ucPerformanceCounterCpu);
                //UpdatePerformanceCounter(performanceCounterRam, ucPerformanceCounterRam);

                tbxDebugLogMsg.Text = mainFlowHandler.DebugLogMsg;

                ucSoc.TagValue = Vehicle.AseBatteryStatus.Percentage.ToString("F1") + $"/" + Vehicle.AseBatteryStatus.Voltage.ToString("F2");

                UpdateListBoxSections(lbxNeedReserveSections, agvcConnector.GetNeedReserveSections());
                UpdateListBoxSections(lbxReserveOkSections, agvcConnector.GetReserveOkSections());

                UpdateLoginLevel();
                UpdateAutoManual();
                UpdateVehLocation();
                UpdateBatteryState();
                UpdateRobotAndCarrierSlotState();
                UpdateGroupBoxFlowStatus();
                UpdateTbxAgvcTransCmd();
                UpdateTbxTransferStep();
                UpdateLastAlarm();
                UpdateAgvcConnection();
                UpdateAgvlConnection();
                UpdateReserveStopState();

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateLoginLevel()
        {
            try
            {
                label1.Text = Vehicle.LoginLevel.ToString();
                switch (Vehicle.LoginLevel)
                {

                    case EnumLoginLevel.Engineer:
                        {
                            ToolStripMenuItemCloseApp.Visible = true;
                            ToolStripMenuItemMode.Visible = true;
                            btnKeyInPosition.Visible = false;
                            btnKeyInSoc.Visible = false;
                            btnKeyInTestAlarm.Visible = false;
                            groupHighLevelManualControl.Visible = Vehicle.AutoState != EnumAutoState.Auto;
                        }
                        break;
                    case EnumLoginLevel.Admin:
                        {
                            ToolStripMenuItemCloseApp.Visible = true;
                            ToolStripMenuItemMode.Visible = true;
                            btnKeyInPosition.Visible = false;
                            btnKeyInSoc.Visible = false;
                            btnKeyInTestAlarm.Visible = false;
                            groupHighLevelManualControl.Visible = Vehicle.AutoState != EnumAutoState.Auto;
                        }
                        break;
                    case EnumLoginLevel.OneAboveAll:
                        {
                            ToolStripMenuItemCloseApp.Visible = true;
                            ToolStripMenuItemMode.Visible = true;
                            btnKeyInPosition.Visible = true;
                            btnKeyInSoc.Visible = true;
                            btnKeyInTestAlarm.Visible = true;
                            groupHighLevelManualControl.Visible = true;
                        }
                        break;
                    case EnumLoginLevel.Op:
                    default:
                        {
                            ToolStripMenuItemCloseApp.Visible = false;
                            ToolStripMenuItemMode.Visible = false;
                            btnKeyInPosition.Visible = false;
                            btnKeyInSoc.Visible = false;
                            btnKeyInTestAlarm.Visible = false;
                            groupHighLevelManualControl.Visible = false;
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void UpdateCannotAutoReason()
        {
            try
            {
                txtCannotAutoReason.Text = mainFlowHandler.CanAutoMsg;
                if (txtCannotAutoReason.Text != "OK")
                {
                    txtCannotAutoReason.BackColor = Color.Pink;
                }
                else
                {
                    txtCannotAutoReason.BackColor = Color.LightGreen;
                }

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool RobotStateError()
        {
            var aseRobotStatus = Vehicle.AseRobotStatus;
            if (aseRobotStatus.RobotState != EnumAseRobotState.Idle)
            {
                txtCannotAutoReason.Text = $"Robot State = {aseRobotStatus.RobotState}";
                txtCannotAutoReason.BackColor = Color.Pink;
                return true;

            }
            else if (!aseRobotStatus.IsHome)
            {
                txtCannotAutoReason.Text = $"Robot IsHome = {aseRobotStatus.IsHome}";
                txtCannotAutoReason.BackColor = Color.Pink;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool MoveStateError()
        {
            var moveState = Vehicle.AseMoveStatus.AseMoveState;
            if (moveState != EnumAseMoveState.Idle && moveState != EnumAseMoveState.Block)
            {
                txtCannotAutoReason.Text = $"Move State = {moveState}";
                txtCannotAutoReason.BackColor = Color.Pink;
                return true;

            }
            else
            {
                return false;
            }
        }

        private bool VehicleLocationLost()
        {
            if (Vehicle.AseMoveStatus.LastSection == null || string.IsNullOrEmpty(Vehicle.AseMoveStatus.LastSection.Id))
            {
                txtCannotAutoReason.Text = "Section Lost";
                txtCannotAutoReason.BackColor = Color.Pink;
                return true;
            }
            else if (Vehicle.AseMoveStatus.LastAddress == null || string.IsNullOrEmpty(Vehicle.AseMoveStatus.LastAddress.Id))
            {
                txtCannotAutoReason.Text = "Address Lost";
                txtCannotAutoReason.BackColor = Color.Pink;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateReserveStopState()
        {
            try
            {
                var reserveStop = Vehicle.AseMovingGuide.ReserveStop == com.mirle.aka.sc.ProtocolFormat.ase.agvMessage.VhStopSingle.On;

                if (reserveStop)
                {
                    lbxNeedReserveSections.ForeColor = Color.OrangeRed;
                    lbxReserveOkSections.ForeColor = Color.OrangeRed;
                }
                else
                {
                    lbxNeedReserveSections.ForeColor = Color.GreenYellow;
                    lbxReserveOkSections.ForeColor = Color.GreenYellow;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void UpdateAgvcConnection()
        {
            try
            {
                if (Vehicle.IsAgvcConnect)
                {
                    txtAgvcConnection.Text = "AGVC Connect";
                    txtAgvcConnection.BackColor = Color.LightGreen;
                    radAgvcOnline.Checked = true;
                }
                else
                {
                    txtAgvcConnection.Text = "AGVC Dis-Connect";
                    txtAgvcConnection.BackColor = Color.Pink;
                    radAgvcOffline.Checked = true;
                }

                ucWifiSignalStrength.TagValue = Vehicle.WifiSignalStrength.ToString();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void UpdateAgvlConnection()
        {
            try
            {
                if (Vehicle.IsLocalConnect)
                {
                    txtAgvlConnection.Text = "AGVL  Connect ";
                    txtAgvlConnection.BackColor = Color.LightGreen;
                    radAgvlOnline.Checked = true;
                }
                else
                {
                    txtAgvlConnection.Text = "AGVL  Dis-Connect ";
                    txtAgvlConnection.BackColor = Color.Pink;
                    radAgvlOffline.Checked = true;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void UpdateLastAlarm()
        {
            try
            {
                txtLastAlarm.Text = alarmHandler.LastAlarmMsg;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void UpdateTbxTransferStep()
        {
            try
            {
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void UpdateTbxAgvcTransCmd()
        {
            try
            {
                ucTransferCommandEnrouteState.TagValue = Vehicle.TransferCommand.EnrouteState.ToString();
                ucTransferCommandTransferStep.TagValue = Vehicle.TransferCommand.TransferStep.ToString();

                var transferCommands = Vehicle.mapTransferCommands.Values.ToList();

                ucCommandCount.TagValue = transferCommands.Count.ToString();

                if (transferCommands.Count == 0)
                {
                    foreach (var textBox in txtTransferCommands)
                    {
                        textBox.Text = "";
                        textBox.Font = new Font(textBox.Font, FontStyle.Regular);
                    }
                    txtTransferCommand01.Text = GetTransferCmdInfo(Vehicle.TransferCommand);
                    txtTransferCommand01.Font = new Font(txtTransferCommand01.Font, FontStyle.Bold);
                }
                else if (transferCommands.Count > 0 && transferCommands.Count < 5)
                {
                    foreach (var textBox in txtTransferCommands)
                    {
                        textBox.Text = "";
                        textBox.Font = new Font(textBox.Font, FontStyle.Regular);
                    }
                    for (int i = 0; i < transferCommands.Count; i++)
                    {
                        txtTransferCommands[i].Text = GetTransferCmdInfo(transferCommands[i]);
                        txtTransferCommands[i].Font = new Font(txtTransferCommands[i].Font, FontStyle.Bold);
                    }
                }
                else if (transferCommands.Count > 4)
                {
                    foreach (var textBox in txtTransferCommands)
                    {
                        textBox.Text = "";
                        textBox.Font = new Font(textBox.Font, FontStyle.Regular);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        txtTransferCommands[i].Text = GetTransferCmdInfo(transferCommands[i]);
                        txtTransferCommands[i].Font = new Font(txtTransferCommands[i].Font, FontStyle.Bold);
                    }
                }               
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string GetTransferCmdInfo(AgvcTransferCommand transferCommand)
        {
            try
            {
                string msg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\r\n",
                                      $"[{transferCommand.AgvcTransCommandType}]\r\n",
                                      $"[{transferCommand.TransferStep}]\r\n",
                                      $"[Command ID={transferCommand.CommandId}]\r\n",
                                      $"[CST ID={transferCommand.CassetteId}]\r\n",
                                      $"[SlotNum={transferCommand.SlotNumber}]\r\n",
                                      $"[EnrouteState={transferCommand.EnrouteState}]\r\n",
                                      $"[Load Adr={transferCommand.LoadAddressId}]\r\n",
                                      $"[Load Port ID={transferCommand.LoadPortId}]\r\n",
                                      $"[Unload Adr={transferCommand.UnloadAddressId}]\r\n",
                                      $"[Unload Port Id={transferCommand.UnloadPortId}]\r\n"
                                      );

                return msg;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return $"GetTransferCmdInfo exception";
            }
        }

        private void UpdateGroupBoxFlowStatus()
        {
            try
            {
                ucCurrentTransferStepType.TagValue = Vehicle.TransferCommand.TransferStep.ToString();

                ucCommanding.TagValue = Vehicle.ActionStatus.ToString();
                ucErrorFlag.TagValue = Vehicle.ErrorStatus.ToString();
                ucReserveFlag.TagValue = Vehicle.AseMovingGuide.ReserveStop.ToString();
                ucPauseFlag.TagValue = Vehicle.IsPause() ? "Pause" : "Resume";
                ucOpPauseFlag.TagValue = Vehicle.OpPauseStatus.ToString();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void UpdateAutoManual()
        {
            try
            {
                switch (Vehicle.Instance.AutoState)
                {
                    case EnumAutoState.Manual:
                        btnAutoManual.BackColor = Color.Pink;
                        txtCannotAutoReason.Visible = true;
                        UpdateCannotAutoReason();
                        break;
                    case EnumAutoState.Auto:
                    default:
                        btnAutoManual.BackColor = Color.LightGreen;
                        txtCannotAutoReason.Visible = false;
                        break;
                }

                btnAutoManual.Text = "Now : " + Vehicle.AutoState.ToString();

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void UpdateVehLocation()
        {
            try
            {
                AseMoveStatus aseMoveStatus = new AseMoveStatus(Vehicle.AseMoveStatus);
                AseMovingGuide aseMovingGuide = new AseMovingGuide(Vehicle.AseMovingGuide);

                var lastPos = aseMoveStatus.LastMapPosition;
                string lastPosX = lastPos.X.ToString("F2");
                tstextLastPosX.Text = lastPosX;
                ucMovePositionX.TagValue = lastPosX;
                string lastPosY = lastPos.Y.ToString("F2");
                tstextLastPosY.Text = lastPosY;
                ucMovePositionY.TagValue = lastPosY;

                var lastAddress = aseMoveStatus.LastAddress;
                ucMoveLastAddress.TagValue = lastAddress.Id;
                ucMapAddressId.TagValue = lastAddress.Id;

                var lastSection = aseMoveStatus.LastSection;
                ucMoveLastSection.TagValue = lastSection.Id;

                ucHeadAngle.TagValue = aseMoveStatus.HeadDirection.ToString();

                ucMoveIsMoveEnd.TagValue = aseMoveStatus.IsMoveEnd.ToString();

                ucMoveMoveState.TagValue = aseMoveStatus.AseMoveState.ToString();

                ucMoveReserveStop.TagValue = aseMovingGuide.ReserveStop.ToString();
                ucMovePauseStop.TagValue = Vehicle.IsPause() ? "Pause" : "Resume";
                ucMoveMovingIndex.TagValue = aseMovingGuide.MovingSectionsIndex.ToString();

                ucVehicleImage.Hide();
                ucVehicleImage.Location = MapPixelExchange(lastPos);
                ucVehicleImage.FixToCenter();
                ucVehicleImage.Show();
                ucVehicleImage.BringToFront();

                txtVehiclePauseFlags.Text = JsonConvert.SerializeObject(Vehicle.PauseFlags, Formatting.Indented);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void UpdateBatteryState()
        {
            try
            {
                bool isCharging = Vehicle.IsCharging;
                ucCharging.TagValue = isCharging ? "Yes" : "No";
                ucBatteryCharging.TagValue = isCharging ? "Yes" : "No";
                ucBatteryCharging.TagColor = isCharging ? Color.LightGreen : Color.Pink;
                string batteryPercentage = Vehicle.AseBatteryStatus.Percentage.ToString("F1");
                ucBatteryPercentage.TagValue = batteryPercentage;
                string batteryVoltage = Vehicle.AseBatteryStatus.Voltage.ToString("F2");
                ucBatteryVoltage.TagValue = batteryVoltage;
                string batteryTemperature = Vehicle.AseBatteryStatus.Temperature.ToString("F1");
                ucBatteryTemperature.TagValue = batteryTemperature;
                #region 200824 dabid for Watch Not AUTO Charge
                string AutoState = Vehicle.AutoState.ToString();
                ucAutoState.TagValue = AutoState;

                string IsVehicleIdle = Vehicle.VehicleIdle.ToString();
                ucIsVehicleIdle.TagValue = IsVehicleIdle;

                string IsLowPower = Vehicle.LowPower.ToString();
                ucIsLowPower.TagValue = IsLowPower;

                string IsLowPowerStartChargeTimeout = Vehicle.LowPowerStartChargeTimeout.ToString();
                ucIsLowPowerStartChargeTimeout.TagValue = IsLowPowerStartChargeTimeout;

                string IsArrivalCharge = Vehicle.ArrivalCharge.ToString();
                ucIsArrivalCharge.TagValue = IsArrivalCharge;

                string IsCharger = Vehicle.IsCharger.ToString();
                ucIsCharger.TagValue = IsCharger;

                string LastAddress = Vehicle.LastAddress;
                ucAddress.TagValue = LastAddress;
                //string IsCharging = isCharging ? "Yes" : "No";
                //ucIsCharging.TagValue = isCharging ? "Yes" : "No"; 

                string IsSimulation = Vehicle.MainFlowConfig.IsSimulation.ToString();
                ucIsSimulation.TagValue = IsSimulation;

                ucCurransferStepType.TagValue = Vehicle.TransferCommand.TransferStep.ToString();

                string ChargeCount = Vehicle.LowPowerRepeatedlyChargeCounter.ToString();
                ucChargeCount.TagValue = ChargeCount;
                #endregion

                #region 200828 dabid for Watch Not AskAllSectionsReserveInOnce

                ucIsAskReservePause.TagValue = agvcConnector.IsAskReservePause.ToString();
                ucIsMoveStep.TagValue = mainFlowHandler.IsMoveStep().ToString();
                ucIsMoveEnd.TagValue = Vehicle.AseMoveStatus.IsMoveEnd.ToString();
                ucIsSleepByAskReserveFail.TagValue = agvcConnector.IsSleepByAskReserveFail.ToString();
                ucIsHome.TagValue = Vehicle.AseRobotStatus.IsHome.ToString();
                ucIsCharging.TagValue = Vehicle.IsCharging.ToString();
                txtVisitTransferCount.Text = mainFlowHandler.VisitTransferStepCounter.ToString();

                string TMP_e = Vehicle.TMP_e;
                labException.Text = TMP_e;
                #endregion

                if (IsEnableStartChargeButton)
                {
                    IsEnableStartChargeButton = false;
                    btnCharge.Enabled = true;
                }
                if (IsEnableStopChargeButton)
                {
                    IsEnableStopChargeButton = false;
                    btnStopCharge.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void UpdateRobotAndCarrierSlotState()
        {
            try
            {
                AseRobotStatus aseRobotStatus = new AseRobotStatus(Vehicle.AseRobotStatus);
                ucRobotRobotState.TagValue = aseRobotStatus.RobotState.ToString();
                ucRobotIsHome.TagValue = aseRobotStatus.IsHome.ToString();
                ucRobotIsHome.TagColor = aseRobotStatus.IsHome ? Color.Black : Color.OrangeRed;
                ucRobotHome.TagValue = aseRobotStatus.IsHome.ToString();
                ucRobotHome.TagColor = aseRobotStatus.IsHome ? Color.Black : Color.OrangeRed;

                AseCarrierSlotStatus slotL = new AseCarrierSlotStatus(Vehicle.AseCarrierSlotL);
                AseCarrierSlotStatus slotR = new AseCarrierSlotStatus(Vehicle.AseCarrierSlotR);
                ucRobotSlotLState.TagValue = slotL.CarrierSlotStatus.ToString();
                ucRobotSlotLId.TagValue = slotL.CarrierId;
                ucLCstId.TagValue = slotL.CarrierId;

                ucRobotSlotRState.TagValue = slotR.CarrierSlotStatus.ToString();
                ucRobotSlotRId.TagValue = slotR.CarrierId;
                ucRCstId.TagValue = slotR.CarrierId;

                ucVehicleImage.Loading = slotL.CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty || slotR.CarrierSlotStatus != EnumAseCarrierSlotStatus.Empty
                    ? true
                    : false;

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void UpdatePerformanceCounter(PerformanceCounter performanceCounter, UcLabelTextBox ucLabelTextBox)
        {
            double value = performanceCounter.NextValue();
            ucLabelTextBox.TagValue = string.Format("{0:0.0}%", value);
        }
        private void UpdateListBoxSections(ListBox aListBox, List<MapSection> aListOfSections)
        {
            try
            {
                aListBox.Items.Clear();
                if (aListOfSections.Count > 0)
                {
                    for (int i = 0; i < aListOfSections.Count; i++)
                    {
                        aListBox.Items.Add(aListOfSections[i].Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public delegate void RichTextBoxAppendHeadCallback(RichTextBox richTextBox, string msg);
        public void RichTextBoxAppendHead(RichTextBox richTextBox, string msg)
        {
            if (richTextBox.InvokeRequired)
            {
                RichTextBoxAppendHeadCallback mydel = new RichTextBoxAppendHeadCallback(RichTextBoxAppendHead);
                this.Invoke(mydel, new object[] { richTextBox, msg });
            }
            else
            {
                var timeStamp = DateTime.Now.ToString("[yyyy-MM-dd HH-mm-ss.fff] ");
                msg = msg + Environment.NewLine;
                richTextBox.Text = string.Concat(timeStamp, msg, richTextBox.Text);

                int RichTextBoxMaxLines = 10000;  // middlerConfig.RichTextBoxMaxLines;

                if (richTextBox.Lines.Count() > RichTextBoxMaxLines)
                {
                    string[] sNewLines = new string[RichTextBoxMaxLines];
                    Array.Copy(richTextBox.Lines, 0, sNewLines, 0, sNewLines.Length);
                    richTextBox.Lines = sNewLines;
                }
            }
        }

        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            btnAlarmReset.Enabled = false;
            mainFlowHandler.ResetAllAlarmsFromAgvm();
            Thread.Sleep(500);
            btnAlarmReset.Enabled = true;
        }

        private void btnAutoManual_Click(object sender, EventArgs e)
        {
            btnAutoManual.Enabled = false;
            SwitchAutoStatus();
            Thread.Sleep(500);
            btnAutoManual.Enabled = true;
        }

        public void SwitchAutoStatus()
        {
            try
            {
                switch (Vehicle.AutoState)
                {
                    case EnumAutoState.Auto:
                        mainFlowHandler.AsePackage_OnModeChangeEvent(this, EnumAutoState.Manual);
                        break;
                    case EnumAutoState.Manual:
                        mainFlowHandler.AsePackage_OnModeChangeEvent(this, EnumAutoState.Auto);
                        break;
                    case EnumAutoState.None:
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

        private void radAgvcOnline_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radAgvcOnline.Checked)
                {
                    if (!agvcConnector.IsConnected())
                    {
                        agvcConnector.ReConnect();
                    }
                }
            }
            catch (Exception ex)
            {

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void radAgvcOffline_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radAgvcOffline.Checked)
                {
                    if (agvcConnector.IsConnected())
                    {
                        agvcConnector.DisConnect();
                    }
                }
            }
            catch (Exception ex)
            {

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void radAgvlOnline_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radAgvlOnline.Checked)
                {
                    if (!Vehicle.IsLocalConnect)
                    {
                        asePackage.Connect();
                    }
                }
            }
            catch (Exception ex)
            {

                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void radAgvlOffline_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radAgvlOffline.Checked)
                {
                    if (Vehicle.IsLocalConnect)
                    {
                        asePackage.DisConnect();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void 關閉ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (agvcConnector.IsConnected())
            {
                agvcConnector.DisConnect();
            }
            if (Vehicle.IsLocalConnect)
            {
                asePackage.DisConnect();
            }
            this.Close();
        }

        private void timer_SetupInitialSoc_Tick(object sender, EventArgs e)
        {
            if (mainFlowHandler.IsFirstAhGet)
            {
                var initialSoc = mainFlowHandler.InitialSoc;
                mainFlowHandler.SetupVehicleSoc(initialSoc);
                timer_SetupInitialSoc.Enabled = false;
            }
        }

        private void btnPrintScreen_Click(object sender, EventArgs e)
        {
            TakeAPicture();
        }

        private void TakeAPicture()
        {
            Image image = new Bitmap(1920, 1080);
            Graphics graphics = Graphics.FromImage(image);
            graphics.CopyFromScreen(0, 0, 0, 0, new Size(1920, 1080));
            IntPtr intPtr = graphics.GetHdc();
            graphics.ReleaseHdc(intPtr);
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = image;
            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpeg";
            string savename = Path.Combine(Environment.CurrentDirectory, "Log", timeStamp);
            image.Save(savename);
        }

        private void ToolStripMenuItemLogin_Click(object sender, EventArgs e)
        {
            if (loginForm.IsDisposed)
            {
                InitialLoginForm();
            }
            if (loginForm != null)
            {
                loginForm.BringToFront();
                loginForm.Show();
            }
        }

        private void InitialLoginForm()
        {
            loginForm = new LoginForm(mainFlowHandler.UserAgent);
        }

        private void ToolStripMenuItemLogout_Click(object sender, EventArgs e)
        {
            Vehicle.LoginLevel = EnumLoginLevel.Op;
        }

        private void LogException(string classMethodName, string exMsg)
        {
            mirleLogger.Log(new LogFormat("Error", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", exMsg));
        }

        private void LogDebug(string classMethodName, string msg)
        {
            mirleLogger.Log(new LogFormat("Debug", "5", classMethodName, Vehicle.AgvcConnectorConfig.ClientName, "CarrierID", msg));
        }

        private void checkBoxDisableSlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDisableLeftSlot.Checked)
            {
                checkBoxDisableLeftSlot.ForeColor = Color.Pink;

                if (checkBoxDisableRightSlot.Checked)
                {
                    checkBoxDisableRightSlot.ForeColor = Color.Pink;

                    Vehicle.MainFlowConfig.SlotDisable = EnumSlotSelect.Both;
                }
                else
                {
                    checkBoxDisableRightSlot.ForeColor = Color.Black;

                    Vehicle.MainFlowConfig.SlotDisable = EnumSlotSelect.Left;
                }
            }
            else
            {
                checkBoxDisableLeftSlot.ForeColor = Color.Black;

                if (checkBoxDisableRightSlot.Checked)
                {
                    checkBoxDisableRightSlot.ForeColor = Color.Pink;

                    Vehicle.MainFlowConfig.SlotDisable = EnumSlotSelect.Right;
                }
                else
                {
                    checkBoxDisableRightSlot.ForeColor = Color.Black;

                    Vehicle.MainFlowConfig.SlotDisable = EnumSlotSelect.None;
                }
            }
        }

        private void btnCharge_Click(object sender, EventArgs e)
        {
            try
            {
                btnCharge.Enabled = false;

                System.Threading.Tasks.Task.Run(() =>
                {
                    mainFlowHandler.MainFormStartCharge();
                    IsEnableStartChargeButton = true;
                });

                // btnCharge.Enabled = true;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                btnCharge.Enabled = true;
            }
        }

        private void btnStopCharge_Click(object sender, EventArgs e)
        {
            try
            {
                btnStopCharge.Enabled = false;

                System.Threading.Tasks.Task.Run(() =>
               {
                   mainFlowHandler.StopCharge();
                   IsEnableStopChargeButton = true;
               });

                // btnStopCharge.Enabled = true;
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                btnStopCharge.Enabled = true;
            }
        }

        private void btnKeyInTestAlarm_Click(object sender, EventArgs e)
        {
            int errorCode = decimal.ToInt32(numTestErrorCode.Value);
            mainFlowHandler.SetAlarmFromAgvm(errorCode);
        }

        private void txtDisableChargerAddressId_TextChanged(object sender, EventArgs e)
        {
            var addressId = txtDisableChargerAddressId.Text.Trim();
            checkEnableToCharge.Checked = Vehicle.Mapinfo.addressMap.ContainsKey(addressId) && Vehicle.Mapinfo.addressMap[addressId].IsCharger();
        }

        private void checkEnableToCharge_CheckedChanged(object sender, EventArgs e)
        {
            var addressId = txtDisableChargerAddressId.Text.Trim();
            if (Vehicle.Mapinfo.addressMap.ContainsKey(addressId))
            {
                Vehicle.Mapinfo.addressMap[addressId].ChargeDirection = checkEnableToCharge.Checked ? EnumAddressDirection.Right : EnumAddressDirection.None;
            }
        }

        private void btnKeyInPosition_Click(object sender, EventArgs e)
        {
            try
            {
                var posX = decimal.ToDouble(numPositionX.Value);
                var posY = decimal.ToDouble(numPositionY.Value);
                AsePositionArgs positionArgs = new AsePositionArgs
                {
                    Arrival = EnumAseArrival.Arrival,
                    MapPosition = new MapPosition(posX, posY)
                };
                asePackage.ReceivePositionArgsQueue.Enqueue(positionArgs);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnKeyInSoc_Click(object sender, EventArgs e)
        {
            Vehicle.AseBatteryStatus.Percentage = decimal.ToInt32(numSoc.Value);
        }
    }
}
