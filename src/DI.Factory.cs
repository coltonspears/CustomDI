using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomDI
{
    /// <summary>
    /// Implementation of the <see cref="IContainer"/> interface for factory methods.
    /// </summary>
    public static class ContainerFactoryExtensions
    {
        /// <summary>
        /// Registers a factory for all implementations of a service.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterFactoryForAll<T>(this IContainer container) where T : class
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            // Create a factory that resolves all implementations
            return container.RegisterFactory<Func<IEnumerable<T>>>(context => 
            {
                return () => context.ResolveAll<T>();
            });
        }

        /// <summary>
        /// Registers a factory for creating services with parameters.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TParam">The parameter type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="factory">The factory method.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterParameterizedFactory<TService, TParam>(
            this IContainer container,
            Func<TParam, TService> factory) where TService : class
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            // Register a factory that returns a function
            return container.RegisterFactory<Func<TParam, TService>>(context => factory);
        }

        /// <summary>
        /// Registers a factory for creating named services.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterNamedFactory<TService>(this IContainer container) where TService : class
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            // Create a factory that resolves named implementations
            return container.RegisterFactory<Func<string, TService>>(context => 
            {
                return name => context.ResolveNamed<TService>(name);
            });
        }

        /// <summary>
        /// Registers a factory for creating keyed services.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterKeyedFactory<TService, TKey>(this IContainer container) where TService : class
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            // Create a factory that resolves keyed implementations
            return container.RegisterFactory<Func<TKey, TService>>(context => 
            {
                return key => context.ResolveKeyed<TService>(key);
            });
        }

        /// <summary>
        /// Registers a singleton service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterSingleton<TService, TImplementation>(this IContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return container.Register<TService, TImplementation>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Registers a transient service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterTransient<TService, TImplementation>(this IContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return container.Register<TService, TImplementation>(ServiceLifetime.Transient);
        }

        /// <summary>
        /// Registers a scoped service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterScoped<TService, TImplementation>(this IContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return container.Register<TService, TImplementation>(ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Registers all types in an assembly that implement a specific interface.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="lifetime">The lifetime of the services.</param>
        public static void RegisterAssemblyTypes<TService>(
            this IContainer container,
            Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Transient) where TService : class
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var serviceType = typeof(TService);
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && serviceType.IsAssignableFrom(t));

            foreach (var type in types)
            {
                container.Register(serviceType, type, lifetime);
            }
        }

        /// <summary>
        /// Registers all types in an assembly that match a predicate.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="predicate">The predicate to match types.</param>
        /// <param name="lifetime">The lifetime of the services.</param>
        public static void RegisterAssemblyTypes(
            this IContainer container,
            Assembly assembly,
            Func<Type, bool> predicate,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && predicate(t));

            foreach (var type in types)
            {
                container.Register(type, type, lifetime);
            }
        }

        /// <summary>
        /// Registers types by convention.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="implementationSuffix">The suffix for implementation types.</param>
        /// <param name="lifetime">The lifetime of the services.</param>
        public static void RegisterAssemblyTypesByConvention(
            this IContainer container,
            Assembly assembly,
            string implementationSuffix,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (string.IsNullOrEmpty(implementationSuffix))
                throw new ArgumentNullException(nameof(implementationSuffix));

            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith(implementationSuffix));

            foreach (var implementationType in types)
            {
                // Find interfaces implemented by this type
                var interfaces = implementationType.GetInterfaces();
                foreach (var interfaceType in interfaces)
                {
                    // Register the implementation for each interface
                    container.Register(interfaceType, implementationType, lifetime);
                }
            }
        }
    }
}
