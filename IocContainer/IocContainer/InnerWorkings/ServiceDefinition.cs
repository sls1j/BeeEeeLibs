using System;

namespace BeeEeeLibs.DependencyInjection.InnerWorkings
{
  /// <summary>
  /// Defines an injectable service 
  /// </summary>
  public class ServiceDefinition
  {
    /// <summary>
    /// The type that a service can be injected into, or created
    /// </summary>
    public Type Key = typeof(object);

    /// <summary>
    /// The function that creates the object
    /// </summary>
    public Func<IocProvider, object?>? Factory = p => throw new NotImplementedException();

    /// <summary>
    /// Function that is called after the service has been created and injected
    /// </summary>
    public Action<IocProvider, object>? PostConstructor;

    /// <summary>
    /// The service life of the service.  Can be Single, or Multi
    /// </summary>
    public ServiceLife Life;

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"ServiceDefinition {Key}:{Life}";
    }

  }
}