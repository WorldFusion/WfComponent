namespace WfComponent.External.Properties
{
    public class FastQCOptions : BaseOptions
    {
        [System.ComponentModel.DataAnnotations.Required()]
        public string targetFastq;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outHtml;
    }
}
