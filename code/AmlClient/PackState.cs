using Newtonsoft.Json;

namespace AzureMachineLearning
{
    public abstract class PackState
    {
        public string Location { get; set; }
        public int ItemsComplete { get; set; }
        public int ItemsPending { get; set; }
        public string Status { get; set; }
        public string ActivityId { get; set; }

        [JsonIgnore]
        public abstract string IdField { get; }
    }

    public sealed class Packing : PackState
    {
        public override string IdField
        {
            get { return "packageActivityId=" + this.ActivityId; }
        }
    }

    public sealed class Unpacking : PackState
    {
        public override string IdField
        {
            get { return "unpackActivityId=" + this.ActivityId; }
        }
    }
}
