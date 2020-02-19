using System;
using System.IO;
using System.Linq;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class GuppyBasecall : BaseProcess
    {
        // windows binary FullPath 
        public string GuppyBasecaller;
        public string GuppyBarcoder;

        private GuppyOptions op;
        private RequestCommand proc;

        // constractor  // Basecaller には2種類あるのでユーザが指定する
        public GuppyBasecall(GuppyOptions options) :base(options)
        {
            this.op = options;
            var binaryName = string.IsNullOrEmpty(op.binaryPath) ?
                                        GuppyCommand.Guppy2DBinName : // default basecaller
                                        Path.GetFileName(op.binaryPath);

            // binary とか救済処置
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            GuppyBasecaller = binaryPath;
            GuppyBarcoder = string.IsNullOrEmpty(binaryPath) ?
                                        string.Empty:
                                        Path.Combine(Path.GetDirectoryName(binaryPath),
                                                                GuppyCommand.GuppyBarcoderBinName);
                

            isSuccess = string.IsNullOrEmpty(Message);
        }

        // Guppy process : basecall -> barcoder.
        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            isSuccess = false;
            var basecallRes = StartBasecallProcess();
            if (basecallRes)
                StartBarcodeProcess();
            else
                return Utils.ConstantValues.ErrorMessage;

            // ここまでくれれば
            isSuccess = true;
            return Utils.ConstantValues.NormalEndMessage;
        }

        public string basecallOut;
        public string barcodeOut;
        public string kit;
        public string flowcell;
        private bool StartBasecallProcess()
        {
            isSuccess = false;
            var message = string.Empty;

            // create command string.
            var (commandArgs, outFastqDir, flowcell, kit) =
                GuppyCommand.GuppyBasecallCommandArgs(op.Fast5Dir, op.OutDir, op.OtherBasecallOption, ref message);

            // Command error.
            if (!string.IsNullOrEmpty(message))
            {
                Message = "GuppyBasecall Process Error....\n" + message;
                System.Diagnostics.Debug.WriteLine(message);
                return isSuccess;
            }


            basecallOut = outFastqDir;
            this.kit = kit;
            this.flowcell = flowcell;
            SetArguments(commandArgs);

            // 出力先
            if (!Directory.Exists(basecallOut))  Directory.CreateDirectory(basecallOut);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(GuppyBasecaller, arguments, ref stdout, ref stderr, basecallOut);
            var pid = proc.Pid; // for kill process

            // if (commandRes == false) StdError に出力があるから false になる。
            // ホントにFastq 出来ているかの確認
            var di = new DirectoryInfo(basecallOut);
            var fs = di.EnumerateFiles("*.fastq", SearchOption.AllDirectories);  // サブフォルダに分かれている場合がある
            if (fs.Count() == 0)
            {
                Message = "Guppy basecall process error! (no create fastq files) "  + Environment.NewLine
                                + message + Environment.NewLine
                                + commandRes;

            }

            return string.IsNullOrEmpty(Message);
        }

        private bool StartBarcodeProcess()
        {
            isSuccess = false;
            var message = string.Empty;

            // create command string.
            var (commandArgs, outFastqDir) =
                GuppyCommand.GuppyBarcodeCommandArgs(basecallOut, op.OutDir, this.kit, op.OtherBarcodeOption, ref message);

            this.barcodeOut = outFastqDir;
            SetArguments(commandArgs);

            // 出力先
            if (!Directory.Exists(barcodeOut)) Directory.CreateDirectory(barcodeOut);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(GuppyBarcoder, arguments, ref stdout, ref stderr, basecallOut);
            var pid = proc.Pid; // to kill process

            // if (commandRes == false) StdError に出力があるから false になる。
            // ホントにFastq 出来ているかの確認
            var di = new DirectoryInfo(barcodeOut);
            var fs = di.EnumerateFiles("*.fastq", SearchOption.AllDirectories);  // サブフォルダに分かれている場合がある
            if (fs.Count() == 0)
            {
                Message = "Guppy basecall process error! (no create fastq files) " + Environment.NewLine
                                + message + Environment.NewLine
                                + commandRes;
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
