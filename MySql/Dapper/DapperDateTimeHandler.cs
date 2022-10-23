using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BeeEeeLibs.Mysql.Dapper
{
  /// <summary>
  /// Makes sure the dates returned from MySql are of the correct Kind
  /// </summary>
  public class DapperDateTimeHandler : SqlMapper.TypeHandler<DateTime>
  {
    ///<inheritdoc/>
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
      // always convert to date time
      parameter.Value = value.ToUniversalTime();
    }

    ///<inheritdoc/>
    public override DateTime Parse(object value)
    {
      DateTime dt = (DateTime)value;
      if (dt.Kind == DateTimeKind.Unspecified)
      {
        return new DateTime(dt.Ticks, DateTimeKind.Utc);
      }
      else
        return dt;
    }
  }
}
