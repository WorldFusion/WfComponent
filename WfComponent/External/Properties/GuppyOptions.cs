namespace WfComponent.External.Properties
{
    public class GuppyOptions : BaseOptions
    {

        [System.ComponentModel.DataAnnotations.Required()]
        public string Fast5Dir = string.Empty;

        public string FastqDir = string.Empty;

        [System.ComponentModel.DataAnnotations.Required()]
        public string OutDir = string.Empty;


        public string OtherBasecallOption = string.Empty;
        public string OtherBarcodeOption = string.Empty;

        public string Config = string.Empty;
    }
}
