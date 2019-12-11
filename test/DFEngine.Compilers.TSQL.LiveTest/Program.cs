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

            foreach(string file in files)
            {
                string tsqlContent = File.ReadAllText(file);
   
                var options = new CompilerOptions() { ConsiderQueries = true };
                var result = compiler.Compile(tsqlContent, "std_server", "std_db", file, options);
            }
        }
    }
}
