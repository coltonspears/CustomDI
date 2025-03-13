using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomDI
{
    /// <summary>
    /// Provides enhanced constructor injection resolution capabilities.
    /// </summary>
    internal static class ConstructorResolver
    {
        /// <summary>
        /// Resolves the best constructor for a type.
        /// </summary>
        /// <param name="type">The type to resolve a constructor for.</param>
        /// <returns>The best constructor for the type.</returns>
        public static ConstructorInfo ResolveBestConstructor(Type type)
        {
            // Get all public instance constructors
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"No public constructors found for type {type.Name}");
            }

            // Check for constructor with [Inject] attribute first
            var attributedConstructor = constructors.FirstOrDefault(c => 
                c.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);
            
            if (attributedConstructor != null)
            {
                return attributedConstructor;
            }

            // Otherwise, return the constructor with the most parameters
            return constructors.OrderByDescending(c => c.GetParameters().Length).First();
        }

        /// <summary>
        /// Resolves constructor parameters for a constructor.
        /// </summary>
        /// <param name="constructor">The constructor to resolve parameters for.</param>
        /// <param name="context">The resolve context.</param>
        /// <param name="explicitParameters">Explicit parameter values.</param>
        /// <returns>The resolved parameter values.</returns>
        public static object[] ResolveConstructorParameters(
            ConstructorInfo constructor, 
            IResolveContext context, 
            IEnumerable<Parameter> explicitParameters)
        {
            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];
            var explicitParamsList = explicitParameters?.ToList() ?? new List<Parameter>();

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                
                // Check if parameter is provided explicitly
                var explicitParam = explicitParamsList.FirstOrDefault(p => 
                    string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));
                
                if (explicitParam != null)
                {
                    arguments[i] = ConvertParameterValue(explicitParam.Value, parameter.ParameterType);
                    continue;
                }

                // Check for parameter attributes
                var injectAttribute = parameter.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null)
                {
                    // Resolve based on attribute configuration
                    if (!string.IsNullOrEmpty(injectAttribute.Name))
                    {
                        arguments[i] = context.ResolveNamed(parameter.ParameterType, injectAttribute.Name);
                    }
                    else if (injectAttribute.Key != null)
                    {
                        arguments[i] = context.ResolveKeyed(parameter.ParameterType, injectAttribute.Key);
                    }
                    else
                    {
                        arguments[i] = context.Resolve(parameter.ParameterType);
                    }
                    continue;
                }

                // Otherwise resolve from container
                try
                {
                    arguments[i] = context.Resolve(parameter.ParameterType);
                }
                catch (Exception ex)
                {
                    // If parameter is optional, use default value
                    if (parameter.IsOptional)
                    {
                        arguments[i] = parameter.DefaultValue;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Failed to resolve parameter '{parameter.Name}' of type {parameter.ParameterType.Name} for constructor of {constructor.DeclaringType.Name}",
                            ex);
                    }
                }
            }

            return arguments;
        }

        /// <summary>
        /// Converts a parameter value to the target type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type.</param>
        /// <returns>The converted value.</returns>
        private static object ConvertParameterValue(object value, Type targetType)
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
    /// Attribute to mark a constructor as the one to use for dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class ConstructorInjectAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorInjectAttribute"/> class.
        /// </summary>
        public ConstructorInjectAttribute()
        {
        }
    }
}
