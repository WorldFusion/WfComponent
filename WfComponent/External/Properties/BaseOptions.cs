namespace WfComponent.External.Properties
{
    public abstract class BaseOptions
    {
        // public int processID;

        [System.ComponentModel.DataAnnotations.Required()]
        public string binaryPath;
        public bool isLinux;
        public string otherOptions;
    }
}
