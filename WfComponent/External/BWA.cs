using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WfComponent.External.Properties;

namespace WfComponent.External
{

    // https://github.com/lh3/bwa
    public class BWA : BaseProcess
    {
        public static string binaryName = "bwa";
        private BWAOptions op;
        private RequestCommand proc;


        public BWA(BWAOptions options) : base(options)
        {
            this.op = options;

            // BWA に必須のパラメータをチェック
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!WSLBinaryExist(binaryName)) // 救済処置含める
                Message += "required program path is not found, Please check OPTION:binaryPath, ";

            if (string.IsNullOrEmpty(op.fwdFasta) ||
                string.IsNullOrEmpty(op.reference) ||
                string.IsNullOrEmpty(op.outSam))
                Message += "required parameter is not found, Please check parameters";
            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message += "BWA Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }
            isSuccess = false;

            // Illumina / 454 / IonTorrentのペアエンド読み取りが70bpを超える場合：
            // bwa mem ref.fa read1.fq read2.fq > aln-pe.sam
            var args = new List<string>();
            args.Add("mem");
            args.Add("-t " + Environment.ProcessorCount.ToString()); // use cpu-cpre
            args.Add(op.otherOptions);

            args.Add(Utils.FileUtils.GetDoubleQuotationPath(
                            Utils.FileUtils.WindowsPath2LinuxPath(op.reference))); // reference 
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(
                            Utils.FileUtils.WindowsPath2LinuxPath(op.fwdFasta)));
            if(File.Exists(op.revFasta))
                args.Add(Utils.FileUtils.GetDoubleQuotationPath(
                                Utils.FileUtils.WindowsPath2LinuxPath(op.revFasta)));
            args.Add(" > ");
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(
                            Utils.FileUtils.WindowsPath2LinuxPath(op.outSam )));

            SetArguments(string.Join(" ", args));  // set arguments
            var workDir = Path.GetDirectoryName(op.outSam);

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
            this.Message += "BWA cancel";
            return Utils.ConstantValues.CanceledMessage;
        }

    }
}
