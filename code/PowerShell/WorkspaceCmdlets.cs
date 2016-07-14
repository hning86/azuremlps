using AzureML.Contract;
using AzureML.PowerShell;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace AzureML.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AmlWorkspace")]
    public class GetWorkspace : AzureMLPsCmdlet
    {     
        protected override void ProcessRecord()
        {
            Workspace ws = this.Sdk.GetWorkspaceFromAmlRP(GetWorkspaceSetting());
            WriteObject(ws);
        }     
    }

    [Cmdlet("List", "AmlWorkspaces")]
    public class ListWorkspaces : AzureMLPsCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string ManagementCertThumbprint;
        [Parameter(Mandatory = true)]
        public string AzureSubscriptionId;
        protected override void ProcessRecord()
        {
            WorkspaceRdfe[] workspaces = Sdk.GetWorkspacesFromRdfe(ManagementCertThumbprint, AzureSubscriptionId);
            WriteObject(workspaces, true);
        }
    }

    [Cmdlet(VerbsCommon.New, "AmlWorkspace")]
    public class NewWorkspace : AzureMLPsCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string ManagementCertThumbprint;
        [Parameter(Mandatory = true)]
        public string AzureSubscriptionId;
        [Parameter(Mandatory = true)]
        public string WorkspaceName;
        [Parameter(Mandatory = true)]
        [ValidateSet("South Central US", "West Europe", "Southeast Asia")]
        public string Location;
        [Parameter(Mandatory = true)]
        public string StorageAccountName;
        [Parameter(Mandatory = true)]
        public string StorageAccountKey;
        [Parameter(Mandatory = true)]
        public string OwnerEmail;
        protected override void ProcessRecord()
        {
            ProgressRecord pr = new ProgressRecord(1, "Create Workspace", WorkspaceName);
            pr.PercentComplete = 1;
            pr.CurrentOperation = "Creating...";
            WriteProgress(pr);
            Task<string> createWS = Sdk.CreateWorkspace(ManagementCertThumbprint, AzureSubscriptionId, WorkspaceName, Location, StorageAccountName, StorageAccountKey, OwnerEmail, "PowerShell");
            while (!createWS.IsCompleted)
            {
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;
                WriteProgress(pr);
                Thread.Sleep(500);
            }
            pr.StatusDescription = string.Format("Getting status for Workspace\"{0}\"", WorkspaceName);
            pr.CurrentOperation = "Getting status...";
            WriteProgress(pr);
            string wsId = createWS.Result;
            WorkspaceRdfe ws = Sdk.GetCreateWorkspaceStatus(ManagementCertThumbprint, AzureSubscriptionId, wsId, Location);
            pr.CurrentOperation = "Status: " + ws.WorkspaceState;
            WriteProgress(pr);
            while (ws.WorkspaceState != "Enabled")
            {
                pr.CurrentOperation = "Status: " + ws.WorkspaceState;
                WriteProgress(pr);
                if (pr.PercentComplete < 100)
                    pr.PercentComplete++;
                else
                    pr.PercentComplete = 1;                
                Thread.Sleep(500);
                ws = Sdk.GetCreateWorkspaceStatus(ManagementCertThumbprint, AzureSubscriptionId, wsId, Location);
            }
            pr.PercentComplete = 100;
            WriteProgress(pr);

            WriteObject(ws);
        }
    }

    [Cmdlet(VerbsCommon.Remove, "AmlWorkspace")]
    public class RemoveWorkspace: AzureMLPsCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string ManagementCertThumbprint;
        [Parameter(Mandatory = true)]
        public string AzureSubscriptionId;
        [Parameter(Mandatory = true)]
        public string WorkspaceId;
        protected override void ProcessRecord()
        {
            var workspaces = Sdk.GetWorkspacesFromRdfe(ManagementCertThumbprint, AzureSubscriptionId);
            var workspace = workspaces.SingleOrDefault(w => w.Id.ToLower() == WorkspaceId.ToLower());
            Sdk.RemoveWorkspace(ManagementCertThumbprint, AzureSubscriptionId, WorkspaceId, workspace.Region);
            WriteObject("Workspace removed.");
        }
    }

    [Cmdlet(VerbsCommon.Add, "AmlWorkspaceUsers")]
    public class AddWorkspaceUsers : AzureMLPsCmdlet {
        [Parameter(Mandatory = true)]
        public string Emails { get; set; }
        [Parameter(Mandatory = true)]
        [ValidateSet("User", "Owner")]
        public string Role { get; set; }
        public AddWorkspaceUsers() { }
        protected override void ProcessRecord()
        {            
            Sdk.AddWorkspaceUsers(GetWorkspaceSetting(), Emails, Role);
            WriteObject("User(s) added to the Workspace.");
        }
    }

    [Cmdlet(VerbsCommon.Get, "AmlWorkspaceUsers")]
    public class GetWorkspaceUsers : AzureMLPsCmdlet
    {
        public GetWorkspaceUsers() { }
        protected override void ProcessRecord()
        {
            WorkspaceUser[] users = Sdk.GetWorkspaceUsers(GetWorkspaceSetting());
            WriteObject(users);
        }
    }
}