namespace WfComponent.External.Properties
{
    public class MiniasmOptions : BaseOptions
    {

        [System.ComponentModel.DataAnnotations.Required()]
        public string sequences;

        [System.ComponentModel.DataAnnotations.Required()]
        public string referencePaf;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outGaf;

    }
}
