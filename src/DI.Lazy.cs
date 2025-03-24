using System;

namespace CustomDI
{
    /// <summary>
    /// Extension methods for lazy resolution of services.
    /// </summary>
    public static class LazyExtensions
    {
        /// <summary>
        /// Registers a lazy factory for a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterLazy<TService>(this IContainer container)
            where TService : class
        {
            return container.RegisterFactory<Lazy<TService>>(context => 
                new Lazy<TService>(() => context.Resolve<TService>()));
        }

        /// <summary>
        /// Registers a lazy factory for a named service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="name">The name of the service.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterLazyNamed<TService>(this IContainer container, string name)
            where TService : class
        {
            return container.RegisterFactory<Lazy<TService>>(context => 
                new Lazy<TService>(() => context.ResolveNamed<TService>(name)));
        }

        /// <summary>
        /// Registers a lazy factory for a keyed service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key of the service.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder RegisterLazyKeyed<TService>(this IContainer container, object key)
            where TService : class
        {
            return container.RegisterFactory<Lazy<TService>>(context => 
                new Lazy<TService>(() => context.ResolveKeyed<TService>(key)));
        }
    }
}
