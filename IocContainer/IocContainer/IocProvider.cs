using BeeEeeLibs.DependencyInjection.InnerWorkings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeEeeLibs.DependencyInjection
{
    /// <summary>
    /// If Single then one the service is created the result is cached.  If Multi then the service will be created everytime it is injected.
    /// </summary>
    public enum ServiceLife
    {
        /// <summary>
        /// A singleton life cycle.  Only one object will be created and that instance will be used globably by the provider
        /// </summary>
        Single,
        /// <summary>
        /// For each injection a new service will be created
        /// </summary>
        Multi
    }

    /// <summary>
    /// The inversion of control provider.  This builds the instances and performs the injection
    /// </summary>
    public class IocProvider : IDisposable
    {
        /// <summary>
        /// The registered services
        /// </summary>
        private ServiceDefinition[] services;

        /// <summary>
        /// The registered functions
        /// </summary>
        private FunctionDefinition[] functions;

        /// <summary>
        /// The registered values
        /// </summary>
        private ValueDefintion[] values;

        /// <summary>
        /// The registered executors
        /// </summary>
        private ExecutorDefinition[] executors;

        /// <summary>
        /// The cache of services
        /// </summary>
        private Dictionary<Type, object> serviceCache = new Dictionary<Type, object>();

        /// <summary>
        /// If true then the provider has been disposed.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="services">The registered services</param>
        /// <param name="functions">The registered functions</param>
        /// <param name="values">The registered values</param>
        /// <param name="executors">The registered executors</param>
        internal IocProvider(ServiceDefinition[] services, FunctionDefinition[] functions, ValueDefintion[] values, ExecutorDefinition[] executors)
        {
            this.services = services;
            this.functions = functions;
            this.values = values;
            this.executors = executors;
        }

        /// <summary>
        /// Get a matching service
        /// </summary>
        /// <returns>The service</returns>
        public object GetService(Type type)
        {
            ServiceDefinition serviceDefinition;
            try
            {
                serviceDefinition = services.First(s => s.Key == type);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to find service type {type.Name}", ex);
            }

            if (serviceDefinition.Factory == null)
                throw new InvalidOperationException($"Service definition for {type.Name} does not have a factory.");

            object? service;
            if (serviceDefinition.Life == ServiceLife.Multi || serviceCache.TryGetValue(type, out service) == false)
            {
                service = serviceDefinition.Factory(this);
                if (service == null)
                    throw new InvalidOperationException($"Service definition for {type.Name} failed to create the service.");
                if (serviceDefinition.PostConstructor != null)
                {
                    serviceDefinition.PostConstructor(this, service);
                }

                if (serviceDefinition.Life == ServiceLife.Single)
                {
                    serviceCache.Add(type, service);
                }
            }
            return service;
        }

        /// <summary>
        /// Get a matching service
        /// </summary>
        /// <typeparam name="T">The base class</typeparam>
        /// <returns>The casted service</returns>
        public T GetService<T>() where T : class => (T)GetService(typeof(T));

        /// <summary>
        /// Gets all of the services with a matching base class
        /// </summary>
        /// <returns>The array of services</returns>

        public object?[] GetServicesByBase(Type type)
        {
            return services
                .Where(s => s.Key.IsSubclassOf(type))
                .Select(s =>
                {
                    if (s.Factory == null) throw new NullReferenceException($"service factory for type {type.Name} is null");
                    object? service = s.Factory(this) ?? throw new InvalidOperationException($"Unable to get service by base for {type.Name}.  Factory returned null");

                    if (s.PostConstructor != null)
                        s.PostConstructor(this, service);
                    return service;
                }).ToArray();
        }

        /// <summary>
        /// Gets all of the services with a matching base class
        /// </summary>
        /// <typeparam name="T">The base class</typeparam>
        /// <returns>The array of casted services</returns>
        public T[] GetServicesByBase<T>() where T : class
        {
            return GetServicesByBase(typeof(T)).Cast<T>().ToArray();
        }


        /// <summary>
        /// Gets the function by it's key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The delegate</returns>
        public Delegate GetFunction(string key)
        {
            FunctionDefinition functionDefiniton;
            try
            {
                functionDefiniton = functions.First(f => f.Key == key);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to find function {key}", ex);
            }
            Delegate function = functionDefiniton.Function;
            return function;
        }

        /// <summary>
        /// Gets the function by it's key
        /// </summary>
        /// <typeparam name="TFunc">The type of the function</typeparam>
        /// <param name="key">The key</param>
        /// <returns>The casted delegate</returns>
        public TFunc GetFunction<TFunc>(string key) where TFunc : Delegate
        {
            return (TFunc)GetFunction(key);
        }

        /// <summary>
        /// Gets the value from the key
        /// </summary>
        /// <param name="Key">The key of the value</param>
        /// <returns>The matching value</returns>
        public object GetValue(string Key)
        {
            return values.First(v => v.Key == Key).Value;
        }

        /// <summary>
        /// Retrieves the value
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="Key">The key of the value</param>
        /// <returns>The value</returns>
        public T GetValue<T>(string Key)
        {
            return (T)GetValue(Key);
        }

        /// <summary>
        /// Searches through the executers using the predicate and executes the first on encountered.
        /// </summary>
        /// <param name="pred">The searching predicate</param>
        /// <returns>The result of the execution</returns>
        public (bool, object?) ExecuteFirst(Predicate<ExecutorDefinition> pred)
        {
            for (int i = 0; i < executors.Length; i++)
            {
                var executor = executors[i];
                if (pred(executor))
                    return (true, executor.Factory(this));
            }
            return (false, null);
        }

        /// <summary>
        /// Runs all of the executors that match the predicate
        /// </summary>
        /// <param name="pred">The predicate that matches the executor</param>
        /// <returns>The result of the execution</returns>
        public object?[] ExecuteAll(Predicate<ExecutorDefinition> pred)
        {
            var result = new List<object?>();
            for (int i = 0; i < executors.Length; i++)
            {
                var executor = executors[i];
                if (pred(executor))
                    result.Add(executor.Factory(this));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Runs all of the executors that match the predicate
        /// </summary>
        /// <typeparam name="TElement">The expected result of the executor</typeparam>
        /// <param name="pred">The predicate that matches the executor</param>
        /// <returns>The result of the execution</returns>
        public TElement[] ExecuteAll<TElement>(Predicate<ExecutorDefinition> pred)
        {
            return ExecuteAll(pred).Cast<TElement>().ToArray();
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var oldCache = serviceCache;
                    oldCache.Values.OfType<IDisposable>().ToList().ForEach(v => v.Dispose());
                    oldCache.Clear();
                }
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
