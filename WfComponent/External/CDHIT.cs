using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;
using WfComponent.Utils;

namespace WfComponent.External
{
    public class CDHIT : BaseProcess
    {
        public static string binaryName = "cd-hit-est.exe";
        private CdHitOptions op;
        private RequestCommand proc;

        public CDHIT(CdHitOptions options) : base(options)
        {
            this.op = options;
            // BLAST に必須のパラメータをチェック

            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            if (string.IsNullOrEmpty(op.fromFasta)||
                string.IsNullOrEmpty(op.toFasta) ||
                string.IsNullOrEmpty(op.identCutoff))
                Message = Message = "required parameter is not found, Please check parameters";


            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "Blast Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            // create command string.
            var args = new List<string>()
            {
                "-i",
                op.fromFasta,
                "-o",
                op.toFasta,
                "-c",
                op.identCutoff,
                "-T ",  // use thread
                ProcessUtils.CpuCore()
            };

            SetArguments(string.Join(" ", args));  // blast arguments

            var workDir = Path.GetDirectoryName(op.toFasta);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

            isSuccess = true;
            return Utils.ConstantValues.NormalEndMessage;

        }

        public override string StopProcess()
        {
            if (this.proc == null) return string.Empty;
            Message += "CD-HIT command cancel!";
            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;

        }
    }
}
