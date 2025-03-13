using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace CustomDI.Wpf
{
    /// <summary>
    /// Provides markup extension for binding view models to views in XAML.
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class ViewModelBindingExtension : MarkupExtension
    {
        private static ViewModelLocator _locator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBindingExtension"/> class.
        /// </summary>
        public ViewModelBindingExtension()
        {
        }

        /// <summary>
        /// Returns the view model instance for the current view.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The view model instance.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
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

            // Get the target object (the view)
            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var targetObject = provideValueTarget?.TargetObject as FrameworkElement;

            if (targetObject == null)
            {
                throw new InvalidOperationException("Target object is not a FrameworkElement.");
            }

            // Get the view model for the view
            return _locator.GetViewModelForView(targetObject);
        }
    }

    /// <summary>
    /// Provides attached properties for automatically binding view models to views.
    /// </summary>
    public static class ViewModelBinder
    {
        /// <summary>
        /// The AutoBind attached property.
        /// </summary>
        public static readonly DependencyProperty AutoBindProperty =
            DependencyProperty.RegisterAttached(
                "AutoBind",
                typeof(bool),
                typeof(ViewModelBinder),
                new PropertyMetadata(false, OnAutoBindChanged));

        /// <summary>
        /// Gets the AutoBind value.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The AutoBind value.</returns>
        public static bool GetAutoBind(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoBindProperty);
        }

        /// <summary>
        /// Sets the AutoBind value.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="value">The AutoBind value.</param>
        public static void SetAutoBind(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoBindProperty, value);
        }

        /// <summary>
        /// Called when the AutoBind property changes.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnAutoBindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && (bool)e.NewValue)
            {
                // Wait until the element is loaded to set the DataContext
                if (element.IsLoaded)
                {
                    BindViewModel(element);
                }
                else
                {
                    element.Loaded += (sender, args) => BindViewModel(element);
                }
            }
        }

        /// <summary>
        /// Binds the view model to the view.
        /// </summary>
        /// <param name="view">The view.</param>
        private static void BindViewModel(FrameworkElement view)
        {
            var app = Application.Current;
            if (app?.Resources != null && app.Resources.Contains("ViewModelLocator"))
            {
                var locator = app.Resources["ViewModelLocator"] as ViewModelLocator;
                if (locator != null)
                {
                    view.DataContext = locator.GetViewModelForView(view);
                }
            }
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
            // If DataContext is not set, try to get it from the ViewModelLocator
            if (DataContext == null)
            {
                var app = Application.Current;
                if (app?.Resources != null && app.Resources.Contains("ViewModelLocator"))
                {
                    var locator = app.Resources["ViewModelLocator"] as ViewModelLocator;
                    if (locator != null)
                    {
                        DataContext = locator.GetViewModelForView(this);
                    }
                }
            }

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
}
