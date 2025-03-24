using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomDI
{
    /// <summary>
    /// Implementation of the <see cref="IResolveContext"/> interface.
    /// </summary>
    internal class ResolveContext : IResolveContext
    {
        private readonly Container _container;
        private readonly IScope _scope;
        private readonly Stack<Type> _resolutionStack = new Stack<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveContext"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="scope">The scope.</param>
        public ResolveContext(Container container, IScope scope)
        {
            _container = container;
            _scope = scope;
        }

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved service.</returns>
        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved service.</returns>
        public object Resolve(Type serviceType)
        {
            // Use the common resolution method with default registration retrieval
            return ResolveInternal(serviceType, 
                () => _container.GetAllRegistrations(serviceType).ToList(),
                registrations => registrations.Count == 0 ? 
                    $"No registration found for service {serviceType.Name}" : null);
        }

        /// <summary>
        /// Resolves a named service from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="name">The name of the service.</param>
        /// <returns>The resolved service.</returns>
        public T ResolveNamed<T>(string name) where T : class
        {
            return (T)ResolveNamed(typeof(T), name);
        }

        /// <summary>
        /// Resolves a named service from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="name">The name of the service.</param>
        /// <returns>The resolved service.</returns>
        public object ResolveNamed(Type serviceType, string name)
        {
            // Use the common resolution method with named registration retrieval
            var registration = _container.GetNamedRegistration(serviceType, name);
            return ResolveInternal(serviceType, 
                () => registration != null ? new List<ServiceRegistration> { registration } : new List<ServiceRegistration>(),
                registrations => registrations.Count == 0 ? 
                    $"No registration found for service {serviceType.Name} with name '{name}'" : null,
                instance => instance == null ? 
                    $"Failed to resolve service {serviceType.Name} with name '{name}'. Registration condition was not met." : null);
        }

        /// <summary>
        /// Resolves a keyed service from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="key">The key of the service.</param>
        /// <returns>The resolved service.</returns>
        public T ResolveKeyed<T>(object key) where T : class
        {
            return (T)ResolveKeyed(typeof(T), key);
        }

        /// <summary>
        /// Resolves a keyed service from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="key">The key of the service.</param>
        /// <returns>The resolved service.</returns>
        public object ResolveKeyed(Type serviceType, object key)
        {
            // Use the common resolution method with keyed registration retrieval
            var registration = _container.GetKeyedRegistration(serviceType, key);
            return ResolveInternal(serviceType, 
                () => registration != null ? new List<ServiceRegistration> { registration } : new List<ServiceRegistration>(),
                registrations => registrations.Count == 0 ? 
                    $"No registration found for service {serviceType.Name} with key '{key}'" : null,
                instance => instance == null ? 
                    $"Failed to resolve service {serviceType.Name} with key '{key}'. Registration condition was not met." : null);
        }

        /// <summary>
        /// Resolves all services of the specified type from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved services.</returns>
        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return ResolveAll(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Resolves all services of the specified type from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved services.</returns>
        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            // Check for circular dependencies
            CheckForCircularDependencies(serviceType);

            // Push the service type onto the stack
            _resolutionStack.Push(serviceType);

            try
            {
                // Get all registrations
                var registrations = _container.GetAllRegistrations(serviceType);
                
                // Get an instance from each registration that meets its condition
                var instances = new List<object>();
                foreach (var registration in registrations)
                {
                    var instance = registration.GetInstance(this, _scope);
                    if (instance != null)
                    {
                        instances.Add(instance);
                    }
                }
                
                return instances;
            }
            finally
            {
                // Pop the service type from the stack
                _resolutionStack.Pop();
            }
        }

        /// <summary>
        /// Common internal method for resolving services to reduce code duplication.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="getRegistrations">Function to get the registrations.</param>
        /// <param name="validateRegistrations">Function to validate registrations and return error message if invalid.</param>
        /// <param name="validateInstance">Optional function to validate the resolved instance and return error message if invalid.</param>
        /// <returns>The resolved service.</returns>
        private object ResolveInternal(
            Type serviceType, 
            Func<List<ServiceRegistration>> getRegistrations, 
            Func<List<ServiceRegistration>, string> validateRegistrations,
            Func<object, string> validateInstance = null)
        {
            // Check for circular dependencies
            CheckForCircularDependencies(serviceType);

            // Push the service type onto the stack
            _resolutionStack.Push(serviceType);

            try
            {
                // Get the registrations
                var registrations = getRegistrations();
                
                // Validate registrations
                var registrationsError = validateRegistrations(registrations);
                if (registrationsError != null)
                {
                    throw new InvalidOperationException(registrationsError);
                }

                // Try to get an instance from each registration until one succeeds
                foreach (var registration in registrations)
                {
                    var instance = registration.GetInstance(this, _scope);
                    if (instance != null)
                    {
                        // Validate instance if needed
                        if (validateInstance != null)
                        {
                            var instanceError = validateInstance(instance);
                            if (instanceError != null)
                            {
                                throw new InvalidOperationException(instanceError);
                            }
                        }
                        
                        return instance;
                    }
                }

                // If we get here, no registration could provide an instance
                throw new InvalidOperationException($"Failed to resolve service {serviceType.Name}. All registrations failed to provide an instance.");
            }
            finally
            {
                // Pop the service type from the stack
                _resolutionStack.Pop();
            }
        }

        /// <summary>
        /// Checks for circular dependencies in the resolution stack.
        /// </summary>
        /// <param name="serviceType">The service type to check.</param>
        private void CheckForCircularDependencies(Type serviceType)
        {
            if (_resolutionStack.Contains(serviceType))
            {
                var dependencyChain = string.Join(" -> ", _resolutionStack.Reverse().Select(t => t.Name)) + " -> " + serviceType.Name;
                throw new InvalidOperationException($"Circular dependency detected: {dependencyChain}");
            }
        }
    }

    /// <summary>
    /// Implementation of the <see cref="IScope"/> interface.
    /// </summary>
    internal class Scope : IScope
    {
        private readonly Container _container;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scope"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public Scope(Container container)
        {
            _container = container;
        }

        /// <summary>
        /// Resolves a service from the scope.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved service.</returns>
        public T Resolve<T>() where T : class
        {
            ThrowIfDisposed();
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves a service from the scope.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved service.</returns>
        public object Resolve(Type serviceType)
        {
            ThrowIfDisposed();

            var context = new ResolveContext(_container, this);
            return context.Resolve(serviceType);
        }

        /// <summary>
        /// Tries to resolve a service from the scope.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <param name="service">The resolved service, or null if not registered.</param>
        /// <returns>True if the service was resolved, false otherwise.</returns>
        public bool TryResolve<T>(out T service) where T : class
        {
            ThrowIfDisposed();

            if (TryResolve(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Tries to resolve a service from the scope.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="service">The resolved service, or null if not registered.</param>
        /// <returns>True if the service was resolved, false otherwise.</returns>
        public bool TryResolve(Type serviceType, out object service)
        {
            ThrowIfDisposed();

            try
            {
                service = Resolve(serviceType);
                return true;
            }
            catch
            {
                service = null;
                return false;
            }
        }

        /// <summary>
        /// Resolves all services of the specified type from the scope.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved services.</returns>
        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            ThrowIfDisposed();

            var context = new ResolveContext(_container, this);
            return context.ResolveAll<T>();
        }

        /// <summary>
        /// Resolves all services of the specified type from the scope.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved services.</returns>
        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            ThrowIfDisposed();

            var context = new ResolveContext(_container, this);
            return context.ResolveAll(serviceType);
        }

        /// <summary>
        /// Disposes the scope.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Remove the scope from the container
            _container.RemoveScope(this);
        }

        /// <summary>
        /// Throws an exception if the scope is disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Scope));
            }
        }
    }
}
