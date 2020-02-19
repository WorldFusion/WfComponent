using System;
using WfComponent.Base;

namespace WfComponent.External
{
    public class DummyProcess : IProcess
    {
        
        // Process single instance
        private RequestCommand proc;
        public string GetMessage() => string.Empty;
        public int ProcessId() => int.MinValue;  // defaut

        //constractor...
        public DummyProcess(string arg)
        {
            System.Diagnostics.Debug.WriteLine("DummyProcess init.");
        }

        public string StartProcess()
        {
            System.Diagnostics.Debug.WriteLine("StartProcess.");
            var stdout = string.Empty;
            var stderr = string.Empty;
            var command = @"C:\Windows\sysnative\wsl.exe";
            SetArguments( " bash -c \" ping localhost > /mnt/c/tmp/ping.txt \"");

            System.Diagnostics.Debug.WriteLine(command + " " + arguments);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(command, arguments, ref stdout, ref stderr);

            if(commandRes)
            {
                System.Diagnostics.Debug.WriteLine("command process error!");
                return Utils.ConstantValues.ErrorMessage;
            }

            System.Diagnostics.Debug.WriteLine(command + " " + arguments + "\n    command result is true.");
            return Utils.ConstantValues.NormalEndMessage;
        }

        public string StopProcess()
        {
            if (this.proc.process == null)
                return "It has been already finished."; 
            try
            {
                this.proc.process.Kill();
                // isSuccess = false // default
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("process-kill call exception: \n" + e.Message);
            }
            return Utils.ConstantValues.NormalEndMessage;
        }

        private bool isSuccess = false;
        public bool IsProcessSuccess()
            => isSuccess;

        private string arguments;
        public void SetArguments(string arguments)
            =>    this.arguments = arguments;

    }
}
