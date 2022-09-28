namespace BioMetrixCore.Info
{
    public class Machine
    {
        public int MachineNumber { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string LastTime { get; set; }
        public string TenantCode { get; set; }
        public string UserName { get; set; }    
        public string Password { get; set; }    

    }

    public class ThreadInfo { 
        public Machine Machine { get; set; }
        public bool IsRunning { get; set; }

        public ThreadInfo(Machine machine, bool isRunning)
        {
            Machine = machine;
            IsRunning = isRunning;
        }
    }
}
