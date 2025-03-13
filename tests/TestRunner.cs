using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomDI.Tests
{
    /// <summary>
    /// Simple test framework for testing the dependency injection container.
    /// </summary>
    public static class TestRunner
    {
        private static readonly List<TestCase> _testCases = new List<TestCase>();
        private static int _passedTests = 0;
        private static int _failedTests = 0;

        /// <summary>
        /// Adds a test case to the test runner.
        /// </summary>
        /// <param name="name">The name of the test case.</param>
        /// <param name="testAction">The test action.</param>
        public static void AddTest(string name, Action testAction)
        {
            _testCases.Add(new TestCase(name, testAction));
        }

        /// <summary>
        /// Runs all test cases.
        /// </summary>
        public static void RunTests()
        {
            Console.WriteLine("Running tests...");
            Console.WriteLine();

            foreach (var testCase in _testCases)
            {
                RunTest(testCase);
            }

            Console.WriteLine();
            Console.WriteLine($"Test results: {_passedTests} passed, {_failedTests} failed");
        }

        /// <summary>
        /// Runs a single test case.
        /// </summary>
        /// <param name="testCase">The test case to run.</param>
        private static void RunTest(TestCase testCase)
        {
            Console.Write($"Running test: {testCase.Name}... ");

            try
            {
                testCase.TestAction();
                Console.WriteLine("PASSED");
                _passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAILED");
                Console.WriteLine($"  Error: {ex.Message}");
                _failedTests++;
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">The error message if the condition is false.</param>
        public static void Assert(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new Exception(message ?? "Assertion failed");
            }
        }

        /// <summary>
        /// Asserts that two objects are equal.
        /// </summary>
        /// <typeparam name="T">The type of the objects.</typeparam>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="message">The error message if the objects are not equal.</param>
        public static void AreEqual<T>(T expected, T actual, string message = null)
        {
            if (!Equals(expected, actual))
            {
                throw new Exception(message ?? $"Expected: {expected}, Actual: {actual}");
            }
        }

        /// <summary>
        /// Asserts that an action throws an exception of the specified type.
        /// </summary>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <param name="action">The action to execute.</param>
        /// <param name="message">The error message if the exception is not thrown.</param>
        public static void ThrowsException<TException>(Action action, string message = null)
            where TException : Exception
        {
            try
            {
                action();
                throw new Exception(message ?? $"Expected exception of type {typeof(TException).Name} was not thrown");
            }
            catch (Exception ex)
            {
                if (!(ex is TException))
                {
                    throw new Exception(message ?? $"Expected exception of type {typeof(TException).Name}, but got {ex.GetType().Name}");
                }
            }
        }

        /// <summary>
        /// Represents a test case.
        /// </summary>
        private class TestCase
        {
            /// <summary>
            /// Gets the name of the test case.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the test action.
            /// </summary>
            public Action TestAction { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TestCase"/> class.
            /// </summary>
            /// <param name="name">The name of the test case.</param>
            /// <param name="testAction">The test action.</param>
            public TestCase(string name, Action testAction)
            {
                Name = name;
                TestAction = testAction;
            }
        }
    }
}
