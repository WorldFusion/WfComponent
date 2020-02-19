using System.Collections.Generic;
using WfComponent.External.Properties;
using WfComponent.Utils;

namespace WfComponent.External
{
    public class SVIM : BaseProcess
    {
        public static string binaryName = "svim.exe";
        public static string linuxName = "svim";  // 現行はWSLつかう

        // class grobal.
        private SVIMOptions op;
        private RequestCommand proc;

        // constractor....
        public SVIM(SVIMOptions options) : base(options)
        {
            this.Message = string.Empty;
            this.op = options;
            if(string.IsNullOrEmpty(op.sequence) || 
                string.IsNullOrEmpty(op.reference) ||
                string.IsNullOrEmpty(op.outDir) )
                Message = "required parameter is not found, Please check parameters";

            // pip install だからパスが通って居るはず。TODO：フルパス指定？
            // binaryPath = "$HOME/.local/bin/" + linuxName;
            binaryPath = linuxName;

            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)  //　前段階でエラー（指定パラメータない）ので直ぐに返す
            {
                Message = "Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            isSuccess = false;   // 処理前の初期値 
            op.isLinux = true;   // 2019.09 時点は強制
            var sequence = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.sequence) :
                                                     op.sequence;
            var reference = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.reference) :
                                                    op.reference;
            var workdir = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.outDir) :
                                                    op.outDir;

            var sequencer = op.isNanopore ? "--nanopore" : string.Empty;  // default は、PacBio で引き数ナシ
            var aligner = op.isNgmlr ? string.Empty  : "--aligner minimap2"; // default は ngmlr  で引き数ナシ
            var highQual = (op.minQuality >= 30) ? "--min_mapq " + op.minQuality : string.Empty;
            // create command string.
            var args = new List<string>();
            args.Add("reads ");    //  align reads , then call SVs
            args.Add(sequencer);     // sequencer = PacBio or ONT
            args.Add(aligner);     // aligner = minimap2 or ngmlr
            args.Add(highQual);  // high-quality alignments
            args.Add(workdir);
            args.Add(sequence);
            args.Add(reference);   // genome.   
            arguments = string.Join(" ", args);

            var workDir = op.outDir;    // Windows Path
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWSLCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 
            // 正常終了
            isSuccess = true;
            return Utils.ConstantValues.NormalEndMessage;
        }

        public override string StopProcess()
        {
            if (this.proc == null) return string.Empty;

            Message = "User request cancel SVIM ";
            this.proc.CommandCancel();
            return Utils.ConstantValues.NormalEndMessage;
        }
    }
}
