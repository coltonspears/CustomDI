using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomDI.Controls
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HomeView _homeView;
        private ControlsView _controlsView;

        public MainWindow()
        {
            InitializeComponent();

            // Create a container builder
            var builder = new ContainerBuilder();

            // Register services
            builder.Register<IDataService, DataService>(ServiceLifetime.Singleton);
            builder.Register<IUserService, UserService>(ServiceLifetime.Singleton);
            builder.Register<INavigationService, NavigationService>(ServiceLifetime.Singleton);
            builder.RegisterFactory<ILogger>(context => new FileLogger("app.log"));

            // Register view models
            builder.Register<MainViewModel>(ServiceLifetime.Transient);
            builder.Register<HomeViewModel>(ServiceLifetime.Transient);
            builder.Register<ControlsViewModel>(ServiceLifetime.Transient);
            //builder.Register<SettingsViewModel>(ServiceLifetime.Transient);
            //builder.Register<UserProfileViewModel>(ServiceLifetime.Transient);

            // Build the container
            var container = builder.Build();

            // Configure the view model locator
            var locator = new ViewModelLocator(container);
            locator.Register<MainViewModel>("Main");
            locator.Register<HomeViewModel>("Home");
            locator.Register<ControlsViewModel>("Controls");
            //locator.Register<SettingsViewModel>("Settings");
            //locator.Register<UserProfileViewModel>("UserProfile");

            // Register design-time view models if needed
            if (ViewModelLocator.IsInDesignMode)
            {
                locator.RegisterDesignTimeViewModel("Main", new MainViewModel(
                    new DesignTimeNavigationService(),
                    new DesignTimeUserService()));
            }

            container.RegisterViewModelLocator(x => {
                // Map views to view models
                x.Map<MainWindow, MainViewModel>();
                x.Map<HomeView, HomeViewModel>();
                x.Map<ControlsView, ControlsViewModel>();
            });

            // Add the locator to the application resources
            Application.Current.Resources["ViewModelLocator"] = container.Resolve<ViewModelLocator>();

            // Initialize views
            _homeView = new HomeView();
            _controlsView = new ControlsView();

            // Set initial view
            ContentArea.Content = _homeView;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = _homeView;
        }

        private void ControlsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = _controlsView;
        }
    }

    public interface ILogger
    {
        void Log(string message);
    }

    public class FileLogger : ILogger
    {
        private readonly string _logFile;

        public FileLogger(string logFile)
        {
            _logFile = logFile;
        }

        public void Log(string message)
        {
            System.IO.File.AppendAllText(_logFile, message);
        }
    }

    public class ConsoleLogger : ILogger
    {
        private readonly string _logFile;

        public ConsoleLogger(string logFile)
        {
            _logFile = logFile;
        }

        public void Log(string message)
        {
            Console.WriteLine($"Logging to {_logFile}: {message}");
        }
    }


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
        private readonly ILogger _logger;
        public NavigationService(ILogger logger)
        {
            _logger = logger;
            _logger.Log("Navigation service created");
        }

        public void NavigateTo(string viewName)
        {
            // Actual navigation logic would go here
            _logger.Log($"Navigating to {viewName}");
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

            NavigateToHome();
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
    /// Main view model.
    /// </summary>
    public class HomeViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        private string _currentUser;
        public string CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value, nameof(CurrentUser));
        }

        private string _currentScreen;
        public string CurrentScreen
        {
            get => _currentScreen;
            set => SetProperty(ref _currentScreen, value, nameof(CurrentScreen));
        }

        public HomeViewModel(INavigationService navigationService, IUserService userService)
        {
            _navigationService = navigationService;
            _userService = userService;
            CurrentScreen = "HOME SCREEN";
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
}
