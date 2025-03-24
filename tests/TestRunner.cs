using System;
using System.Collections.Generic;

namespace CustomDI.Tests
{
    /// <summary>
    /// Simple test runner for the CustomDI tests.
    /// </summary>
    public static class TestRunner
    {
        // Delegates for test operations
        private static Action<string, Action> _addTest = AddTestImpl;
        private static Action _runTests = RunTestsImpl;
        private static Action<bool, string> _assert = AssertImpl;
        private static Action<string, string, string> _areEqual = AreEqualImpl;

        // Test collection
        private static Dictionary<string, Action> _tests = new Dictionary<string, Action>();
        private static List<string> _failures = new List<string>();
        private static int _passCount = 0;
        private static int _failCount = 0;

        /// <summary>
        /// Adds a test to the test collection.
        /// </summary>
        /// <param name="name">The name of the test.</param>
        /// <param name="test">The test action.</param>
        public static void AddTest(string name, Action test)
        {
            _addTest(name, test);
        }

        /// <summary>
        /// Runs all tests in the test collection.
        /// </summary>
        public static void RunTests()
        {
            _runTests();
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">The message to display if the condition is false.</param>
        public static void Assert(bool condition, string message)
        {
            _assert(condition, message);
        }

        /// <summary>
        /// Asserts that two values are equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="message">The message to display if the values are not equal.</param>
        public static void AreEqual(string expected, string actual, string message)
        {
            _areEqual(expected, actual, message);
        }

        // Default implementations
        private static void AddTestImpl(string name, Action test)
        {
            _tests[name] = test;
        }

        private static void RunTestsImpl()
        {
            _failures.Clear();
            _passCount = 0;
            _failCount = 0;

            Console.WriteLine("Running tests...");
            foreach (var test in _tests)
            {
                try
                {
                    Console.WriteLine($"Running test: {test.Key}");
                    test.Value();
                    _passCount++;
                    Console.WriteLine($"Test passed: {test.Key}");
                }
                catch (Exception ex)
                {
                    _failCount++;
                    _failures.Add($"{test.Key}: {ex.Message}");
                    Console.WriteLine($"Test failed: {test.Key} - {ex.Message}");
                }
            }

            Console.WriteLine($"Tests completed. Passed: {_passCount}, Failed: {_failCount}");
            if (_failures.Count > 0)
            {
                Console.WriteLine("Failures:");
                foreach (var failure in _failures)
                {
                    Console.WriteLine($"  {failure}");
                }
            }
        }

        private static void AssertImpl(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }

        private static void AreEqualImpl(string expected, string actual, string message)
        {
            if (expected != actual)
            {
                throw new Exception($"{message} - Expected: {expected}, Actual: {actual}");
            }
        }
    }
}
