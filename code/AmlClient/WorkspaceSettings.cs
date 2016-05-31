using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace AzureMachineLearning
{
    public sealed class WorkspaceConfig
    {
        public string WorkspaceId { get; set; }
        public string Location { get; set; }
    }
}
