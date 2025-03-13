using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace CustomDI.Wpf
{
    /// <summary>
    /// Provides a locator for view models in a WPF application.
    /// </summary>
    public class ViewModelLocator
    {
        private readonly IContainer _container;
        private readonly Dictionary<string, Type> _viewModelTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, object> _designTimeViewModels = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelLocator"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public ViewModelLocator(IContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <summary>
        /// Registers a view model type with the locator.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="key">The key to register the view model with.</param>
        /// <returns>The view model locator for method chaining.</returns>
        public ViewModelLocator Register<TViewModel>(string key) where TViewModel : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            _viewModelTypes[key] = typeof(TViewModel);
            return this;
        }

        /// <summary>
        /// Registers a design-time view model instance with the locator.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="key">The key to register the view model with.</param>
        /// <param name="viewModel">The view model instance.</param>
        /// <returns>The view model locator for method chaining.</returns>
        public ViewModelLocator RegisterDesignTimeViewModel<TViewModel>(string key, TViewModel viewModel) where TViewModel : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            _designTimeViewModels[key] = viewModel;
            return this;
        }

        /// <summary>
        /// Gets a view model by key.
        /// </summary>
        /// <param name="key">The key of the view model.</param>
        /// <returns>The view model instance.</returns>
        public object this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                // Check if we're in design mode
                if (IsInDesignMode && _designTimeViewModels.TryGetValue(key, out var designTimeViewModel))
                {
                    return designTimeViewModel;
                }

                // Otherwise resolve from container
                if (_viewModelTypes.TryGetValue(key, out var viewModelType))
                {
                    return _container.Resolve(viewModelType);
                }

                throw new InvalidOperationException($"No view model registered with key '{key}'");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the application is in design mode.
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                var descriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                    DesignerProperties.IsInDesignModeProperty,
                    typeof(FrameworkElement));

                return (bool)(descriptor?.Metadata.DefaultValue ?? false);
            }
        }
    }

    /// <summary>
    /// Provides extension methods for registering view models with the container.
    /// </summary>
    public static class ViewModelLocatorExtensions
    {
        /// <summary>
        /// Registers a view model locator with the container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The container for method chaining.</returns>
        public static IContainer RegisterViewModelLocator(this IContainer container, Action<ViewModelLocator> configure)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var locator = new ViewModelLocator(container);
            configure(locator);
            container.RegisterInstance<ViewModelLocator>(locator);

            return container;
        }
    }
}
