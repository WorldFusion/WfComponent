using System.ComponentModel.DataAnnotations;

namespace WfComponent.External.Properties
{
    public class SeqkitOptions : BaseOptions
    {
        [Required()]
        public string fastqPath;
        // public string fastqPath2;

        [Required()]
        public string outFastq;
        // public string outFastq2;


        [Required()]
        public string subCommand;
        public string threads;
        public string acquireCounts = "-s100";   // ランダム変数 通常は固定
        public string samplingNumbers = "8000";  // デフォルト リード数



    }
}
