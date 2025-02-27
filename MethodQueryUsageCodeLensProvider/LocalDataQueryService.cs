using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
                QueryCount = 0,
                TotalBytes_Total = 0,
                UniqueUserCount = 0,
                AdditionalInfo = $"(No local CSV data found / partial match failed)"
            });
        }

        private Dictionary<string, QueryPerformanceDetails> LoadData(string csvFilePath)
        {
            var result = new Dictionary<string, QueryPerformanceDetails>(StringComparer.OrdinalIgnoreCase);

            // If file doesn't exist, return empty
            if (!File.Exists(csvFilePath))
                return result;

            var lines = File.ReadAllLines(csvFilePath);

            if (lines.Length < 2)
                return result; // No data lines

            // 1) Parse the header to build a column name → index map
            var header = lines[0];
            var headerParts = header.Split(',');

            // Map: "Tag" -> 0, "totalBytesSum" -> 1, etc.
            var columnIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headerParts.Length; i++)
            {
                // Trim whitespace then remove surrounding quotes if present
                var colName = headerParts[i].Trim().Trim('"');
                if (!string.IsNullOrEmpty(colName) && !columnIndexByName.ContainsKey(colName))
                {
                    columnIndexByName[colName] = i;
                }
            }

            var props = typeof(QueryPerformanceDetails)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                var details = new QueryPerformanceDetails();
                details.AdditionalInfo = "(Local CSV data)";

                foreach (var kvp in columnIndexByName)
                {
                    string colName = kvp.Key;        // e.g. BytesSent_Min
                    int colIndex = kvp.Value;        // e.g. 3

                    if (colIndex >= parts.Length)
                        continue;

                    // Now strip quotes from the raw field (like "783.375")
                    string rawValue = parts[colIndex].Trim().Trim('"');

                    if (props.TryGetValue(colName, out PropertyInfo propInfo))
                    {
                        object parsedValue = ConvertValue(rawValue, propInfo.PropertyType);
                        propInfo.SetValue(details, parsedValue);
                    }
                }

                if (string.IsNullOrWhiteSpace(details.Tag))
                    continue;

                result[details.Tag] = details;
            }

            return result;
        }

        /// <summary>
        /// Safely converts a raw CSV string into the specified type (long, double, or string).
        /// Returns default(T) on parse failure.
        /// Extend as needed (DateTime, decimal, etc.).
        /// </summary>
        private object ConvertValue(string rawValue, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return rawValue;
            }
            if (targetType == typeof(long))
            {
                if (long.TryParse(rawValue, out long l)) return l;
                return default(long); 
            }
            if (targetType == typeof(int))
            {
                if (int.TryParse(rawValue, out int i)) return i;
                return default(int);
            }
            if (targetType == typeof(double))
            {
                if (double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                    return d;
                return default(double); 
            }
            return null;
        }
    }    
}