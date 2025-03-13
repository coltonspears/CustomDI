using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomDI.Factory
{
    /// <summary>
    /// Provides factory support for creating instances with dependencies.
    /// </summary>
    public static class FactoryExtensions
    {
        /// <summary>
        /// Registers a factory for creating instances of a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TFactory">The factory type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterFactory<TService, TFactory>(this IContainer container)
            where TService : class
            where TFactory : class, IFactory<TService>
        {
            // Register the factory
            container.Register<IFactory<TService>, TFactory>(ServiceLifetime.Singleton);
            
            // Register the service as a factory resolution
            return container.RegisterFactory<TService>(context => 
                context.Resolve<IFactory<TService>>().Create());
        }

        /// <summary>
        /// Registers a generic factory for creating instances of a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterGenericFactory<TService>(this IContainer container)
            where TService : class
        {
            // Register the factory
            container.RegisterInstance<IFactory<TService>>(new DelegateFactory<TService>(
                () => container.Resolve<TService>()));
            
            return container.RegisterFactory<Func<TService>>(context => 
                () => context.Resolve<TService>());
        }

        /// <summary>
        /// Registers a parameterized factory for creating instances of a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TParam">The parameter type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="factory">The factory function.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterParameterizedFactory<TService, TParam>(
            this IContainer container, 
            Func<TParam, TService> factory)
            where TService : class
        {
            // Register the factory
            container.RegisterInstance<IParameterizedFactory<TService, TParam>>(
                new DelegateParameterizedFactory<TService, TParam>(factory));
            
            return container.RegisterFactory<Func<TParam, TService>>(context => factory);
        }

        /// <summary>
        /// Registers a factory for creating multiple instances of a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterFactoryForAll<TService>(this IContainer container)
            where TService : class
        {
            // Register the factory
            container.RegisterInstance<IFactoryForAll<TService>>(new DelegateFactoryForAll<TService>(
                () => container.ResolveAll<TService>()));
            
            return container.RegisterFactory<Func<IEnumerable<TService>>>(context => 
                () => context.ResolveAll<TService>());
        }

        /// <summary>
        /// Registers a keyed factory for creating instances of a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterKeyedFactory<TService>(this IContainer container)
            where TService : class
        {
            // Register the factory
            container.RegisterInstance<IKeyedFactory<TService>>(new DelegateKeyedFactory<TService>(
                key => {
                    var context = new ResolveContext((Container)container, null);
                    return context.ResolveKeyed<TService>(key);
                }));
            
            return container.RegisterFactory<Func<object, TService>>(context => 
                key => {
                    var innerContext = new ResolveContext((Container)container, null);
                    return innerContext.ResolveKeyed<TService>(key);
                });
        }

        /// <summary>
        /// Registers a named factory for creating instances of a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public static IRegistrationBuilder RegisterNamedFactory<TService>(this IContainer container)
            where TService : class
        {
            // Register the factory
            container.RegisterInstance<INamedFactory<TService>>(new DelegateNamedFactory<TService>(
                name => {
                    var context = new ResolveContext((Container)container, null);
                    return context.ResolveNamed<TService>(name);
                }));
            
            return container.RegisterFactory<Func<string, TService>>(context => 
                name => {
                    var innerContext = new ResolveContext((Container)container, null);
                    return innerContext.ResolveNamed<TService>(name);
                });
        }
    }

    #region Factory Interfaces

    /// <summary>
    /// Represents a factory for creating instances of a service.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    public interface IFactory<out T>
    {
        /// <summary>
        /// Creates an instance of the service.
        /// </summary>
        /// <returns>The created instance.</returns>
        T Create();
    }

    /// <summary>
    /// Represents a factory for creating instances of a service with a parameter.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <typeparam name="TParam">The parameter type.</typeparam>
    public interface IParameterizedFactory<out T, in TParam>
    {
        /// <summary>
        /// Creates an instance of the service with the specified parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>The created instance.</returns>
        T Create(TParam param);
    }

    /// <summary>
    /// Represents a factory for creating multiple instances of a service.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    public interface IFactoryForAll<out T>
    {
        /// <summary>
        /// Creates multiple instances of the service.
        /// </summary>
        /// <returns>The created instances.</returns>
        IEnumerable<T> CreateAll();
    }

    /// <summary>
    /// Represents a factory for creating instances of a service by key.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    public interface IKeyedFactory<out T>
    {
        /// <summary>
        /// Creates an instance of the service with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The created instance.</returns>
        T CreateKeyed(object key);
    }

    /// <summary>
    /// Represents a factory for creating instances of a service by name.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    public interface INamedFactory<out T>
    {
        /// <summary>
        /// Creates an instance of the service with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The created instance.</returns>
        T CreateNamed(string name);
    }

    #endregion

    #region Factory Implementations

    /// <summary>
    /// Implements a factory using a delegate.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    internal class DelegateFactory<T> : IFactory<T>
    {
        private readonly Func<T> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateFactory{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory function.</param>
        public DelegateFactory(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates an instance of the service.
        /// </summary>
        /// <returns>The created instance.</returns>
        public T Create()
        {
            return _factory();
        }
    }

    /// <summary>
    /// Implements a parameterized factory using a delegate.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <typeparam name="TParam">The parameter type.</typeparam>
    internal class DelegateParameterizedFactory<T, TParam> : IParameterizedFactory<T, TParam>
    {
        private readonly Func<TParam, T> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateParameterizedFactory{T, TParam}"/> class.
        /// </summary>
        /// <param name="factory">The factory function.</param>
        public DelegateParameterizedFactory(Func<TParam, T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates an instance of the service with the specified parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>The created instance.</returns>
        public T Create(TParam param)
        {
            return _factory(param);
        }
    }

    /// <summary>
    /// Implements a factory for creating multiple instances using a delegate.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    internal class DelegateFactoryForAll<T> : IFactoryForAll<T>
    {
        private readonly Func<IEnumerable<T>> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateFactoryForAll{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory function.</param>
        public DelegateFactoryForAll(Func<IEnumerable<T>> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates multiple instances of the service.
        /// </summary>
        /// <returns>The created instances.</returns>
        public IEnumerable<T> CreateAll()
        {
            return _factory();
        }
    }

    /// <summary>
    /// Implements a factory for creating instances by key using a delegate.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    internal class DelegateKeyedFactory<T> : IKeyedFactory<T>
    {
        private readonly Func<object, T> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateKeyedFactory{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory function.</param>
        public DelegateKeyedFactory(Func<object, T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates an instance of the service with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The created instance.</returns>
        public T CreateKeyed(object key)
        {
            return _factory(key);
        }
    }

    /// <summary>
    /// Implements a factory for creating instances by name using a delegate.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    internal class DelegateNamedFactory<T> : INamedFactory<T>
    {
        private readonly Func<string, T> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateNamedFactory{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory function.</param>
        public DelegateNamedFactory(Func<string, T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates an instance of the service with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The created instance.</returns>
        public T CreateNamed(string name)
        {
            return _factory(name);
        }
    }

    #endregion
}
