using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Seqkit : BaseProcess
    {
        public static string binaryName = "seqkit.exe";
        private SeqkitOptions op;
        private RequestCommand proc;

        public Seqkit(SeqkitOptions options) : base(options)
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
                Message = "seqkit Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            // command args is set
            SetArguments(GetCommandArgs());

            var workDir = Path.GetDirectoryName(op.outFastq);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

            isSuccess = true; // 取り敢えず終わって居るハズ
            return Utils.ConstantValues.NormalEndMessage;

        }

        public static string subSample = "sample ";
        public static string subSeq = "fq2fa ";
        private string GetCommandArgs()
        {
            // sample (rand getting) と、seq (fastq -> fasta)を使う。
            if (op.subCommand.Equals(subSample))
                return GetSampleCommandArgs();

            if (op.subCommand.Equals(subSeq))
                return GetSeqCommandArgs();

            // ここに来たらエラー。
            isSuccess = false;
            Message = "not found prediction sub-command, " + op.subCommand;
            return string.Empty;
        }


        private string GetSampleCommandArgs()
        {
            int cpus = 2;
            int.TryParse(op.threads, out cpus);
            var thread = cpus > 2 ? "--threads " + cpus :
                                                string.Empty;

            var args = new List<string>();
            args.Add(subSample); // sub-command = sample
            args.Add(op.acquireCounts);
            args.Add("--number");
            args.Add(op.samplingNumbers);
            args.Add("\"" + op.fastqPath + "\"");
            args.Add("--out-file");
            args.Add("\"" + op.outFastq + "\"");
            args.Add(thread);

            return string.Join(" ", args);
        }

        private string GetSeqCommandArgs()
        {

            var args = new List<string>();
            args.Add(subSeq); // sub-command = sample
            args.Add("\"" + op.fastqPath + "\"");
            args.Add("--out-file");
            args.Add("\"" + op.outFastq + "\"");

            return string.Join(" ", args);
        }

        public override string StopProcess()
        {
            this.Message = "Cancel Seqkit process!!";
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;
        }
    }
}
