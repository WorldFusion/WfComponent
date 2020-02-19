using System.ComponentModel.DataAnnotations;

namespace WfComponent.External.Properties
{
    public class Bowtie2Options : BaseOptions
    {

        public bool isFasta = true;
        public bool isFastq = false;
        [Required()]
        public string fwdFasta;
        public string revFasta;

        [Required()]

        public string reference;
        [Required()]
        public string outSam;
    }
}
