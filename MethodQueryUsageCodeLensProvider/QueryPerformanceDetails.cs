using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQueryUsageCodeLensProvider
{
    public class QueryPerformanceDetails
    {
        public long InvocationCount { get; set; }
        public long TotalBytes { get; set; }
        public long UniqueUserCount { get; set; }
        public string AdditionalInfo { get; set; }

        public override string ToString()
        {
            // For quick display, adapt as needed
            return $"Invocations: {InvocationCount}, Bytes: {BytesToReadableString(TotalBytes)}, Unique Users: {UniqueUserCount}";
        }

        public static string BytesToReadableString(long bytes)
        {
            if (bytes < 0)
                throw new ArgumentException("Bytes cannot be negative", nameof(bytes));
            if (bytes == 0)
                return "0B";

            // Use base-1000 for conversion.
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int unitIndex = (int)Math.Floor(Math.Log(bytes, 1000));
            double adjustedSize = bytes / Math.Pow(1000, unitIndex);

            return $"{adjustedSize:0.#}{units[unitIndex]}";
        }
    }
}
