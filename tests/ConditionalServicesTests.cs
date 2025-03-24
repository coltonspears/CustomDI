using System;

namespace CustomDI.Tests
{
    /// <summary>
    /// Test cases for conditional services.
    /// </summary>
    public static class ConditionalServicesTests
    {
        // Test interfaces and classes
        public interface IConditionalService { string GetValue(); }
        public class ConditionalServiceImplementation : IConditionalService { public string GetValue() { return "Conditional"; } }
        public class AlternativeServiceImplementation : IConditionalService { public string GetValue() { return "Alternative"; } }
        
        /// <summary>
        /// Runs all conditional services tests.
        /// </summary>
        public static void RunAllTests()
        {
            TestRunner.AddTest("Conditional_Service_When_True", Test_Conditional_Service_When_True);
            TestRunner.AddTest("Conditional_Service_When_False", Test_Conditional_Service_When_False);
            TestRunner.AddTest("Multiple_Conditional_Services", Test_Multiple_Conditional_Services);
            
            // Run all tests
            TestRunner.RunTests();
        }
        
        /// <summary>
        /// Tests that a conditional service is resolved when the condition is true.
        /// </summary>
        private static void Test_Conditional_Service_When_True()
        {
            var container = ContainerFactory.CreateContainer();
            
            // Register a service with a condition that is always true
            container.Register<IConditionalService, ConditionalServiceImplementation>()
                .When(context => true);
            
            // Resolve the service
            var service = container.Resolve<IConditionalService>();
            
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.Assert(service is ConditionalServiceImplementation, "Service should be of type ConditionalServiceImplementation");
            TestRunner.AreEqual("Conditional", service.GetValue(), "Service should return correct value");
        }
        
        /// <summary>
        /// Tests that a conditional service is not resolved when the condition is false.
        /// </summary>
        private static void Test_Conditional_Service_When_False()
        {
            var container = ContainerFactory.CreateContainer();
            
            // Register a service with a condition that is always false
            container.Register<IConditionalService, ConditionalServiceImplementation>()
                .When(context => false);
            
            // Register a fallback service
            container.Register<IConditionalService, AlternativeServiceImplementation>();
            
            // Resolve the service - should get the fallback
            var service = container.Resolve<IConditionalService>();
            
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.Assert(service is AlternativeServiceImplementation, "Service should be of type AlternativeServiceImplementation");
            TestRunner.AreEqual("Alternative", service.GetValue(), "Service should return correct value");
        }
        
        /// <summary>
        /// Tests that multiple conditional services work correctly.
        /// </summary>
        private static void Test_Multiple_Conditional_Services()
        {
            var container = ContainerFactory.CreateContainer();
            
            // Register two services with different conditions
            container.Register<IConditionalService, ConditionalServiceImplementation>()
                .When(context => context.GetHashCode() % 2 == 0);
            
            container.Register<IConditionalService, AlternativeServiceImplementation>()
                .When(context => context.GetHashCode() % 2 != 0);
            
            // Resolve all services
            var services = container.ResolveAll<IConditionalService>();
            
            // Should get exactly one service depending on the hash code
            int count = 0;
            foreach (var service in services)
            {
                count++;
                TestRunner.Assert(service != null, "Service should not be null");
            }
            
            TestRunner.Assert(count == 1, "Should resolve exactly one service");
        }
    }
}
