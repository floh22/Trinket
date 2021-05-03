namespace floh22.Trinket.Windows
{

    //https://github.com/Johannes-Schneider/GoldDiff/blob/master/GoldDiff/OperatingSystem/ProcessEventEventArguments.cs
    public class ProcessEventEventArguments
    {
        public int ProcessId { get; }

        public ProcessEventEventArguments(int processId)
        {
            ProcessId = processId;
        }
    }
}
