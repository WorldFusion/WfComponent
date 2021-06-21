using System;
using WfComponent.Base;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public abstract class BaseProcess : IProcess
    {
        protected string binaryPath;
        public string Message;
        public bool isSuccess;
        public string GetMessage() => Message;
        public string logMessage = string.Empty;

        protected string stdout = string.Empty;
        protected string stderr = string.Empty;

        protected IProgress<string> progress;
        protected BaseOptions baseoption;
        public BaseProcess(BaseOptions options)
        {
            this.baseoption = options;
            this.progress = options.progress;
            if (this.progress == null)
                this.progress = new Progress<string>(s => System.Diagnostics.Debug.WriteLine(s));
        }

        // 外部ツールのログ
        public string GetExecLog()
        {
            var s = string.IsNullOrEmpty(stderr) ? string.Empty : stderr + System.Environment.NewLine;
            s += string.IsNullOrEmpty(stdout) ? string.Empty : stdout;
            return s;
        }

        // まとめて取得するため
        protected void LogMessages(string log)
            => this.logMessage += log.Trim() + System.Environment.NewLine;


        // search windows binary full-path
        protected bool BinaryExist(string binaryName) // 救済処置
        {
            System.Diagnostics.Debug.WriteLine("external binary search :" + binaryName );
            // var curDir = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

           binaryPath = CommandUtils.FindProgramFile(
                                                            System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                                            binaryName, false  , false);
            if (!string.IsNullOrEmpty(binaryPath)) return true;

            Message = "not found program : " + binaryName;
            return false;
        }

        protected bool WSLBinaryExist(string wslName)
        {
            System.Diagnostics.Debug.WriteLine("external binary search :" + wslName);

            var curDir = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            binaryPath = Utils.FileUtils.WindowsPath2LinuxPath(
                                CommandUtils.FindProgramFile(curDir, wslName, true, false));
            if (!string.IsNullOrEmpty(binaryPath)) return true;   // binary見つかった。

            // 普通に見つからない場合の回避
            if(!string.IsNullOrEmpty(this.baseoption.binaryPath))
                binaryPath = Utils.FileUtils.WindowsPath2LinuxPath(
                    CommandUtils.FindProgramFile(this.baseoption.binaryPath, wslName, true, false));
            if (!string.IsNullOrEmpty(binaryPath)) return true;   // binary見つかった。

            Message = "not found program : " + wslName;
            return false;
        }


        protected string arguments;
        public void SetArguments(string arguments) => this.arguments = arguments;
        public bool IsProcessSuccess() => isSuccess;

        protected int winProcessId = int.MinValue;    // 初期値
        public int ProcessId() => winProcessId;

        public abstract string StartProcess();
        public abstract string StopProcess();
    }
}
