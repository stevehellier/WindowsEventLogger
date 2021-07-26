using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uptime_recorder
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App(args);
            app.Run();
        }
    }
}
