using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace CustomDI.Tests
{
    /// <summary>
    /// Main test program for running NUnit tests.
    /// </summary>
    [TestFixture]
    public class TestProgram
    {
        [OneTimeSetUp]
        public void Setup()
        {
            // Ensure the CustomDI assembly is loaded
            string assemblyPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "CustomDI.dll");
            
            if (File.Exists(assemblyPath))
            {
                Assembly.LoadFrom(assemblyPath);
                Console.WriteLine($"Loaded CustomDI assembly from {assemblyPath}");
            }
            else
            {
                Console.WriteLine($"Warning: Could not find CustomDI assembly at {assemblyPath}");
            }
        }

        [Test]
        public void RunAllTests()
        {
            Console.WriteLine("Running all CustomDI tests with NUnit...");
            
            // Run container tests
            Console.WriteLine("\n=== Container Tests ===");
            var containerAdapter = new NUnitTestAdapter();
            containerAdapter.RunContainerTests();
            
            // Run fixes tests
            Console.WriteLine("\n=== Fixes Tests ===");
            var fixesAdapter = new NUnitTestAdapter();
            fixesAdapter.RunFixesTests();
            
            // Run conditional services tests
            Console.WriteLine("\n=== Conditional Services Tests ===");
            var conditionalAdapter = new NUnitTestAdapter();
            conditionalAdapter.RunConditionalServicesTests();
            
            Console.WriteLine("\nAll tests completed successfully!");
        }
    }
}
