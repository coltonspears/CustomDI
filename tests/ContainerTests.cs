using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomDI.Tests
{
    /// <summary>
    /// Test cases for the dependency injection container.
    /// </summary>
    public static class ContainerTests
    {
        /// <summary>
        /// Test interfaces and classes for testing the container.
        /// </summary>
        public interface IService { string GetValue(); }
        public class ServiceImplementation : IService { public string GetValue() { return "Service"; } }
        public class AnotherServiceImplementation : IService { public string GetValue() { return "AnotherService"; } }
        
        public interface IDependentService { string GetValue(); }
        public class DependentServiceImplementation : IDependentService 
        { 
            private readonly IService _service;
            
            public DependentServiceImplementation(IService service)
            {
                _service = service;
            }
            
            public string GetValue() { return "Dependent: " + _service.GetValue(); } 
        }
        
        public class ServiceWithPropertyInjection
        {
            [Inject]
            public IService Service { get; set; }
            
            public string GetValue() { return "Property: " + (Service?.GetValue() ?? "null"); }
        }
        
        public class ServiceWithMultipleConstructors
        {
            private readonly IService _service;
            private readonly IDependentService _dependentService;
            
            public ServiceWithMultipleConstructors()
            {
                _service = null;
                _dependentService = null;
            }
            
            [ConstructorInject]
            public ServiceWithMultipleConstructors(IService service)
            {
                _service = service;
                _dependentService = null;
            }
            
            public ServiceWithMultipleConstructors(IService service, IDependentService dependentService)
            {
                _service = service;
                _dependentService = dependentService;
            }
            
            public string GetValue() 
            { 
                return "Multiple: " + (_service?.GetValue() ?? "null") + ", " + 
                    (_dependentService?.GetValue() ?? "null"); 
            }
        }
        
        public class ServiceWithOptionalDependency
        {
            private readonly IService _service;
            
            public ServiceWithOptionalDependency(IService service = null)
            {
                _service = service;
            }
            
            public string GetValue() { return "Optional: " + (_service?.GetValue() ?? "null"); }
        }
        
        public interface IFactory { IService CreateService(); }
        public class ServiceFactory : IFactory
        {
            public IService CreateService() { return new ServiceImplementation(); }
        }

        /// <summary>
        /// Runs all container tests.
        /// </summary>
        public static void RunAllTests()
        {
            // Registration tests
            TestRunner.AddTest("Register_Interface_To_Implementation", Test_Register_Interface_To_Implementation);
            TestRunner.AddTest("Register_Concrete_Type", Test_Register_Concrete_Type);
            TestRunner.AddTest("Register_Instance", Test_Register_Instance);
            TestRunner.AddTest("Register_Factory", Test_Register_Factory);
            TestRunner.AddTest("Register_Named", Test_Register_Named);
            TestRunner.AddTest("Register_Keyed", Test_Register_Keyed);
            
            // Resolution tests
            TestRunner.AddTest("Resolve_Registered_Type", Test_Resolve_Registered_Type);
            TestRunner.AddTest("Resolve_With_Dependencies", Test_Resolve_With_Dependencies);
            TestRunner.AddTest("Resolve_With_Property_Injection", Test_Resolve_With_Property_Injection);
            TestRunner.AddTest("Resolve_With_Multiple_Constructors", Test_Resolve_With_Multiple_Constructors);
            TestRunner.AddTest("Resolve_With_Optional_Dependency", Test_Resolve_With_Optional_Dependency);
            TestRunner.AddTest("Resolve_All", Test_Resolve_All);
            TestRunner.AddTest("Try_Resolve", Test_Try_Resolve);
            TestRunner.AddTest("Resolve_Named", Test_Resolve_Named);
            TestRunner.AddTest("Resolve_Keyed", Test_Resolve_Keyed);
            
            // Lifecycle tests
            TestRunner.AddTest("Singleton_Lifecycle", Test_Singleton_Lifecycle);
            TestRunner.AddTest("Transient_Lifecycle", Test_Transient_Lifecycle);
            TestRunner.AddTest("Scoped_Lifecycle", Test_Scoped_Lifecycle);
            
            // Factory tests
            TestRunner.AddTest("Factory_Support", Test_Factory_Support);
            TestRunner.AddTest("Parameterized_Factory", Test_Parameterized_Factory);
            TestRunner.AddTest("Factory_For_All", Test_Factory_For_All);
            
            // Error handling tests
            TestRunner.AddTest("Circular_Dependency_Detection", Test_Circular_Dependency_Detection);
            TestRunner.AddTest("Missing_Dependency_Handling", Test_Missing_Dependency_Handling);
            
            // Run all tests
            TestRunner.RunTests();
        }

        #region Registration Tests

        private static void Test_Register_Interface_To_Implementation()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>();
            
            TestRunner.Assert(container.IsRegistered<IService>(), "Service should be registered");
            
            var service = container.Resolve<IService>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.Assert(service is ServiceImplementation, "Service should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Register_Concrete_Type()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<ServiceImplementation, ServiceImplementation>();
            
            TestRunner.Assert(container.IsRegistered<ServiceImplementation>(), "Service should be registered");
            
            var service = container.Resolve<ServiceImplementation>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.AreEqual("Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Register_Instance()
        {
            var container = ContainerFactory.CreateContainer();
            var instance = new ServiceImplementation();
            container.RegisterInstance<IService>(instance);
            
            TestRunner.Assert(container.IsRegistered<IService>(), "Service should be registered");
            
            var service = container.Resolve<IService>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.Assert(ReferenceEquals(instance, service), "Resolved service should be the same instance");
            TestRunner.AreEqual("Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Register_Factory()
        {
            var container = ContainerFactory.CreateContainer();
            container.RegisterFactory<IService>(context => new ServiceImplementation());
            
            TestRunner.Assert(container.IsRegistered<IService>(), "Service should be registered");
            
            var service = container.Resolve<IService>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.Assert(service is ServiceImplementation, "Service should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Register_Named()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>().Named("service1");
            container.Register<IService, AnotherServiceImplementation>().Named("service2");
            
            var context = new ResolveContext((Container)container, null);
            
            var service1 = context.ResolveNamed<IService>("service1");
            TestRunner.Assert(service1 != null, "Service1 should not be null");
            TestRunner.Assert(service1 is ServiceImplementation, "Service1 should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", service1.GetValue(), "Service1 should return correct value");
            
            var service2 = context.ResolveNamed<IService>("service2");
            TestRunner.Assert(service2 != null, "Service2 should not be null");
            TestRunner.Assert(service2 is AnotherServiceImplementation, "Service2 should be of type AnotherServiceImplementation");
            TestRunner.AreEqual("AnotherService", service2.GetValue(), "Service2 should return correct value");
        }

        private static void Test_Register_Keyed()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>().Keyed(1);
            container.Register<IService, AnotherServiceImplementation>().Keyed(2);
            
            var context = new ResolveContext((Container)container, null);
            
            var service1 = context.ResolveKeyed<IService>(1);
            TestRunner.Assert(service1 != null, "Service1 should not be null");
            TestRunner.Assert(service1 is ServiceImplementation, "Service1 should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", service1.GetValue(), "Service1 should return correct value");
            
            var service2 = context.ResolveKeyed<IService>(2);
            TestRunner.Assert(service2 != null, "Service2 should not be null");
            TestRunner.Assert(service2 is AnotherServiceImplementation, "Service2 should be of type AnotherServiceImplementation");
            TestRunner.AreEqual("AnotherService", service2.GetValue(), "Service2 should return correct value");
        }

        #endregion

        #region Resolution Tests

        private static void Test_Resolve_Registered_Type()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>();
            
            var service = container.Resolve<IService>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.Assert(service is ServiceImplementation, "Service should be of type ServiceImplementation");
            TestRunner.AreEqual("Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Resolve_With_Dependencies()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>();
            container.Register<IDependentService, DependentServiceImplementation>();
            
            var service = container.Resolve<IDependentService>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.Assert(service is DependentServiceImplementation, "Service should be of type DependentServiceImplementation");
            TestRunner.AreEqual("Dependent: Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Resolve_With_Property_Injection()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>();
            container.Register<ServiceWithPropertyInjection, ServiceWithPropertyInjection>();
            
            var service = container.Resolve<ServiceWithPropertyInjection>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.Assert(service.Service != null, "Injected service should not be null");
            TestRunner.AreEqual("Property: Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Resolve_With_Multiple_Constructors()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>();
            container.Register<ServiceWithMultipleConstructors, ServiceWithMultipleConstructors>();
            
            var service = container.Resolve<ServiceWithMultipleConstructors>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.AreEqual("Multiple: Service, null", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Resolve_With_Optional_Dependency()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<ServiceWithOptionalDependency, ServiceWithOptionalDependency>();
            
            var service = container.Resolve<ServiceWithOptionalDependency>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.AreEqual("Optional: null", service.GetValue(), "Service should return correct value");
            
            container.Register<IService, ServiceImplementation>();
            service = container.Resolve<ServiceWithOptionalDependency>();
            TestRunner.AreEqual("Optional: Service", service.GetValue(), "Service should return correct value with dependency");
        }

        private static void Test_Resolve_All()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>();
            container.Register<IService, AnotherServiceImplementation>();
            
            var services = container.ResolveAll<IService>().ToList();
            TestRunner.Assert(services.Count == 2, "Should resolve 2 services");
            TestRunner.Assert(services[0] is ServiceImplementation, "First service should be of type ServiceImplementation");
            TestRunner.Assert(services[1] is AnotherServiceImplementation, "Second service should be of type AnotherServiceImplementation");
        }

        private static void Test_Try_Resolve()
        {
            var container = ContainerFactory.CreateContainer();
            
            IService service;
            bool result = container.TryResolve(out service);
            TestRunner.Assert(!result, "TryResolve should return false for unregistered service");
            TestRunner.Assert(service == null, "Service should be null");
            
            container.Register<IService, ServiceImplementation>();
            result = container.TryResolve(out service);
            TestRunner.Assert(result, "TryResolve should return true for registered service");
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.AreEqual("Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Resolve_Named()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>().Named("service1");
            container.Register<IService, AnotherServiceImplementation>().Named("service2");
            
            var context = new ResolveContext((Container)container, null);
            
            var service1 = context.ResolveNamed<IService>("service1");
            TestRunner.AreEqual("Service", service1.GetValue(), "Service1 should return correct value");
            
            var service2 = context.ResolveNamed<IService>("service2");
            TestRunner.AreEqual("AnotherService", service2.GetValue(), "Service2 should return correct value");
        }

        private static void Test_Resolve_Keyed()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>().Keyed(1);
            container.Register<IService, AnotherServiceImplementation>().Keyed(2);
            
            var context = new ResolveContext((Container)container, null);
            
            var service1 = context.ResolveKeyed<IService>(1);
            TestRunner.AreEqual("Service", service1.GetValue(), "Service1 should return correct value");
            
            var service2 = context.ResolveKeyed<IService>(2);
            TestRunner.AreEqual("AnotherService", service2.GetValue(), "Service2 should return correct value");
        }

        #endregion

        #region Lifecycle Tests

        private static void Test_Singleton_Lifecycle()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>(ServiceLifetime.Singleton);
            
            var service1 = container.Resolve<IService>();
            var service2 = container.Resolve<IService>();
            
            TestRunner.Assert(ReferenceEquals(service1, service2), "Singleton services should be the same instance");
        }

        private static void Test_Transient_Lifecycle()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>(ServiceLifetime.Transient);
            
            var service1 = container.Resolve<IService>();
            var service2 = container.Resolve<IService>();
            
            TestRunner.Assert(!ReferenceEquals(service1, service2), "Transient services should be different instances");
        }

        private static void Test_Scoped_Lifecycle()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>(ServiceLifetime.Scoped);
            
            using (var scope1 = container.CreateScope())
            {
                var service1 = scope1.Resolve<IService>();
                var service2 = scope1.Resolve<IService>();
                
                TestRunner.Assert(ReferenceEquals(service1, service2), "Scoped services within same scope should be the same instance");
                
                using (var scope2 = container.CreateScope())
                {
                    var service3 = scope2.Resolve<IService>();
                    
                    TestRunner.Assert(!ReferenceEquals(service1, service3), "Scoped services from different scopes should be different instances");
                }
            }
        }

        #endregion

        #region Factory Tests

        private static void Test_Factory_Support()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IFactory, ServiceFactory>();
            container.RegisterFactory<IService>(context => context.Resolve<IFactory>().CreateService());
            
            var service = container.Resolve<IService>();
            TestRunner.Assert(service != null, "Service should not be null");
            TestRunner.AreEqual("Service", service.GetValue(), "Service should return correct value");
        }

        private static void Test_Parameterized_Factory()
        {
            var container = ContainerFactory.CreateContainer();
            
            container.RegisterFactory<Func<string, IService>>(context => 
                name => name == "Service1" ? new ServiceImplementation() : new AnotherServiceImplementation());
            
            var factory = container.Resolve<Func<string, IService>>();
            TestRunner.Assert(factory != null, "Factory should not be null");
            
            var service1 = factory("Service1");
            TestRunner.AreEqual("Service", service1.GetValue(), "Service1 should return correct value");
            
            var service2 = factory("Service2");
            TestRunner.AreEqual("AnotherService", service2.GetValue(), "Service2 should return correct value");
        }

        private static void Test_Factory_For_All()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<IService, ServiceImplementation>();
            container.Register<IService, AnotherServiceImplementation>();
            
            container.RegisterFactory<Func<IEnumerable<IService>>>(context => () => context.ResolveAll<IService>());
            
            var factory = container.Resolve<Func<IEnumerable<IService>>>();
            TestRunner.Assert(factory != null, "Factory should not be null");
            
            var services = factory().ToList();
            TestRunner.Assert(services.Count == 2, "Should resolve 2 services");
            TestRunner.Assert(services[0] is ServiceImplementation, "First service should be of type ServiceImplementation");
            TestRunner.Assert(services[1] is AnotherServiceImplementation, "Second service should be of type AnotherServiceImplementation");
        }

        #endregion

        #region Error Handling Tests

        private class CircularDependency1 { public CircularDependency1(CircularDependency2 dep) { } }
        private class CircularDependency2 { public CircularDependency2(CircularDependency1 dep) { } }

        private static void Test_Circular_Dependency_Detection()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<CircularDependency1, CircularDependency1>();
            container.Register<CircularDependency2, CircularDependency2>();
            
            TestRunner.ThrowsException<InvalidOperationException>(() => container.Resolve<CircularDependency1>());
        }

        private class ServiceWithRequiredDependency { public ServiceWithRequiredDependency(IService service) { } }

        private static void Test_Missing_Dependency_Handling()
        {
            var container = ContainerFactory.CreateContainer();
            container.Register<ServiceWithRequiredDependency, ServiceWithRequiredDependency>();
            
            TestRunner.ThrowsException<InvalidOperationException>(() => container.Resolve<ServiceWithRequiredDependency>());
        }

        #endregion
    }
}
