# CustomDI Framework Documentation

## Overview

CustomDI is a lightweight, feature-rich dependency injection framework for .NET Framework 4.8 WPF applications. It provides a robust solution for managing dependencies without relying on external NuGet packages, making it suitable for environments with restricted package management.

## Features

- **Service Registration**: Register services by interface, concrete type, instance, or factory
- **Service Resolution**: Resolve services with constructor injection, property injection, and more
- **Lifecycle Management**: Support for singleton, transient, and scoped lifetimes
- **WPF Integration**: View model locator with automatic view-to-viewmodel binding, XAML markup extensions, and design-time support
- **Factory Support**: Various factory patterns for creating service instances
- **Configuration Helpers**: Fluent API, convention-based registration, and assembly scanning

## Getting Started

### Basic Container Usage

```csharp
// Create a container
var container = ContainerFactory.CreateContainer();

// Register services
container.Register<IService, ServiceImplementation>();
container.RegisterInstance<IConfig>(new Config());
container.RegisterFactory<ILogger>(context => new Logger("app.log"));

// Resolve services
var service = container.Resolve<IService>();
```

### Using the Fluent API

```csharp
// Create a container builder
var builder = new ContainerBuilder();

// Register services
builder.Register<IService, ServiceImplementation>(ServiceLifetime.Singleton);
builder.RegisterInstance<IConfig>(new Config());
builder.RegisterFactory<ILogger>(context => new Logger("app.log"));

// Build the container
var container = builder.Build();
```

### Lifecycle Management

```csharp
// Singleton - one instance for the entire application
container.RegisterSingleton<IService, ServiceImplementation>();

// Transient - new instance each time
container.RegisterTransient<IService, ServiceImplementation>();

// Scoped - one instance per scope
container.RegisterScoped<IService, ServiceImplementation>();

// Using scopes
using (var scope = container.CreateScope())
{
    var service = scope.Resolve<IService>();
    // Service instance is scoped to this scope
}
```

## Dependency Resolution

### Constructor Injection

```csharp
public class ServiceA : IServiceA
{
    private readonly IServiceB _serviceB;
    private readonly IServiceC _serviceC;

    // Dependencies are automatically resolved and injected
    public ServiceA(IServiceB serviceB, IServiceC serviceC)
    {
        _serviceB = serviceB;
        _serviceC = serviceC;
    }
}
```

### Property Injection

```csharp
public class ServiceA : IServiceA
{
    // Use [Inject] attribute for property injection
    [Inject]
    public IServiceB ServiceB { get; set; }

    // Optional dependency
    [Inject(Required = false)]
    public IServiceC ServiceC { get; set; }
}
```

### Explicit Constructor Selection

```csharp
public class ServiceWithMultipleConstructors
{
    // This constructor will be used for dependency injection
    [Inject]
    public ServiceWithMultipleConstructors(IServiceA serviceA)
    {
        // ...
    }

    public ServiceWithMultipleConstructors()
    {
        // ...
    }
}
```

### Optional Dependencies

```csharp
public class ServiceA : IServiceA
{
    private readonly IServiceB _serviceB;

    // Optional dependency with default value
    public ServiceA(IServiceB serviceB = null)
    {
        _serviceB = serviceB ?? new DefaultServiceB();
    }
}
```

## Advanced Registration

### Named and Keyed Registration

```csharp
// Named registration
container.Register<IService, ServiceImplementation>().Named("main");
container.Register<IService, AnotherServiceImplementation>().Named("backup");

// Keyed registration
container.Register<IService, ServiceImplementation>().Keyed(1);
container.Register<IService, AnotherServiceImplementation>().Keyed("backup");

// Resolving named and keyed services
var context = new ResolveContext((Container)container, null);
var mainService = context.ResolveNamed<IService>("main");
var backupService = context.ResolveKeyed<IService>("backup");

// Resolving with Type parameter
var mainServiceByType = context.ResolveNamed(typeof(IService), "main");
var backupServiceByType = context.ResolveKeyed(typeof(IService), "backup");
```

### Conditional Registration

```csharp
// Register a service with a condition
container.Register<IService, ServiceImplementation>()
    .When(context => Environment.UserInteractive);
```

### Registration with Parameters

```csharp
// Register with constructor parameters
container.Register<IService, ServiceImplementation>()
    .WithParameters(
        Parameter.FromNamedValue("connectionString", "Data Source=..."),
        Parameter.FromNamedValue("timeout", 30)
    );

// Register with property values
container.Register<IService, ServiceImplementation>()
    .WithProperties(
        PropertyValue.FromNamedValue("ConnectionString", "Data Source=..."),
        PropertyValue.FromNamedValue("Timeout", 30)
    );
```

### Post-Activation Configuration

```csharp
// Configure a service after it's created
container.Register<IService, ServiceImplementation>()
    .OnActivated(args => {
        var service = (ServiceImplementation)args.Instance;
        service.Initialize();
    });
```

## Factory Support

### Basic Factory

```csharp
// Register a factory for creating services
container.RegisterFactory<IService>(context => new ServiceImplementation());

// Resolve the service
var service = container.Resolve<IService>();
```

### Parameterized Factory

```csharp
// Register a parameterized factory
container.RegisterParameterizedFactory<IService, string>((name) => 
    new ServiceImplementation(name));

// Resolve the factory
var factory = container.Resolve<Func<string, IService>>();

// Create services with parameters
var service1 = factory("Service1");
var service2 = factory("Service2");
```

### Factory for Collections

```csharp
// Register multiple implementations
container.Register<IService, ServiceImplementation>();
container.Register<IService, AnotherServiceImplementation>();

// Register a factory for all implementations
container.RegisterFactoryForAll<IService>();

// Resolve the factory
var factory = container.Resolve<Func<IEnumerable<IService>>>();

// Get all implementations
var services = factory();
```

### Named and Keyed Factories

```csharp
// Register named implementations
container.Register<IService, ServiceImplementation>().Named("main");
container.Register<IService, AnotherServiceImplementation>().Named("backup");

// Register a named factory
container.RegisterNamedFactory<IService>();

// Resolve the factory
var factory = container.Resolve<Func<string, IService>>();

// Create services by name
var mainService = factory("main");
var backupService = factory("backup");
```

## WPF Integration

### View-to-ViewModel Binding

CustomDI provides several ways to bind view models to views:

#### 1. Using the ViewModelLocator with View-to-ViewModel Mapping

```csharp
// Configure the view model locator with view-to-viewmodel mappings
container.RegisterViewModelLocator(locator => {
    // Map views to view models
    locator.Map<MainView, MainViewModel>();
    locator.Map<HomeView, HomeViewModel>();
    locator.Map<SettingsView, SettingsViewModel>();
    
    // For backward compatibility, also register by key
    locator.Register<MainViewModel>("Main");
});

// Add the locator to application resources
Application.Current.Resources["ViewModelLocator"] = container.Resolve<ViewModelLocator>();

// Get a view model for a view
var view = new MainView();
var viewModel = locator.GetViewModelForView(view);
```

#### 2. Using the ViewModelBinding Markup Extension

```xml
<!-- In XAML -->
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:di="clr-namespace:CustomDI.Wpf;assembly=CustomDI"
        DataContext="{di:ViewModelBinding}">
    <Grid>
        <!-- Content here -->
    </Grid>
</Window>
```

#### 3. Using the AutoBind Attached Property

```xml
<!-- In XAML -->
<UserControl x:Class="MyApp.Views.HomeView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:di="clr-namespace:CustomDI.Wpf;assembly=CustomDI"
             di:ViewModelBinder.AutoBind="True">
    <Grid>
        <!-- Content here -->
    </Grid>
</UserControl>
```

#### 4. Using the ViewBase<T> Base Class

```csharp
// In code-behind
public class MainView : ViewBase<MainViewModel>
{
    protected override void OnViewModelLoaded()
    {
        // The ViewModel property is now available
        Console.WriteLine($"Main view loaded with user: {ViewModel.CurrentUser}");
    }
}
```

```xml
<!-- In XAML -->
<local:MainView x:Class="MyApp.Views.MainView"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:local="clr-namespace:MyApp.Views">
    <Grid>
        <!-- Content here -->
    </Grid>
</local:MainView>
```

### Using the ViewModelBase Class

```csharp
public class MainViewModel : ViewModelBase
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value, nameof(Name));
    }
}
```

### Design-Time Support

```csharp
// Register design-time view models
if (ViewModelLocator.IsInDesignMode)
{
    locator.RegisterDesignTimeViewModel("MainView", new MainViewModel(
        new DesignTimeDataService()));
}
```

## Assembly Scanning and Convention-Based Registration

```csharp
// Register all types in an assembly that implement IService
builder.RegisterAssemblyTypes<IService>(typeof(Program).Assembly);

// Register all types in an assembly that match a predicate
builder.RegisterAssemblyTypes(
    typeof(Program).Assembly, 
    type => type.Name.EndsWith("Service"));

// Register types by naming convention
builder.RegisterAssemblyTypesByConvention(
    typeof(Program).Assembly, 
    "Implementation");
```

## Error Handling

### Circular Dependency Detection

The framework automatically detects circular dependencies and throws an `InvalidOperationException` with a detailed dependency chain.

### Missing Dependency Handling

When a required dependency cannot be resolved, the framework throws an `InvalidOperationException` with information about the missing dependency.

### Optional Dependencies

```csharp
// Mark a property as optional
[Inject(Required = false)]
public ILogger Logger { get; set; }

// Use optional constructor parameter
public Service(ILogger logger = null)
{
    _logger = logger;
}
```

## Best Practices

1. **Register by Interface**: Prefer registering services by interface rather than concrete type to promote loose coupling.

2. **Use Appropriate Lifetimes**: Choose the appropriate lifetime for each service:
   - Singleton: For stateless services or services that maintain global state
   - Scoped: For services that maintain state within a logical operation
   - Transient: For stateful services that should not be shared

3. **Dispose Containers and Scopes**: Always dispose containers and scopes when they are no longer needed to release resources.

4. **Prefer Constructor Injection**: Use constructor injection as the primary method of dependency injection, with property injection as a fallback.

5. **Use the ViewModelLocator with View-to-ViewModel Mapping**: In WPF applications, use the ViewModelLocator with view-to-viewmodel mapping for automatic binding.

6. **Avoid Service Locator Pattern**: Avoid using the container as a service locator; instead, inject dependencies directly.

7. **Keep Registration Centralized**: Maintain service registration in a centralized location for better maintainability.

8. **Test with Mock Dependencies**: Use the container to inject mock dependencies during testing.

## Conclusion

CustomDI provides a comprehensive dependency injection solution for .NET Framework 4.8 WPF applications without external dependencies. Its rich feature set and flexible API make it suitable for a wide range of application scenarios.
