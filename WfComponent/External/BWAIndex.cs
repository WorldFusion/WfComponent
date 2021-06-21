using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class BWAIndex : BaseProcess
    {
        public static string binaryName = "bwa";
        private BWAIndexOptions op;
        private RequestCommand proc;

        public BWAIndex(BWAIndexOptions options) : base(options)
        {
            this.op = options;
            op.isLinux = true;   // windows bin 無いので WSL経由

            // BWA-index に必須のパラメータをチェック
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!WSLBinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            if (string.IsNullOrEmpty(op.targetFasta))
                Message = Message = "required parameter is not found, Please check parameters";

            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "BWA　Index Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            isSuccess = false;
            // コマンド オプション
            var args = new List<string>();
            args.Add("index ");
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(
                        Utils.FileUtils.WindowsPath2LinuxPath(op.targetFasta)));

            SetArguments(string.Join(" ", args));  // set arguments
            var workDir = Path.GetDirectoryName(op.targetFasta);

            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWSLCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 


            isSuccess = true;  // 一応コマンドは通った。エラーはoutFile の有無で判断
            return Utils.ConstantValues.NormalEndMessage;
        }

        public override string StopProcess()
        {
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            this.Message += "BWA create index command is cancel";
            return Utils.ConstantValues.CanceledMessage;
        }

    }
}
