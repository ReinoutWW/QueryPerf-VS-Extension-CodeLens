using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.Linq;
using MethodQueryUsageCodeLensProvider.Extensions;

namespace MethodQueryUsageCodeLensProvider
{
    internal class MethodQueryUsageDataPoint : IAsyncCodeLensDataPoint
    {
        private readonly CodeLensDescriptor _descriptor;
        public event AsyncEventHandler InvalidatedAsync;

        private static readonly IUsageQueryService _queryService = new LocalDataQueryService("C:\\CodeLens\\CodeLensSourceData3.csv");

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

            var descriptorData = new CodeLensDataPointDescriptor
            {
                Description = details.GetDisplayString(),
                TooltipText = details.AdditionalInfo
            };

            return descriptorData;
        }

        public async Task<CodeLensDetailsDescriptor> GetDetailsAsync(
            CodeLensDescriptorContext context,
            CancellationToken token
        )
        {
            string methodSignature = _descriptor.ElementDescription;
            QueryPerformanceDetails perf = await _queryService.GetMethodPerformanceDetailsAsync(methodSignature);
            List<CodeLensDetailHeaderDescriptor> headers = GetQueryMetricsHeaders();

            var entries = new List<CodeLensDetailEntryDescriptor>();

            entries.Add(CreateRow(
                metric: "Rows",
                min: ValueFormatHelper.ValueOrNA(perf.Rows_Min),
                max: ValueFormatHelper.ValueOrNA(perf.Rows_Max),
                avg: ValueFormatHelper.ValueOrNA(perf.Rows_Avg),
                total: ValueFormatHelper.ValueOrNA(perf.Rows_Total)));

            entries.Add(CreateRow(
                metric: "Columns",
                min: ValueFormatHelper.ValueOrNA(perf.Columns_Min),
                max: ValueFormatHelper.ValueOrNA(perf.Columns_Max),
                avg: ValueFormatHelper.FormatDouble(perf.Columns_Avg),
                total: ValueFormatHelper.ValueOrNA(perf.Columns_Total)));

            entries.Add(CreateRow(
                metric: "TotalBytes",
                min: ValueFormatHelper.FormatDoubleBytes(perf.TotalBytes_Min),
                max: ValueFormatHelper.FormatDoubleBytes(perf.TotalBytes_Max),
                avg: ValueFormatHelper.FormatDoubleBytes(perf.TotalBytes_Avg),
                total: ValueFormatHelper.FormatDoubleBytes(perf.TotalBytes_Total)));

            entries.Add(CreateRow(
                metric: "BytesSent",
                min: ValueFormatHelper.FormatDoubleBytes(perf.BytesSent_Min),
                max: ValueFormatHelper.FormatDoubleBytes(perf.BytesSent_Max),
                avg: ValueFormatHelper.FormatDoubleBytes(perf.BytesSent_Avg),
                total: ValueFormatHelper.FormatDoubleBytes(perf.BytesSent_Total)));

            entries.Add(CreateRow(
                metric: "BytesReceived",
                min: ValueFormatHelper.FormatDoubleBytes(perf.BytesReceived_Min),
                max: ValueFormatHelper.FormatDoubleBytes(perf.BytesReceived_Max),
                avg: ValueFormatHelper.FormatDoubleBytes(perf.BytesReceived_Avg),
                total: ValueFormatHelper.FormatDoubleBytes(perf.BytesReceived_Total)));

            entries.Add(CreateRow(
                metric: "ExecutionTime (ms)",
                min: ValueFormatHelper.ValueOrNAAsTime(perf.ExecutionTime_Min),
                max: ValueFormatHelper.ValueOrNAAsTime(perf.ExecutionTime_Max),
                avg: ValueFormatHelper.ValueOrNAAsTime(perf.ExecutionTime_Avg),
                total: ValueFormatHelper.ValueOrNAAsTime(perf.ExecutionTime_Total)));

            var descriptor = new CodeLensDetailsDescriptor
            {
                Headers = headers,
                Entries = entries
            };

            return descriptor;
        }

        CodeLensDetailEntryDescriptor CreateRow(
         string metric,
         string min,
         string max,
         string avg,
         string total)
        {
            return new CodeLensDetailEntryDescriptor
            {
                Fields = new List<CodeLensDetailEntryField>
            {
                new CodeLensDetailEntryField { Text = metric },
                new CodeLensDetailEntryField { Text = min  ?? "N/A" },
                new CodeLensDetailEntryField { Text = max  ?? "N/A" },
                new CodeLensDetailEntryField { Text = avg  ?? "N/A" },
                new CodeLensDetailEntryField { Text = total ?? "N/A" }
            }
            };
        }

        private static List<CodeLensDetailHeaderDescriptor> GetQueryMetricsHeaders()
        {
            return new List<CodeLensDetailHeaderDescriptor>
            {
                new CodeLensDetailHeaderDescriptor
                {
                    UniqueName = "Metric",
                    DisplayName = "Metric",
                    Width = 0.4
                },
                new CodeLensDetailHeaderDescriptor
                {
                    UniqueName = "Min",
                    DisplayName = "Min",
                    Width = 0.15
                },
                new CodeLensDetailHeaderDescriptor
                {
                    UniqueName = "Max",
                    DisplayName = "Max",
                    Width = 0.15
                },
                new CodeLensDetailHeaderDescriptor
                {
                    UniqueName = "Avg",
                    DisplayName = "Avg",
                    Width = 0.15
                },
                new CodeLensDetailHeaderDescriptor
                {
                    UniqueName = "Total",
                    DisplayName = "Total",
                    Width = 0.15
                }
            };
        }
    }
}
