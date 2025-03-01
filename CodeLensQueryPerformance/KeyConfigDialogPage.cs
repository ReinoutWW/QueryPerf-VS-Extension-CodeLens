using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.IO;

namespace CodeLensQueryPerformance
{
    public class KeyConfigDialogPage : DialogPage
    {
        [Category("My Extension Settings")]
        [DisplayName("API Key")]
        [Description("Enter your API Key")]
        public string ApiKey { get; set; } = string.Empty;

        [Category("My Extension Settings")]
        [DisplayName("App ID")]
        [Description("Enter your App ID")]
        public string AppId { get; set; } = string.Empty;

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            try
            {
                string configFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MethodQueryUsageCodeLensProvider");
                Directory.CreateDirectory(configFolder);

                string configFilePath = Path.Combine(configFolder, "config.txt");
                string contents = $"{AppId};{ApiKey}";

                File.WriteAllText(configFilePath, contents);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error writing config file: " + ex.Message);
            }
        }
    }

}
