using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //guide followed from this video https://www.youtube.com/watch?v=g1VWGdHRkHs
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = @"C:\Users\Kevin Ying\AppData\Local\Programs\Python\Python37\python.exe";
            var script = @"C:\Users\Kevin Ying\Source\Repos\backusd\StockTrader\PythonApplication1\PythonApplication1.py ";
            var val = 1122;
            
            psi.Arguments = $"\"{script}\" \"{val}\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
          try{
                using (Process p = Process.Start(psi))
                {
                    using (StreamReader reader = p.StandardOutput)
                    {
                        String result = reader.ReadToEnd();
                        
                        Console.Write(result);

                        Console.Write("Back in C#");
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception e) {
                Console.Write("Something went wrong! Error {0}", e);
            }   
        }
    }
}
