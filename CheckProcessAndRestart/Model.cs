using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckProcessAndRestart
{
    public class Model
    {
        public class ComputerProcess
        {
            public string ComputerName { get; set; }
            public List<ComputerProcessItem> ProcessName { get; set; }

            public class ComputerProcessItem
            {
                public string Name { get; set; }
                public string Mode { get; set; }
                public string Path { get; set; }
                public string Args { get; set; }
                public string Directory { get; set; }
            }
        }
    }
}
