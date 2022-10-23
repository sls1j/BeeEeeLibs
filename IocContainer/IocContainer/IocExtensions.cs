using BeeEeeLibs.DependencyInjection.InnerWorkings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeeEeeLibs.DependencyInjection
{
  /// <summary>
  /// Provides standard ways of adding services and executors.
  /// </summary>
  public static class IocExtensions
  {
    /// <summary>
    /// Adds a service allowing to manually set the key, life, injection factory and post construction step
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="key">The key type of the service.  This could be an interface or base class</param>
    /// <param name="constructionType">The type that will be constructed</param>
    /// <param name="life">The life model of the service</param>
    /// <param name="buildInjectorFactory">The injection method</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddService(this IocContainer container, Type key, Type constructionType, ServiceLife life, BuildInjectorDelegate buildInjectorFactory, Action<IocProvider, object>? postConstructor)
    {
      ServiceDefinition definiton = new ServiceDefinition()
      {
        Key = key,
        Factory = buildInjectorFactory(constructionType),
        PostConstructor = postConstructor,
        Life = life
      };

      container.AddService(definiton);

      return container;
    }

    /// <summary>
    /// Add a service with an inter
    /// </summary>
    /// <param name="container"></param>
    /// <param name="key"></param>
    /// <param name="life"></param>
    /// <param name="buildInjectorFactory"></param>
    /// <param name="postConstructor"></param>
    /// <returns></returns>
    public static IocContainer AddService(this IocContainer container, Type key, ServiceLife life, BuildInjectorDelegate buildInjectorFactory, Action<IocProvider, object>? postConstructor)
    {
      return AddService(container, key, key, life, buildInjectorFactory, postConstructor);
    }

    /// <summary>
    /// Add a service for a given base type
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="baseType">The base type to add as</param>
    /// <param name="key">The key type of the service</param>
    /// <param name="life">The life model of the service</param>
    /// <param name="buildInjectorFactory">The injection method</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServiceAsBaseType(this IocContainer container, Type baseType, Type key, ServiceLife life, BuildInjectorDelegate buildInjectorFactory, Action<IocProvider, object>? postConstructor)
    {
      ServiceDefinition definition = new ServiceDefinition()
      {
        Key = baseType,
        Factory = buildInjectorFactory(key),
        PostConstructor = postConstructor,
        Life = life
      };

      container.AddService(definition);

      return container;
    }
    /// <summary>
    /// Adds a service with constructor injection.
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="key">The type to add</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServiceInjectConstructor(this IocContainer container, Type key, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      return container.AddService(key, life, IocFactories.BuildConstructorInject, postConstructor);
    }

    /// <summary>
    /// Adds a service with constructor ingestion for a given base type
    /// </summary>
    /// <param name="container"></param>
    /// <param name="key"></param>
    /// <param name="instanceType"></param>
    /// <param name="life"></param>
    /// <param name="postConstructor"></param>
    /// <returns></returns>
    public static IocContainer AddServiceInjectConstructor(this IocContainer container, Type key, Type instanceType, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      return container.AddService(key, instanceType, life, IocFactories.BuildConstructorInject, postConstructor);
    }

    /// <summary>
    /// Adds a service with constructor ingestion for a given base type
    /// </summary>
    /// <typeparam name="TBase"></typeparam>
    /// <typeparam name="TImpl"></typeparam>
    /// <param name="container"></param>
    /// <param name="life"></param>
    /// <param name="postConstructor"></param>
    /// <returns></returns>
    public static IocContainer AddServiceInjectConstructor<TBase,TImpl>(this IocContainer container, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      return container.AddService(typeof(TBase), typeof(TImpl), life, IocFactories.BuildConstructorInject, postConstructor);
    }

    /// <summary>
    /// Adds a service with constructor injection.
    /// </summary>
    /// <typeparam name="TType">The type key</typeparam>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServiceInjectConstructor<TType>(this IocContainer container, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      return container.AddServiceInjectConstructor(typeof(TType), life, postConstructor);
    }

    /// <summary>
    /// Adds many services with constructor injection by a matching base type.  Uses the calling assembly to enumerate the types
    /// </summary>
    /// <typeparam name="TType">The type key</typeparam>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServicesInjectConstructorByBaseType<TType>(this IocContainer container, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      return container.AddServicesInjectConstructorByBaseType(typeof(TType), life, postConstructor);
    }

    /// <summary>
    /// Adds many services with constructor injection by a matching base type.  Uses the calling assembly to enumerate the types
    /// </summary>
    /// <param name="baseType">The base type to look for</param>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServicesInjectConstructorByBaseType(this IocContainer container, Type baseType, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      foreach (var type in baseType.Assembly.DefinedTypes.Where(t => t.IsSubclassOf(baseType)))
      {
        container.AddServiceInjectConstructor(type, life, postConstructor);
      }

      return container;
    }

    /// <summary>
    /// Adds a service with property injection.
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="key">The type to add</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServiceInjectProperties(this IocContainer container, Type key, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      return container.AddService(key, life, IocFactories.BuildPropertyInject, postConstructor);
    }

    /// <summary>
    /// Adds a service with property injection.
    /// </summary>
    /// <typeparam name="TKey">The type key</typeparam>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServiceInjectProperties<TKey>(this IocContainer container, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null) where TKey : class
    {
      return container.AddServiceInjectProperties(typeof(TKey), life, postConstructor);
    }

    /// <summary>
    /// Adds many services with property injection by a matching base type.  Uses the calling assembly to enumerate the types
    /// </summary>
    /// <param name="key">The base type key</param>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServicesInjectPropertiesByBaseType(this IocContainer container, Type key, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      foreach (var type in key.Assembly.DefinedTypes.Where(t => t.IsSubclassOf(key)))
      {
        container.AddServiceInjectProperties(type, life, postConstructor);
      }

      return container;
    }

    /// <summary>
    /// Adds many services with property injection by a matching base type.  Uses the calling assembly to enumerate the types
    /// </summary>
    /// <typeparam name="TKey">The base type key</typeparam>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServicesInjectProperties<TKey>(this IocContainer container, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null) where TKey : class
    {
      return container.AddServicesInjectPropertiesByBaseType(typeof(TKey), life, postConstructor);
    }

    /// <summary>
    /// Adds a service with field injection.
    /// </summary>
    /// <param name="key">The type key</param>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServiceInjectFields(this IocContainer container, Type key, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      return container.AddService(key, life, IocFactories.BuildFieldInject, postConstructor);
    }

    /// <summary>
    /// Adds a service with field injection.
    /// </summary>
    /// <typeparam name="TKey">The key</typeparam>    
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServiceInjectFields<TKey>(this IocContainer container, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null) where TKey : class
    {
      return container.AddServiceInjectFields(typeof(TKey), life, postConstructor);
    }

    /// <summary>
    /// Adds many service with field injection by the given base type.  Looks in the calling assembly for the types.
    /// </summary>
    /// <param name="key">The base type key</param>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>

    public static IocContainer AddServicesInjectFieldsByBaseType(this IocContainer container, Type key, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null)
    {
      foreach (var type in key.Assembly.DefinedTypes.Where(t => t.IsSubclassOf(key)))
      {
        container.AddServiceInjectFields(type, life, postConstructor);
      }

      return container;
    }

    /// <summary>
    /// Adds many service with field injection by the given base type.  Looks in the calling assembly for the types.
    /// </summary>
    /// <typeparam name="TKey">The base type key</typeparam>
    /// <param name="container">The container</param>
    /// <param name="life">The life model</param>
    /// <param name="postConstructor">The post construction step</param>
    /// <returns>The container</returns>
    public static IocContainer AddServicesInjectFieldsByBaseType<TKey>(this IocContainer container, ServiceLife life = ServiceLife.Single, Action<IocProvider, object>? postConstructor = null) where TKey : class
    {
      return container.AddServicesInjectFieldsByBaseType(typeof(TKey), life, postConstructor);
    }

    /// <summary>
    /// Adds a singleton instance to the service
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="instance">The instance to add</param>
    /// <returns>The conatiner</returns>
    public static IocContainer AddSingletonService(this IocContainer container, object instance)
    {
      ServiceDefinition def = new ServiceDefinition()
      {
        Key = instance.GetType(),
        Factory = (provider) => instance,
        Life = ServiceLife.Single,
        PostConstructor = null
      };

      container.AddService(def);

      return container;
    }

    /// <summary>
    /// Adds a singleton under the supplied base type
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="baseType">The base type</param>
    /// <param name="instance">The instance</param>
    /// <param name="manage">If true then the singleton will be added to the cache so the provider can dispose of the instance if needed </param>
    /// <returns>The container</returns>
    public static IocContainer AddSingletonService(this IocContainer container, Type baseType, object instance, bool manage = false)
    {
      ServiceDefinition def = new ServiceDefinition()
      {
        Key = baseType,
        Factory = (provider) => instance,
        Life = manage ? ServiceLife.Single : ServiceLife.Multi,
        PostConstructor = null
      };

      container.AddService(def);

      return container;
    }

    /// <summary>
    /// Adds all of the executors that match the base type and the name of the executor
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="type">The type that hosts the function</param>
    /// <param name="functionName">The name of the function to register an executor for</param>
    /// <returns>The container</returns>
    /// <exception cref="ArgumentNullException">if any of the parameters are null</exception>
    /// <exception cref="InvalidOperationException">If the executor function isn't static</exception>
    public static IocContainer AddExecutorByTypeAndName(this IocContainer container, Type type, string functionName)
    {
      if (type == null)
        throw new ArgumentNullException(nameof(type));

      if (string.IsNullOrWhiteSpace(functionName))
        throw new ArgumentNullException(nameof(functionName));

      var function = type.GetMethod(functionName);

      if (function == null)
        throw new InvalidOperationException($"No function of name {type.Name}.{functionName} found");

      if (!function.IsStatic)
        throw new InvalidOperationException($"Function of name {type.Name}.{functionName} must be static");

      ExecutorDefinition executorDefinition = new ExecutorDefinition()
      {
        KeyType = type,
        KeyFunction = functionName,
        Factory = (provider) =>
        {

          StringBuilder errors = new StringBuilder();
          object?[] parameters = function
                      .GetParameters()
                      .Select(p => IocFactories.InjectByType(provider, type, p.ParameterType, p.Name ?? throw new InvalidOperationException($"In function {functionName} has an empty property name"), p.HasDefaultValue, p.DefaultValue, errors))
                      .ToArray();

          if (errors.Length > 0)
          {
            throw new InvalidOperationException(errors.ToString());
          }

          return function.Invoke(null, parameters);
        }
      };

      container.AddExecutor(executorDefinition);

      return container;
    }

    /// <summary>
    /// Finds of the static methods that are hosted in the provided type.
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="type">The hosting class type</param>
    /// <returns>The container</returns>
    /// <exception cref="ArgumentNullException">Type is null</exception>
    public static IocContainer AddExecutorByType(this IocContainer container, Type type)
    {
      if (type == null)
        throw new ArgumentNullException(nameof(type));

      var functions = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

      foreach (var function in functions)
      {
        ExecutorDefinition executorDefinition = new ExecutorDefinition()
        {
          KeyType = function.DeclaringType,
          KeyFunction = function.Name,
          Factory = (provider) =>
          {
            StringBuilder errors = new StringBuilder();
            object?[] parameters = function
                          .GetParameters()
                          .Select(p => IocFactories.InjectByType(provider, type, p.ParameterType, p.Name, p.HasDefaultValue, p.DefaultValue, errors))
                          .ToArray();

            if (errors.Length > 0)
              throw new InvalidOperationException(errors.ToString());

            return function.Invoke(null, parameters);
          }
        };

        container.AddExecutor(executorDefinition);
      }

      return container;
    }

    /// <summary>
    /// Adds all of the functions that have a matching name.  It looks in all of the types defined by the calling assembly
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="functionName">The function name</param>
    /// <returns>The container</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IocContainer AddExecutorByName(this IocContainer container, string functionName)
    {
      var functions = Assembly
          .GetCallingAssembly()
          .GetTypes()
          .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
          .Where(t => t.Name == functionName);

      foreach (var function in functions)
      {
        ExecutorDefinition executorDefinition = new ExecutorDefinition()
        {
          KeyType = function.DeclaringType,
          KeyFunction = function.Name,
          Factory = (provider) =>
          {

            StringBuilder errors = new StringBuilder();
            object?[] parameters = function
                          .GetParameters()
                          .Select(p => IocFactories.InjectByType(provider, function.DeclaringType ?? throw new InvalidOperationException($"Cannot add executor {functionName} because DeclaringType is null"), p.ParameterType, p.Name, p.HasDefaultValue, p.DefaultValue, errors))
                          .ToArray();

            if (errors.Length > 0)
            {
              throw new InvalidOperationException(errors.ToString());
            }

            return function?.Invoke(null, parameters);
          }
        };

        container.AddExecutor(executorDefinition);
      }

      return container;
    }
  }
}
