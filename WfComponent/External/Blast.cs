using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public class Blast : BaseProcess
    {

        public static string binaryName = "blastn.exe";
        private BlastOption op;
        private RequestCommand proc;
        public Blast(BlastOption options) : base(options)
        {
            this.op = options;
            // BLAST に必須のパラメータをチェック

            if (!string.IsNullOrEmpty(op.binaryPath) && File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath";

            if( string.IsNullOrEmpty(op.queryFasta) ||
                string.IsNullOrEmpty(op.reference) ||
                string.IsNullOrEmpty(op.outFile) )
                Message = Message = "required parameter is not found, Please check parameters";


            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "Blast Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            // create command string.
            var args = new List<string>();
            args.Add("-db");
            args.Add("\"" + op.reference + "\"");
            args.Add("-query");
            args.Add("\"" + op.queryFasta + "\"");
            if(! string.IsNullOrEmpty(op.outFormat)) args.Add("-outfmt " + op.outFormat);
            if( ! string.IsNullOrEmpty(op.outHits)) args.Add("-max_target_seqs " + op.outHits );
            if( ! string.IsNullOrEmpty(op.useCore)) args.Add("-num_threads " + op.useCore);
            if( ! string.IsNullOrEmpty(op.evalue)) args.Add("-evalue " + op.evalue);
            if (!string.IsNullOrEmpty(op.otherOptions)) args.Add(op.otherOptions); // 引き数のパラメータ文字も含む
            args.Add("-out");
            args.Add("\"" + op.outFile + "\"");

            SetArguments(string.Join(" ", args));  // blast arguments

            var workDir = Path.GetDirectoryName(op.outFile);
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


    public class BlastOption : BaseOptions
    {
        [Required()]
        public string reference;    // require
        [Required()]
        public string queryFasta; // require
        [Required()]
        public string outFile;       // require
        public string outHits;
        public string useCore;
        public string evalue;

        public string outFormat;

    }

}
