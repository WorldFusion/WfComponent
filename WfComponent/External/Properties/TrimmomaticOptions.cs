using System.ComponentModel.DataAnnotations;

namespace WfComponent.External.Properties
{
    public class TrimmomaticOptions : BaseOptions
    {

        [Required()]
        public string fastqPath1;
        public string fastqPath2;

        [Required()]
        public string outFastq1;
        public string outFastq2;

        public string threads;
        [Required()]
        public string minPhreadScore;
        [Required()]
        public string windowSize;
        [Required()]
        public string minLength;
        // public string logfile;


    }
}
