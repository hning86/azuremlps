using System.Management.Automation;

namespace AzureMLPS.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlNotebook")]
    public class GetNotebook : AzureMLPsCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(Sdk.GetNotebook(GetWorkspaceSetting()), true);
        }
    }

    [Cmdlet(VerbsCommon.Get, "AmlNotebookAttachmentName")]
    public class GetNotebookAttachmentName : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string FamilyId { get; set; }
        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(FamilyId))
            {
                WriteWarning("FamilyId must be specified.");
                return;
            }
            WriteObject(Sdk.GetNotebookAttachmentName(GetWorkspaceSetting(), FamilyId), true);
        }
    }

    [Cmdlet(VerbsCommon.Get, "AmlNotebookSession")]
    public class GetNotebookSession : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string FamilyId { get; set; }
        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(FamilyId))
            {
                WriteWarning("FamilyId must be specified.");
                return;
            }
            WriteObject(Sdk.GetNotebookSession(GetWorkspaceSetting(), FamilyId), true);
        }
    }
}