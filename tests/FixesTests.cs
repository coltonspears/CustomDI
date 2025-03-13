using System;
using System.Collections.Generic;

namespace CustomDI.Tests
{
    /// <summary>
    /// Test cases for verifying the fixes to the failing tests.
    /// </summary>
    public static class FixesTests
    {
        /// <summary>
        /// Runs all tests for the fixes.
        /// </summary>
        public static void RunAllTests()
        {
            TestRunner.AddTest("Named_Registration_Fix", Test_Named_Registration_Fix);
            TestRunner.AddTest("Keyed_Registration_Fix", Test_Keyed_Registration_Fix);
            TestRunner.AddTest("Factory_For_All_Fix", Test_Factory_For_All_Fix);
            
            // Run all tests
            TestRunner.RunTests();
        }

        /// <summary>
        /// Tests that named registrations work correctly.
        /// </summary>
        private static void Test_Named_Registration_Fix()
        {
            var container = ContainerFactory.CreateContainer();
            
            // Register two implementations with different names
            container.Register<IService, ServiceImplementation>().Named("service1");
            container.Register<IService, AnotherServiceImplementation>().Named("service2");
            
            // Create a resolve context
            var context = new ResolveContext((Container)container, null);
            
            // Resolve by name
            var service1 = context.ResolveNamed<IService>("service1");
            TestRunner.Assert(service1 != null, "Service1 should not be null");
            TestRunner.Assert(service1 is ServiceImplementation, "Service1 should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", service1.GetValue(), "Service1 should return correct value");
            
            var service2 = context.ResolveNamed<IService>("service2");
            TestRunner.Assert(service2 != null, "Service2 should not be null");
            TestRunner.Assert(service2 is AnotherServiceImplementation, "Service2 should be of type AnotherServiceImplementation");
            TestRunner.AreEqual("AnotherService", service2.GetValue(), "Service2 should return correct value");
            
            // Test the non-generic overload
            var service1ByType = context.ResolveNamed(typeof(IService), "service1");
            TestRunner.Assert(service1ByType != null, "Service1 by type should not be null");
            TestRunner.Assert(service1ByType is ServiceImplementation, "Service1 by type should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", ((IService)service1ByType).GetValue(), "Service1 by type should return correct value");
        }

        /// <summary>
        /// Tests that keyed registrations work correctly.
        /// </summary>
        private static void Test_Keyed_Registration_Fix()
        {
            var container = ContainerFactory.CreateContainer();
            
            // Register two implementations with different keys
            container.Register<IService, ServiceImplementation>().Keyed(1);
            container.Register<IService, AnotherServiceImplementation>().Keyed(2);
            
            // Create a resolve context
            var context = new ResolveContext((Container)container, null);
            
            // Resolve by key
            var service1 = context.ResolveKeyed<IService>(1);
            TestRunner.Assert(service1 != null, "Service1 should not be null");
            TestRunner.Assert(service1 is ServiceImplementation, "Service1 should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", service1.GetValue(), "Service1 should return correct value");
            
            var service2 = context.ResolveKeyed<IService>(2);
            TestRunner.Assert(service2 != null, "Service2 should not be null");
            TestRunner.Assert(service2 is AnotherServiceImplementation, "Service2 should be of type AnotherServiceImplementation");
            TestRunner.AreEqual("AnotherService", service2.GetValue(), "Service2 should return correct value");
            
            // Test the non-generic overload
            var service1ByType = context.ResolveKeyed(typeof(IService), 1);
            TestRunner.Assert(service1ByType != null, "Service1 by type should not be null");
            TestRunner.Assert(service1ByType is ServiceImplementation, "Service1 by type should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", ((IService)service1ByType).GetValue(), "Service1 by type should return correct value");
        }

        /// <summary>
        /// Tests that factory for all works correctly.
        /// </summary>
        private static void Test_Factory_For_All_Fix()
        {
            var container = ContainerFactory.CreateContainer();
            
            // Register two implementations
            container.Register<IService, ServiceImplementation>();
            container.Register<IService, AnotherServiceImplementation>();
            
            // Register a factory for all implementations
            container.RegisterFactoryForAll<IService>();
            
            // Resolve the factory
            var factory = container.Resolve<Func<IEnumerable<IService>>>();
            TestRunner.Assert(factory != null, "Factory should not be null");
            
            // Get all implementations
            var services = new List<IService>(factory());
            TestRunner.Assert(services.Count == 2, "Should resolve 2 services");
            TestRunner.Assert(services[0] is ServiceImplementation, "First service should be of type ServiceImplementation");
            TestRunner.Assert(services[1] is AnotherServiceImplementation, "Second service should be of type AnotherServiceImplementation");
        }

        // Test interfaces and classes from ContainerTests
        public interface IService { string GetValue(); }
        public class ServiceImplementation : IService { public string GetValue() { return "Service"; } }
        public class AnotherServiceImplementation : IService { public string GetValue() { return "AnotherService"; } }
    }
}
