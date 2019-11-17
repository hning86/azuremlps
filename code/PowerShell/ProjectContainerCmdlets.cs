using System.Management.Automation;

namespace AzureMLPS.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlProjectContainer")]
    public class GetProjectContainer : AzureMLPsCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(Sdk.GetProjectContainer(GetWorkspaceSetting()), true);
        }
    }
}