namespace WfComponent.External.Properties
{
    public class PycoQCOptions : BaseOptions
    {
        [System.ComponentModel.DataAnnotations.Required()]
        public string summaryText;

        [System.ComponentModel.DataAnnotations.Required()]
        public string targetBam;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outHtml;
    }
}
