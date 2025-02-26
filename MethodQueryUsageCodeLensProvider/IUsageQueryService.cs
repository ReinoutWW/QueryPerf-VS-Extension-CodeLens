using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQueryUsageCodeLensProvider
{
    public interface IUsageQueryService
    {
        /// <summary>
        /// Fetches usage/performance data for a given method signature.
        /// e.g. "MyNamespace.MyClass.MyMethod"
        /// </summary>
        Task<QueryPerformanceDetails> GetMethodPerformanceDetailsAsync(string methodSignature);
    }
}
