using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CustomDI.Wpf
{
    /// <summary>
    /// Provides an example of how to use the view model binding in a WPF application.
    /// </summary>
    public class ViewModelBindingExample
    {
        /// <summary>
        /// Example of configuring the container with view model binding.
        /// </summary>
        public static void ConfigureContainer()
        {
            // Create a container builder
            var builder = new ContainerBuilder();

            // Register services
            builder.Register<IDataService, DataService>(ServiceLifetime.Singleton);
            builder.Register<IUserService, UserService>(ServiceLifetime.Singleton);
            builder.Register<INavigationService, NavigationService>(ServiceLifetime.Singleton);

            // Register view models
            builder.Register<MainViewModel>(ServiceLifetime.Transient);
            builder.Register<HomeViewModel>(ServiceLifetime.Transient);
            builder.Register<SettingsViewModel>(ServiceLifetime.Transient);
            builder.Register<UserProfileViewModel>(ServiceLifetime.Transient);

            // Build the container
            var container = builder.Build();

            // Configure the view model locator with view-to-viewmodel mappings
            container.RegisterViewModelLocator(locator => {
                // Map views to view models
                locator.Map<MainView, MainViewModel>();
                locator.Map<HomeView, HomeViewModel>();
                locator.Map<SettingsView, SettingsViewModel>();
                locator.Map<UserProfileView, UserProfileViewModel>();
                
                // For backward compatibility, also register by key
                locator.Register<MainViewModel>("Main");
                locator.Register<HomeViewModel>("Home");
                locator.Register<SettingsViewModel>("Settings");
                locator.Register<UserProfileViewModel>("UserProfile");

                // Register design-time view models if needed
                if (ViewModelLocator.IsInDesignMode)
                {
                    locator.RegisterDesignTimeViewModel("MainView", new MainViewModel(
                        new DesignTimeNavigationService(),
                        new DesignTimeUserService()));
                }
            });

            // Add the locator to the application resources
            Application.Current.Resources["ViewModelLocator"] = container.Resolve<ViewModelLocator>();
        }
    }

    #region Example Views

    /// <summary>
    /// Example main view.
    /// </summary>
    public class MainView : ViewBase<MainViewModel>
    {
        protected override void OnViewModelLoaded()
        {
            // The ViewModel property is now available
            Console.WriteLine($"Main view loaded with user: {ViewModel.CurrentUser}");
        }
    }

    /// <summary>
    /// Example home view.
    /// </summary>
    public class HomeView : UserControl
    {
        public HomeView()
        {
            // Using the attached property for automatic binding
            ViewModelBinder.SetAutoBind(this, true);
        }
    }

    /// <summary>
    /// Example settings view.
    /// </summary>
    public class SettingsView : UserControl
    {
        // This view will use the markup extension in XAML:
        // <UserControl DataContext="{di:ViewModelBinding}" />
    }

    /// <summary>
    /// Example user profile view.
    /// </summary>
    public class UserProfileView : UserControl
    {
        // This view will use the markup extension in XAML:
        // <UserControl DataContext="{di:ViewModelBinding}" />
    }

    #endregion

    #region Example XAML Usage

    /*
    <!-- Example of using the ViewModelBinding markup extension -->
    <Window x:Class="MyApp.MainWindow"
            xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:di="clr-namespace:CustomDI.Wpf;assembly=CustomDI"
            DataContext="{di:ViewModelBinding}">
        <Grid>
            <!-- Content here -->
        </Grid>
    </Window>
    
    <!-- Example of using the AutoBind attached property -->
    <UserControl x:Class="MyApp.Views.HomeView"
                 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:di="clr-namespace:CustomDI.Wpf;assembly=CustomDI"
                 di:ViewModelBinder.AutoBind="True">
        <Grid>
            <!-- Content here -->
        </Grid>
    </UserControl>
    
    <!-- Example of using the ViewBase<T> base class -->
    <local:MainView x:Class="MyApp.Views.MainView"
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:MyApp.Views">
        <Grid>
            <!-- Content here -->
        </Grid>
    </local:MainView>
    */

    #endregion
}
