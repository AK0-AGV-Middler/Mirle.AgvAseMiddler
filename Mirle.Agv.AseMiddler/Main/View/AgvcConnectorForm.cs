using com.mirle.aka.sc.ProtocolFormat.ase.agvMessage;
using Google.Protobuf.Collections;
using Mirle.Agv.AseMiddler.Controller;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AgvcConnectorForm : Form
    {
        private AgvcConnector agvcConnector;
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;

        public AgvcConnectorForm(AgvcConnector agvcConnector)
        {
            InitializeComponent();
            this.agvcConnector = agvcConnector;
        }

        private void CommunicationForm_Load(object sender, EventArgs e)
        {
            ConfigToUI();
            if (agvcConnector.ClientAgent != null)
            {
                if (agvcConnector.ClientAgent.IsConnection)
                {
                    toolStripStatusLabel1.Text = "Connect";
                }
            }
        }

        private void ConfigToUI()
        {
            txtRemoteIp.Text = Vehicle.AgvcConnectorConfig.RemoteIp;
            txtRemotePort.Text = Vehicle.AgvcConnectorConfig.RemotePort.ToString();

            boxAgvCmdNums.DataSource = Enum.GetValues(typeof(EnumCmdNums));
            boxAgvCmdNums.SelectedItem = EnumCmdNums.Cmd31_TransferRequest;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            WrapperMessage wrapper = new WrapperMessage();
            EnumCmdNums cmdNum = (EnumCmdNums)boxAgvCmdNums.SelectedItem;

            switch (cmdNum)
            {
                case EnumCmdNums.Cmd11_CouplerInfoReport:
                    break;
                case EnumCmdNums.Cmd31_TransferRequest:
                    break;
                case EnumCmdNums.Cmd32_TransferCompleteResponse:
                    break;
                case EnumCmdNums.Cmd35_CarrierIdRenameRequest:
                    break;
                case EnumCmdNums.Cmd36_TransferEventResponse:
                    break;
                case EnumCmdNums.Cmd37_TransferCancelRequest:
                    break;
                case EnumCmdNums.Cmd38_GuideInfoResponse:
                    break;
                case EnumCmdNums.Cmd39_PauseRequest:
                    break;
                case EnumCmdNums.Cmd41_ModeChange:
                    break;
                case EnumCmdNums.Cmd43_StatusRequest:
                    break;
                case EnumCmdNums.Cmd45_PowerOnoffRequest:
                    break;
                case EnumCmdNums.Cmd51_AvoidRequest:
                    break;
                case EnumCmdNums.Cmd52_AvoidCompleteResponse:
                    break;
                case EnumCmdNums.Cmd91_AlarmResetRequest:
                    break;
                case EnumCmdNums.Cmd94_AlarmResponse:
                    break;
                case EnumCmdNums.Cmd131_TransferResponse:
                    break;
                case EnumCmdNums.Cmd132_TransferCompleteReport:
                    {
                        var cmdInfo = JsonConvert.DeserializeObject<ID_132_TRANS_COMPLETE_REPORT>(txtAgvCommandInfo.Text);

                        wrapper.ID = WrapperMessage.TranCmpRepFieldNumber;
                        wrapper.TranCmpRep = cmdInfo;
                    }
                    break;
                case EnumCmdNums.Cmd134_TransferEventReport:
                    {
                        var cmdInfo = JsonConvert.DeserializeObject<ID_134_TRANS_EVENT_REP>(txtAgvCommandInfo.Text);

                        wrapper.ID = WrapperMessage.TransEventRepFieldNumber;
                        wrapper.TransEventRep = cmdInfo;
                    }
                    break;
                case EnumCmdNums.Cmd135_CarrierIdRenameResponse:
                    break;
                case EnumCmdNums.Cmd136_TransferEventReport:
                    {
                        var cmdInfo = JsonConvert.DeserializeObject<ID_136_TRANS_EVENT_REP>(txtAgvCommandInfo.Text);

                        wrapper.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                        wrapper.ImpTransEventRep = cmdInfo;
                    }
                    break;
                case EnumCmdNums.Cmd137_TransferCancelResponse:
                    break;
                case EnumCmdNums.Cmd138_GuideInfoRequest:
                    {
                        var cmdInfo = JsonConvert.DeserializeObject<ID_138_GUIDE_INFO_REQUEST>(txtAgvCommandInfo.Text);

                        wrapper.ID = WrapperMessage.GuideInfoReqFieldNumber;
                        wrapper.GuideInfoReq = cmdInfo;
                    }
                    break;
                case EnumCmdNums.Cmd139_PauseResponse:
                    break;
                case EnumCmdNums.Cmd141_ModeChangeResponse:
                    break;
                case EnumCmdNums.Cmd143_StatusResponse:
                    break;
                case EnumCmdNums.Cmd144_StatusReport:
                    {
                        var cmdInfo = JsonConvert.DeserializeObject<ID_144_STATUS_CHANGE_REP>(txtAgvCommandInfo.Text);

                        wrapper.ID = WrapperMessage.StatueChangeRepFieldNumber;
                        wrapper.StatueChangeRep = cmdInfo;
                    }
                    break;
                case EnumCmdNums.Cmd145_PowerOnoffResponse:
                    break;
                case EnumCmdNums.Cmd151_AvoidResponse:
                    break;
                case EnumCmdNums.Cmd152_AvoidCompleteReport:
                    {
                        var cmdInfo = JsonConvert.DeserializeObject<ID_144_STATUS_CHANGE_REP>(txtAgvCommandInfo.Text);

                        wrapper.ID = WrapperMessage.StatueChangeRepFieldNumber;
                        wrapper.StatueChangeRep = cmdInfo;
                    }
                    break;
                case EnumCmdNums.Cmd191_AlarmResetResponse:
                    {
                        var cmdInfo = JsonConvert.DeserializeObject<ID_191_ALARM_RESET_RESPONSE>(txtAgvCommandInfo.Text);

                        wrapper.ID = WrapperMessage.AlarmResetRespFieldNumber;
                        wrapper.AlarmResetResp = cmdInfo;
                    }
                    break;
                case EnumCmdNums.Cmd194_AlarmReport:
                    {
                        var cmdInfo = JsonConvert.DeserializeObject<ID_194_ALARM_REPORT>(txtAgvCommandInfo.Text);

                        wrapper.ID = WrapperMessage.AlarmRepFieldNumber;
                        wrapper.AlarmRep = cmdInfo;
                    }
                    break;
                default:
                    break;
            }

            agvcConnector.SendWrapperToSchedule(wrapper, false, false);
        }

        private void cbSend_SelectedValueChanged(object sender, EventArgs e)
        {
            txtAgvCommandInfo.Text = GetCommandPropertiesFromCommandNumber((EnumCmdNums)boxAgvCmdNums.SelectedItem);
        }
        private string GetCommandPropertiesFromCommandNumber(EnumCmdNums cmdNums)
        {
            switch (cmdNums)
            {
                case EnumCmdNums.Cmd000_EmptyCommand:
                    break;
                case EnumCmdNums.Cmd11_CouplerInfoReport:
                    var cmd = new ID_11_COUPLER_INFO_REP();
                    cmd.CouplerInfos.Add(new CouplerInfo() { AddressID = "10004", CouplerStatus = CouplerStatus.Disable });
                    cmd.CouplerInfos.Add(new CouplerInfo() { AddressID = "10002", CouplerStatus = CouplerStatus.Enable });
                    return JsonConvert.SerializeObject(cmd, Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd31_TransferRequest:
                    return JsonConvert.SerializeObject(new ID_31_TRANS_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd32_TransferCompleteResponse:
                    return JsonConvert.SerializeObject(new ID_32_TRANS_COMPLETE_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd35_CarrierIdRenameRequest:
                    return JsonConvert.SerializeObject(new ID_35_CST_ID_RENAME_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd36_TransferEventResponse:
                    return JsonConvert.SerializeObject(new ID_36_TRANS_EVENT_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd37_TransferCancelRequest:
                    return JsonConvert.SerializeObject(new ID_37_TRANS_CANCEL_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd38_GuideInfoResponse:
                    return JsonConvert.SerializeObject(new ID_38_GUIDE_INFO_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd39_PauseRequest:
                    return JsonConvert.SerializeObject(new ID_39_PAUSE_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd41_ModeChange:
                    return JsonConvert.SerializeObject(new ID_41_MODE_CHANGE_REQ(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd43_StatusRequest:
                    return JsonConvert.SerializeObject(new ID_43_STATUS_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd44_StatusRequest:
                    return JsonConvert.SerializeObject(new ID_44_STATUS_CHANGE_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd45_PowerOnoffRequest:
                    return JsonConvert.SerializeObject(new ID_45_POWER_OPE_REQ(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd51_AvoidRequest:
                    return JsonConvert.SerializeObject(new ID_51_AVOID_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd52_AvoidCompleteResponse:
                    return JsonConvert.SerializeObject(new ID_52_AVOID_COMPLETE_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd91_AlarmResetRequest:
                    return JsonConvert.SerializeObject(new ID_91_ALARM_RESET_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd94_AlarmResponse:
                    return JsonConvert.SerializeObject(new ID_94_ALARM_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd131_TransferResponse:
                    return JsonConvert.SerializeObject(new ID_131_TRANS_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd132_TransferCompleteReport:
                    return JsonConvert.SerializeObject(new ID_132_TRANS_COMPLETE_REPORT(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd134_TransferEventReport:
                    return JsonConvert.SerializeObject(new ID_134_TRANS_EVENT_REP(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd135_CarrierIdRenameResponse:
                    return JsonConvert.SerializeObject(new ID_135_CST_ID_RENAME_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd136_TransferEventReport:
                    return JsonConvert.SerializeObject(new ID_136_TRANS_EVENT_REP(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd137_TransferCancelResponse:
                    return JsonConvert.SerializeObject(new ID_137_TRANS_CANCEL_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd139_PauseResponse:
                    return JsonConvert.SerializeObject(new ID_139_PAUSE_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd141_ModeChangeResponse:
                    return JsonConvert.SerializeObject(new ID_141_MODE_CHANGE_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd143_StatusResponse:
                    return JsonConvert.SerializeObject(new ID_143_STATUS_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd144_StatusReport:
                    return JsonConvert.SerializeObject(new ID_144_STATUS_CHANGE_REP(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd145_PowerOnoffResponse:
                    return JsonConvert.SerializeObject(new ID_145_POWER_OPE_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd151_AvoidResponse:
                    return JsonConvert.SerializeObject(new ID_151_AVOID_RESPONSE(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd152_AvoidCompleteReport:
                    return JsonConvert.SerializeObject(new ID_31_TRANS_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd191_AlarmResetResponse:
                    return JsonConvert.SerializeObject(new ID_31_TRANS_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                case EnumCmdNums.Cmd194_AlarmReport:
                    return JsonConvert.SerializeObject(new ID_31_TRANS_REQUEST(), Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
                default:
                    break;
            }

            return "";
        }

        private void btnIsClientAgentNull_Click(object sender, EventArgs e)
        {
            if (agvcConnector.IsClientAgentNull())
            {
                agvcConnector.AppendCommLog("ClientAgent is null.");
            }
            else
            {
                agvcConnector.AppendCommLog("ClientAgent is not null.");
            }
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            agvcConnector.DisConnect();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                agvcConnector.Connect();
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            agvcConnector.StopClientAgent();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }

        private void timerUI_Tick(object sender, EventArgs e)
        {
            try
            {
                toolStripStatusLabel1.Text = Vehicle.IsAgvcConnect ? " Connect " : " Dis-Connect ";
                if (!Vehicle.IsIgnoreAppendDebug)
                {
                    tbxCommLogMsg.Text = agvcConnector.SbCommMsg.ToString();
                }               
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private NLog.Logger _CommLogger = NLog.LogManager.GetLogger("Comm");

        private void LogException(string classMethodName, string exMsg)
        {
            _CommLogger.Error($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{classMethodName}][{exMsg}]");
        }
    }
}
