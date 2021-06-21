using System.IO;

namespace WfComponent.External
{
    public class AliView
    {

        public static string binaryName = "aliview.jar";
        public static readonly string javaCmd = "java.exe";

        public static string AliViewStart(string alignmentFastaPath)
        {
            var res = string.Empty;
            if (!File.Exists(alignmentFastaPath)) res += "not found alignment file.  " + alignmentFastaPath;

            // find IGV-jar Path
            var searchDir = Path.Combine(
                                        System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                        "bin");


            var javaPath = CommandUtils.FindProgramFile(searchDir, javaCmd, false, false);
            var appJarPath = CommandUtils.FindProgramFile(searchDir, binaryName, true, false);

            if (string.IsNullOrEmpty(javaPath)) res += "not found java program.";
            if (string.IsNullOrEmpty(appJarPath)) res += "not found Alivew program.";
            if (!string.IsNullOrEmpty(res)) return res;  //　指定されたファイルが無い。



            // AliView 起動
            res += RequestCommand.ExecCommandLeave(
                                                    javaPath,
                                                    $" -jar {Utils.FileUtils.GetDoubleQuotationPath(appJarPath)}  {Utils.FileUtils.GetDoubleQuotationPath(alignmentFastaPath)}");
            return res;
        }

    }
}
