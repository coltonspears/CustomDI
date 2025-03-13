# CustomDI Framework

A robust dependency injection framework for .NET Framework 4.8 WPF applications that doesn't rely on external NuGet packages.

## Features

- **Service Registration**: Register services by interface, concrete type, instance, or factory
- **Service Resolution**: Resolve services with constructor injection, property injection, and more
- **Lifecycle Management**: Support for singleton, transient, and scoped lifetimes
- **WPF Integration**: View model locator with automatic view-to-viewmodel binding, XAML markup extensions, and design-time support
- **Factory Support**: Various factory patterns for creating service instances
- **Configuration Helpers**: Fluent API, convention-based registration, and assembly scanning

## Installation

Since this framework doesn't rely on NuGet packages, you can add it to your project in one of the following ways:

### Option 1: Add the project to your solution

1. Clone or download this repository
2. Add the `CustomDI.csproj` project to your solution
3. Add a reference to the CustomDI project from your WPF application project

### Option 2: Build and reference the DLL

1. Clone or download this repository
2. Open the solution in Visual Studio
3. Build the solution
4. Add a reference to the built `CustomDI.dll` from your WPF application project

## Quick Start

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

### WPF Integration with View-to-ViewModel Binding

CustomDI provides several ways to bind view models to views:

#### Option 1: Using the ViewModelLocator with View-to-ViewModel Mapping

```csharp
// Configure the view model locator with view-to-viewmodel mappings
container.RegisterViewModelLocator(locator => {
    // Map views to view models
    locator.Map<MainView, MainViewModel>();
    locator.Map<HomeView, HomeViewModel>();
    locator.Map<SettingsView, SettingsViewModel>();
});

// Add the locator to application resources
Application.Current.Resources["ViewModelLocator"] = container.Resolve<ViewModelLocator>();

// Get a view model for a view
var view = new MainView();
var viewModel = locator.GetViewModelForView(view);
```

#### Option 2: Using the ViewModelBinding Markup Extension

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

#### Option 3: Using the AutoBind Attached Property

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

#### Option 4: Using the ViewBase<T> Base Class

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

## Documentation

For detailed documentation, see the [documentation.md](docs/documentation.md) file.

## Examples

Check out the examples in the `examples` folder to see how to use the framework in a WPF application.

## Running the Tests

The solution includes a simple test framework and comprehensive tests for the dependency injection container. To run the tests:

1. Open the solution in Visual Studio
2. Set the `CustomDI.Tests` project as the startup project
3. Run the project

Alternatively, you can create a simple console application that calls:

```csharp
CustomDI.Tests.ContainerTests.RunAllTests();
CustomDI.Tests.FixesTests.RunAllTests();
```

## Project Structure

- **src**: Contains the source code for the dependency injection framework
- **tests**: Contains unit tests for the framework
- **examples**: Contains example usage of the framework
- **docs**: Contains documentation

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

This framework was created to provide a robust dependency injection solution for .NET Framework 4.8 WPF applications without relying on external NuGet packages.
