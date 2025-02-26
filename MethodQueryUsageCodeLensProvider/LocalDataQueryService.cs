using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQueryUsageCodeLensProvider
{
    internal sealed class LocalDataQueryService : IUsageQueryService
    {
        private readonly Dictionary<string, QueryPerformanceDetails> _dataStore;

        public LocalDataQueryService(string csvFilePath)
        {
            _dataStore = LoadData(csvFilePath);
        }

        public Task<QueryPerformanceDetails> GetMethodPerformanceDetailsAsync(string methodSignature)
        {
            methodSignature = methodSignature?.Trim() ?? string.Empty;

            var kvp = _dataStore.FirstOrDefault(pair =>
                IsPartialMatch(methodSignature, pair.Key));

            if (!kvp.Equals(default(KeyValuePair<string, QueryPerformanceDetails>)))
            {
                return Task.FromResult(kvp.Value);
            }

            return Task.FromResult(new QueryPerformanceDetails
            {
                InvocationCount = 0,
                TotalBytes = 0,
                AdditionalInfo = "(No local CSV data found / partial match failed)"
            });
        }

        private Dictionary<string, QueryPerformanceDetails> LoadData(string csvFilePath)
        {
            var result = new Dictionary<string, QueryPerformanceDetails>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(csvFilePath))
                return result;

            var lines = File.ReadAllLines(csvFilePath).Skip(1);

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length < 2)
                    continue;

                string tag = parts[0].Trim();
                if (!long.TryParse(parts[1].Trim(), out long sumBytes))
                {
                    sumBytes = 0;
                }
                if (!long.TryParse(parts[2].Trim(), out long uniqueUserCount))
                {
                    uniqueUserCount = 0;
                }
                if (!long.TryParse(parts[3].Trim(), out long queryCount))
                {
                    queryCount = 0;
                }

                // Create a new details object
                var details = new QueryPerformanceDetails
                {
                    InvocationCount = queryCount,
                    UniqueUserCount = uniqueUserCount,
                    TotalBytes = sumBytes,
                    AdditionalInfo = "(Local CSV data)"
                };


                result[tag] = details;
            }

            return result;
        }

        /// <summary>
        /// Returns true if:
        /// 1) One string equals the other (case-insensitive), OR
        /// 2) One string ends with the other. E.g., 
        ///    "Common.FileService.UpdateCamasFileAsync" ends with "FileService.UpdateCamasFileAsync"
        /// </summary>
        private bool IsPartialMatch(string s1, string s2)
        {
            if (string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase))
                return true;

            if (s1.EndsWith(s2, StringComparison.OrdinalIgnoreCase))
                return true;

            if (s2.EndsWith(s1, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}