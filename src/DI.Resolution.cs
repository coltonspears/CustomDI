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
            // Check for circular dependencies
            if (_resolutionStack.Contains(serviceType))
            {
                var dependencyChain = string.Join(" -> ", _resolutionStack.Reverse().Select(t => t.Name)) + " -> " + serviceType.Name;
                throw new InvalidOperationException($"Circular dependency detected: {dependencyChain}");
            }

            // Push the service type onto the stack
            _resolutionStack.Push(serviceType);

            try
            {
                // Get the registration
                var registration = _container.GetRegistration(serviceType);
                if (registration == null)
                {
                    throw new InvalidOperationException($"No registration found for service {serviceType.Name}");
                }

                // Get an instance from the registration
                return registration.GetInstance(this, _scope);
            }
            finally
            {
                // Pop the service type from the stack
                _resolutionStack.Pop();
            }
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
            // Check for circular dependencies
            if (_resolutionStack.Contains(serviceType))
            {
                var dependencyChain = string.Join(" -> ", _resolutionStack.Reverse().Select(t => t.Name)) + " -> " + serviceType.Name;
                throw new InvalidOperationException($"Circular dependency detected: {dependencyChain}");
            }

            // Push the service type onto the stack
            _resolutionStack.Push(serviceType);

            try
            {
                // Get the registration
                var registration = _container.GetNamedRegistration(serviceType, name);
                if (registration == null)
                {
                    throw new InvalidOperationException($"No registration found for service {serviceType.Name} with name '{name}'");
                }

                // Get an instance from the registration
                return registration.GetInstance(this, _scope);
            }
            finally
            {
                // Pop the service type from the stack
                _resolutionStack.Pop();
            }
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
            // Check for circular dependencies
            if (_resolutionStack.Contains(serviceType))
            {
                var dependencyChain = string.Join(" -> ", _resolutionStack.Reverse().Select(t => t.Name)) + " -> " + serviceType.Name;
                throw new InvalidOperationException($"Circular dependency detected: {dependencyChain}");
            }

            // Push the service type onto the stack
            _resolutionStack.Push(serviceType);

            try
            {
                // Get the registration
                var registration = _container.GetKeyedRegistration(serviceType, key);
                if (registration == null)
                {
                    throw new InvalidOperationException($"No registration found for service {serviceType.Name} with key '{key}'");
                }

                // Get an instance from the registration
                return registration.GetInstance(this, _scope);
            }
            finally
            {
                // Pop the service type from the stack
                _resolutionStack.Pop();
            }
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
            if (_resolutionStack.Contains(serviceType))
            {
                var dependencyChain = string.Join(" -> ", _resolutionStack.Reverse().Select(t => t.Name)) + " -> " + serviceType.Name;
                throw new InvalidOperationException($"Circular dependency detected: {dependencyChain}");
            }

            // Push the service type onto the stack
            _resolutionStack.Push(serviceType);

            try
            {
                // Get all registrations
                var registrations = _container.GetAllRegistrations(serviceType);
                
                // Get an instance from each registration
                var instances = new List<object>();
                foreach (var registration in registrations)
                {
                    instances.Add(registration.GetInstance(this, _scope));
                }
                
                return instances;
            }
            finally
            {
                // Pop the service type from the stack
                _resolutionStack.Pop();
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
