using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Windows.Forms;

namespace BioMetrixCore
{
    [RunInstaller(true)]
    public partial class HiStaffBioMetrixInstaller : Installer
    {
        public HiStaffBioMetrixInstaller() : base()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);

            string logFile = "undefined";

            try
            {
                
                string executingAssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string executingAssemblyPath = executingAssemblyLocation.Substring(0, executingAssemblyLocation.LastIndexOf("\\"));

                logFile = executingAssemblyPath + @"\log.txt";

                File.WriteAllText(logFile, "Installation started at " + DateTime.Now.ToString() + "\n");
                File.AppendAllText(logFile, "executingAssemblyPath is " + executingAssemblyPath + "\n");

                /*
                    PROTECT [App].exe.config
                */
                // Get the configuration file
                Configuration config = ConfigurationManager.OpenExeConfiguration(executingAssemblyLocation);
                // Create the provider name
                string provider = "DataProtectionConfigurationProvider";
                //Encrypt the connectionStrings
                ConfigurationSection connstrings = config.ConnectionStrings;
                if (!connstrings.SectionInformation.IsProtected)
                {
                    connstrings.SectionInformation.ProtectSection(provider);
                }
                connstrings.SectionInformation.ForceSave = true;
                config.Save(ConfigurationSaveMode.Full);

                /*
                    REGISTER SDK x86
                */
                System.Diagnostics.Process.Start(executingAssemblyPath + @"\dlls\32bit\Register_SDK_x86.bat");
                File.AppendAllText(logFile, "App installed\n");
                File.AppendAllText(logFile, "=============\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"D:\log.txt", ex.Message + " (" + logFile +") " + "\n");
            }
        }
    }
}
