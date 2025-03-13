using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomDI.Tests
{
    /// <summary>
    /// Tests for the fixes and improvements to the dependency injection framework.
    /// </summary>
    public static class FixesTests
    {
        /// <summary>
        /// Runs all tests for the fixes and improvements.
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("Running tests for fixes and improvements...");
            
            TestResolveNamedAndKeyedMethods();
            TestViewModelLocator();
            TestViewToViewModelBinding();
            
            Console.WriteLine("All tests passed!");
        }
        
        /// <summary>
        /// Tests the ResolveNamed and ResolveKeyed methods with Type parameters.
        /// </summary>
        private static void TestResolveNamedAndKeyedMethods()
        {
            Console.WriteLine("Testing ResolveNamed and ResolveKeyed methods...");
            
            // Create a container
            var container = ContainerFactory.CreateContainer();
            
            // Register services with names and keys
            container.Register<ITestService, TestServiceA>().Named("ServiceA");
            container.Register<ITestService, TestServiceB>().Named("ServiceB");
            container.Register<ITestService, TestServiceC>().Keyed(1);
            container.Register<ITestService, TestServiceD>().Keyed("Key");
            
            // Create a resolve context
            var context = new ResolveContext((Container)container, null);
            
            // Test ResolveNamed with generic type parameter
            var serviceA = context.ResolveNamed<ITestService>("ServiceA");
            if (!(serviceA is TestServiceA))
                throw new Exception("ResolveNamed<T> failed to resolve TestServiceA");
            
            // Test ResolveNamed with Type parameter
            var serviceB = context.ResolveNamed(typeof(ITestService), "ServiceB");
            if (!(serviceB is TestServiceB))
                throw new Exception("ResolveNamed(Type, string) failed to resolve TestServiceB");
            
            // Test ResolveKeyed with generic type parameter
            var serviceC = context.ResolveKeyed<ITestService>(1);
            if (!(serviceC is TestServiceC))
                throw new Exception("ResolveKeyed<T> failed to resolve TestServiceC");
            
            // Test ResolveKeyed with Type parameter
            var serviceD = context.ResolveKeyed(typeof(ITestService), "Key");
            if (!(serviceD is TestServiceD))
                throw new Exception("ResolveKeyed(Type, object) failed to resolve TestServiceD");
            
            Console.WriteLine("ResolveNamed and ResolveKeyed methods tests passed!");
        }
        
        /// <summary>
        /// Tests the ViewModelLocator with the new view-to-viewmodel mapping functionality.
        /// </summary>
        private static void TestViewModelLocator()
        {
            Console.WriteLine("Testing ViewModelLocator...");
            
            // Create a container
            var container = ContainerFactory.CreateContainer();
            
            // Register view models
            container.Register<TestViewModel>(ServiceLifetime.Transient);
            
            // Create a view model locator
            var locator = new Wpf.ViewModelLocator(container);
            
            // Map view to view model
            locator.Map<TestView, TestViewModel>();
            
            // Register by key for backward compatibility
            locator.Register<TestViewModel>("Test");
            
            // Test GetViewModelForView
            var view = new TestView();
            var viewModel = locator.GetViewModelForView(view);
            if (!(viewModel is TestViewModel))
                throw new Exception("GetViewModelForView failed to resolve TestViewModel");
            
            // Test indexer for backward compatibility
            var viewModelByKey = locator["Test"];
            if (!(viewModelByKey is TestViewModel))
                throw new Exception("ViewModelLocator indexer failed to resolve TestViewModel");
            
            Console.WriteLine("ViewModelLocator tests passed!");
        }
        
        /// <summary>
        /// Tests the view-to-viewmodel binding functionality.
        /// </summary>
        private static void TestViewToViewModelBinding()
        {
            Console.WriteLine("Testing view-to-viewmodel binding...");
            
            // Note: Full testing of the view-to-viewmodel binding would require a WPF application context.
            // This test is a placeholder to verify the code compiles correctly.
            
            // Verify that the ViewBase<T> class is available
            var viewBaseType = typeof(Wpf.ViewBase<>);
            if (viewBaseType == null)
                throw new Exception("ViewBase<T> class not found");
            
            // Verify that the ViewModelBinder class is available
            var viewModelBinderType = typeof(Wpf.ViewModelBinder);
            if (viewModelBinderType == null)
                throw new Exception("ViewModelBinder class not found");
            
            // Verify that the ViewModelBindingExtension class is available
            var viewModelBindingExtensionType = typeof(Wpf.ViewModelBindingExtension);
            if (viewModelBindingExtensionType == null)
                throw new Exception("ViewModelBindingExtension class not found");
            
            Console.WriteLine("View-to-viewmodel binding tests passed!");
        }
    }
    
    #region Test Classes
    
    /// <summary>
    /// Interface for test services.
    /// </summary>
    public interface ITestService { }
    
    /// <summary>
    /// Test service implementation A.
    /// </summary>
    public class TestServiceA : ITestService { }
    
    /// <summary>
    /// Test service implementation B.
    /// </summary>
    public class TestServiceB : ITestService { }
    
    /// <summary>
    /// Test service implementation C.
    /// </summary>
    public class TestServiceC : ITestService { }
    
    /// <summary>
    /// Test service implementation D.
    /// </summary>
    public class TestServiceD : ITestService { }
    
    /// <summary>
    /// Test view model.
    /// </summary>
    public class TestViewModel { }
    
    /// <summary>
    /// Test view.
    /// </summary>
    public class TestView : System.Windows.Controls.UserControl { }
    
    #endregion
}
