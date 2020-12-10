using Mirle.Agv.AseMiddler.Controller;
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
using Mirle.Tools;
using System.Reflection;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class LoginForm : Form
    {
        private UserAgent userAgent;
        private Vehicle Vehicle { get; set; } = Vehicle.Instance;
        //private MirleLogger mirleLogger = MirleLogger.Instance;
        private NLog.Logger _transferLogger = NLog.LogManager.GetLogger("Transfer");

        public LoginForm(UserAgent userAgent)
        {
            InitializeComponent();
            this.userAgent = userAgent;
            InitialBoxUserName();
        }

        private void InitialBoxUserName()
        {
            foreach (var userName in Enum.GetNames(typeof(EnumLoginLevel)))
            {
                if (userName != EnumLoginLevel.OneAboveAll.ToString())
                {
                    boxUserName.Items.Add(userName);
                }
            }
            boxUserName.SelectedIndex = (int)EnumLoginLevel.Op;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                Vehicle.LoginLevel = userAgent.GetLoginLevel(boxUserName.SelectedItem.ToString(), txtPassword.Text);
                if (Vehicle.LoginLevel != EnumLoginLevel.Op)
                {
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Vehicle.LoginLevel = EnumLoginLevel.Op;
            this.Hide();
        }

        private void LogException(string classMethodName, string exMsg)
        {
            _transferLogger.Error($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{classMethodName}][{exMsg}]");
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
