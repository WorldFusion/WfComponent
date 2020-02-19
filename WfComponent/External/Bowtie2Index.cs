using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Bowtie2Index : BaseProcess
    {

        public static string binaryName = "bowtie2-build-s.exe";
        private Bowtie2IndexOption op;
        private RequestCommand proc;

        public Bowtie2Index(Bowtie2IndexOption options) : base(options)
        {
            this.op = options;

            // Bowtie2-index に必須のパラメータをチェック
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            if (string.IsNullOrEmpty(op.targetFasta) ||
                string.IsNullOrEmpty(op.referenceName) )
                Message = Message = "required parameter is not found, Please check parameters";

            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "Bowtie2 Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            isSuccess = false;
            // コマンド オプション
            var args = new List<string>();
            args.Add("-f");
            args.Add($"\"{op.targetFasta}\"");
            args.Add(op.referenceName);

            SetArguments(string.Join(" ", args));  // set arguments
            var workDir = Path.GetDirectoryName(op.targetFasta);

            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 


            isSuccess = true;  // 一応コマンドは通った。エラーはoutFile の有無で判断
            return Utils.ConstantValues.NormalEndMessage;
        }

        public override string StopProcess()
        {
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            this.Message += "Bowtie2 create index command is cancel";
            return Utils.ConstantValues.CanceledMessage;
        }
    }

}
