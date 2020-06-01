using System.Collections.Generic;
using System.IO;
using System.Linq;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Minimap2 : BaseProcess
    {
        public static string binaryName = "minimap2.exe";
        private Minimap2Options op;
        private RequestCommand proc;
        // constractor....
        public Minimap2(Minimap2Options options) : base(options)
        {
            this.op = options;

            // クラスインスタンス作成時にMessageがnull の場合のみ実行可能。
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if ( !BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            // error check...
            if (string.IsNullOrEmpty(op.Reference) ||
                string.IsNullOrEmpty(op.OutFile) ||
                op.QueryFastqs == null || 
                !op.QueryFastqs.Any())
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

            isSuccess = false;

            // create command string.
            var args = new List<string>();
            if ( op.isMapping ) args.Add(" -a ");  // sam or paf
            if (! string.IsNullOrEmpty(op.Preset)) args.Add("-x " + op.Preset);  // default? 
            if (! string.IsNullOrEmpty(op.UseCore)) args.Add("-t " + op.UseCore);
            if (! string.IsNullOrEmpty(op.OtherOptions)) args.Add(op.OtherOptions);

            args.Add(Utils.FileUtils.GetDoubleQuotationPath(op.Reference));
            args.Add( string.Join(" ", Utils.FileUtils.GetDoubleQuotationPaths(op.QueryFastqs)));
            args.Add("-o " + Utils.FileUtils.GetDoubleQuotationPath(op.OutFile));
            SetArguments (string.Join(" ", args));  // minimap2 arguments

            System.Diagnostics.Debug.WriteLine(binaryPath);
            System.Diagnostics.Debug.WriteLine(arguments);

            var workDir = System.IO.Path.GetDirectoryName(op.OutFile);
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

}
