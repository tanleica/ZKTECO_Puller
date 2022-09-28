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
            config.AppSettings.Settings.Remove("apiLoginUrl");
            config.AppSettings.Settings.Add("apiLoginUrl", tbApiLoginUrl.Text);
            config.AppSettings.Settings.Remove("apiPostUrl");
            config.AppSettings.Settings.Add("apiPostUrl", tbApiPostUrl.Text);

            config.Save(ConfigurationSaveMode.Modified);
            this.Dispose();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            string startingDate = ConfigurationManager.AppSettings["startingDate"];
            string autoStart = ConfigurationManager.AppSettings["autoStart"];
            string autoRepeatWhenFails = ConfigurationManager.AppSettings["autoRepeatWhenFails"];
            string autoRepeatTimer = ConfigurationManager.AppSettings["autoRepeatTimer"];
            string apiLoginUrl = ConfigurationManager.AppSettings["apiLoginUrl"];
            string apiPostUrl = ConfigurationManager.AppSettings["apiPostUrl"];

            checkBoxAutoStart.Checked = autoStart == "True" ? true : false;
            checkBoxRepeatWhenFails.Checked = autoRepeatWhenFails == "True" ? true : false;
            // Timeout value: minimum = 15000 (15 seconds), maximum = 3600000 (1 hour)
            numericUpDownTimer.Value = autoRepeatTimer == null? 150000 : (int)Convert.ToDecimal(autoRepeatTimer);
            tbApiLoginUrl.Text = apiLoginUrl == null? "http://core.vn:82/api/Authen/SignInPortalHR" : apiLoginUrl;
            tbApiPostUrl.Text = apiPostUrl == null ? "http://core.vn:82/api/hr/TimeSheetMonthly/ImportSwipeMachine" : apiPostUrl;

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
