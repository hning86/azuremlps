using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMachineLearning
{
    public sealed class ResourceUploadResponse
    {
        public string Id { get; set; }
        public string DataTypeId { get; set; }
        public string ContentType { get; set; }
        public string Committed { get; set; }
        public string WorkspaceId { get; set; }
        public string SharedAccessUri { get; set; }
    }
}
