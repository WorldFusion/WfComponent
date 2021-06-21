using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class ClustalOmega : BaseProcess
    {
        public static string binaryName = "clustalo.exe";
        public static readonly string treeFileFooter = ".dnd";
        private ClustalOmegaOptions op;
        private RequestCommand proc;

        // 使用したオプションの再取得。デフォルトで名前つけているため。
        public ClustalOmegaOptions GetUseOptions()
            => this.op;


        public ClustalOmega(ClustalOmegaOptions options) : base(options)
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
                Message = "ClustalOmega Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            // command args is set
            SetArguments(GetCommandArgs());

            var workDir = Path.GetDirectoryName(binaryPath);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 


            if (! string.IsNullOrEmpty(stderr))
            {
                isSuccess = true; // 取り敢えず終わって居るハズ
                return Utils.ConstantValues.ErrorMessage;
            }

            var message = string.Empty;
            if (Utils.FileUtils.FileSize(op.outGuidTreeFile, ref message) > 100)
                TreeView.TreeViewStart(op.outGuidTreeFile);

            return Utils.ConstantValues.NormalEndMessage;
        }

        private string GetCommandArgs()
        {
            // default parameters ...
            if (string.IsNullOrEmpty(op.outFormat))
                op.outFormat = ClustalOmegaOptions.outMsf;

            if (string.IsNullOrEmpty(op.outFile))
                op.outFile = Path.ChangeExtension(op.targetFile, op.outFormat);

            if (string.IsNullOrEmpty(op.outGuidTreeFile))
                op.outGuidTreeFile = Path.ChangeExtension(op.targetFile, treeFileFooter);

            if (string.IsNullOrEmpty(op.Threads))
                op.Threads = Utils.ProcessUtils.CpuCore();


            var args = new List<string>();
            args.Add("--infile");
            args.Add(op.targetFile);
            args.Add("--threads");
            args.Add(op.Threads);
            args.Add("--guidetree-out");
            args.Add(op.outGuidTreeFile);
            args.Add("--outfmt");
            args.Add(op.outFormat);
            args.Add("--outfile");
            args.Add(op.outFile);
            args.Add("--output-order tree-order");
            args.Add("--seqtype dna");
            args.Add("--force");

            return string.Join(" ", args);

        }

        public override string StopProcess()
        {
            this.Message = "Cancel ClustalOmega process!!";
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;
        }
    }
}
