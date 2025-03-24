using System;
using System.IO;
using System.Reflection;

namespace CustomDI.Tests.Verification
{
    /// <summary>
    /// Simple program to verify the fixes made to the CustomDI library.
    /// </summary>
    class VerificationProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CustomDI Verification Program");
            Console.WriteLine("============================");
            
            // Test transient lifecycle fix
            TestTransientLifecycle();
            
            // Test conditional services fix
            TestConditionalServices();
            
            // Test disposable services
            TestDisposableServices();
            
            // Test lazy resolution
            TestLazyResolution();
            
            // Test decorator pattern
            TestDecoratorPattern();
            
            Console.WriteLine("\nAll verification tests passed!");
        }
        
        static void TestTransientLifecycle()
        {
            Console.WriteLine("\nTesting Transient Lifecycle Fix:");
            
            // Create a container
            var container = ContainerFactory.CreateContainer();
            
            // Register a transient service
            container.Register<ITestService, TestService>(ServiceLifetime.Transient);
            
            // Resolve the service multiple times
            var service1 = container.Resolve<ITestService>();
            var service2 = container.Resolve<ITestService>();
            
            // Verify that different instances are returned
            Console.WriteLine($"  Service1 ID: {service1.GetId()}");
            Console.WriteLine($"  Service2 ID: {service2.GetId()}");
            
            if (service1.GetId() == service2.GetId())
            {
                throw new Exception("Transient lifecycle fix verification failed: Same instance returned for transient service");
            }
            
            Console.WriteLine("  Transient lifecycle fix verified successfully");
        }
        
        static void TestConditionalServices()
        {
            Console.WriteLine("\nTesting Conditional Services Fix:");
            
            // Create a container
            var container = ContainerFactory.CreateContainer();
            
            // Register services with conditions
            container.Register<ITestService, TestService>()
                .When(context => false); // This should never be resolved
                
            container.Register<ITestService, AlternateTestService>(); // This should be resolved as fallback
            
            // Resolve the service
            var service = container.Resolve<ITestService>();
            
            // Verify that the fallback service is returned
            Console.WriteLine($"  Resolved service type: {service.GetType().Name}");
            
            if (service.GetType() != typeof(AlternateTestService))
            {
                throw new Exception("Conditional services fix verification failed: Wrong service type resolved");
            }
            
            Console.WriteLine("  Conditional services fix verified successfully");
        }
        
        static void TestDisposableServices()
        {
            Console.WriteLine("\nTesting Disposable Services Feature:");
            
            // Create a container
            var container = ContainerFactory.CreateContainer();
            
            // Register a disposable service
            container.RegisterInstance<IDisposableService>(new DisposableService());
            
            // Resolve and use the service
            var service = container.Resolve<IDisposableService>();
            service.DoWork();
            
            // Dispose the container
            container.Dispose();
            
            // Verify that the service was disposed
            if (!DisposableService.WasDisposed)
            {
                throw new Exception("Disposable services feature verification failed: Service was not disposed");
            }
            
            Console.WriteLine("  Disposable services feature verified successfully");
        }
        
        static void TestLazyResolution()
        {
            Console.WriteLine("\nTesting Lazy Resolution Feature:");
            
            // Create a container
            var container = ContainerFactory.CreateContainer();
            
            // Register a service
            container.Register<ITestService, TestService>();
            
            // Register a lazy factory for the service
            container.RegisterLazy<ITestService>();
            
            // Resolve the lazy factory
            var lazyService = container.Resolve<Lazy<ITestService>>();
            
            Console.WriteLine("  Lazy factory resolved, service not yet created");
            
            // Access the service
            var service = lazyService.Value;
            
            Console.WriteLine($"  Service created with ID: {service.GetId()}");
            
            if (service == null)
            {
                throw new Exception("Lazy resolution feature verification failed: Service was not created");
            }
            
            Console.WriteLine("  Lazy resolution feature verified successfully");
        }
        
        static void TestDecoratorPattern()
        {
            Console.WriteLine("\nTesting Decorator Pattern Feature:");
            
            // Create a container
            var container = ContainerFactory.CreateContainer();
            
            // Register the base service
            container.Register<ITestService, TestService>();
            
            // Register a decorator for the service
            container.RegisterDecorator<ITestService, TestServiceDecorator>();
            
            // Resolve the service
            var service = container.Resolve<ITestService>();
            
            // Verify that the service is decorated
            Console.WriteLine($"  Resolved service type: {service.GetType().Name}");
            
            if (service.GetType() != typeof(TestServiceDecorator))
            {
                throw new Exception("Decorator pattern feature verification failed: Service was not decorated");
            }
            
            Console.WriteLine("  Decorator pattern feature verified successfully");
        }
    }
    
    // Test interfaces and classes
    public interface ITestService
    {
        string GetId();
    }
    
    public class TestService : ITestService
    {
        private readonly string _id = Guid.NewGuid().ToString();
        
        public string GetId() => _id;
    }
    
    public class AlternateTestService : ITestService
    {
        private readonly string _id = Guid.NewGuid().ToString();
        
        public string GetId() => _id;
    }
    
    public class TestServiceDecorator : ITestService
    {
        private readonly ITestService _inner;
        
        public TestServiceDecorator(ITestService inner)
        {
            _inner = inner;
        }
        
        public string GetId() => $"Decorated({_inner.GetId()})";
    }
    
    public interface IDisposableService
    {
        void DoWork();
    }
    
    public class DisposableService : IDisposableService, IDisposable
    {
        public static bool WasDisposed { get; private set; }
        
        public DisposableService()
        {
            WasDisposed = false;
        }
        
        public void DoWork()
        {
            Console.WriteLine("  DisposableService doing work");
        }
        
        public void Dispose()
        {
            WasDisposed = true;
            Console.WriteLine("  DisposableService was disposed");
        }
    }
}
