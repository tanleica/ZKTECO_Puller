using System;
using System.Configuration;
using System.Windows.Forms;

namespace BioMetrixCore
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            config.AppSettings.Settings.Remove("autoStart");
            config.AppSettings.Settings.Add("autoStart", checkBoxAutoStart.Checked.ToString());
            config.AppSettings.Settings.Remove("autoRepeatWhenFails");
            config.AppSettings.Settings.Add("autoRepeatWhenFails", checkBoxRepeatWhenFails.Checked.ToString());
            config.AppSettings.Settings.Remove("autoRepeatTimer");
            config.AppSettings.Settings.Add("autoRepeatTimer", numericUpDownTimer.Value.ToString());


            config.Save(ConfigurationSaveMode.Modified);
            this.Dispose();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            string autoStart = ConfigurationManager.AppSettings["autoStart"];
            string autoRepeatWhenFails = ConfigurationManager.AppSettings["autoRepeatWhenFails"];
            string autoRepeatTimer = ConfigurationManager.AppSettings["autoRepeatTimer"];
            checkBoxAutoStart.Checked = autoStart == "True" ? true : false;
            checkBoxRepeatWhenFails.Checked = autoRepeatWhenFails == "True" ? true : false;
            numericUpDownTimer.Value = autoRepeatTimer == null? 15 : (int)Convert.ToDecimal(autoRepeatTimer);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
