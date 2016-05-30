using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMachineLearning
{
    /// <summary>
    /// 
    /// </summary>
    public enum DataSourceType
    {
        /// <value>An ordinary text file</value>
        PlainText,
        /// <value>CSV file with a header</value>
        GenericCSV,
        /// <value>CSV file without a header</value>
        GenericCSVNoHeader,
        /// <value>TSV with header</value>
        GenericTSV,
        /// <value>TSV file without a header</value>
        GenericTSVNoHeader,
        /// <value>ARFF file</value>
        ARFF,
        /// <value>A zip file</value>
        Zip,
        /// <value>Rdata files</value>
        RData,
    }
}
