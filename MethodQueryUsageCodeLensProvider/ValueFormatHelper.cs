using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQueryUsageCodeLensProvider
{
    public static class ValueFormatHelper
    {
        /// <summary>
        /// Converts milliseconds to a short time string.
        /// Examples:
        ///   750      => "750ms"
        ///   1000     => "1s"
        ///   45000    => "45s"
        ///   120000   => "2m"
        ///   5400000  => "1.5h"
        ///   86400000 => "1d"
        /// </summary>
        public static string FormatMilliseconds(long ms)
        {
            if (ms < 0)
                return "N/A"; // or handle negative differently, if desired

            // If under 1000, just show "xxx ms"
            if (ms < 1000)
                return $"{ms}ms";

            double seconds = ms / 1000.0;
            if (seconds < 60)
                return $"{seconds:0.##}s";

            double minutes = seconds / 60.0;
            if (minutes < 60)
                return $"{minutes:0.##}m";

            double hours = minutes / 60.0;
            if (hours < 24)
                return $"{hours:0.##}h";

            double days = hours / 24.0;
            return $"{days:0.##}d";
        }

        /// <summary>
        /// Returns "N/A" if null. If the object is a long, we convert it as a time.
        /// Otherwise just do a generic ToString() or "N/A".
        /// </summary>
        public static string ValueOrNAAsTime(object v)
        {
            if (v == null)
                return "N/A";

            if (v is long msValue)
                return FormatMilliseconds(msValue);

            if(v is double msValueDouble)
                return FormatMilliseconds((long)msValueDouble);

            return "N/A";
        }

        public static string FormatDouble(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d))
                return "N/A";
            double rounded = Math.Round(d);
            if (Math.Abs(d - rounded) < 0.0000001)
                return ((long)rounded).ToString();
            return d.ToString("0.##");
        }

        /// <summary>
        /// Converts a long to a short thousands string. 
        /// For example, 100,000 -> "100k", 99,999 -> "100k" (rounded), 123,456 -> "123k".
        /// </summary>
        public static string FormatLongAsThousands(long value)
        {
            // If below 1,000, just show the exact value
            if (value < 1000)
                return value.ToString();

            // Otherwise, convert to thousands and round
            double thousands = (double)value / 1000.0;
            long rounded = (long)Math.Round(thousands);
            return $"{rounded}k";
        }

        /// <summary>
        /// Returns "N/A" if null. If the object is a long, convert it to a "k" format (e.g. 123,456 => "123k").
        /// Otherwise, return the object's .ToString().
        /// </summary>
        public static string ValueOrNA(object v)
        {
            if (v == null)
                return "N/A";

            // If it's a long, convert to a thousands string
            if (v is long longValue)
                return FormatLongAsThousands(longValue);

            if (v is double doubleValue)
                return FormatLongAsThousands((long)doubleValue);

            // Otherwise fall back to .ToString()
            return v.ToString() ?? "N/A";
        }

        public static string FormatLongBytes(long b) => b == 0 ? "0B" : b > 0 ? BytesToReadableStringDouble(b) : "N/A";

        public static string FormatDoubleBytes(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d) || d <= 0)
                return "N/A";
            return BytesToReadableStringDouble(d);
        }

        public static string BytesToReadableStringDouble(double bytes)
        {
            if (bytes < 1.0)
                return "0B";

            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int order = 0;

            while (bytes >= 1000 && order < units.Length - 1)
            {
                order++;
                bytes /= 1000;
            }

            return $"{bytes:0.##}{units[order]}";
        }

    }
}
