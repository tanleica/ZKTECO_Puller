using System;
using System.Configuration;
using System.Windows.Forms;

namespace BioMetrixCore.Security
{
    public static class Crypto
    {
        public static void EncryptConnectionString()
        {
            try
            {
                // Get the configuration file
                Configuration config =
                    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Create the provider name
                string provider = "DataProtectionConfigurationProvider";

                //Encrypt the connectionStrings
                ConfigurationSection connstrings = config.ConnectionStrings;
                if (!connstrings.SectionInformation.IsProtected)
                {
                    connstrings.SectionInformation.ProtectSection(provider);
                    connstrings.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Full);
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void DecryptConnectionString()
        {
            //Get the configuration file
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Decrypt the connectionStrings
            ConfigurationSection connstrings = config.ConnectionStrings;
            if (connstrings.SectionInformation.IsProtected)
            {
                connstrings.SectionInformation.UnprotectSection();
                connstrings.SectionInformation.ForceSave = true;
                config.Save(ConfigurationSaveMode.Full);
            }
        }
    }
}
