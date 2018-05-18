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
    [Cmdlet(VerbsCommon.Get, "AmlProjectContainer")]
    public class GetProjectContainer : AzureMLPsCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(Sdk.GetProjectContainer(GetWorkspaceSetting()), true);
        }
    }
}