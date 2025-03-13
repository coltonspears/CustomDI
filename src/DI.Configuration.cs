using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomDI
{
    /// <summary>
    /// Provides fluent configuration for the container.
    /// </summary>
    public class ContainerBuilder
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerBuilder"/> class.
        /// </summary>
        public ContainerBuilder()
        {
            _container = ContainerFactory.CreateContainer();
        }

        /// <summary>
        /// Registers a service with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The container builder for method chaining.</returns>
        public ContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            _container.Register<TService, TImplementation>(lifetime);
            return this;
        }

        /// <summary>
        /// Registers a service with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The container builder for method chaining.</returns>
        public ContainerBuilder Register<TService>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
        {
            _container.Register<TService, TService>(lifetime);
            return this;
        }

        /// <summary>
        /// Registers a singleton instance with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <returns>The container builder for method chaining.</returns>
        public ContainerBuilder RegisterInstance<TService>(TService instance)
            where TService : class
        {
            _container.RegisterInstance(instance);
            return this;
        }

        /// <summary>
        /// Registers a factory method with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The factory method.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The container builder for method chaining.</returns>
        public ContainerBuilder RegisterFactory<TService>(Func<IResolveContext, TService> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
        {
            _container.RegisterFactory(factory, lifetime);
            return this;
        }

        /// <summary>
        /// Registers all types in an assembly that match a predicate.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="predicate">The predicate to match types.</param>
        /// <param name="lifetime">The lifetime of the services.</param>
        /// <returns>The container builder for method chaining.</returns>
        public ContainerBuilder RegisterAssemblyTypes(Assembly assembly, Func<Type, bool> predicate, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && predicate(t));

            foreach (var type in types)
            {
                _container.Register(type, type, lifetime);
            }

            return this;
        }

        /// <summary>
        /// Registers all types in an assembly that implement a specific interface.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="lifetime">The lifetime of the services.</param>
        /// <returns>The container builder for method chaining.</returns>
        public ContainerBuilder RegisterAssemblyTypes<TService>(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var serviceType = typeof(TService);
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && serviceType.IsAssignableFrom(t));

            foreach (var type in types)
            {
                _container.Register(serviceType, type, lifetime);
            }

            return this;
        }

        /// <summary>
        /// Registers all types in an assembly that follow a naming convention.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="implementationSuffix">The suffix for implementation types.</param>
        /// <param name="lifetime">The lifetime of the services.</param>
        /// <returns>The container builder for method chaining.</returns>
        public ContainerBuilder RegisterAssemblyTypesByConvention(Assembly assembly, string implementationSuffix, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (string.IsNullOrEmpty(implementationSuffix))
                throw new ArgumentException("Implementation suffix cannot be null or empty", nameof(implementationSuffix));

            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith(implementationSuffix));

            foreach (var implementationType in types)
            {
                // Find interfaces implemented by this type
                var interfaces = implementationType.GetInterfaces();
                var baseName = implementationType.Name.Substring(0, implementationType.Name.Length - implementationSuffix.Length);

                // Look for an interface that matches the naming convention
                var serviceType = interfaces.FirstOrDefault(i => i.Name == $"I{baseName}");
                if (serviceType != null)
                {
                    _container.Register(serviceType, implementationType, lifetime);
                }
            }

            return this;
        }

        /// <summary>
        /// Builds the container.
        /// </summary>
        /// <returns>The built container.</returns>
        public IContainer Build()
        {
            return _container;
        }
    }

    /// <summary>
    /// Provides extension methods for container configuration.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Registers a service as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterSingleton<TService, TImplementation>(this IContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.Register<TService, TImplementation>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Registers a service as a singleton.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterSingleton<TService>(this IContainer container)
            where TService : class
        {
            return container.Register<TService, TService>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Registers a service as scoped.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterScoped<TService, TImplementation>(this IContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.Register<TService, TImplementation>(ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Registers a service as scoped.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterScoped<TService>(this IContainer container)
            where TService : class
        {
            return container.Register<TService, TService>(ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Registers a service as transient.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterTransient<TService, TImplementation>(this IContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.Register<TService, TImplementation>(ServiceLifetime.Transient);
        }

        /// <summary>
        /// Registers a service as transient.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterTransient<TService>(this IContainer container)
            where TService : class
        {
            return container.Register<TService, TService>(ServiceLifetime.Transient);
        }

        /// <summary>
        /// Registers a lazy factory for a service.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterLazy<TService>(this IContainer container)
            where TService : class
        {
            return container.RegisterFactory<Lazy<TService>>(context => 
                new Lazy<TService>(() => context.Resolve<TService>()));
        }
    }
}
