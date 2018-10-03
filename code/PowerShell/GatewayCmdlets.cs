using System.Management.Automation;

namespace AzureMLPS.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlGateway")]
    public class GetGateway : AzureMLPsCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(Sdk.GetGateway(GetWorkspaceSetting()), true);
        }
    }
}