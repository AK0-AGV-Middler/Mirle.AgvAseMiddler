using Mirle.Agv.AseMiddler.Controller;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Mirle.Agv.AseMiddler.View
{
    public partial class AlarmForm : Form
    {
        private AlarmHandler alarmHandler;
        private MainFlowHandler mainFlowHandler;

        public AlarmForm(MainFlowHandler mainFlowHandler)
        {
            InitializeComponent();
            this.mainFlowHandler = mainFlowHandler;
            alarmHandler = mainFlowHandler.alarmHandler;
        }

        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            btnAlarmReset.Enabled = false;
            mainFlowHandler.ResetAllAlarmsFromAgvm();
            SpinWait.SpinUntil(()=>false,500);
            btnAlarmReset.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.SendToBack();
            this.Hide();
        }

        private void timeUpdateUI_Tick(object sender, EventArgs e)
        {
            tbxHappendingAlarms.Text = alarmHandler.AlarmLogMsg;
            tbxHistoryAlarms.Text = alarmHandler.AlarmHistoryLogMsg;
        }
    }
}
