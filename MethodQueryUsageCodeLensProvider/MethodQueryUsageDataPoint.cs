using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;

namespace MethodQueryUsageCodeLensProvider
{
    internal class MethodQueryUsageDataPoint : IAsyncCodeLensDataPoint
    {
        private readonly CodeLensDescriptor _descriptor;
        public event AsyncEventHandler InvalidatedAsync;

        private static readonly IUsageQueryService _queryService = new LocalDataQueryService("<csv code source path here>");

        public MethodQueryUsageDataPoint(CodeLensDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public CodeLensDescriptor Descriptor => _descriptor;

        public async Task<CodeLensDataPointDescriptor> GetDataAsync(CodeLensDescriptorContext context, CancellationToken token)
        {
            // e.g.: "Namespace.Class.Method"
            string methodSignature = _descriptor.ElementDescription;

            QueryPerformanceDetails details = await _queryService.GetMethodPerformanceDetailsAsync(methodSignature);

            string description = string.Empty;

            if (!string.IsNullOrWhiteSpace(details.AdditionalInfo))
            {
                description += details.ToString();
            }

            var descriptorData = new CodeLensDataPointDescriptor
            {
                Description = description,
                TooltipText = "Src: " + _descriptor.ElementDescription
            };
            return descriptorData;
        }

        public Task<CodeLensDetailsDescriptor> GetDetailsAsync(CodeLensDescriptorContext context, CancellationToken token)
        {
            return Task.FromResult<CodeLensDetailsDescriptor>(null);
        }

    }
}
