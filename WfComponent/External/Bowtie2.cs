using System.Collections.Generic;
using System;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Bowtie2 : BaseProcess
    {
        public static string binaryName = "bowtie2-align-s.exe";
        private Bowtie2Options op;
        private RequestCommand proc;

        public Bowtie2(Bowtie2Options options) : base(options)
        {
            this.op = options;

            // Bowtie2 に必須のパラメータをチェック
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            if (string.IsNullOrEmpty(op.fwdFasta) ||
                string.IsNullOrEmpty(op.reference) ||
                string.IsNullOrEmpty(op.outSam))
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

            args.Add("-x " + op.reference); // reference
            args.Add("--threads " + Environment.ProcessorCount.ToString()); // use cpu-cpre

            // mappingCommandArgs.Add("-q"); // query type = fastq 
            if ( op.isFasta ) args.Add("-f"); // query type = fasta 2019.04.22
            if ( op.isFastq ) args.Add("-q");
            if (string.IsNullOrEmpty(op.revFasta))   // single end.
            {
                args.Add("-U " + op.fwdFasta);
            }
            else
            {
                args.Add("-1 " + op.fwdFasta);
                args.Add("-2 " + op.revFasta);
            }
            args.Add("--local");   // oka-options
            if (!string.IsNullOrEmpty(op.otherOptions))
                args.Add(op.otherOptions);

            args.Add("-S "); // out-sam
            args.Add("\"" + op.outSam + "\"");

            SetArguments(string.Join(" ", args));  // set arguments
            var workDir = Path.GetDirectoryName(op.outSam);

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
            this.Message += "Bowtie2 cancel";
            return Utils.ConstantValues.CanceledMessage;
        }
    }
}
