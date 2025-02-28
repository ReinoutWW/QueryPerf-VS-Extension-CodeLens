using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MethodQueryUsageCodeLensProvider
{
    internal sealed class ApplicationInsightsQueryService : IUsageQueryService
    {
        private readonly string _appId;
        private readonly string _apiKey;
        private static readonly HttpClient _httpClient = new HttpClient();

        private static readonly ConcurrentDictionary<string, (QueryPerformanceDetails details, DateTime fetchTime)> _cache
            = new ConcurrentDictionary<string, (QueryPerformanceDetails, DateTime)>();

        private static readonly TimeSpan _cacheLifetime = TimeSpan.FromHours(1);

        public ApplicationInsightsQueryService(string appId, string apiKey)
        {
            _appId = appId ?? throw new ArgumentNullException(nameof(appId));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public async Task<QueryPerformanceDetails> GetMethodPerformanceDetailsAsync(string methodSignature)
        {
            if (_cache.TryGetValue(methodSignature, out var entry) &&
                (DateTime.UtcNow - entry.fetchTime) < _cacheLifetime)
            {
                return entry.details;
            }

            string kustoQuery = $@"
            customEvents
            | where name == 'TelemetryEvent.Query.Intercepted'
            | extend 
                bytesSent        = tolong(customMeasurements['TelemetryProperty.Query.BytesSent']),
                bytesReceived    = tolong(customMeasurements['TelemetryProperty.Query.BytesReceived']),
                execTime         = tolong(customMeasurements['TelemetryProperty.Query.ExecutionTime']),
                selectRows       = tolong(customMeasurements['TelemetryProperty.Query.SelectRows']),
                iduRows          = tolong(customMeasurements['TelemetryProperty.Query.IduRows']),
                columnCount      = tolong(customMeasurements['TelemetryProperty.Query.ColumnCount']),
                queryText        = tostring(customDimensions['TelemetryProperty.Query.QueryString'])
            | extend queryText = replace_regex(queryText, '^-- ', '')
            | extend Tag       = extract(@'^(.+?)(?:\s*\(|\s*\[TagWithCallFrom)', 1, queryText)
            | extend totalBytes = bytesSent + bytesReceived
            | where isnotnull(Tag)
            | where Tag contains '{methodSignature}'
            | summarize
                QueryCount          = count(),
                UniqueUserCount     = dcount(user_Id),
                BytesSent_Min       = min(bytesSent),
                BytesSent_Max       = max(bytesSent),
                BytesSent_Avg       = avg(bytesSent),
                BytesSent_Total     = sum(bytesSent),
                BytesReceived_Min   = min(bytesReceived),
                BytesReceived_Max   = max(bytesReceived),
                BytesReceived_Avg   = avg(bytesReceived),
                BytesReceived_Total = sum(bytesReceived),
                TotalBytes_Min      = min(totalBytes),
                TotalBytes_Max      = max(totalBytes),
                TotalBytes_Avg      = avg(totalBytes),
                TotalBytes_Total    = sum(totalBytes),
                Rows_Min            = min(selectRows),
                Rows_Max            = max(selectRows),
                Rows_Avg            = avg(selectRows),
                Rows_Total          = sum(selectRows),
                Columns_Min         = min(columnCount),
                Columns_Max         = max(columnCount),
                Columns_Avg         = avg(columnCount),
                Columns_Total       = sum(columnCount),
                ExecutionTime_Min   = min(execTime),
                ExecutionTime_Max   = max(execTime),
                ExecutionTime_Avg   = avg(execTime),
                ExecutionTime_Total = sum(execTime)
              by Tag
            ";

            string timespan = "P7D";
            string url = $"https://api.applicationinsights.io/v1/apps/{_appId}/query" +
                         $"?query={Uri.EscapeDataString(kustoQuery)}&timespan={timespan}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-api-key", _apiKey);

            QueryPerformanceDetails details;
            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                details = ParseByColumnName(json);
                details.Tag = methodSignature;
                details.AdditionalInfo = $"Past 7d | Application Insights | Tag {methodSignature}";
            }
            catch (Exception ex)
            {
                details = new QueryPerformanceDetails
                {
                    Tag = methodSignature,
                    AdditionalInfo = $"(API error: {ex.Message})"
                };
            }

            _cache[methodSignature] = (details, DateTime.UtcNow);
            return details;
        }

        /// <summary>
        /// Converts the JSON from the AI query into QueryPerformanceDetails,
        /// using column names instead of relying on row positions.
        /// </summary>
        private QueryPerformanceDetails ParseByColumnName(string json)
        {
            var doc = JsonDocument.Parse(json);

            JsonElement table = doc.RootElement.GetProperty("tables")[0];

            Dictionary<string, int> columnMap = BuildColumnMap(table.GetProperty("columns"));

            JsonElement rows = table.GetProperty("rows");
            if (rows.GetArrayLength() == 0)
            {
                return new QueryPerformanceDetails { AdditionalInfo = "No rows returned." };
            }

            JsonElement row = rows[0];

            var details = new QueryPerformanceDetails
            {
                QueryCount = ValueFormatHelper.GetLong(row, columnMap, "QueryCount"),
                UniqueUserCount = ValueFormatHelper.GetLong(row, columnMap, "UniqueUserCount"),

                BytesSent_Min = ValueFormatHelper.GetLong(row, columnMap, "BytesSent_Min"),
                BytesSent_Max = ValueFormatHelper.GetLong(row, columnMap, "BytesSent_Max"),
                BytesSent_Avg = ValueFormatHelper.GetDouble(row, columnMap, "BytesSent_Avg"),
                BytesSent_Total = ValueFormatHelper.GetLong(row, columnMap, "BytesSent_Total"),

                BytesReceived_Min = ValueFormatHelper.GetLong(row, columnMap, "BytesReceived_Min"),
                BytesReceived_Max = ValueFormatHelper.GetLong(row, columnMap, "BytesReceived_Max"),
                BytesReceived_Avg = ValueFormatHelper.GetDouble(row, columnMap, "BytesReceived_Avg"),
                BytesReceived_Total = ValueFormatHelper.GetLong(row, columnMap, "BytesReceived_Total"),

                TotalBytes_Min = ValueFormatHelper.GetLong(row, columnMap, "TotalBytes_Min"),
                TotalBytes_Max = ValueFormatHelper.GetLong(row, columnMap, "TotalBytes_Max"),
                TotalBytes_Avg = ValueFormatHelper.GetDouble(row, columnMap, "TotalBytes_Avg"),
                TotalBytes_Total = ValueFormatHelper.GetLong(row, columnMap, "TotalBytes_Total"),

                Rows_Min = ValueFormatHelper.GetLong(row, columnMap, "Rows_Min"),
                Rows_Max = ValueFormatHelper.GetLong(row, columnMap, "Rows_Max"),
                Rows_Avg = ValueFormatHelper.GetDouble(row, columnMap, "Rows_Avg"),
                Rows_Total = ValueFormatHelper.GetLong(row, columnMap, "Rows_Total"),

                Columns_Min = ValueFormatHelper.GetLong(row, columnMap, "Columns_Min"),
                Columns_Max = ValueFormatHelper.GetLong(row, columnMap, "Columns_Max"),
                Columns_Avg = ValueFormatHelper.GetDouble(row, columnMap, "Columns_Avg"),
                Columns_Total = ValueFormatHelper.GetLong(row, columnMap, "Columns_Total"),

                ExecutionTime_Min = ValueFormatHelper.GetLong(row, columnMap, "ExecutionTime_Min"),
                ExecutionTime_Max = ValueFormatHelper.GetLong(row, columnMap, "ExecutionTime_Max"),
                ExecutionTime_Avg = ValueFormatHelper.GetDouble(row, columnMap, "ExecutionTime_Avg"),
                ExecutionTime_Total = ValueFormatHelper.GetLong(row, columnMap, "ExecutionTime_Total")
            };

            return details;
        }

        /// <summary>
        /// Reads all column definitions in the 'columns' array
        /// and builds a map of { columnName -> columnIndex }.
        /// </summary>
        private static Dictionary<string, int> BuildColumnMap(JsonElement columnsArray)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < columnsArray.GetArrayLength(); i++)
            {
                var colName = columnsArray[i].GetProperty("name").GetString();
                if (colName != null)
                {
                    map[colName] = i;
                }
            }
            return map;
        }
    }
}
