using System;

namespace CustomDI
{
    /// <summary>
    /// Extension methods for registering decorators.
    /// </summary>
    public static class DecoratorExtensions
    {
        /// <summary>
        /// Registers a decorator for a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TDecorator">The decorator type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterDecorator<TService, TDecorator>(this IContainer container)
            where TService : class
            where TDecorator : class, TService
        {
            // Register the decorator factory
            return container.RegisterFactory<TService>(context =>
            {
                // Resolve the inner service
                var innerService = context.Resolve<TService>();
                
                // Create the decorator with the inner service
                var decorator = (TDecorator)Activator.CreateInstance(typeof(TDecorator), innerService);
                
                return decorator;
            });
        }

        /// <summary>
        /// Registers a decorator for a service with a condition.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TDecorator">The decorator type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="condition">The condition to check.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterDecoratorWhen<TService, TDecorator>(
            this IContainer container, 
            Func<IResolveContext, bool> condition)
            where TService : class
            where TDecorator : class, TService
        {
            // Register the decorator factory with a condition
            return container.RegisterFactory<TService>(context =>
            {
                // Resolve the inner service
                var innerService = context.Resolve<TService>();
                
                // Create the decorator with the inner service
                var decorator = (TDecorator)Activator.CreateInstance(typeof(TDecorator), innerService);
                
                return decorator;
            }).When(condition);
        }

        /// <summary>
        /// Registers a decorator factory for a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="decoratorFactory">The decorator factory.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterDecoratorFactory<TService>(
            this IContainer container, 
            Func<IResolveContext, TService, TService> decoratorFactory)
            where TService : class
        {
            // Register the decorator factory
            return container.RegisterFactory<TService>(context =>
            {
                // Resolve the inner service
                var innerService = context.Resolve<TService>();
                
                // Create the decorator using the factory
                var decorator = decoratorFactory(context, innerService);
                
                return decorator;
            });
        }
    }
}
