using System.Diagnostics;

namespace ProcessMonitor.Models
{
    public class MProcess
    {
        public int Id { get; set; }
        public string Application { get; set; }
        public string Agurment { get; set; }
        public Process Process { get; set; }
    }
}