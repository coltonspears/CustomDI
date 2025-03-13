using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomDI
{
    /// <summary>
    /// Represents a service registration in the container.
    /// </summary>
    internal class ServiceRegistration
    {
        /// <summary>
        /// Gets the service type.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the implementation type.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Gets the instance (if registered as singleton).
        /// </summary>
        public object Instance { get; private set; }

        /// <summary>
        /// Gets the factory method (if registered with a factory).
        /// </summary>
        public Func<IResolveContext, object> Factory { get; }

        /// <summary>
        /// Gets the lifetime of the service.
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Gets the name of the service (if registered with a name).
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the key of the service (if registered with a key).
        /// </summary>
        public object Key { get; internal set; }

        /// <summary>
        /// Gets the condition for resolving the service (if registered with a condition).
        /// </summary>
        public Func<IResolveContext, bool> Condition { get; internal set; }

        /// <summary>
        /// Gets the constructor parameters for the service.
        /// </summary>
        public List<Parameter> Parameters { get; } = new List<Parameter>();

        /// <summary>
        /// Gets the property values for the service.
        /// </summary>
        public List<PropertyValue> PropertyValues { get; } = new List<PropertyValue>();

        /// <summary>
        /// Gets the initializers for the service.
        /// </summary>
        public List<Action<IActivatedEventArgs>> Initializers { get; } = new List<Action<IActivatedEventArgs>>();

        /// <summary>
        /// Gets or sets the lock object for thread safety.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Gets or sets the scoped instances.
        /// </summary>
        private readonly ConcurrentDictionary<IScope, object> _scopedInstances = new ConcurrentDictionary<IScope, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistration"/> class for a type registration.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        public ServiceRegistration(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistration"/> class for an instance registration.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="instance">The instance.</param>
        public ServiceRegistration(Type serviceType, object instance)
        {
            ServiceType = serviceType;
            ImplementationType = instance.GetType();
            Instance = instance;
            Lifetime = ServiceLifetime.Singleton;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistration"/> class for a factory registration.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="factory">The factory method.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        public ServiceRegistration(Type serviceType, Func<IResolveContext, object> factory, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            Factory = factory;
            Lifetime = lifetime;
        }

        /// <summary>
        /// Gets an instance of the service.
        /// </summary>
        /// <param name="context">The resolve context.</param>
        /// <param name="scope">The current scope.</param>
        /// <returns>An instance of the service.</returns>
        public object GetInstance(IResolveContext context, IScope scope)
        {
            // Check if the condition is met
            if (Condition != null && !Condition(context))
            {
                throw new InvalidOperationException($"Condition not met for service {ServiceType.Name}");
            }

            // Handle different lifetimes
            switch (Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return GetOrCreateSingletonInstance(context);
                case ServiceLifetime.Scoped:
                    return GetOrCreateScopedInstance(context, scope);
                case ServiceLifetime.Transient:
                default:
                    return CreateInstance(context);
            }
        }

        /// <summary>
        /// Gets or creates a singleton instance of the service.
        /// </summary>
        /// <param name="context">The resolve context.</param>
        /// <returns>A singleton instance of the service.</returns>
        private object GetOrCreateSingletonInstance(IResolveContext context)
        {
            if (Instance == null)
            {
                lock (_lock)
                {
                    if (Instance == null)
                    {
                        Instance = CreateInstance(context);
                    }
                }
            }

            return Instance;
        }

        /// <summary>
        /// Gets or creates a scoped instance of the service.
        /// </summary>
        /// <param name="context">The resolve context.</param>
        /// <param name="scope">The current scope.</param>
        /// <returns>A scoped instance of the service.</returns>
        private object GetOrCreateScopedInstance(IResolveContext context, IScope scope)
        {
            if (scope == null)
            {
                throw new InvalidOperationException($"Cannot resolve scoped service {ServiceType.Name} without a scope");
            }

            return _scopedInstances.GetOrAdd(scope, _ => CreateInstance(context));
        }

        /// <summary>
        /// Creates a new instance of the service.
        /// </summary>
        /// <param name="context">The resolve context.</param>
        /// <returns>A new instance of the service.</returns>
        private object CreateInstance(IResolveContext context)
        {
            // Use factory if provided
            if (Factory != null)
            {
                return Factory(context);
            }

            // Otherwise create instance using reflection
            object instance = CreateInstanceUsingReflection(context);

            // Apply property injection
            ApplyPropertyInjection(instance, context);

            // Run initializers
            RunInitializers(instance, context);

            return instance;
        }

        /// <summary>
        /// Creates a new instance of the service using reflection.
        /// </summary>
        /// <param name="context">The resolve context.</param>
        /// <returns>A new instance of the service.</returns>
        private object CreateInstanceUsingReflection(IResolveContext context)
        {
            // Get constructors ordered by parameter count (descending)
            var constructors = ImplementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderByDescending(c => c.GetParameters().Length)
                .ToList();

            if (constructors.Count == 0)
            {
                throw new InvalidOperationException($"No public constructors found for type {ImplementationType.Name}");
            }

            // Try each constructor
            foreach (var constructor in constructors)
            {
                try
                {
                    var parameters = constructor.GetParameters();
                    var arguments = new object[parameters.Length];

                    // Resolve constructor parameters
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];

                        // Check if parameter is provided explicitly
                        var explicitParam = Parameters.FirstOrDefault(p => p.Name == parameter.Name);
                        if (explicitParam != null)
                        {
                            arguments[i] = explicitParam.Value;
                            continue;
                        }

                        // Otherwise resolve from container
                        try
                        {
                            arguments[i] = context.Resolve(parameter.ParameterType);
                        }
                        catch (Exception)
                        {
                            // If parameter is optional, use default value
                            if (parameter.IsOptional)
                            {
                                arguments[i] = parameter.DefaultValue;
                            }
                            else
                            {
                                // Rethrow if not optional
                                throw;
                            }
                        }
                    }

                    // Create instance
                    return constructor.Invoke(arguments);
                }
                catch (Exception)
                {
                    // Try next constructor if this one fails
                    if (constructor == constructors.Last())
                    {
                        throw; // Rethrow if this was the last constructor
                    }
                }
            }

            // Should never reach here
            throw new InvalidOperationException($"Failed to create instance of type {ImplementationType.Name}");
        }

        /// <summary>
        /// Applies property injection to the instance.
        /// </summary>
        /// <param name="instance">The instance to inject properties into.</param>
        /// <param name="context">The resolve context.</param>
        private void ApplyPropertyInjection(object instance, IResolveContext context)
        {
            // Apply explicit property values
            foreach (var propertyValue in PropertyValues)
            {
                var property = ImplementationType.GetProperty(propertyValue.Name);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(instance, propertyValue.Value);
                }
            }

            // Apply attribute-based property injection
            var properties = ImplementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetCustomAttributes<InjectAttribute>().Any());

            foreach (var property in properties)
            {
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
                catch (Exception)
                {
                    // If property is required, rethrow
                    if (injectAttribute.Required)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Runs the initializers on the instance.
        /// </summary>
        /// <param name="instance">The instance to initialize.</param>
        /// <param name="context">The resolve context.</param>
        private void RunInitializers(object instance, IResolveContext context)
        {
            var args = new ActivatedEventArgs(instance, context);
            foreach (var initializer in Initializers)
            {
                initializer(args);
            }
        }

        /// <summary>
        /// Removes a scoped instance.
        /// </summary>
        /// <param name="scope">The scope to remove.</param>
        public void RemoveScopedInstance(IScope scope)
        {
            _scopedInstances.TryRemove(scope, out _);
        }
    }

}
