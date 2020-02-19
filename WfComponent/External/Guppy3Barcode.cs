using System;
using System.IO;
using System.Linq;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Guppy3Barcode : BaseProcess
    {
        // public static string binaryName = "guppy_barcoder.exe";
        // public string GuppyBarcoder;
        private GuppyOptions op;
        private RequestCommand proc;

        public Guppy3Barcode(GuppyOptions options) : base(options)
        {
            this.op = options;
            var binaryName = string.IsNullOrEmpty(op.binaryPath) ?
                                        GuppyCommand.GuppyBarcoderBinName : // default  "guppy_barcoder"
                                        Path.GetFileName(op.binaryPath);

            // binary とか救済処置
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath :" + op.binaryPath;

            // if (string.IsNullOrEmpty(op.Config))
            //    Message += "required parameter is not found : guppy config ";

            isSuccess = string.IsNullOrEmpty(Message);
        }

        // Guppy process 
        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message += Environment.NewLine + "Process is initialisation Error, ";
                return Utils.ConstantValues.ErrorMessage;
            }

            isSuccess = false;
            var processRes = StartBarcodeProcess();
            if (!processRes)
                return Utils.ConstantValues.ErrorMessage;

            // ここまでくれれば
            isSuccess = true;
            return Utils.ConstantValues.NormalEndMessage;
        }

        private bool StartBarcodeProcess()
        {
            isSuccess = false;
            var message = string.Empty;

            // create command string.
            var commandArgs = GuppyCommand.GuppyFastBarcodeCommandArgs(op.FastqDir, op.OutDir, ref message);

            // Command error.
            if (!string.IsNullOrEmpty(message))
            {
                Message = "GuppyBasecall Process Error....\n" + message;
                System.Diagnostics.Debug.WriteLine(message);
                return isSuccess;
            }

            // ここで設定
            SetArguments(commandArgs);

            // 出力先
            proc = RequestCommand.GetInstance();
            winProcessId = proc.Pid; // for kill process
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, op.OutDir);
            

            // ホントにFastq 出来ているかの確認
            var di = new DirectoryInfo(op.OutDir);
            if (!di.Exists)
            {
                Message = "Guppy basecall process error! (no create fastq files) " + Environment.NewLine
                                + message + Environment.NewLine
                                + "command return : " + commandRes;
                return false;
            }

            var fs = di.EnumerateFiles("*.fastq", SearchOption.AllDirectories);  // サブフォルダに分かれている場合がある
            if (fs.Count() == 0)
            {
                Message = "Guppy basecall process error! (no create fastq files) " + Environment.NewLine
                                + message + Environment.NewLine
                                + "command return : " + commandRes;
                return false;
            }
            return string.IsNullOrEmpty(Message);
        }

        public override string StopProcess()
        {
            this.Message = "Cancel Guppy process!!";
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;
        }

    }
}
