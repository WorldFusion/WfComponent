using System.ComponentModel.DataAnnotations;

namespace WfComponent.External.Properties
{
    public class CdHitOptions : BaseOptions
    {
        [Required()]
        public string fromFasta;
        [Required()]
        public string toFasta;
        [Required()]
        public string identCutoff;

    }
}
