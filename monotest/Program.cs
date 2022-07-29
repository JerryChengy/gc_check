using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Tests;
namespace monotest
{
    class Program
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        static void Main(string[] args)
        {
            StringBuilder temp = new StringBuilder(500);
            GetPrivateProfileString("gc", "dll", "", temp, 500, "./config.ini");
            string dllStr = temp.ToString();
            string[] dllArray = dllStr.Split('|');
            temp.Clear();
            GetPrivateProfileString("gc", "filter", "", temp, 500, "./config.ini");
            string filterStr = temp.ToString();
            string[] filterArray = filterStr.Split('|');
            foreach (var dll in dllArray)
            {
                foreach (var filter in filterArray)
                {
                    PortablePdbTests pp = new PortablePdbTests();
                    pp.PortablePdbLineInfo1(dll, filter);
                }
            }
            
        }

    }
}
