using System;
using WfComponent.Base;
using WfComponent.Utils;

namespace WfComponent.External
{
    public class CollectWsl : IProcess
    {
        // Process single instance
        private RequestCommand proc;
        public string GetMessage() => string.Empty;

        private bool isSuccess = false;
        public bool IsProcessSuccess() => isSuccess;

        private string arguments;
        public void SetArguments(string arguments)
            => this.arguments = arguments;

        public string command;
        public CollectWsl()
            => this.command = RequestCommand.WslCommand;

        public int ProcessId()
        {
            if (this.proc.process == null) return int.MinValue;
            return this.proc.Pid;
        }

        public CollectWsl(string arguments)
        {
            this.command = RequestCommand.WslCommand;
            this.arguments = arguments;
        }
        public CollectWsl(string command, string arguments, string workDir = null)
        {
            this.command = command;
            this.arguments = arguments;
            this.workDir = workDir;
        }
        // 

        public string stdout;
        public string stderr;
        public string workDir;
        public string CallIsInitUbuntu(string arguments = null)
        {
            proc = RequestCommand.GetInstance();
            this.arguments = string.IsNullOrEmpty(arguments) ?
                                                        "--distribution " + ConstantValues.WSLname  +   // 利用するWSL登録名
                                                        " --user " + ConstantValues.WSLUser  +
                                                        " -- cat $HOME/init.install" 
                                      :arguments ;
            var res = StartProcess();
            return res;
        }
                          
        public string StartProcess()
        {
            System.Diagnostics.Debug.WriteLine("StartProcess.");
            // argument はよばれる前にセットしているはず

            this.stdout = string.Empty;
            this.stderr = string.Empty;
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(command, arguments, ref stdout, ref stderr, workDir);

            if (commandRes)
            {
                isSuccess = false;
                System.Diagnostics.Debug.WriteLine("command process error!");
                return Utils.ConstantValues.ErrorMessage;
            }
            return Utils.ConstantValues.NormalEndMessage;
        }

        public string StopProcess()
        {
            if (this.proc.process == null)
                return "It has been already finished.";
            try
            {
                this.proc.process.Kill();
                ProcessUtils.TryKillProcessByProcessId(proc.Pid);
                // isSuccess = false // default
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("process-kill call exception: \n" + e.Message);
            }
            return Utils.ConstantValues.NormalEndMessage;
        }

    }
}
