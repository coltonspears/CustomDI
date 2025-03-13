using System;

namespace CustomDI
{
    /// <summary>
    /// Implementation of the <see cref="IRegistrationBuilder"/> interface.
    /// </summary>
    internal class RegistrationBuilder : IRegistrationBuilder
    {
        private readonly ServiceRegistration _registration;
        private readonly Container _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationBuilder"/> class.
        /// </summary>
        /// <param name="registration">The service registration to configure.</param>
        /// <param name="container">The container to update when configuration changes.</param>
        public RegistrationBuilder(ServiceRegistration registration, Container container)
        {
            _registration = registration;
            _container = container;
        }

        /// <summary>
        /// Configures the service to be resolved with a specific name.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder Named(string name)
        {
            _registration.Name = name;
            _container.AddRegistration(_registration);
            return this;
        }

        /// <summary>
        /// Configures the service to be resolved with a specific key.
        /// </summary>
        /// <param name="key">The key of the service.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder Keyed(object key)
        {
            _registration.Key = key;
            _container.AddRegistration(_registration);
            return this;
        }

        /// <summary>
        /// Configures the service to be resolved only when a condition is met.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder When(Func<IResolveContext, bool> condition)
        {
            _registration.Condition = condition;
            return this;
        }

        /// <summary>
        /// Configures the service to be resolved with specific constructor parameters.
        /// </summary>
        /// <param name="parameters">The constructor parameters.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder WithParameters(params Parameter[] parameters)
        {
            _registration.Parameters.AddRange(parameters);
            return this;
        }

        /// <summary>
        /// Configures the service to be resolved with specific property values.
        /// </summary>
        /// <param name="propertyValues">The property values.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder WithProperties(params PropertyValue[] propertyValues)
        {
            _registration.PropertyValues.AddRange(propertyValues);
            return this;
        }

        /// <summary>
        /// Configures the service to be initialized after construction.
        /// </summary>
        /// <param name="initializer">The initializer action.</param>
        /// <returns>The registration builder for further configuration.</returns>
        public IRegistrationBuilder OnActivated(Action<IActivatedEventArgs> initializer)
        {
            _registration.Initializers.Add(initializer);
            return this;
        }
    }

    /// <summary>
    /// Implementation of the <see cref="IActivatedEventArgs"/> interface.
    /// </summary>
    internal class ActivatedEventArgs : IActivatedEventArgs
    {
        /// <summary>
        /// Gets the instance that was activated.
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// Gets the context that was used to resolve the instance.
        /// </summary>
        public IResolveContext Context { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatedEventArgs"/> class.
        /// </summary>
        /// <param name="instance">The instance that was activated.</param>
        /// <param name="context">The context that was used to resolve the instance.</param>
        public ActivatedEventArgs(object instance, IResolveContext context)
        {
            Instance = instance;
            Context = context;
        }
    }
}
