using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class BlastIndex : BaseProcess
    {
        public static string binaryName = "makeblastdb.exe";
        private BlastIndexOption op;
        private RequestCommand proc;
        public BlastIndex(BlastIndexOption options) : base(options)
        {
            this.op = options;
            // BLAST に必須のパラメータをチェック

            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            if (string.IsNullOrEmpty(op.reference))
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

            var args = new List<string>()
            {
                    "-dbtype nucl",
                    "-parse_seqids",
                    "-in",
                    op.reference,
                    "-out",
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(op.reference),
                        System.IO.Path.GetFileNameWithoutExtension(op.reference)
                    ),
            };
            SetArguments(string.Join(" ", args));  // blast arguments

            var workDir = Path.GetDirectoryName(op.reference);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

            // if (commandRes == false) StdError に出力があるから false になる。

            isSuccess = true;
            return Utils.ConstantValues.NormalEndMessage;
        }

        public override string StopProcess()
        {
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;
        }
    }


    public class BlastIndexOption : BaseOptions
    {
        [Required()]
        public string reference;    // require
    }



}
