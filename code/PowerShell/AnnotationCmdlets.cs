using AzureML.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AzureML.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlAnnotation")]
    public class GetAnnotation : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ExperimentId { get; set; }
        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(ExperimentId))
            {
                WriteWarning("ExperimentId must be specified.");
                return;
            }
            WriteObject(Sdk.GetAnnotation(GetWorkspaceSetting(), ExperimentId), true);
        }
    }
}