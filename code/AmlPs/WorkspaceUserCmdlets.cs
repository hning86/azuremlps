using AzureMachineLearning.PowerShell;
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

namespace AzureMachineLearning.PowerShell
{

    //[Cmdlet(VerbsCommon.New, "AmlWorkspace")]
    //public class NewWorkspace : AmlCmdlet
    //{
    //    [Parameter(Mandatory = true)]
    //    public string WorkspaceName;
    //    [Parameter(Mandatory = true)]
    //    [ValidateSet("South Central US", "West Europe", "Southeast Asia")]
    //    public string Location;
    //    [Parameter(Mandatory = true)]
    //    public string StorageAccountName;
    //    [Parameter(Mandatory = true)]
    //    public string StorageAccountKey;
    //    [Parameter(Mandatory = true)]
    //    public string OwnerEmail;
    //    protected override void ProcessRecord()
    //    {
    //        ProgressRecord pr = new ProgressRecord(1, "Create Workspace", WorkspaceName);
    //        pr.PercentComplete = 1;
    //        pr.CurrentOperation = "Creating...";
    //        WriteProgress(pr);
    //        Task<string> createWS = Workspace.CreateWorkspace(ManagementCertThumbprint, AzureSubscriptionId, WorkspaceName, Location, StorageAccountName, StorageAccountKey, OwnerEmail, "PowerShell");
    //        while (!createWS.IsCompleted)
    //        {
    //            if (pr.PercentComplete < 100)
    //                pr.PercentComplete++;
    //            else
    //                pr.PercentComplete = 1;
    //            WriteProgress(pr);
    //            Thread.Sleep(500);
    //        }
    //        pr.StatusDescription = string.Format("Getting status for Workspace\"{0}\"", WorkspaceName);
    //        pr.CurrentOperation = "Getting status...";
    //        WriteProgress(pr);
    //        string wsId = createWS.Result;
    //        WorkspaceStorage ws = Workspace.GetCreateWorkspaceStatus(ManagementCertThumbprint, AzureSubscriptionId, wsId);
    //        pr.CurrentOperation = "Status: " + ws.WorkspaceState;
    //        WriteProgress(pr);
    //        while (ws.WorkspaceState != "Enabled")
    //        {
    //            pr.CurrentOperation = "Status: " + ws.WorkspaceState;
    //            WriteProgress(pr);
    //            if (pr.PercentComplete < 100)
    //                pr.PercentComplete++;
    //            else
    //                pr.PercentComplete = 1;                
    //            Thread.Sleep(500);
    //            ws = Workspace.GetCreateWorkspaceStatus(ManagementCertThumbprint, AzureSubscriptionId, wsId);
    //        }
    //        pr.PercentComplete = 100;
    //        WriteProgress(pr);

    //        WriteObject(ws);
    //    }
    //}

    [Cmdlet(VerbsCommon.Get, "AmlWorkspaceUser")]
    public class GetWorkspaceUser : AmlCmdlet
    {
        [Parameter(Mandatory = true)]
        public string WorkspaceId;

        protected override void ProcessRecord()
        {
            WorkspaceUser[] users = WorkspaceEx.GetWorkspaceUsers(GetWorkspaceSetting());
            WriteObject(users);
        }
    }
}