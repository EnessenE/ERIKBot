using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ERIKBot
{
    class Program
    {
        public static Task Main(string[] args)
            => Startup.RunAsync(args);
    }
}
