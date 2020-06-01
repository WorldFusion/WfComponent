using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WfComponent
{
    public static class CommandUtils
    {

        // Windows 版のプログラムを探してFullパスを返します。
        public static string FindProgramFile(string searchDir, string pgName, bool isWSL = false,  bool isNotFoundErr = false)
        {
            if(! isWSL)
                if (!pgName.EndsWith("exe") && !pgName.EndsWith("bat")) pgName += ".exe";
            var di = new DirectoryInfo(searchDir);
            var files = di.EnumerateFiles(pgName, SearchOption.AllDirectories);

            // find success. 
            if (files.Count() > 0) return files.First().FullName;

            if (isNotFoundErr)
            {
                System.Windows.MessageBox.Show("not found program error.", "Required " + pgName + " is not found.");
                Environment.Exit(1);
            }
            return string.Empty;
        }

        public static bool IsWslExist()
        {
            var wslExist = File.Exists(RequestCommand.WslCommand);
            return wslExist;
        }

        public static bool IsUbuntuImported()
        {
            var proc = new External.CollectWsl();
            var call = proc.CallIsInitUbuntu();
            if (!Utils.ConstantValues.NormalEndMessage.Equals(call))
                return false;   // ubuntu 入って居ないと判定

            // System.Diagnostics.Debug.WriteLine(proc.stdout + proc.stderr);
            var res = proc.stdout.StartsWith("OK");    // 必要なプログラムを入れたUbuntuを持っている

            return res;
        }

    }
}
