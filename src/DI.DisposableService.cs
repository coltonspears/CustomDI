using System;
using System.Collections.Generic;

namespace CustomDI
{
    /// <summary>
    /// Provides support for automatic disposal of IDisposable services.
    /// </summary>
    internal class DisposableTracker
    {
        private readonly Dictionary<object, ServiceLifetime> _disposables = new Dictionary<object, ServiceLifetime>();
        private readonly object _lock = new object();

        /// <summary>
        /// Tracks a disposable service for later disposal.
        /// </summary>
        /// <param name="instance">The service instance to track.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        public void TrackDisposable(object instance, ServiceLifetime lifetime)
        {
            if (instance is IDisposable disposable)
            {
                lock (_lock)
                {
                    if (!_disposables.ContainsKey(instance))
                    {
                        _disposables.Add(instance, lifetime);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes all tracked services of the specified lifetime.
        /// </summary>
        /// <param name="lifetime">The lifetime of services to dispose.</param>
        public void DisposeServices(ServiceLifetime lifetime)
        {
            List<object> toDispose = new List<object>();
            
            lock (_lock)
            {
                foreach (var kvp in _disposables)
                {
                    if (kvp.Value == lifetime)
                    {
                        toDispose.Add(kvp.Key);
                    }
                }
                
                foreach (var instance in toDispose)
                {
                    _disposables.Remove(instance);
                }
            }
            
            foreach (var instance in toDispose)
            {
                try
                {
                    ((IDisposable)instance).Dispose();
                }
                catch (Exception)
                {
                    // Ignore exceptions during disposal
                }
            }
        }

        /// <summary>
        /// Disposes all tracked services.
        /// </summary>
        public void DisposeAllServices()
        {
            List<object> toDispose = new List<object>();
            
            lock (_lock)
            {
                toDispose.AddRange(_disposables.Keys);
                _disposables.Clear();
            }
            
            foreach (var instance in toDispose)
            {
                try
                {
                    ((IDisposable)instance).Dispose();
                }
                catch (Exception)
                {
                    // Ignore exceptions during disposal
                }
            }
        }
    }
}
