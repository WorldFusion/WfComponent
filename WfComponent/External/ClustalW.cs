using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{



    public class ClustalW : BaseProcess
    {
        public static string binaryName = "clustalw2.exe";
        private ClustalWOptions op;
        private RequestCommand proc;

        public ClustalW(ClustalWOptions options) : base(options)
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
                Message = "ClustalW Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            // command args is set
            SetArguments(GetCommandArgs());

            var workDir = Path.GetDirectoryName(binaryPath);  // libとか必要なので
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

            if (string.IsNullOrEmpty(stderr))
                return Utils.ConstantValues.ErrorMessage;


            isSuccess = true; // 取り敢えず終わって居るハズ
            return Utils.ConstantValues.NormalEndMessage;
        }


        private string GetCommandArgs()
        {
            if (string.IsNullOrEmpty(op.inputDataType))
                op.inputDataType = ClustalWOptions.typeDna;

            if (string.IsNullOrEmpty(op.outFormatType))
                op.outFormatType = ClustalWOptions.bootstrap;

            var args = new List<string>();
            args.Add(op.outFormatType); // -BOOTSTRAP
            args.Add(op.inputDataType);
            args.Add("-INFILE=" + op.targetFile);
            args.Add("-OUTFILE=" + op.outFile);

            return string.Join(" ", args);
        }

        public override string StopProcess()
        {
            this.Message = "Cancel clustalw2 process!!";
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;
        }


    }
}
