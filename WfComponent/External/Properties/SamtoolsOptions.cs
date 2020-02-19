namespace WfComponent.External.Properties
{
    // http://www.htslib.org/doc/samtools.html
    public class SamtoolsOptions : BaseOptions
    {
        // public static readonly string WindowsBinaryName = "samtools.exe";
        public static readonly string fastaIndex = "faidx";
        public static readonly string fastqIndex = "fqidx";
        public static readonly string bamIndex = "index ";

        public static readonly string view = "view ";
        public static readonly string sort = "sort  ";
        public static readonly string outTypeBam = "-O bam";

        public string indexOp;

        [System.ComponentModel.DataAnnotations.Required()]
        public string targetFile;

        [System.ComponentModel.DataAnnotations.Required()]
        public string referenceFile;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outFile;

    }
}
