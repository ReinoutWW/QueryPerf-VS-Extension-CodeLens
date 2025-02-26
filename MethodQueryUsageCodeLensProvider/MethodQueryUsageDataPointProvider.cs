using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

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
            // Create our custom data point. We'll implement it in a separate class next.
            IAsyncCodeLensDataPoint dataPoint = new MethodQueryUsageDataPoint(descriptor);
            return Task.FromResult(dataPoint);
        }
    }
}
