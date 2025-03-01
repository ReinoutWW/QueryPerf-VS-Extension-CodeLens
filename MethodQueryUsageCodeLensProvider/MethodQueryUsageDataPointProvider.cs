using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;

namespace MethodQueryUsageCodeLensProvider
{
    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name(Id)]
    [ContentType("CSharp")]  // Only apply to C# code
    internal class MethodQueryUsageDataPointProvider : IAsyncCodeLensDataPointProvider
    {
        public const string Id = "MethodQueryUsageDataPointProvider";

        public Task<bool> CanCreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext context, CancellationToken token)
        {
            // We'll only show CodeLens for methods.
            bool isMethod = descriptor.Kind == CodeElementKinds.Method;
            return Task.FromResult(isMethod);
        }

        public Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext context, CancellationToken token)
        {
            // Default values in case file isn't found
            string appId = "MISSING";
            string apiKey = "MISSING";

            try
            {
                GetKeyConfigFromFile(ref appId, ref apiKey);
            }
            catch (Exception ex)
            {
                apiKey = $"Error: {ex.Message}";
            }

            IAsyncCodeLensDataPoint dataPoint = new MethodQueryUsageDataPoint(descriptor, appId, apiKey);
            return Task.FromResult(dataPoint);
        }

        private static void GetKeyConfigFromFile(ref string appId, ref string apiKey)
        {
            string configFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MethodQueryUsageCodeLensProvider");
            string configFilePath = Path.Combine(configFolder, "config.txt");

            if (File.Exists(configFilePath))
            {
                // We expect "AppId;ApiKey"
                string contents = File.ReadAllText(configFilePath);
                var parts = contents.Split(';');
                if (parts.Length == 2)
                {
                    appId = parts[0];
                    apiKey = parts[1];
                }
            }
        }
    }
}
