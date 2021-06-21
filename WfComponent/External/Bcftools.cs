using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public  class Bcftools : BaseProcess
    {

        public static string binaryName = "bcftools.exe";
        private BcftoolsOptions op;
        private RequestCommand proc;
        // constractor
        public Bcftools(BcftoolsOptions options) : base(options)
        {
            this.op = options;

            // クラスインスタンス作成時にMessageがnull の場合のみ実行可能。
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            // error check...
            if (string.IsNullOrEmpty(op.reference) ||
                string.IsNullOrEmpty(op.sortedbam) ||
                string.IsNullOrEmpty(op.outVcf) )
                Message = "required parameter is not found, Please check parameters";

            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            var cpucore = string.IsNullOrEmpty(op.threads) ?
                                   Utils.ProcessUtils.DefaultCpuCore().ToString() :
                                   op.threads;

            var outTmpPileup = Path.ChangeExtension(op.outVcf, ".pileup");
            if (File.Exists(outTmpPileup)) File.Delete(outTmpPileup);

            // create command string.
            // bcftools mpileup -Ou -f corona-reference.fna sorted.bam | bcftools call -mv  -o sorted.bam.vcf
            var args = new List<string>();
            args.Add("mpileup -BQ0 -d10000000 -Ou ");
            args.Add("--threads " + cpucore);
            args.Add("-f ");  // reference
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(op.reference));
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(op.sortedbam));
            args.Add(" -o ");
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(outTmpPileup));

            SetArguments(string.Join(" ", args));  // bcftools  arguments
            System.Diagnostics.Debug.WriteLine(binaryPath);
            System.Diagnostics.Debug.WriteLine(arguments);


            var workDir = System.IO.Path.GetDirectoryName(op.outVcf);
            proc = RequestCommand.GetInstance();
            isSuccess = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

            // 最初のPileup時にエラー
            if (! File.Exists(outTmpPileup)) 
                return Utils.ConstantValues.ErrorMessage;

            if (File.Exists(op.outVcf)) File.Delete(op.outVcf);

            // 2回目
            args = new List<string>();
            args.Add("call -v -c -o ");  // call variants
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(op.outVcf));
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(outTmpPileup));

            SetArguments(string.Join(" ", args));  // bcftools  arguments
            System.Diagnostics.Debug.WriteLine(binaryPath);
            System.Diagnostics.Debug.WriteLine(arguments);
            isSuccess = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);


            // if (commandRes == false) StdError に出力があるから false になる。
            if (File.Exists(op.outVcf))
                return Utils.ConstantValues.NormalEndMessage;
            return Utils.ConstantValues.ErrorMessage;

        }


        public override string StopProcess()
        {
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;
        }

    }

    public class BcftoolsOptions : BaseOptions
    {

        [System.ComponentModel.DataAnnotations.Required()]
        public string reference;

        [System.ComponentModel.DataAnnotations.Required()]
        public string sortedbam;

        [System.ComponentModel.DataAnnotations.Required()]
        public string outVcf;

        public string threads;

    }

}
