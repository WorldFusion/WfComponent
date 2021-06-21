using System;

namespace WfComponent.External.Properties
{
    public abstract class BaseOptions
    {
        // public int processID;

        [System.ComponentModel.DataAnnotations.Required()]
        public string binaryPath = string.Empty;
        public bool isLinux ;
        public string otherOptions;

        // add 2021.05
        public IProgress<string> progress;
    }
}
