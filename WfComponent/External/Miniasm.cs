using System.Collections.Generic;
using System.IO;
using System.Linq;
using WfComponent.External.Properties;
using WfComponent.Utils;

namespace WfComponent.External
{
    public class Miniasm : BaseProcess
    {

        public static string binaryName = "miniasm.exe";
        public static string linuxName = "miniasm";  // 現行はWSLつかう

        // class grobal.
        private MiniasmOptions op;
        private RequestCommand proc;
        public Miniasm(MiniasmOptions options) : base(options)
        {
            this.op = options;
            // require parameters.
            if (string.IsNullOrEmpty(op.referencePaf) ||
                string.IsNullOrEmpty(op.outGaf) ||
                ! op.sequences.Any() )
                Message = "required parameter is not found, Please check parameters";

            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            if (op.isLinux && !WSLBinaryExist(linuxName)) // linux 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath " + linuxName;
            if (!op.isLinux && !BinaryExist(binaryName)) // Windows 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath " + binaryName;
 

            isSuccess = string.IsNullOrEmpty(Message) || ! string.IsNullOrEmpty(binaryPath);
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
            var referPaf = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.referencePaf) :
                                                   op.referencePaf;
            var outPaf = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.outGaf) :
                                                   op.outGaf;

            // create command string.
            var args = new List<string>();
            if (!string.IsNullOrEmpty(op.otherOptions)) args.Add(op.otherOptions);
            args.Add("-f");
            args.Add(sequence); // <sequences> 大元Fasta/Fastq
            args.Add(referPaf);    // <overlaps> PAF とか
            args.Add(">");       // <target sequences> Contig にしたやつ。
            args.Add(outPaf);      // out-fasta
            arguments = string.Join(" ", args);

            var workDir = System.IO.Path.GetDirectoryName(op.referencePaf);
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

            this.proc.CommandCancel();
            return Utils.ConstantValues.NormalEndMessage;
        }

        //
        // awk '/^S/{print ">"$2"\n"$3}' miniasm_out.gfa | fold > raw_assembly.fasta
        public static string CreateRawFasta(string gfaPath, string outFasta)
        {
            var message = string.Empty;
            var preFasta = FileUtils.ReadFile(gfaPath, ref message)
                                                .Where(s => s.StartsWith("S"))
                                                .ToArray();

            if (preFasta.Any())
            {
                var fasta = new List<string>();
                foreach (var line in preFasta)
                {
                    var field = line.Split('\t');
                    fasta.Add( ">" + field[1]);
                    fasta.Add(field[2]);
                }
                FileUtils.WriteFile(outFasta, fasta, ref message);
            }


            return message;
        }
    }
}
