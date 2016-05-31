using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMachineLearning
{
    public sealed class ExperimentStatus
    {
        public string StatusCode { get; set; }
        public string StatusDetail { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Metadata { get; set; }

        public ExperimentStatus Clone()
        {
            var s = (ExperimentStatus)this.MemberwiseClone();
            return s;
        }
    }

    public sealed class Experiment : IEntityTag
    {
        public string ExperimentId { get; set; }
        public string RunId { get; set; }
        public string ParentExperimentId { get; set; }
        public string OriginalExperimentDocumentationLink { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Graph { get; set; }
        public ExperimentStatus Status { get; set; }
        public string Etag { get; set; }
        public string NodeStatuses { get; set; }
        public string Creator { get; set; }
        public bool IsLeaf { get; set; }
        public bool DisableNodesUpdate { get; set; }
        public string WebService { get; set; }
        public string Category { get; set; }
        public string[] Tags { get; set; }
        public bool IsPartialRun { get; set; }

        public Experiment() { }

        public Experiment Clone()
        {
            var e = (Experiment) this.MemberwiseClone();
            e.Status = this.Status.Clone();
            return e;
        }
    }

}
