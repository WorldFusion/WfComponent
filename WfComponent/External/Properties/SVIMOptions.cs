namespace WfComponent.External.Properties
{
    public class SVIMOptions : BaseOptions
    {

        [System.ComponentModel.DataAnnotations.Required()]
        public string sequence;

        [System.ComponentModel.DataAnnotations.Required()]
        public string reference;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outDir;


        /**
         * tartget read sequence の シーケンサを指定する
         * デフォルトは PacBio
         * ONT の時は この値を true とする
         * */
        public bool isNanopore = false;
        // public bool isPacbio = false;

        /**
         * mapping の aligner を指定する
         * デフォルトは ngmlr
         * false の 時は minimap2 
         * */
        public bool isNgmlr = true;

        /**
         * # Use only high-quality alignments for SV calling (minimum mapping quality of 30)
         * Fastq の min-Quarity を指定する。
         * このオプションを指定しない場合、値を ０ にする（=引数を与えない）
         * */
        public int minQuality = 30;
    }
}
