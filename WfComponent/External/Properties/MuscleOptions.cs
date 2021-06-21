namespace WfComponent.External.Properties
{
    public class MuscleOptions : BaseOptions
    {

        public static string outHtml = "-html";
        public static string outClustalW = "-clw";

        [System.ComponentModel.DataAnnotations.Required()]
        public string outFormat;


        [System.ComponentModel.DataAnnotations.Required()]
        public string targetFile;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outFile;



    }
}
