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
            methodSignature = methodSignature?.Trim() ?? string.Empty;

            var matchingEntry = _dataStore.FirstOrDefault(
                entry => entry.Key.Contains(methodSignature)
            );

            if (!string.IsNullOrEmpty(matchingEntry.Key))
            {
                return Task.FromResult(matchingEntry.Value);
            }

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

            if (!File.Exists(csvFilePath))
                return result;

            var lines = File.ReadAllLines(csvFilePath);

            if (lines.Length < 2)
                return result;

            var header = lines[0];
            var headerParts = header.Split(',');

            var columnIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headerParts.Length; i++)
            {
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
            
            ReadColumnDataFromLines(result, lines, columnIndexByName, props);

            return result;
        }

        private void ReadColumnDataFromLines(Dictionary<string, QueryPerformanceDetails> result, string[] lines, Dictionary<string, int> columnIndexByName, Dictionary<string, PropertyInfo> props)
        {
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                var details = new QueryPerformanceDetails();
                details.AdditionalInfo = "(Local CSV data)";
                SetColumnValues(columnIndexByName, props, parts, details);

                if (string.IsNullOrWhiteSpace(details.Tag))
                    continue;

                result[details.Tag] = details;
            }
        }

        private void SetColumnValues(Dictionary<string, int> columnIndexByName, Dictionary<string, PropertyInfo> props, string[] parts, QueryPerformanceDetails details)
        {
            foreach (var kvp in columnIndexByName)
            {
                string colName = kvp.Key;
                int colIndex = kvp.Value;

                if (colIndex >= parts.Length)
                    continue;

                string rawValue = parts[colIndex].Trim().Trim('"');

                if (props.TryGetValue(colName, out PropertyInfo propInfo))
                {
                    object parsedValue = ConvertValue(rawValue, propInfo.PropertyType);
                    propInfo.SetValue(details, parsedValue);
                }
            }
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