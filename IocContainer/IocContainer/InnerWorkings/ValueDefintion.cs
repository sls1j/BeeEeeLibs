using System;

namespace BeeEeeLibs.DependencyInjection.InnerWorkings
{
  /// <summary>
  /// The definition of a static value that is avaiable to be injected
  /// </summary>
  public class ValueDefintion
  {
    /// <summary>
    /// The parameter name to inject into
    /// </summary>
    public string Key = String.Empty;

    /// <summary>
    /// The parameter type to inject into
    /// </summary>
    public Type Type = typeof(object);

    /// <summary>
    /// The actual value
    /// </summary>
    public object Value = new object();

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"ValueDefinition {Key}:{Type} {Value??"null"}";
    }

  }
}