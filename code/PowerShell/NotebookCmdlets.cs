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