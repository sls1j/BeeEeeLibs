using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeEeeLibs.DependencyInjection.InnerWorkings
{
  /// <summary>
  /// Defines an executer
  /// </summary>
  public class ExecutorDefinition
  {
    /// <summary>
    /// The type that contains the function to execute
    /// </summary>
    public Type KeyType = typeof(object);

    /// <summary>
    /// The name of the static method to execut
    /// </summary>
    public string KeyFunction = String.Empty;

    /// <summary>
    /// The factory that does the injection for the function
    /// </summary>
    public Func<IocProvider, object?> Factory = p => throw new NotImplementedException();

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"ExecutorDefiniton {KeyType} {KeyFunction}";
    }
  }
}
