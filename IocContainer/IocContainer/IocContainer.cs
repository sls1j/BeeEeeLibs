using BeeEeeLibs.DependencyInjection.InnerWorkings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeeEeeLibs.DependencyInjection
{
  /// <summary>
  /// Ioc Container.  Used to register services, functions, values, and executors
  /// </summary>
  public class IocContainer
  {
    private List<ServiceDefinition> _services;
    private List<FunctionDefinition> _functions;
    private List<ValueDefintion> _values;
    private List<ExecutorDefinition> _executors;

    /// <summary>
    /// The constructor
    /// </summary>
    public IocContainer()
    {
      _services = new List<ServiceDefinition>();
      _functions = new List<FunctionDefinition>();
      _values = new List<ValueDefintion>();
      _executors = new List<ExecutorDefinition>();
    }

    /// <summary>
    /// Adds a service to the container
    /// </summary>
    /// <param name="service">The service defintion</param>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="ArgumentException">Thrown if the service is missing a key of factory</exception>
    /// <exception cref="InvalidOperationException">Thrown if a service with a matching key is already registered</exception>
    public void AddService(ServiceDefinition service)
    {
      if (service == null) throw new ArgumentNullException(nameof(service));
      if (service.Key == null || service.Factory == null) throw new ArgumentException($"Key or Factory not defined");
      if (_services.Any(s => s.Key == service.Key))
        throw new InvalidOperationException($"Service {service.Key.FullName} is already added");

      _services.Add(service);
    }

    /// <summary>
    /// Add a function to the container that can be injected into a service
    /// </summary>
    /// <param name="key">The key or name of the function.  If get this to inject the parameter must match this key</param>
    /// <param name="function">The function delegate</param>
    /// <exception cref="ArgumentNullException">If the function or key are null this is thrown</exception>
    /// <exception cref="InvalidOperationException">If a function with the same key already exists</exception>
    public void AddFunction(string key, Delegate function)
    {
      if (function == null) throw new ArgumentNullException(nameof(function));
      if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
      if (_functions.Any(f => f.Key == key))
        throw new InvalidOperationException($"Function {key} is already defined");

      _functions.Add(new FunctionDefinition() { Key = key, Function = function });
    }

    /// <summary>
    /// Add a value that can be injected into a service
    /// </summary>
    /// <param name="key">The key to value</param>
    /// <param name="value">The value to inject</param>
    /// <exception cref="ArgumentNullException">If the key is null then this is thrown</exception>
    /// <exception cref="InvalidOperationException">If a value already exists with a given key</exception>
    public void AddValue(string key, object value)
    {
      if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("Key not defined correctly");
      if (_values.Any(v => v.Key == key))
        throw new InvalidOperationException($"Value {key} is already defined");

      _values.Add(new ValueDefintion() { Key = key, Value = value });
    }

    /// <summary>
    /// Inserts an exectuor, that can be executed and have it's parameters filled in with injection
    /// </summary>
    /// <param name="executor">The definition of the executor function</param>
    /// <exception cref="ArgumentNullException">If the KeyFunction is null or the executer is null</exception>
    public void AddExecutor(ExecutorDefinition executor)
    {
      if (executor == null) throw new ArgumentNullException(nameof(executor));
      if (string.IsNullOrWhiteSpace(executor.KeyFunction)) throw new ArgumentNullException(nameof(executor.KeyFunction));

      _executors.Add(executor);
    }

    /// <summary>
    /// Builds the provider from the container.
    /// </summary>
    /// <returns>The provider</returns>
    public IocProvider BuildProvider()
    {
      return new IocProvider(_services.ToArray(), _functions.ToArray(), _values.ToArray(), _executors.ToArray());
    }
  }
}