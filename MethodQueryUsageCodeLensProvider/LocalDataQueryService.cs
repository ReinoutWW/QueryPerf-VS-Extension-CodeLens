using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            // Safely trim the incoming signature to avoid null issues
            methodSignature = methodSignature?.Trim() ?? string.Empty;

            // Try to find a matching entry in the dictionary
            var matchingEntry = _dataStore.FirstOrDefault(
                entry => entry.Key.Contains(methodSignature)
            );

            // If we found a valid match (the Key won't be null in that case)
            if (!string.IsNullOrEmpty(matchingEntry.Key))
            {
                return Task.FromResult(matchingEntry.Value);
            }

            // Otherwise, return a "default" performance object
            return Task.FromResult(new QueryPerformanceDetails
            {
                InvocationCount = 0,
                TotalBytes = 0,
                UniqueUserCount = 0,
                AdditionalInfo = $"(No local CSV data found / partial match failed)"
            });
        }

        private Dictionary<string, QueryPerformanceDetails> LoadData(string csvFilePath)
        {
            var result = new Dictionary<string, QueryPerformanceDetails>(
                StringComparer.OrdinalIgnoreCase
            );

            // If no file, return empty
            if (!File.Exists(csvFilePath))
                return result;

            // Skip header row
            var lines = File.ReadAllLines(csvFilePath).Skip(1);

            foreach (var line in lines)
            {
                // Basic sanity check
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                // We expect at least 4 columns: Tag, sumBytes, uniqueUserCount, queryCount
                if (parts.Length < 4) continue;

                string tag = parts[0].Trim();
                if (!long.TryParse(parts[1].Trim(), out long sumBytes)) sumBytes = 0;
                if (!long.TryParse(parts[2].Trim(), out long uniqueUserCount)) uniqueUserCount = 0;
                if (!long.TryParse(parts[3].Trim(), out long queryCount)) queryCount = 0;

                // Create the details object
                var details = new QueryPerformanceDetails
                {
                    InvocationCount = queryCount,
                    UniqueUserCount = uniqueUserCount,
                    TotalBytes = sumBytes,
                    AdditionalInfo = "(Local CSV data)"
                };

                // Store it
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    result[tag] = details;
                }
            }

            return result;
        }
    }    
}