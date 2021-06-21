using System.IO;

namespace WfComponent.External
{

    /// <summary>
    ///   show Tree View window.
    ///   if target file is not exist, no throw error .
    /// </summary>
    public static class TreeView
    {
        public static readonly string binaryName = "njplot.exe";

        public static string TreeViewStart(string GuidetreeFile)
        {

            var res = string.Empty;
            if (!File.Exists(GuidetreeFile)) res += "not found Guidetree file.\n  Path  " + GuidetreeFile;

            // find binary Path
            var binFullPath = CommandUtils.FindProgramFile(
                                        System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                        binaryName, false, false);


            // TreeView 起動
            res = RequestCommand.ExecCommandLeave(
                                                    binFullPath,
                                                    GuidetreeFile);
            return res;

        }

    }
}
