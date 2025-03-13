using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDI.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CustomDI.Tests.ContainerTests.RunAllTests();
            //CustomDI.Tests.TestRunner.RunTests();
            //CustomDI.Tests.FixesTests.RunAllTests();
            Console.ReadLine();
        }
    }
}
