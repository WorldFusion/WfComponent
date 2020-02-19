using System.ComponentModel.DataAnnotations;

namespace WfComponent.External.Properties
{
    public class Bowtie2IndexOption : BaseOptions
    {
        [Required()]
        public string targetFasta;

        [Required()]
        public string referenceName;

    }
}
