using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BeeEeeLibs.Mysql.Dapper
{
  /// <summary>
  /// A class to help dapper handle enumeration types properly
  /// </summary>
  public class DapperStringArrayHandler : SqlMapper.TypeHandler<string[]>    
  {
    /// <inheritdoc />
    public override void SetValue(IDbDataParameter parameter, string[] value)
    {
      if (value != null && value.Length > 0)
      {
        parameter.Value = string.Join(',', value);
      }
    }

    /// <inheritdoc />
    public override string[] Parse(object value)
    {
      string valueStr = value as string;
      if(string.IsNullOrEmpty(valueStr))
      {
        return new string[0];
      }
      else
      {
        return valueStr.Split(',');
      }
    }
  }
}
