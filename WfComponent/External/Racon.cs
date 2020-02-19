using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;
using WfComponent.Utils;

namespace WfComponent.External
{
    public class Racon : BaseProcess
    {
        public static string binaryName = "racon.exe";
        public static string linuxName = "racon";  // 現行はWSLつかう

        // class grobal.
        private RaconOptions op;
        private RequestCommand proc;

        // constractor....
        public Racon(RaconOptions options) : base(options)
        {
            this.op = options;
            // require parameters.
            if(string.IsNullOrEmpty(op.sequences) ||
                string.IsNullOrEmpty(op.overlaps) ||
                string.IsNullOrEmpty(op.target) ||
                string.IsNullOrEmpty(op.outFile) )
                    Message = "required parameter is not found, Please check parameters";

            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            if (op.isLinux && !WSLBinaryExist(linuxName)) // linux 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath " + linuxName;
            if (!op.isLinux && !BinaryExist(binaryName)) // Windows 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath " + binaryName;


            isSuccess = string.IsNullOrEmpty(Message) || !string.IsNullOrEmpty(binaryPath);
        }


        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            isSuccess = false;
            op.isLinux = true;   // 2019.09 時点は強制
            var sequence = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.sequences) :
                                                     op.sequences;
            var overlaps = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.overlaps) :
                                                    op.overlaps;
            var target = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.target) :
                                                 op.target;
            var outFile = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.outFile) :
                                                  op.outFile;
            // create command string.
            var args = new List<string>();
            args.Add("-t " + op.useCore);
            args.Add(sequence); // <sequences> 大元Fasta/Fastq
            args.Add(overlaps);    // <overlaps> PAF とか
            args.Add(target);       // <target sequences> Contig にしたやつ。
            args.Add(">");
            args.Add(outFile);      // out-fasta
            arguments = string.Join(" ", args);

            var workDir = System.IO.Path.GetDirectoryName(op.sequences);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWSLCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

            // if (commandRes == false) StdError に出力があるから false になる。

            // 正常終了
            isSuccess = true;
            return Utils.ConstantValues.NormalEndMessage;
        }

        public override string StopProcess()
        {
            if (this.proc == null) return string.Empty;

            Message = "User request cancel Racon";
            this.proc.CommandCancel();
            return Utils.ConstantValues.NormalEndMessage;
        }
    }
}
