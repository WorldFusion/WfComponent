using System;
using System.IO;
using System.Linq;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Guppy3FastBasecall : BaseProcess
    {
        private GuppyOptions op;
        private RequestCommand proc;

        public Guppy3FastBasecall(GuppyOptions options) : base(options)
        {
            this.op = options;
            var binaryName = string.IsNullOrEmpty(op.binaryPath) ?
                                        GuppyCommand.Guppy2DBinName : // default basecaller "guppy_basecaller.exe"
                                        Path.GetFileName(op.binaryPath);

            // binary とか救済処置
            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath " + op.binaryPath;


            if (string.IsNullOrEmpty(op.Config))
                Message += "required parameter is not found : guppy config ";

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
            var basecallRes = StartBasecallProcess();
            if (! basecallRes)
                return Utils.ConstantValues.ErrorMessage;

            // ここまでくれれば
            isSuccess = true;
            return Utils.ConstantValues.NormalEndMessage;
        }

        private bool StartBasecallProcess()
        {
            isSuccess = false;
            var message = string.Empty;

            // create command string.
            var commandArgs = GuppyCommand.GuppyFastBasecallCommandArgs(op.Fast5Dir, op.OutDir, op.Config, ref message);

            // Command error.
            if (!string.IsNullOrEmpty(message))
            {
                Message = "GuppyBasecall Process error....(command args fail) " + Environment.NewLine + message;
                System.Diagnostics.Debug.WriteLine(message);
                return false;
            }

            // ここで設定
            SetArguments(commandArgs);

            // 出力先
            proc = RequestCommand.GetInstance();
            winProcessId = proc.Pid; // for kill process?

            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, op.OutDir);

            // if (commandRes == false) StdError に出力があるから false になる。
            // ホントにFastq 出来ているかの確認
            if(!Directory.Exists(op.OutDir))
            {
                Message = "Guppy basecall process error! (command fail) " + Environment.NewLine
                                + message + Environment.NewLine
                                + commandRes + Environment.NewLine
                                + stdout + Environment.NewLine
                                + stderr;
            }
            var di = new DirectoryInfo(op.OutDir);
            var fs = di.EnumerateFiles("*.fastq", SearchOption.AllDirectories);  // サブフォルダに分かれている場合がある
            if (!fs.Any())
            {
                Message = "Guppy basecall process error! (no create fastq files) " + Environment.NewLine
                                + message + Environment.NewLine
                                + commandRes + Environment.NewLine
                                + stdout + Environment.NewLine
                                + stderr;
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
