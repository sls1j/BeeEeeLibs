using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BeeEeeLibs.Mysql.Dapper
{
  /// <summary>
  /// A class to help dapper handle enumeration types properly
  /// </summary>
  public class DapperSerializerHandler<T> : SqlMapper.TypeHandler<T>    
  {
    private T DefaultValue;

    /// <summary>
    /// Constructor of serializer handler
    /// </summary>
    /// <param name="defaultValue"></param>
    public DapperSerializerHandler(T defaultValue)
    {
      DefaultValue = defaultValue;
    }

    /// <inheritdoc />
    public override void SetValue(IDbDataParameter parameter, T value)
    {
      if (value != null)
      {
        string strValue = JsonConvert.SerializeObject(value);
        parameter.Value = strValue;
      }
    }

    /// <inheritdoc />
    public override T Parse(object value)
    {
      string valueStr = value as string;
      if(string.IsNullOrEmpty(valueStr))
      {
        return DefaultValue;
      }
      else
      {
        return JsonConvert.DeserializeObject<T>(valueStr);
      }
    }
  }
}
