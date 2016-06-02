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
    [Cmdlet(VerbsCommon.Get, "WorkspaceStorage")]
    public class GetWorkspaceStorage : AmlCmdlet
    {
        [Parameter()]
        public string Id;

        protected override void ProcessRecord()
        {
            var client = GetClient();
            if (Id == null)
            {
                var wsa = client.Get().GetAwaiter().GetResult();
                WriteObject(wsa);
                return;
            }

            var ws = client.Get(Id).GetAwaiter().GetResult();
            WriteObject(ws);
        }
    }

    [Cmdlet(VerbsCommon.New, "WorkspaceStorage")]
    public class NewWorkspaceStorage : AmlCmdlet
    {
    }

    [Cmdlet(VerbsCommon.Remove, "WorkspaceStorage")]
    public class RemoveWorkspaceStorage : AmlCmdlet
    {
    }
    }