using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;
using WfComponent.Utils;

namespace WfComponent.External
{
    public class PycoQC : BaseProcess
    {
        public static string binaryName = "pycoQC.exe";
        public static string linuxName = "pycoQC";  // 現行はWSLつかう
                                                    // class grobal.
        private PycoQCOptions op;
        private RequestCommand proc;

        // constractor....
        public PycoQC(PycoQCOptions options) : base(options)
        {
            this.Message = string.Empty;
            this.op = options;
            if (string.IsNullOrEmpty(op.summaryText) ||
                string.IsNullOrEmpty(op.outHtml))
                Message += "\nrequired parameter is not found, Please check parameters";

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
                return ConstantValues.ErrorMessage;
            }

            isSuccess = false;   // 処理前の初期値 
            op.isLinux = true;   // 2019.09 時点は強制
            var summaryFilePth = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.summaryText) :
                                                               op.summaryText;
            var outHtmlPath = op.isLinux ? FileUtils.WindowsPath2LinuxPath(op.outHtml) :
                                                          op.outHtml;

            // create command string.
            var args = new List<string>();
            args.Add("--summary_file ");    //  Minimal 
            args.Add(summaryFilePth);     // Guppy out summary text
            args.Add("--html_outfile ");    
            args.Add(outHtmlPath);
            arguments = string.Join(" ", args);

            var workDir = Path.GetDirectoryName( op.outHtml);  // Windows Path
            proc = RequestCommand.GetInstance();
            var commandIsError = proc.ExecuteWSLCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

           

            // ここまで来たら正常終了
            isSuccess = !commandIsError;
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
