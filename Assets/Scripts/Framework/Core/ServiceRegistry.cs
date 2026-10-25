using System;
using System.Collections.Generic;

namespace Framework.Core
{
    /// <summary>按接口类型注册/获取服务，Gameplay 只依赖这里。</summary>
    public class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (!_services.TryAdd(type, service))
                throw new InvalidOperationException($"[ServiceRegistry] 服务已注册: {type.Name}");
        }

        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException($"[ServiceRegistry] 服务未注册: {typeof(T).Name}");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }
    }
}