using System;
using System.Windows;
using System.Windows.Markup;

namespace CustomDI.Wpf
{
    /// <summary>
    /// Provides markup extension for resolving view models in XAML.
    /// </summary>
    public class ViewModelExtension : MarkupExtension
    {
        private static ViewModelLocator _locator;

        /// <summary>
        /// Gets or sets the key of the view model to resolve.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelExtension"/> class.
        /// </summary>
        public ViewModelExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelExtension"/> class with the specified key.
        /// </summary>
        /// <param name="key">The key of the view model to resolve.</param>
        public ViewModelExtension(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Returns the view model instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The view model instance.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key))
                throw new InvalidOperationException("Key cannot be null or empty");

            if (_locator == null)
            {
                var app = Application.Current;
                if (app?.Resources != null && app.Resources.Contains("ViewModelLocator"))
                {
                    _locator = app.Resources["ViewModelLocator"] as ViewModelLocator;
                }

                if (_locator == null)
                {
                    throw new InvalidOperationException(
                        "ViewModelLocator not found. Make sure to add it to the application resources with the key 'ViewModelLocator'.");
                }
            }

            return _locator[Key];
        }
    }

    /// <summary>
    /// Provides a base class for view models with property change notification.
    /// </summary>
    public abstract class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">The backing field for the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the value changed, false otherwise.</returns>
        protected bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Provides a base class for views with automatic view model binding.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    public abstract class ViewBase<TViewModel> : System.Windows.Controls.UserControl
        where TViewModel : class
    {
        /// <summary>
        /// Gets the view model for the view.
        /// </summary>
        public TViewModel ViewModel => DataContext as TViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewBase{TViewModel}"/> class.
        /// </summary>
        protected ViewBase()
        {
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Called when the view is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                throw new InvalidOperationException(
                    $"DataContext is not of type {typeof(TViewModel).Name}. Make sure to set the DataContext to an instance of {typeof(TViewModel).Name}.");
            }

            OnViewModelLoaded();
        }

        /// <summary>
        /// Called when the view model is loaded.
        /// </summary>
        protected virtual void OnViewModelLoaded()
        {
        }
    }

    /// <summary>
    /// Provides attached properties for binding view models to views.
    /// </summary>
    public static class ViewModelBinder
    {
        /// <summary>
        /// The view model key attached property.
        /// </summary>
        public static readonly DependencyProperty ViewModelKeyProperty =
            DependencyProperty.RegisterAttached(
                "ViewModelKey",
                typeof(string),
                typeof(ViewModelBinder),
                new PropertyMetadata(null, OnViewModelKeyChanged));

        /// <summary>
        /// Gets the view model key.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The view model key.</returns>
        public static string GetViewModelKey(DependencyObject obj)
        {
            return (string)obj.GetValue(ViewModelKeyProperty);
        }

        /// <summary>
        /// Sets the view model key.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="value">The view model key.</param>
        public static void SetViewModelKey(DependencyObject obj, string value)
        {
            obj.SetValue(ViewModelKeyProperty, value);
        }

        /// <summary>
        /// Called when the view model key changes.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnViewModelKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && e.NewValue is string key)
            {
                // Wait until the element is loaded to set the DataContext
                if (element.IsLoaded)
                {
                    SetDataContext(element, key);
                }
                else
                {
                    element.Loaded += (sender, args) => SetDataContext(element, key);
                }
            }
        }

        /// <summary>
        /// Sets the DataContext of an element to a view model.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="key">The view model key.</param>
        private static void SetDataContext(FrameworkElement element, string key)
        {
            var app = Application.Current;
            if (app?.Resources != null && app.Resources.Contains("ViewModelLocator"))
            {
                var locator = app.Resources["ViewModelLocator"] as ViewModelLocator;
                if (locator != null)
                {
                    element.DataContext = locator[key];
                }
            }
        }
    }
}
