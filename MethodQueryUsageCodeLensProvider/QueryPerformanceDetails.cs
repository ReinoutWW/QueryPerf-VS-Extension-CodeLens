using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQueryUsageCodeLensProvider
{
    public class QueryPerformanceDetails
    {
        // --- CSV columns from KQL ---
        public string Tag { get; set; }

        public long QueryCount { get; set; }
        public long UniqueUserCount { get; set; }

        // BytesSent
        public long BytesSent_Min { get; set; }
        public long BytesSent_Max { get; set; }
        public double BytesSent_Avg { get; set; }
        public long BytesSent_Total { get; set; }

        // BytesReceived
        public long BytesReceived_Min { get; set; }
        public long BytesReceived_Max { get; set; }
        public double BytesReceived_Avg { get; set; }
        public long BytesReceived_Total { get; set; }

        // TotalBytes
        public long TotalBytes_Min { get; set; }
        public long TotalBytes_Max { get; set; }
        public double TotalBytes_Avg { get; set; }
        public long TotalBytes_Total { get; set; }

        // Rows
        public long Rows_Min { get; set; }
        public long Rows_Max { get; set; }
        public double Rows_Avg { get; set; }
        public long Rows_Total { get; set; }

        // Columns
        public long Columns_Min { get; set; }
        public long Columns_Max { get; set; }
        public double Columns_Avg { get; set; }
        public long Columns_Total { get; set; }

        // ExecutionTime
        public long ExecutionTime_Min { get; set; }
        public long ExecutionTime_Max { get; set; }
        public double ExecutionTime_Avg { get; set; }
        public long ExecutionTime_Total { get; set; }

        public string AdditionalInfo { get; set; }

        public override string ToString()
        {
            return $"Query's logged: {QueryCount} | "
                 + $"Processed bytes: {ValueFormatHelper.BytesToReadableStringDouble(TotalBytes_Total)} | "
                 + $"Unique Users: {UniqueUserCount}";
        }
    }

}
