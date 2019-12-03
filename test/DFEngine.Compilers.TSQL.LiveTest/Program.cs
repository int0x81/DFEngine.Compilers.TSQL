using System;
using System.IO;

namespace DFEngine.Compilers.TSQL.LiveTest
{
    /// <summary>
    /// Simple console application for testting purposes that compiles a local script
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            const string folderPath = @"C:\Users\Finn.Fiedler\source\Workspaces\BI\BI_ETL\ETL_Scripts\active";

            var files = Directory.EnumerateFiles(folderPath, "*.sql");

            var compiler = new Compiler();

            int good = 0;
            int fail = 0;

            foreach(string file in files)
            {
                string tsqlContent = File.ReadAllText(file);
                //try
                //{
                    var options = new CompilerOptions() { ConsiderQueries = true };
                    compiler.Compile(tsqlContent, "std_server", "std_db", file, options);
                    good++;
                //}
                //catch(Exception) { fail++; }
                Console.WriteLine(file + " Done");
            }
        }
    }
}
