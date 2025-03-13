using System;
using System.Windows;
using CustomDI;
using CustomDI.Wpf;

namespace CustomDI.Examples
{
    /// <summary>
    /// Example of using the dependency injection framework in a WPF application.
    /// </summary>
    public class WpfExample
    {
        /// <summary>
        /// Example of configuring the container for a WPF application.
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

            // Configure the view model locator
            var locator = new ViewModelLocator(container);
            locator.Register<MainViewModel>("Main");
            locator.Register<HomeViewModel>("Home");
            locator.Register<SettingsViewModel>("Settings");
            locator.Register<UserProfileViewModel>("UserProfile");

            // Register design-time view models if needed
            if (ViewModelLocator.IsInDesignMode)
            {
                locator.RegisterDesignTimeViewModel("Main", new MainViewModel(
                    new DesignTimeNavigationService(),
                    new DesignTimeUserService()));
            }

            // Add the locator to the application resources
            Application.Current.Resources["ViewModelLocator"] = locator;
        }
    }

    #region Services

    /// <summary>
    /// Interface for data service.
    /// </summary>
    public interface IDataService
    {
        string GetData();
    }

    /// <summary>
    /// Implementation of data service.
    /// </summary>
    public class DataService : IDataService
    {
        public string GetData()
        {
            return "Real data from service";
        }
    }

    /// <summary>
    /// Interface for user service.
    /// </summary>
    public interface IUserService
    {
        string GetCurrentUser();
    }

    /// <summary>
    /// Implementation of user service.
    /// </summary>
    public class UserService : IUserService
    {
        public string GetCurrentUser()
        {
            return "John Doe";
        }
    }

    /// <summary>
    /// Design-time implementation of user service.
    /// </summary>
    public class DesignTimeUserService : IUserService
    {
        public string GetCurrentUser()
        {
            return "Design-Time User";
        }
    }

    /// <summary>
    /// Interface for navigation service.
    /// </summary>
    public interface INavigationService
    {
        void NavigateTo(string viewName);
    }

    /// <summary>
    /// Implementation of navigation service.
    /// </summary>
    public class NavigationService : INavigationService
    {
        public void NavigateTo(string viewName)
        {
            // Actual navigation logic would go here
            Console.WriteLine($"Navigating to {viewName}");
        }
    }

    /// <summary>
    /// Design-time implementation of navigation service.
    /// </summary>
    public class DesignTimeNavigationService : INavigationService
    {
        public void NavigateTo(string viewName)
        {
            // No-op for design time
        }
    }

    #endregion

    #region ViewModels

    /// <summary>
    /// Main view model.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        private string _currentUser;
        public string CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value, nameof(CurrentUser));
        }

        public MainViewModel(INavigationService navigationService, IUserService userService)
        {
            _navigationService = navigationService;
            _userService = userService;
            CurrentUser = _userService.GetCurrentUser();
        }

        public void NavigateToHome()
        {
            _navigationService.NavigateTo("Home");
        }

        public void NavigateToSettings()
        {
            _navigationService.NavigateTo("Settings");
        }

        public void NavigateToUserProfile()
        {
            _navigationService.NavigateTo("UserProfile");
        }
    }

    /// <summary>
    /// Home view model.
    /// </summary>
    public class HomeViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        private string _data;
        public string Data
        {
            get => _data;
            set => SetProperty(ref _data, value, nameof(Data));
        }

        public HomeViewModel(IDataService dataService)
        {
            _dataService = dataService;
            LoadData();
        }

        private void LoadData()
        {
            Data = _dataService.GetData();
        }
    }

    /// <summary>
    /// Settings view model.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set => SetProperty(ref _darkModeEnabled, value, nameof(DarkModeEnabled));
        }

        private bool _notificationsEnabled;
        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set => SetProperty(ref _notificationsEnabled, value, nameof(NotificationsEnabled));
        }

        public SettingsViewModel()
        {
            // Default settings
            DarkModeEnabled = false;
            NotificationsEnabled = true;
        }
    }

    /// <summary>
    /// User profile view model.
    /// </summary>
    public class UserProfileViewModel : ViewModelBase
    {
        private readonly IUserService _userService;

        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value, nameof(UserName));
        }

        public UserProfileViewModel(IUserService userService)
        {
            _userService = userService;
            UserName = _userService.GetCurrentUser();
        }
    }

    #endregion
}
