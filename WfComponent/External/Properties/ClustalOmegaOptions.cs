namespace WfComponent.External.Properties
{
   public  class ClustalOmegaOptions : BaseOptions
    {
        public static string outFasta = "fasta";
        public static string outClustal = "clustal";
        public static string outMsf = "msf";
        public static string outPhylip = "phylip";
        public static string outSelex = "selex";
        public static string outVienna = "vienna";
        

        /// <summary>
        ///  Alignment file type
        /// </summary>
        public string outFormat = outMsf; // default


        /// <summary>
        ///  multi fasta > 3-seqs
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required()]
        public string targetFile;

        /// <summary>
        ///  targetFile + outFormat
        /// </summary>
        public string outFile;

        /// <summary>
        ///  for TreeXvIew or  njplot  file
        ///  --guidetree-out 
        /// </summary>
        public string outGuidTreeFile;

        /// <summary>
        ///  Use CPU core
        /// </summary>
        public string Threads;

    }
}
