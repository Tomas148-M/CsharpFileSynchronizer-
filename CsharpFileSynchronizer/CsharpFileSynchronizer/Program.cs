using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpFileSynchronizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hellow This is File Synchronizer");

            Logger logger = new Logger("app.log");

            FileSynchronizer fileSynchronizer = new FileSynchronizer(logger.LoggerInstance, args[0], args[1]);
            fileSynchronizer.StartSync(1);

            // Dispose the Serilog logger when done
            Log.CloseAndFlush();
        }
    }
}
