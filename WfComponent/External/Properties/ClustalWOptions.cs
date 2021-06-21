namespace WfComponent.External.Properties
{

    // https://www.ebi.ac.uk/Tools/msa/clustalw2/
    public class ClustalWOptions : BaseOptions
    {

        public static readonly string bootstrap = "-BOOTSTRAP ";
        public static readonly string typeDna = "-TYPE=DNA  ";
        public static readonly string phbFooter = ".phb ";


        [System.ComponentModel.DataAnnotations.Required()]
        public string targetFile;

        [System.ComponentModel.DataAnnotations.Required()]
        public string inputDataType;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outFormatType;


        [System.ComponentModel.DataAnnotations.Required()]
        public string outFile;

    }
}
