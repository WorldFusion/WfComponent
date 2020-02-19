namespace WfComponent.External.Properties
{
    public class RaconOptions : BaseOptions
    {
        [System.ComponentModel.DataAnnotations.Required()]
        public string sequences;

        public string overlaps;

        [System.ComponentModel.DataAnnotations.Required()]
        public string target;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outFile;


        public string useCore = System.Environment.ProcessorCount.ToString();

    }
}
