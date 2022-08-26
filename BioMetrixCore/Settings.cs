using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


            config.Save(ConfigurationSaveMode.Modified);
            this.Dispose();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            string autoStart = ConfigurationManager.AppSettings["autoStart"];
            string autoRepeatWhenFails = ConfigurationManager.AppSettings["autoRepeatWhenFails"];
            checkBoxAutoStart.Checked = autoStart == "True" ? true : false;
            checkBoxRepeatWhenFails.Checked = autoRepeatWhenFails == "True" ? true : false;
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
