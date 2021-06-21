using System.Collections.Generic;
using System.IO;
using WfComponent.External.Properties;

namespace WfComponent.External
{
    public static class Tablet
    {
        public static string binaryName = "tablet.exe";


        public static string TabletStart(string binDir, string referencePath, string bamPath, bool isBamCheck = true)
        {
            var res = string.Empty;
            if (!File.Exists(referencePath)) res += "not found reference fasta file.\n  Path  " + referencePath;
            if (!File.Exists(bamPath) && isBamCheck) res += "not found sorted bam file.\n  Path  " + bamPath;

            // find tablet windows binary Path
            binDir = string.IsNullOrEmpty(binDir) ?
                System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') :
                binDir;

            var binFullPath = CommandUtils.FindProgramFile(binDir, binaryName, false, false);
            if (string.IsNullOrEmpty(binFullPath)) res += "not found Tablet, Please check bin-directory .\n";
            if (!string.IsNullOrEmpty(res)) return res;  //　指定されたファイルが無い。


            // create BAM index // IGV と同じ
            if (!System.IO.File.Exists(referencePath + ".fai")) res = IGV.CreateIndex(binDir, referencePath, false);
            if (!string.IsNullOrEmpty(res)) return res;
            if (!System.IO.File.Exists(bamPath + ".bai")) res = IGV.CreateIndex(binDir, bamPath, true);
            if (!string.IsNullOrEmpty(res)) return res;

            // IGV 起動
            res = RequestCommand.ExecCommandLeave(
                                                    binFullPath,
                                                    $" {Utils.FileUtils.GetDoubleQuotationPath(referencePath)}  {Utils.FileUtils.GetDoubleQuotationPath(bamPath)}");
            return res;
        }
    }
}
