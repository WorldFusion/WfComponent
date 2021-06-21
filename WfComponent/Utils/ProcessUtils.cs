using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WfComponent.Utils
{
    public static class ProcessUtils
    {
        public static void ReleaseOlderFiles(string workDir, int[] pids)
        {
            string[] files = Directory.GetFiles(workDir);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                if (fi.LastAccessTime < DateTime.Now.AddMilliseconds(-5000))
                {
                    try
                    {
                        Process[] runningProcs = Process.GetProcesses();

                        foreach (Process proc in runningProcs)
                        {
                            foreach (int pid in pids)
                            {
                                if (pid == proc.Id)
                                {
                                    TryKillProcessByProcessId(pid);
                                }
                            }
                        }
                    }

                    catch { }
                    Thread.Sleep(500);
                    fi.Delete();
                }

            }
        }


        public static bool TryKillProcessByProcessId(int processID)
        {
            try
            {
                Process.GetProcessById((int)processID).Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static int DefaultCpuCore()
        {
            var coreCnt = Environment.ProcessorCount;
            if (coreCnt > 5)
                return (coreCnt - 2);
            else
                return (coreCnt - 1);
        }

        public static string MaxCpuCore()
        {
            return Environment.ProcessorCount.ToString();
        }

        public static string CpuCore()
        {
            return DefaultCpuCore().ToString();
        }

        // public static void OutLog(string mes)
        //     => System.Diagnostics.Debug.WriteLine(mes);
    }
}
