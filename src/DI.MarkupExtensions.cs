using System;
using System.Windows;
using System.Windows.Markup;

namespace CustomDI.Wpf
{
    /// <summary>
    /// Provides a markup extension for resolving view models in XAML.
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class ViewModelExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the key of the view model.
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
        /// <param name="key">The key of the view model.</param>
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
            {
                throw new InvalidOperationException("Key cannot be null or empty");
            }

            var app = Application.Current;
            if (app?.Resources != null && app.Resources.Contains("ViewModelLocator"))
            {
                var locator = app.Resources["ViewModelLocator"] as ViewModelLocator;
                if (locator != null)
                {
                    return locator[Key];
                }
            }

            throw new InvalidOperationException(
                "ViewModelLocator not found. Make sure to add it to the application resources with the key 'ViewModelLocator'.");
        }
    }
}
