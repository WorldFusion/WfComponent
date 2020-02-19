namespace WfComponent.Utils
{
    public static class WindowsShutoffReboot
    {

        
        public static string ShutDown()
        {
            var command = "shutdown.exe";
            var args = "-f -s -t 3";
            var commandRes = RequestCommand.ExecCommandLeave(command, args);
            return commandRes;
        }


        public static string Reboot()
        {
            var command = "shutdown.exe";
            var args = "-f -r -t 3";
            var commandRes = RequestCommand.ExecCommandLeave(command, args);
            return commandRes;
        }
    }
}
