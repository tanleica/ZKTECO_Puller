namespace BioMetrixCore.Info
{
    public class ThreadResult
    {
        public Machine Machine { get; set; }
        public bool IsSuccess { get; set; }
        public bool Cancelled { get; set; }

        public ThreadResult(Machine machine, bool isSuccess, bool cancelled = false)
        {
            Machine = machine;
            IsSuccess = isSuccess;
            Cancelled = cancelled;
        }
    }
}
