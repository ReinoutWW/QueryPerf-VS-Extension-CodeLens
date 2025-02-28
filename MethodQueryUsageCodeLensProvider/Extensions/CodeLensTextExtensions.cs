using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQueryUsageCodeLensProvider.Extensions
{
    public static class CodeLensTextExtensions
    {
        public static string GetDisplayString(this QueryPerformanceDetails details)
        {
            return details.QueryCount != 0
                ? details.ToString()
                : "0 Query's logged";
        }
    }
}
