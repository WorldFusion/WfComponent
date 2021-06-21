using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Muscle : BaseProcess
    {
        public static string binaryName = "muscle3.8.31_i86win32.exe";
        private MuscleOptions op;
        private RequestCommand proc;

        public Muscle(MuscleOptions options) : base(options)
        {
            this.op = options;
            if (File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath = " + op.binaryPath;

            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "Muscle Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            // command args is set
            SetArguments(GetCommandArgs());

            var workDir = Path.GetDirectoryName(binaryPath);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

            isSuccess = true; // 取り敢えず終わって居るハズ
            return Utils.ConstantValues.NormalEndMessage;
        }


        private string GetCommandArgs()
        {
            var args = new List<string>();
            args.Add(op.outFormat); //  clustalw   or   html 
            args.Add("-in");
            args.Add(op.targetFile);
            args.Add("-out");
            args.Add(op.outFile);
            return string.Join(" ", args);

        }

        public override string StopProcess()
        {
            this.Message = "Cancel Muscle process!!";
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;
        }

    }
}
