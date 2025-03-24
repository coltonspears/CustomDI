using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace CustomDI.Tests
{
    /// <summary>
    /// NUnit test adapter for the existing test framework.
    /// </summary>
    [TestFixture]
    public class NUnitTestAdapter
    {
        private static Dictionary<string, Action> _tests = new Dictionary<string, Action>();
        private static List<string> _failures = new List<string>();
        private static int _passCount = 0;
        private static int _failCount = 0;

        /// <summary>
        /// Runs all container tests using NUnit.
        /// </summary>
        [Test]
        public void RunContainerTests()
        {
            // Reset test state
            _tests.Clear();
            _failures.Clear();
            _passCount = 0;
            _failCount = 0;
            
            // Replace the TestRunner implementation with our NUnit adapter
            ReplaceTestRunner();
            
            // Run the tests
            ContainerTests.RunAllTests();
            
            // Check for failures
            if (_failures.Count > 0)
            {
                Assert.Fail($"Container tests failed: {string.Join(", ", _failures)}");
            }
            
            Console.WriteLine($"Container tests passed: {_passCount}, failed: {_failCount}");
        }
        
        /// <summary>
        /// Runs all fixes tests using NUnit.
        /// </summary>
        [Test]
        public void RunFixesTests()
        {
            // Reset test state
            _tests.Clear();
            _failures.Clear();
            _passCount = 0;
            _failCount = 0;
            
            // Replace the TestRunner implementation with our NUnit adapter
            ReplaceTestRunner();
            
            // Run the tests
            FixesTests.RunAllTests();
            
            // Check for failures
            if (_failures.Count > 0)
            {
                Assert.Fail($"Fixes tests failed: {string.Join(", ", _failures)}");
            }
            
            Console.WriteLine($"Fixes tests passed: {_passCount}, failed: {_failCount}");
        }
        
        /// <summary>
        /// Runs all conditional services tests using NUnit.
        /// </summary>
        [Test]
        public void RunConditionalServicesTests()
        {
            // Reset test state
            _tests.Clear();
            _failures.Clear();
            _passCount = 0;
            _failCount = 0;
            
            // Replace the TestRunner implementation with our NUnit adapter
            ReplaceTestRunner();
            
            // Run the tests
            ConditionalServicesTests.RunAllTests();
            
            // Check for failures
            if (_failures.Count > 0)
            {
                Assert.Fail($"Conditional services tests failed: {string.Join(", ", _failures)}");
            }
            
            Console.WriteLine($"Conditional services tests passed: {_passCount}, failed: {_failCount}");
        }
        
        /// <summary>
        /// Replaces the TestRunner implementation with our NUnit adapter.
        /// </summary>
        private void ReplaceTestRunner()
        {
            // Get the TestRunner type
            Type testRunnerType = typeof(ContainerTests).Assembly.GetType("CustomDI.Tests.TestRunner");
            if (testRunnerType == null)
            {
                Assert.Fail("Could not find TestRunner type");
                return;
            }
            
            // Replace the AddTest method
            MethodInfo addTestMethod = testRunnerType.GetMethod("AddTest", BindingFlags.Public | BindingFlags.Static);
            if (addTestMethod == null)
            {
                Assert.Fail("Could not find AddTest method");
                return;
            }
            
            // Create a delegate that points to our AddTest method
            Delegate addTestDelegate = Delegate.CreateDelegate(addTestMethod.GetParameters()[1].ParameterType, 
                typeof(NUnitTestAdapter).GetMethod("AddTest", BindingFlags.NonPublic | BindingFlags.Static));
            
            // Replace the RunTests method
            MethodInfo runTestsMethod = testRunnerType.GetMethod("RunTests", BindingFlags.Public | BindingFlags.Static);
            if (runTestsMethod == null)
            {
                Assert.Fail("Could not find RunTests method");
                return;
            }
            
            // Create a delegate that points to our RunTests method
            Delegate runTestsDelegate = Delegate.CreateDelegate(typeof(Action), 
                typeof(NUnitTestAdapter).GetMethod("RunTests", BindingFlags.NonPublic | BindingFlags.Static));
            
            // Replace the Assert method
            MethodInfo assertMethod = testRunnerType.GetMethod("Assert", BindingFlags.Public | BindingFlags.Static);
            if (assertMethod == null)
            {
                Assert.Fail("Could not find Assert method");
                return;
            }
            
            // Create a delegate that points to our Assert method
            Delegate assertDelegate = Delegate.CreateDelegate(typeof(Action<bool, string>), 
                typeof(NUnitTestAdapter).GetMethod("AssertTrue", BindingFlags.NonPublic | BindingFlags.Static));
            
            // Replace the AreEqual method
            MethodInfo areEqualMethod = testRunnerType.GetMethod("AreEqual", BindingFlags.Public | BindingFlags.Static);
            if (areEqualMethod == null)
            {
                Assert.Fail("Could not find AreEqual method");
                return;
            }
            
            // Create a delegate that points to our AreEqual method
            Delegate areEqualDelegate = Delegate.CreateDelegate(typeof(Action<string, string, string>), 
                typeof(NUnitTestAdapter).GetMethod("AreEqual", BindingFlags.NonPublic | BindingFlags.Static));
            
            // Set the delegates using reflection
            FieldInfo addTestField = testRunnerType.GetField("_addTest", BindingFlags.NonPublic | BindingFlags.Static);
            if (addTestField != null)
            {
                addTestField.SetValue(null, addTestDelegate);
            }
            
            FieldInfo runTestsField = testRunnerType.GetField("_runTests", BindingFlags.NonPublic | BindingFlags.Static);
            if (runTestsField != null)
            {
                runTestsField.SetValue(null, runTestsDelegate);
            }
            
            FieldInfo assertField = testRunnerType.GetField("_assert", BindingFlags.NonPublic | BindingFlags.Static);
            if (assertField != null)
            {
                assertField.SetValue(null, assertDelegate);
            }
            
            FieldInfo areEqualField = testRunnerType.GetField("_areEqual", BindingFlags.NonPublic | BindingFlags.Static);
            if (areEqualField != null)
            {
                areEqualField.SetValue(null, areEqualDelegate);
            }
        }
        
        /// <summary>
        /// Adds a test to the test collection.
        /// </summary>
        /// <param name="name">The name of the test.</param>
        /// <param name="test">The test action.</param>
        private static void AddTest(string name, Action test)
        {
            _tests[name] = test;
        }
        
        /// <summary>
        /// Runs all tests in the test collection.
        /// </summary>
        private static void RunTests()
        {
            foreach (var test in _tests)
            {
                try
                {
                    Console.WriteLine($"Running test: {test.Key}");
                    test.Value();
                    _passCount++;
                }
                catch (Exception ex)
                {
                    _failCount++;
                    _failures.Add($"{test.Key}: {ex.Message}");
                    Console.WriteLine($"Test failed: {test.Key} - {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">The message to display if the condition is false.</param>
        private static void AssertTrue(bool condition, string message)
        {
            Assert.IsTrue(condition, message);
        }
        
        /// <summary>
        /// Asserts that two values are equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="message">The message to display if the values are not equal.</param>
        private static void AreEqual(string expected, string actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
        }
    }
}
