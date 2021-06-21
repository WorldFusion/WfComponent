using System.ComponentModel.DataAnnotations;

namespace WfComponent.External.Properties
{
    public class BWAOptions : BaseOptions
    {

        [Required()]
        public string fwdFasta;
        public string revFasta;

        [Required()]
        public string reference;
        [Required()]
        public string outSam;
    }
}
