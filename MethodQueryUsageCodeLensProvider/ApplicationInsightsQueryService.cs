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
            if (_cache.TryGetValue(methodSignature, out var cacheEntry))
            {
                if ((DateTime.UtcNow - cacheEntry.fetchTime) < _cacheLifetime)
                {
                    return cacheEntry.details;
                }
            }

            string kustoQuery = $"traces | where message contains '{methodSignature}' | summarize count()";
            string timespan = "P1D";
            string url = $"https://api.applicationinsights.io/v1/apps/{_appId}/query" +
                         $"?query={Uri.EscapeDataString(kustoQuery)}&timespan={timespan}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-api-key", _apiKey);

            var details = new QueryPerformanceDetails();
            try
            {
                using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();

                    int count = ParseCountFromQuery(json);
                    details.QueryCount = count;
                    details.TotalBytes_Total = 0; // we don't track bytes from AI in this example
                    details.AdditionalInfo = "(AI-based data)";
                }
            }
            catch (Exception ex)
            {
                details.QueryCount = 0;
                details.AdditionalInfo = $"(AI error: {ex.Message})";
            }

            _cache[methodSignature] = (details, DateTime.UtcNow);
            return details;
        }

        private int ParseCountFromQuery(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                return doc.RootElement
                          .GetProperty("tables")[0]
                          .GetProperty("rows")[0][0]
                          .GetInt32();
            }
            catch
            {
                return 0;
            }
        }
    }
}
