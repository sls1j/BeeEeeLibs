using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeEeeLibs.DependencyInjection.InnerWorkings
{
    /// <summary>
    /// Delegate to create an injector
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The injector function</returns>
    public delegate Func<IocProvider, object?> BuildInjectorDelegate(Type key);

    /// <summary>
    /// The injection factories
    /// </summary>
    public class IocFactories
    {
        /// <summary>
        /// Builds a constructor injector for the given type
        /// </summary>
        /// <param name="key">The class type</param>
        /// <returns>The injector factory</returns>
        /// <exception cref="InvalidOperationException">If there are errors</exception>
        public static Func<IocProvider, object?> BuildConstructorInject(Type key)
        {
            return (provider) =>
            {
                // get the constructor
                var constructor = key.GetConstructors().First();

                // get the parameters
                StringBuilder errors = new StringBuilder();
                object?[] parameters = constructor
                    .GetParameters()
                    .Select(p => InjectByType(provider, key, p.ParameterType, p.Name ?? string.Empty, p.HasDefaultValue, p.DefaultValue, errors))
                    .ToArray();

                if (errors.Length > 0)
                    throw new InvalidOperationException(errors.ToString());
                try
                {
                    return constructor.Invoke(parameters);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to create constructor for {key.Name}: {ex.Message}", ex);
                }
            };
        }

        /// <summary>
        /// Builds an injector function that injects into public properties
        /// </summary>
        /// <param name="key">The type to inject into</param>
        /// <returns>The injector function</returns>
        /// <exception cref="InvalidOperationException">If there are errors</exception>
        public static Func<IocProvider, object?> BuildPropertyInject(Type key)
        {
            return (provider) =>
            {
                // get the constructor                
                var constructor = key.GetConstructor(Type.EmptyTypes);
                var service = constructor?.Invoke(new object[0]);

                // get the properties
                var properties = key
                    .GetProperties()
                    .Where(p => p.CanWrite);

                StringBuilder errors = new StringBuilder();
                foreach (var p in properties)
                {
                    object? value = InjectByType(provider, key, p.PropertyType, p.Name, false, null, errors);
                    p.SetValue(service, value);
                }
                if (errors.Length > 0)
                {
                    throw new InvalidOperationException(errors.ToString());
                }
                return service;
            };
        }

        /// <summary>
        /// Builds an injector function that injects into public fields
        /// </summary>
        /// <param name="key">The object type to inject into</param>
        /// <returns>The injector function</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Func<IocProvider, object?> BuildFieldInject(Type key)
        {
            return (provider) =>
            {
                // get the constructor                
                var constructor = key.GetConstructor(Type.EmptyTypes);
                object? service = constructor?.Invoke(new object[0]);

                // get the properties
                var fields = key
                    .GetFields();

                StringBuilder errors = new StringBuilder();
                foreach (var f in fields)
                {
                    object? value = InjectByType(provider, key, f.FieldType, f.Name, false, null, errors);
                    f.SetValue(service, value);
                }
                if (errors.Length > 0)
                {
                    throw new InvalidOperationException(errors.ToString());
                }
                return service;
            };
        }

        /// <summary>
        /// A function that gets a matching value within the provider to inject
        /// </summary>
        /// <param name="provider">The provider to look into</param>
        /// <param name="rootType">The base or root type of object</param>
        /// <param name="type">The actual type of the object</param>
        /// <param name="name">The name of the thing to inject.  Could be constructor parameter, fields name, or property name, depending on which type of injector is being used</param>
        /// <param name="hadDefaultValue">If true then there is a defaultValue even if that value is null</param>
        /// <param name="defaultValue">The default value if there isn't a match for a given parameter</param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static object? InjectByType(IocProvider provider, Type rootType, Type type, string name, bool hadDefaultValue, object? defaultValue, StringBuilder errors)
        {
            try
            {
                if (name != null && type.IsSubclassOf(typeof(Delegate)))
                {
                    return provider.GetFunction(name);
                }
                else if (name != null && (type.IsArray || type.IsValueType || type == typeof(string)))
                {
                    return provider.GetValue(name);
                }
                else
                {
                    return provider.GetService(type);
                }
            }
            catch (Exception ex)
            {
                if (hadDefaultValue)
                {
                    return defaultValue;
                }
                else
                {
                    errors.AppendLine($"Failed to inject {name} : {type.Name} into {rootType.Name} {ex.Message} {ex.StackTrace}");
                    return null;
                }
            }
        }
    }
}
