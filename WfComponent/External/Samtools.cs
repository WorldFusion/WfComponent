using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Samtools : BaseProcess
    {
        public static string binaryName = "samtools.exe";
        // private string arguments;
        private RequestCommand proc;
        private SamtoolsOptions op;

        // constractor
        public Samtools(SamtoolsOptions options):base(options)
        {
            op = options;

            if (File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath = " + options.binaryPath;

            isSuccess = string.IsNullOrEmpty(Message);
        }

        public string CreateIndex()
            => CreateIndex(op.indexOp, op.targetFile);

        public string CreateIndex(string indexOp, string target)
        {

            this.arguments = $"{indexOp}  {Utils.FileUtils.GetDoubleQuotationPath(target)}";
            return StartProcess();
        }

        public string Sam2BamWithIndex()
            => Sam2BamWithIndex(op.targetFile, op.outFile);

        // bam, bam-index
        public string Sam2BamWithIndex(string inSamFile, string outSortedBam)
        {
            System.Diagnostics.Debug.WriteLine("samtools sort/index  : " + inSamFile );

            var thread = " -@ " +  Utils.ProcessUtils.DefaultCpuCore();
            this.arguments = $"sort  {thread}  -O bam -o {Utils.FileUtils.GetDoubleQuotationPath(outSortedBam)}  {Utils.FileUtils.GetDoubleQuotationPath( inSamFile)}";
            var sort = StartProcess();

            if (sort == Utils.ConstantValues.NormalEndMessage)
                this.arguments = $"index   {Utils.FileUtils.GetDoubleQuotationPath(outSortedBam)}";

            return StartProcess();
        }

        // mpileup 
        public string Pileup() => Pileup(op.referenceFile, op.targetFile, op.outFile);
        public string Pileup(string referencePath, string inBamPath, string mplilupPath = null)
        {
            mplilupPath = string.IsNullOrEmpty(mplilupPath) ?
                                    Path.Combine(
                                        Path.GetDirectoryName(inBamPath),
                                        Path.GetFileNameWithoutExtension(inBamPath) + ".mpileup") :
                                    mplilupPath;

            var args = new List<string>();
            args.Add("mpileup");
            args.Add("-BQ0 -d10000000");  //
            args.Add("-f");
            args.Add( Utils.FileUtils.GetDoubleQuotationPath( referencePath ));
            args.Add("-o");
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(mplilupPath));
            args.Add("-a");   // マッピングされない場所も塩基をすべて出力する（Humanとかには向かないので注意）
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(inBamPath));

            // 
            this.arguments = string.Join(" ", args);
            return StartProcess();
        }


        public override string StartProcess()
        {
            System.Diagnostics.Debug.WriteLine("samtools start :" + op.targetFile );
            // create command string.
            proc = RequestCommand.GetInstance();
            isSuccess = ! proc.ExecuteWinCommand(   // Command は、isError が返答
                                                    binaryPath,
                                                    arguments,
                                                    ref stdout,
                                                    ref stderr
                                                    );

            Message = isSuccess.ToString() + stdout + stderr;
            if (!string.IsNullOrEmpty(stdout) || !string.IsNullOrEmpty(stderr))
                System.Diagnostics.Debug.WriteLine("samtools command results \n" + stdout + "\n" + stderr);

            if (isSuccess) return Utils.ConstantValues.NormalEndMessage;
            return Utils.ConstantValues.ErrorMessage;
        }

        public override string StopProcess()
        {
            this.proc.CommandCancel();
            this.Message += "samtools cancel";
            return Utils.ConstantValues.CanceledMessage;
        }
    }
}
