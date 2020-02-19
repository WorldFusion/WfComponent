using System;
using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;
using WfComponent.Utils;

namespace WfComponent.External
{

    public class Trimmomatic : BaseProcess
    {

        public static string binaryName = "java.exe";
        private TrimmomaticOptions op;
        private RequestCommand proc;
        

        public static readonly string programJarName = "trimmomatic-0.39.jar";
        public static readonly string programJarName1 = "trimmomatic.jar";
        public static readonly string mainClass = "org.usadellab.trimmomatic.Trimmomatic";
        public static readonly string pairend = "PE";
        public static readonly string singleend = "SE";

        private static readonly string threads = "-threads";
        private static readonly string phred33 = "-phred33";
        private static readonly string headTrim = "LEADING:";
        private static readonly string tailTrim = "TRAILING:";     // FluGAS1 後方Trim行って居ない。
        private static readonly string window = "SLIDINGWINDOW:";
        private static readonly string minlen = "MINLEN:";
        private static readonly string outLog = "-trimlog";


        private string jarPath;
        public Trimmomatic (TrimmomaticOptions options) : base(options)
        {
            this.op = options;
            if (File.Exists(op.binaryPath))
                this.binaryPath = op.binaryPath;
            else if (!BinaryExist(binaryName)) // 救済処置含める
                Message = "required program path is not found, Please check OPTION:binaryPath = " + op.binaryPath;

            jarPath = CommandUtils.FindProgramFile(
                                                    AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                                    programJarName1,
                                                    true,
                                                    false);
            if(string.IsNullOrEmpty(jarPath))
                Message = "required program path is not found, Please check bin directory = " + programJarName1;

            isSuccess = string.IsNullOrEmpty(Message);
        }

        public override string StartProcess()
        {
            if (!isSuccess)
            {
                Message = "Trimmomatic Process is initialisation Error";
                return Utils.ConstantValues.ErrorMessage;
            }

            // command args is set
            SetArguments(GetCommandArgs());

            var workDir = Path.GetDirectoryName(op.outFastq1);
            proc = RequestCommand.GetInstance();
            var commandRes = proc.ExecuteWinCommand(binaryPath, arguments, ref stdout, ref stderr, workDir);
            winProcessId = proc.Pid; // to kill process ?? 

            isSuccess = true; // 取り敢えず終わって居るハズ
            return Utils.ConstantValues.NormalEndMessage;
        }

        private string trimdlog;
        private string useThread;
        private string GetCommandArgs()
        {
            this.trimdlog = Path.Combine(
                                    Path.GetDirectoryName(op.fastqPath1),
                                    "Trimmomatic.log");

            this.useThread = string.IsNullOrEmpty(op.threads) ?
                                        Environment.ProcessorCount.ToString() :
                                        op.threads;

            if (string.IsNullOrEmpty(op.outFastq2))    // out2 が なければ Single
                return SingleFastqQcCommandArgs();


            return PairFastqQcCommandArgs();
        }

        private string SingleFastqQcCommandArgs()
        {
            var args = new List<string>();
            args.Add("-classpath");
            args.Add(jarPath);
            args.Add(mainClass); // main-class
            args.Add(singleend);  // SE
            args.Add(phred33);
            args.Add(threads);
            args.Add(useThread);
            args.Add( "\"" + op.fastqPath1 + "\"");
            args.Add( "\"" + op.outFastq1 + "\"");
            args.Add($"{headTrim}{op.minPhreadScore}");
            args.Add($"{window}{op.windowSize}:{op.minPhreadScore}");
            args.Add($"{minlen}{op.minLength}");
            args.Add("\"" + outLog + "\"");
            args.Add("\"" + trimdlog + "\"");
            var commandArgs = string.Join(" ", args);
            return commandArgs;
        }

        private string PairFastqQcCommandArgs()
        {
            var outUnpaired1 = Path.Combine(
                                            Path.GetDirectoryName(op.outFastq1),
                                            FileUtils.GetFileBaseName(op.outFastq1) + "_unpaired.fastq");
            var outUnpaired2 = Path.Combine(
                                            Path.GetDirectoryName(op.outFastq2),
                                            FileUtils.GetFileBaseName(op.outFastq2) + "_unpaired.fastq");

            var args = new List<string>();
            // java is windows-edition.  // java.exe
            args.Add("-classpath");
            args.Add(jarPath);
            args.Add(mainClass); // main-class
            args.Add(pairend);    // PE
            args.Add(phred33);
            args.Add(threads);
            args.Add(useThread);
            args.Add("\"" + op.fastqPath1 + "\"");
            args.Add("\"" + op.fastqPath2 + "\"");
            args.Add("\"" + op.outFastq1 + "\"");
            args.Add("\"" + outUnpaired1 + "\"");
            args.Add("\"" + op.outFastq2 + "\"");
            args.Add("\"" + outUnpaired2 + "\"");

            args.Add($"{headTrim}{op.minPhreadScore}");
            args.Add($"{window}{op.windowSize}:{op.minPhreadScore}");
            args.Add($"{minlen}{op.minLength}");
            args.Add("\"" + outLog + "\"");
            args.Add("\"" + trimdlog + "\"");
            var commandArgs = string.Join(" ", args);
            return commandArgs;
        }


        public override string StopProcess()
        {
            this.Message = "Cancel Trimmomatic process!!";
            if (this.proc == null) return string.Empty;

            this.proc.CommandCancel();
            return Utils.ConstantValues.CanceledMessage;
        }
    }



}
