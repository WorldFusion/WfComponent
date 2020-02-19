namespace WfComponent.Base
{
    public interface IProcess
    {
        void SetArguments(string arguments);
        string StartProcess();
        string StopProcess();
        bool IsProcessSuccess();
        string GetMessage();
        int ProcessId();
    }
}
