using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomDI
{
    #region Interfaces

    /// <summary>
    /// Represents the main dependency injection container.
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// Registers a service with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TImplementation : class, TService;

        /// <summary>
        /// Registers a service with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient);

        /// <summary>
        /// Registers a singleton instance with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder RegisterInstance<TService>(TService instance) where TService : class;

        /// <summary>
        /// Registers a factory method with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The factory method.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder RegisterFactory<TService>(Func<IResolveContext, TService> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class;

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved service.</returns>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved service.</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Tries to resolve a service from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="service">The resolved service, or null if not registered.</param>
        /// <returns>True if the service was resolved, false otherwise.</returns>
        bool TryResolve<T>(out T service) where T : class;

        /// <summary>
        /// Tries to resolve a service from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="service">The resolved service, or null if not registered.</param>
        /// <returns>True if the service was resolved, false otherwise.</returns>
        bool TryResolve(Type serviceType, out object service);

        /// <summary>
        /// Resolves all services of the specified type from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved services.</returns>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Resolves all services of the specified type from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved services.</returns>
        IEnumerable<object> ResolveAll(Type serviceType);

        /// <summary>
        /// Creates a new scope from the container.
        /// </summary>
        /// <returns>A new scope.</returns>
        IScope CreateScope();

        /// <summary>
        /// Checks if a service is registered with the container.
        /// </summary>
        /// <typeparam name="T">The service type to check.</typeparam>
        /// <returns>True if the service is registered, false otherwise.</returns>
        bool IsRegistered<T>() where T : class;

        /// <summary>
        /// Checks if a service is registered with the container.
        /// </summary>
        /// <param name="serviceType">The service type to check.</param>
        /// <returns>True if the service is registered, false otherwise.</returns>
        bool IsRegistered(Type serviceType);
    }

    /// <summary>
    /// Represents a scope for resolving scoped services.
    /// </summary>
    public interface IScope : IDisposable
    {
        /// <summary>
        /// Resolves a service from the scope.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved service.</returns>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Resolves a service from the scope.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved service.</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Tries to resolve a service from the scope.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="service">The resolved service, or null if not registered.</param>
        /// <returns>True if the service was resolved, false otherwise.</returns>
        bool TryResolve<T>(out T service) where T : class;

        /// <summary>
        /// Tries to resolve a service from the scope.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="service">The resolved service, or null if not registered.</param>
        /// <returns>True if the service was resolved, false otherwise.</returns>
        bool TryResolve(Type serviceType, out object service);

        /// <summary>
        /// Resolves all services of the specified type from the scope.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved services.</returns>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Resolves all services of the specified type from the scope.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved services.</returns>
        IEnumerable<object> ResolveAll(Type serviceType);
    }

    /// <summary>
    /// Represents a builder for configuring service registrations.
    /// </summary>
    public interface IRegistrationBuilder
    {
        /// <summary>
        /// Configures the service to be resolved with a specific name.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder Named(string name);

        /// <summary>
        /// Configures the service to be resolved with a specific key.
        /// </summary>
        /// <param name="key">The key of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder Keyed(object key);

        /// <summary>
        /// Configures the service to be resolved only when a condition is met.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder When(Func<IResolveContext, bool> condition);

        /// <summary>
        /// Configures the service to be resolved with specific constructor parameters.
        /// </summary>
        /// <param name="parameters">The constructor parameters.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder WithParameters(params Parameter[] parameters);

        /// <summary>
        /// Configures the service to be resolved with specific property values.
        /// </summary>
        /// <param name="propertyValues">The property values.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder WithProperties(params PropertyValue[] propertyValues);

        /// <summary>
        /// Configures the service to be initialized after construction.
        /// </summary>
        /// <param name="initializer">The initializer action.</param>
        /// <returns>The registration builder for further configuration.</returns>
        IRegistrationBuilder OnActivated(Action<IActivatedEventArgs> initializer);
    }

    /// <summary>
    /// Represents the context for resolving services.
    /// </summary>
    public interface IResolveContext
    {
        /// <summary>
        /// Resolves a service from the context.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved service.</returns>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Resolves a service from the context.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved service.</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Resolves a named service from the context.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="name">The name of the service.</param>
        /// <returns>The resolved service.</returns>
        T ResolveNamed<T>(string name) where T : class;

        /// <summary>
        /// Resolves a named service from the context.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="name">The name of the service.</param>
        /// <returns>The resolved service.</returns>
        object ResolveNamed(Type serviceType, string name);

        /// <summary>
        /// Resolves a keyed service from the context.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="key">The key of the service.</param>
        /// <returns>The resolved service.</returns>
        T ResolveKeyed<T>(object key) where T : class;

        /// <summary>
        /// Resolves a keyed service from the context.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="key">The key of the service.</param>
        /// <returns>The resolved service.</returns>
        object ResolveKeyed(Type serviceType, object key);

        /// <summary>
        /// Resolves all services of the specified type from the context.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved services.</returns>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Resolves all services of the specified type from the context.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved services.</returns>
        IEnumerable<object> ResolveAll(Type serviceType);
    }

    /// <summary>
    /// Represents the event arguments for an activated service.
    /// </summary>
    public interface IActivatedEventArgs
    {
        /// <summary>
        /// Gets the instance that was activated.
        /// </summary>
        object Instance { get; }

        /// <summary>
        /// Gets the context that was used to resolve the instance.
        /// </summary>
        IResolveContext Context { get; }
    }

    #endregion

    #region Enums

    /// <summary>
    /// Represents the lifetime of a service.
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// A new instance is created each time the service is requested.
        /// </summary>
        Transient,

        /// <summary>
        /// A single instance is created and shared within a scope.
        /// </summary>
        Scoped,

        /// <summary>
        /// A single instance is created and shared for the lifetime of the container.
        /// </summary>
        Singleton
    }

    #endregion

    #region Classes

    /// <summary>
    /// Represents a parameter for constructor injection.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public Parameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Creates a new parameter with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>A new parameter.</returns>
        public static Parameter FromNamedValue(string name, object value)
        {
            return new Parameter(name, value);
        }
    }

    /// <summary>
    /// Represents a property value for property injection.
    /// </summary>
    public class PropertyValue
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValue"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public PropertyValue(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Creates a new property value with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>A new property value.</returns>
        public static PropertyValue FromNamedValue(string name, object value)
        {
            return new PropertyValue(name, value);
        }
    }

    #endregion

    #region Attributes

    /// <summary>
    /// Specifies that a property should be injected by the container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the service to inject.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key of the service to inject.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dependency is required.
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectAttribute"/> class.
        /// </summary>
        public InjectAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectAttribute"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the service to inject.</param>
        public InjectAttribute(string name)
        {
            Name = name;
        }
    }

    #endregion
}
