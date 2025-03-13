using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomDI
{
    /// <summary>
    /// Implementation of the <see cref="IContainer"/> interface.
    /// </summary>
    public class Container : IContainer
    {
        private readonly ConcurrentDictionary<Type, List<ServiceRegistration>> _registrations = 
            new ConcurrentDictionary<Type, List<ServiceRegistration>>();
        
        private readonly ConcurrentDictionary<string, ServiceRegistration> _namedRegistrations = 
            new ConcurrentDictionary<string, ServiceRegistration>();
        
        private readonly ConcurrentDictionary<KeyValuePair<Type, object>, ServiceRegistration> _keyedRegistrations = 
            new ConcurrentDictionary<KeyValuePair<Type, object>, ServiceRegistration>();
        
        private readonly HashSet<IScope> _scopes = new HashSet<IScope>();
        private readonly object _scopesLock = new object();
        
        private bool _disposed;

        /// <summary>
        /// Registers a service with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            ThrowIfDisposed();
            return Register(typeof(TService), typeof(TImplementation), lifetime);
        }

        /// <summary>
        /// Registers a service with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ThrowIfDisposed();
            
            var registration = new ServiceRegistration(serviceType, implementationType, lifetime);
            AddRegistration(registration);
            
            return new RegistrationBuilder(registration, this);
        }

        /// <summary>
        /// Registers a singleton instance with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder RegisterInstance<TService>(TService instance) where TService : class
        {
            ThrowIfDisposed();
            
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            
            var registration = new ServiceRegistration(typeof(TService), instance);
            AddRegistration(registration);
            
            return new RegistrationBuilder(registration, this);
        }

        /// <summary>
        /// Registers a factory method with the container.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The factory method.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder RegisterFactory<TService>(Func<IResolveContext, TService> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
        {
            ThrowIfDisposed();
            
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            
            var registration = new ServiceRegistration(
                typeof(TService),
                context => factory(context),
                lifetime);
            
            AddRegistration(registration);
            
            return new RegistrationBuilder(registration, this);
        }

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved service.</returns>
        public T Resolve<T>() where T : class
        {
            ThrowIfDisposed();
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved service.</returns>
        public object Resolve(Type serviceType)
        {
            ThrowIfDisposed();
            
            var context = new ResolveContext(this, null);
            return context.Resolve(serviceType);
        }

        /// <summary>
        /// Tries to resolve a service from the container.
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
        /// Tries to resolve a service from the container.
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
        /// Resolves all services of the specified type from the container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>The resolved services.</returns>
        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            ThrowIfDisposed();
            
            var context = new ResolveContext(this, null);
            return context.ResolveAll<T>();
        }

        /// <summary>
        /// Resolves all services of the specified type from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>The resolved services.</returns>
        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            ThrowIfDisposed();
            
            var context = new ResolveContext(this, null);
            return context.ResolveAll(serviceType);
        }

        /// <summary>
        /// Creates a new scope from the container.
        /// </summary>
        /// <returns>A new scope.</returns>
        public IScope CreateScope()
        {
            ThrowIfDisposed();
            
            var scope = new Scope(this);
            
            lock (_scopesLock)
            {
                _scopes.Add(scope);
            }
            
            return scope;
        }

        /// <summary>
        /// Checks if a service is registered with the container.
        /// </summary>
        /// <typeparam name="T">The service type to check.</typeparam>
        /// <returns>True if the service is registered, false otherwise.</returns>
        public bool IsRegistered<T>() where T : class
        {
            ThrowIfDisposed();
            return IsRegistered(typeof(T));
        }

        /// <summary>
        /// Checks if a service is registered with the container.
        /// </summary>
        /// <param name="serviceType">The service type to check.</param>
        /// <returns>True if the service is registered, false otherwise.</returns>
        public bool IsRegistered(Type serviceType)
        {
            ThrowIfDisposed();
            return _registrations.ContainsKey(serviceType);
        }

        /// <summary>
        /// Disposes the container and all scoped instances.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            
            _disposed = true;
            
            // Dispose all scopes
            lock (_scopesLock)
            {
                foreach (var scope in _scopes)
                {
                    scope.Dispose();
                }
                
                _scopes.Clear();
            }
            
            // Clear registrations
            _registrations.Clear();
            _namedRegistrations.Clear();
            _keyedRegistrations.Clear();
        }

        /// <summary>
        /// Adds a registration to the container.
        /// </summary>
        /// <param name="registration">The registration to add.</param>
        internal void AddRegistration(ServiceRegistration registration)
        {
            // Add to main registrations
            _registrations.AddOrUpdate(
                registration.ServiceType,
                new List<ServiceRegistration> { registration },
                (_, list) =>
                {
                    list.Add(registration);
                    return list;
                });
            
            // Add to named registrations if applicable
            if (!string.IsNullOrEmpty(registration.Name))
            {
                var key = GetNamedRegistrationKey(registration.ServiceType, registration.Name);
                _namedRegistrations[key] = registration;
            }
            
            // Add to keyed registrations if applicable
            if (registration.Key != null)
            {
                var key = new KeyValuePair<Type, object>(registration.ServiceType, registration.Key);
                _keyedRegistrations[key] = registration;
            }
        }

        /// <summary>
        /// Gets a registration for the specified service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The registration, or null if not found.</returns>
        internal ServiceRegistration GetRegistration(Type serviceType)
        {
            if (_registrations.TryGetValue(serviceType, out var registrations) && registrations.Count > 0)
            {
                return registrations[0];
            }
            
            return null;
        }

        /// <summary>
        /// Gets a named registration for the specified service type and name.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="name">The name.</param>
        /// <returns>The registration, or null if not found.</returns>
        internal ServiceRegistration GetNamedRegistration(Type serviceType, string name)
        {
            var key = GetNamedRegistrationKey(serviceType, name);
            
            if (_namedRegistrations.TryGetValue(key, out var registration))
            {
                return registration;
            }
            
            return null;
        }

        /// <summary>
        /// Gets a keyed registration for the specified service type and key.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="key">The key.</param>
        /// <returns>The registration, or null if not found.</returns>
        internal ServiceRegistration GetKeyedRegistration(Type serviceType, object key)
        {
            var keyPair = new KeyValuePair<Type, object>(serviceType, key);
            
            if (_keyedRegistrations.TryGetValue(keyPair, out var registration))
            {
                return registration;
            }
            
            return null;
        }

        /// <summary>
        /// Gets all registrations for the specified service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The registrations.</returns>
        internal IEnumerable<ServiceRegistration> GetAllRegistrations(Type serviceType)
        {
            if (_registrations.TryGetValue(serviceType, out var registrations))
            {
                return registrations;
            }
            
            return Enumerable.Empty<ServiceRegistration>();
        }

        /// <summary>
        /// Removes a scope from the container.
        /// </summary>
        /// <param name="scope">The scope to remove.</param>
        internal void RemoveScope(IScope scope)
        {
            lock (_scopesLock)
            {
                _scopes.Remove(scope);
            }
        }

        /// <summary>
        /// Gets the key for a named registration.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="name">The name.</param>
        /// <returns>The key.</returns>
        private string GetNamedRegistrationKey(Type serviceType, string name)
        {
            return $"{serviceType.FullName}:{name}";
        }

        /// <summary>
        /// Throws an exception if the container is disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Container));
            }
        }
    }

    /// <summary>
    /// Factory class for creating containers.
    /// </summary>
    public static class ContainerFactory
    {
        /// <summary>
        /// Creates a new container.
        /// </summary>
        /// <returns>A new container.</returns>
        public static IContainer CreateContainer()
        {
            return new Container();
        }
    }

    //public class Scope : IScope
    //{
    //    public void Dispose()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public T Resolve<T>() where T : class
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object Resolve(Type serviceType)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public IEnumerable<T> ResolveAll<T>() where T : class
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public IEnumerable<object> ResolveAll(Type serviceType)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool TryResolve<T>(out T service) where T : class
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool TryResolve(Type serviceType, out object service)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
