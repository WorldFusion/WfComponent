using System.ComponentModel.DataAnnotations;

namespace WfComponent.External.Properties
{
    public class BWAIndexOptions : BaseOptions
    {
        [Required()]
        public string targetFasta;

        // public string referenceName;

    }
}
