using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mock
{
    public class ErrorContext
    {
        public string ErrorMessage { get; set; }

        public int ErrorCode { get; set; }

    }


    public class MeasureMetric
    {

        public static MeasureMetric Create(
            string monitoringAccount, 
            string metricNamespace, 
            string metricName, 
            ref ErrorContext errorContext,
            bool addDefaultDimensions,
            IEnumerable<string> dimensionNames
            )
        {
            return new MeasureMetric();
        }


        public bool LogValue(DateTime timestamp, 
            long value, 
            ref ErrorContext errorContext, 
            IEnumerable<string> dimensionNames)
        {
            return true;
        }


        public bool LogValue(long value,
            ref ErrorContext errorContext,
            IEnumerable<string> dimensionNames)
        {
            return true;
        }
    }
}
