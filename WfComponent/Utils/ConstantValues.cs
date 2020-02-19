namespace WfComponent.Utils
{
    public static class ConstantValues
    {
        public const bool NormalEnd = false;
        public const bool ErrorEnd = true;

        public const string CanceledMessage = "canceled";
        public const string ErrorMessage = "external program error";
        public const string NormalEndMessage = "";

        public static readonly string ConvertDir = "fastq";
        public static readonly string MappingDir = "mapping";
        public static readonly string CurrentLogDir = "logs";




        public const int DefaultCutoff = 30;
            
        public static readonly string x86WSL = @"C:\Windows\sysnative\wsl.exe";
        public static readonly string x64WSL = @"C:\Windows\system32\wsl.exe";

        // public const string WSLname = "Ubuntu-18.04-nanopore";
        public const string WSLname = "Ubuntu-18.04-nanotools";
        public const string WSLUser = "user";
    }
}
