using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomDI
{
    /// <summary>
    /// Provides property injection capabilities.
    /// </summary>
    internal static class PropertyInjector
    {
        /// <summary>
        /// Injects properties into an instance.
        /// </summary>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="context">The resolve context.</param>
        /// <param name="explicitPropertyValues">Explicit property values.</param>
        public static void InjectProperties(
            object instance, 
            IResolveContext context, 
            IEnumerable<PropertyValue> explicitPropertyValues)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var type = instance.GetType();
            var explicitValues = explicitPropertyValues?.ToList() ?? new List<PropertyValue>();

            // Apply explicit property values
            foreach (var propertyValue in explicitValues)
            {
                var property = type.GetProperty(propertyValue.Name, 
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (property != null && property.CanWrite)
                {
                    try
                    {
                        var convertedValue = ConvertPropertyValue(propertyValue.Value, property.PropertyType);
                        property.SetValue(instance, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to set property '{property.Name}' on type {type.Name}", ex);
                    }
                }
            }

            // Apply attribute-based property injection
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetCustomAttributes<InjectAttribute>().Any());

            foreach (var property in properties)
            {
                // Skip properties that were already set explicitly
                if (explicitValues.Any(v => string.Equals(v.Name, property.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var injectAttribute = property.GetCustomAttribute<InjectAttribute>();
                object value = null;

                try
                {
                    // Resolve based on attribute configuration
                    if (!string.IsNullOrEmpty(injectAttribute.Name))
                    {
                        value = context.ResolveNamed(property.PropertyType, injectAttribute.Name);
                    }
                    else if (injectAttribute.Key != null)
                    {
                        value = context.ResolveKeyed(property.PropertyType, injectAttribute.Key);
                    }
                    else
                    {
                        value = context.Resolve(property.PropertyType);
                    }

                    // Set property value
                    property.SetValue(instance, value);
                }
                catch (Exception ex)
                {
                    // If property is required, rethrow
                    if (injectAttribute.Required)
                    {
                        throw new InvalidOperationException(
                            $"Failed to inject required property '{property.Name}' of type {property.PropertyType.Name} on {type.Name}", 
                            ex);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a property value to the target type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type.</param>
        /// <returns>The converted value.</returns>
        private static object ConvertPropertyValue(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            var valueType = value.GetType();

            // If the value is already of the target type, return it
            if (targetType.IsAssignableFrom(valueType))
            {
                return value;
            }

            // Try to convert the value
            try
            {
                // Handle nullable types
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var underlyingType = Nullable.GetUnderlyingType(targetType);
                    var convertedValue = Convert.ChangeType(value, underlyingType);
                    return convertedValue;
                }

                // Handle enum types
                if (targetType.IsEnum)
                {
                    if (value is string stringValue)
                    {
                        return Enum.Parse(targetType, stringValue, true);
                    }
                    else
                    {
                        return Enum.ToObject(targetType, value);
                    }
                }

                // Handle other types
                return Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to convert value of type {valueType.Name} to type {targetType.Name}",
                    ex);
            }
        }
    }

    /// <summary>
    /// Provides extension methods for property injection.
    /// </summary>
    public static class PropertyInjectionExtensions
    {
        /// <summary>
        /// Injects properties into an instance using the container.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <returns>The instance with injected properties.</returns>
        public static T InjectProperties<T>(this IContainer container, T instance) where T : class
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var context = new ResolveContext((Container)container, null);
            PropertyInjector.InjectProperties(instance, context, null);
            
            return instance;
        }
    }
}
