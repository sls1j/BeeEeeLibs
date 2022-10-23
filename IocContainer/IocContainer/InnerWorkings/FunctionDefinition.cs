using System;

namespace BeeEeeLibs.DependencyInjection.InnerWorkings
{
  /// <summary>
  /// defines a function that is available for injection
  /// </summary>
  public class FunctionDefinition
  {
    /// <summary>
    /// The name of the parameter that will be injected
    /// </summary>
    public string Key = String.Empty;

    /// <summary>
    /// The function that will be injected
    /// </summary>
    public Delegate Function = new Action(() => throw new NotImplementedException());

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"FunctionDefinition {Key}";
    }

  }
}