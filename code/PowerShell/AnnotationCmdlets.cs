using System.Management.Automation;

namespace AzureMLPS.PowerShell
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