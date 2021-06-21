using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{

    public class Kalign : BaseProcess
    {
        public static string binaryName = "kalign.exe";
        public static string clustelFooter = ".clu";
        public static string fastaFooter = ".fasta";
        private KalignOptions op;
        private RequestCommand proc;

        public Kalign(KalignOptions options) : base(options)
        {
            this.op = options;

            // クラスインスタンス作成時にMessageがnull の場合のみ実行可能。
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            // error check...
            if (string.IsNullOrEmpty(op.fastaPath) ||
                string.IsNullOrEmpty(op.outAlign))
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

            // create command string.
            // kalign -i marged-consensus.fna -f clu -o kalign.out.clu

            var outFormat = op.isFastaOut ?
                                            " -f fasta " :
                                            " -f clu ";
            var args = new List<string>();
            args.Add("-i ");  // reference
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(op.fastaPath));
            args.Add(outFormat);  // cluster or fasta. 
            args.Add("-o ");
            args.Add(Utils.FileUtils.GetDoubleQuotationPath(op.outAlign));

            SetArguments(string.Join(" ", args));  // kalign  arguments
            System.Diagnostics.Debug.WriteLine(binaryPath);
            System.Diagnostics.Debug.WriteLine(arguments);


            var workDir = System.IO.Path.GetDirectoryName(op.outAlign);
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


    public class KalignOptions : BaseOptions
    {

        [System.ComponentModel.DataAnnotations.Required()]
        public string fastaPath;


        [System.ComponentModel.DataAnnotations.Required()]
        public string outAlign;


        [System.ComponentModel.DataAnnotations.Required()]
        public bool isFastaOut;

    }
}
