using AzureML.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace AzureML.PowerShell
{
    [Cmdlet("List", "AmlProject")]
    public class ListProjects : AzureMLPsCmdlet
    {
        protected override void ProcessRecord()
        {
            Project[] projects = Sdk.GetProjects(GetWorkspaceSetting());
            WriteObject(projects);
        }
    }

    [Cmdlet(VerbsCommon.Get, "AmlProject")]
    public class GetProject : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ProjectId { get; set; }

        protected override void ProcessRecord()
        {
            Project project = Sdk.GetProject(GetWorkspaceSetting(), ProjectId);
            WriteObject(project);
        }
    }

    [Cmdlet(VerbsCommon.Get, "AmlAssetDependencies")]
    public class GetAssetDependencies : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public Assets Assets { get; set; }

        protected override void ProcessRecord()
        {
            Assets assets = Sdk.GetAssetDependencies(GetWorkspaceSetting(), Assets);
            WriteObject(assets);
        }
    }

    [Cmdlet("Update", "AmlProjectAssets")]
    public class AddAssetsToProject : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ProjectId { get; set; }
        [Parameter(Mandatory = false)]
        public Assets Assets { get; set; }

        protected override void ProcessRecord()
        {
            Project project = Sdk.AddAssetsToProject(GetWorkspaceSetting(), Assets, ProjectId);

            WriteObject(project);
        }
    }

    [Cmdlet(VerbsCommon.New, "AmlProject")]
    public class NewProject : AzureMLPsCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }
        [Parameter(Mandatory = false)]
        public string Description { get; set; }

        protected override void ProcessRecord()
        {
            Project project = Sdk.CreateProject(GetWorkspaceSetting(), Name, Description);
            WriteObject(project);
        }
    }
}
