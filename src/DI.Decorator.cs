using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomDI
{
    /// <summary>
    /// Extension methods for decorating services.
    /// </summary>
    public static class DecoratorExtensions
    {
        /// <summary>
        /// Registers a decorator for a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TDecorator">The decorator type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="lifetime">The lifetime of the decorator.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterDecorator<TService, TDecorator>(
            this IContainer container,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TDecorator : class, TService
        {
            // Check if the service is already registered
            if (!container.IsRegistered<TService>())
            {
                throw new InvalidOperationException($"Cannot register decorator for {typeof(TService).Name} because the service is not registered.");
            }

            // Get the original registration
            var originalRegistration = container.Resolve<TService>();

            // Register the decorator with a factory that resolves the inner service
            return container.RegisterFactory<TService>(context =>
            {
                // Create the decorator with the original service as a parameter
                var constructors = typeof(TDecorator).GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length);

                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    var args = new object[parameters.Length];
                    bool canUseConstructor = true;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        
                        // If the parameter is the service type, use the original instance
                        if (parameter.ParameterType == typeof(TService))
                        {
                            args[i] = originalRegistration;
                        }
                        else
                        {
                            // Otherwise try to resolve from container
                            try
                            {
                                args[i] = context.Resolve(parameter.ParameterType);
                            }
                            catch
                            {
                                if (parameter.IsOptional)
                                {
                                    args[i] = parameter.DefaultValue;
                                }
                                else
                                {
                                    canUseConstructor = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (canUseConstructor)
                    {
                        return (TService)constructor.Invoke(args);
                    }
                }

                throw new InvalidOperationException($"No suitable constructor found for decorator {typeof(TDecorator).Name}");
            }, lifetime);
        }
    }
}
