using System;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public static class IGV
    {
        // IGV Windows 版（JDK11 同梱）
        // https://software.broadinstitute.org/software/igv/

        public static readonly string igvCmd = "igv.bat";
        public static readonly string samtoolsCmd = "samtools.exe";
        public static readonly string javaCmd = "java.exe";

        public static string IGVstart(string binDir, string referencePath, string sortedBamPath, bool isBamCheck = true)
        {
            var res = string.Empty;
            if (!File.Exists(referencePath)) res += "not found reference fasta file.\n  Path  " + referencePath;
            if (!File.Exists(sortedBamPath) && isBamCheck) res += "not found sorted bam file.\n  Path  " + sortedBamPath;

            // find IGV-jar Path
            binDir = string.IsNullOrEmpty(binDir) ?
                AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') :
                binDir;

            var binFullPath = CommandUtils.FindProgramFile(binDir, igvCmd, false, false);
            if (string.IsNullOrEmpty(binFullPath)) res += "not found IGV-program.\n";
            if (!string.IsNullOrEmpty(res)) return res;  //　指定されたファイルが無い。


            // create BAM index
            if (!System.IO.File.Exists(referencePath + ".fai")) res = CreateIndex(binDir, referencePath, false);
            if (!string.IsNullOrEmpty(res)) return res;
            if (!System.IO.File.Exists(sortedBamPath + ".bai")) res = CreateIndex(binDir, sortedBamPath, true);
            if (!string.IsNullOrEmpty(res)) return res;

            // IGV 起動
            res = RequestCommand.ExecCommandLeave(
                                                    binFullPath,
                                                    $" -g {Utils.FileUtils.GetDoubleQuotationPath(referencePath)}  {Utils.FileUtils.GetDoubleQuotationPath(sortedBamPath)}");
            return res;
        }

        // それぞれインデックスが必要
        public static string CreateIndex(string binDir, string target, Boolean isBam = true)
        {
            // var indexOp = isBam ? "index" : "faidx";
            var indexOp = isBam ? SamtoolsOptions.bamIndex : SamtoolsOptions.fastaIndex;

            var samtools = new Samtools(new SamtoolsOptions());
            if (!samtools.IsProcessSuccess()) return samtools.Message;  // create check.　ありえないはず

            var res = samtools.CreateIndex(indexOp, target);

            if (!samtools.IsProcessSuccess()) return res; 
            return string.Empty;
          }

    }
}
