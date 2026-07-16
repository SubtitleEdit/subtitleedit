using System;
using System.Collections.Generic;

namespace AvaloniaEdit.Utils
{
    public interface IServiceContainer : IServiceProvider
    {
        void AddService(Type serviceType, object serviceInstance);

        void RemoveService(Type serviceType);
    }

    public static class ServiceExtensions
    {
        public static T GetService<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetService(typeof(T)) as T;
        }

        public static void AddService<T>(this IServiceContainer container, T serviceInstance)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            container.AddService(typeof(T), serviceInstance);
        }

        public static void RemoveService<T>(this IServiceContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            container.RemoveService(typeof(T));
        }
    }

    internal class ServiceContainer : IServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public ServiceContainer()
        {
            _services.Add(typeof(IServiceProvider), this);
            _services.Add(typeof(IServiceContainer), this);
        }

        public object GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return service;
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            _services[serviceType] = serviceInstance;
        }

        public void RemoveService(Type serviceType)
        {
            _services.Remove(serviceType);
        }
    }
}