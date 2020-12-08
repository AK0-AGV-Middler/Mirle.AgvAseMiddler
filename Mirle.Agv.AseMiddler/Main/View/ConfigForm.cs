using Mirle.Agv.AseMiddler.Model;
using Mirle.Tools;
using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Reflection;
using Mirle.Agv.AseMiddler.Model.Configs;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class ConfigForm : Form
    {
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;

        public ConfigForm()
        {
            InitializeComponent();
            InitialBoxVehicleConfigs();
        }

        private void InitialBoxVehicleConfigs()
        {
            //Main Config

            boxVehicleConfigs.Items.Add("MainFlowConfig");
            boxVehicleConfigs.Items.Add("AgvcConnectorConfig");
            boxVehicleConfigs.Items.Add("AlarmConfig");
            boxVehicleConfigs.Items.Add("MapConfig");
            boxVehicleConfigs.Items.Add("BatteryLog");

            //AsePackage Config

            boxVehicleConfigs.Items.Add("PspConnectionConfig");
            boxVehicleConfigs.Items.Add("AsePackageConfig");
            boxVehicleConfigs.Items.Add("AseMoveConfig");
            boxVehicleConfigs.Items.Add("AseBatteryConfig");

            boxVehicleConfigs.SelectedIndex = 0;
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }

        private NLog.Logger _transferLogger = NLog.LogManager.GetLogger("Transfer");

        private void LogException(string classMethodName, string exMsg)
        {
            //MirleLogger.Instance.Log(new LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));
            _transferLogger.Error($"[{classMethodName}][{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{exMsg}]");
        }

        private void btnLoadConfig_Click(object sender, EventArgs e)
        {
            switch (boxVehicleConfigs.SelectedItem.ToString())
            {
                //Main Config

                case "MainFlowConfig":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.MainFlowConfig, Formatting.Indented);
                    break;
                case "AgvcConnectorConfig":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.AgvcConnectorConfig, Formatting.Indented);
                    break;
                case "AlarmConfig":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.AlarmConfig, Formatting.Indented);
                    break;
                case "MapConfig":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.MapConfig, Formatting.Indented);
                    break;
                case "BatteryLog":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.BatteryLog, Formatting.Indented);
                    break;

                //AsePackage Config
                case "PspConnectionConfig":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.PspConnectionConfig, Formatting.Indented);                   
                    break;
                case "AsePackageConfig":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.AsePackageConfig, Formatting.Indented);
                    break;
                case "AseMoveConfig":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.AseMoveConfig, Formatting.Indented);
                    break;
                case "AseBatteryConfig":
                    txtJsonStringConfig.Text = JsonConvert.SerializeObject(Vehicle.AseBatteryConfig, Formatting.Indented);
                    break;
                default:
                    break;
            }
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                switch (boxVehicleConfigs.SelectedItem.ToString())
                {  
                    //Main Config

                    case "MainFlowConfig":
                        Vehicle.MainFlowConfig = JsonConvert.DeserializeObject<MainFlowConfig>(txtJsonStringConfig.Text);
                        break;
                    case "AgvcConnectorConfig":
                        Vehicle.AgvcConnectorConfig = JsonConvert.DeserializeObject<AgvcConnectorConfig>(txtJsonStringConfig.Text);
                        break;
                    case "BatteryLog":
                        Vehicle.BatteryLog = JsonConvert.DeserializeObject<BatteryLog>(txtJsonStringConfig.Text);
                        break;

                    //AsePackage Config
                    case "PspConnectionConfig":
                        Vehicle.PspConnectionConfig = JsonConvert.DeserializeObject<PspConnectionConfig>(txtJsonStringConfig.Text);
                        break;
                    case "AsePackageConfig":
                        Vehicle.AsePackageConfig = JsonConvert.DeserializeObject<AsePackageConfig>(txtJsonStringConfig.Text);
                        break;
                    case "AseMoveConfig":
                        Vehicle.AseMoveConfig = JsonConvert.DeserializeObject<AseMoveConfig>(txtJsonStringConfig.Text);
                        break;
                    case "AseBatteryConfig":
                        Vehicle.AseBatteryConfig = JsonConvert.DeserializeObject<AseBatteryConfig>(txtJsonStringConfig.Text);
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

        private void btnCleanTextBox_Click(object sender, EventArgs e)
        {
            txtJsonStringConfig.Clear();
        }

        private void btnSaveConfigsToFile_Click(object sender, EventArgs e)
        {
            string filename = string.Concat(boxVehicleConfigs.SelectedItem.ToString(), ".json");
            System.IO.File.WriteAllText(filename, txtJsonStringConfig.Text);
        }

        private void btnLoadConfigsFromFile_Click(object sender, EventArgs e)
        {
            string filename = string.Concat(boxVehicleConfigs.SelectedItem.ToString(), ".json");
            txtJsonStringConfig.Text = System.IO.File.ReadAllText(filename);
        }
    }
}
